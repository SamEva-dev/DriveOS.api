using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Application.Abstractions.Persistence;
using DriveOS.Modules.CRM.Domain.Conversions;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.Modules.CRM.Domain.Offers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Application.Leads.ConvertLead;

public sealed class ConvertLeadCommandHandler(
    ILeadRepository leads,
    ILeadConversionRepository conversions,
    ICommercialOfferRepository offers,
    ICrmUnitOfWork unitOfWork)
    : ICommandHandler<ConvertLeadCommand, ConvertLeadResponse>
{
    public async Task<Result<ConvertLeadResponse>> Handle(ConvertLeadCommand command,
        CancellationToken cancellationToken)
    {
        LeadConversion? existing = await conversions.GetByLeadIdAsync(
            command.OrganizationId, command.LeadId, cancellationToken);
        if (existing is not null)
            return Result.Success(ToResponse(existing, true));

        Lead? lead = await leads.GetByIdForUpdateAsync(command.OrganizationId,
            command.LeadId, cancellationToken);
        if (lead is null)
            return Result.Failure<ConvertLeadResponse>(LeadErrors.NotFound);

        CommercialOffer? offer = await offers.GetByIdAsync(command.OrganizationId,
            command.AcceptedOfferId, cancellationToken);
        if (offer is null || offer.LeadId != command.LeadId)
            return Result.Failure<ConvertLeadResponse>(LeadErrors.ConversionAcceptedOfferRequired);
        if (offer.Status != CommercialOfferStatus.Accepted)
            return Result.Failure<ConvertLeadResponse>(LeadErrors.ConversionAcceptedOfferRequired);
        if (!command.IdentityVerified || !command.ConsentsVerified || !command.DuplicateCheckCompleted)
            return Result.Failure<ConvertLeadResponse>(LeadErrors.ConversionPreconditionsIncomplete);

        // CRM owns the conversion request, not the student aggregates. Person and
        // enrollment are created by Student Administration after this request.
        LeadConversion conversion = LeadConversion.Request(command.OrganizationId, lead,
            command.AcceptedOfferId, command.BranchId, command.ResponsibleUserId,
            command.TrainingCode, command.IdentityVerified, command.ConsentsVerified,
            command.DuplicateCheckCompleted, command.GuardianSummary, command.PayerSummary,
            string.Join(',', command.RequiredDocumentCodes.Select(x => x.Trim()).Where(x => x.Length > 0).Distinct()));
        await conversions.AddAsync(conversion, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(ToResponse(conversion, false));
    }

    private static ConvertLeadResponse ToResponse(LeadConversion value, bool existing) =>
        new(value.Id.Value, value.Status.ToString(), existing, value.AcceptedOfferId.Value,
            value.StudentPersonId?.Value, value.StudentEnrollmentId?.Value,
            [new("identity", value.IdentityVerified), new("duplicates", value.DuplicateCheckCompleted),
             new("consents", value.ConsentsVerified), new("studentProfile", value.StudentPersonId.HasValue),
             new("enrollment", value.StudentEnrollmentId.HasValue), new("contract", false),
             new("documents", string.IsNullOrWhiteSpace(value.RequiredDocumentCodes))]);
}
