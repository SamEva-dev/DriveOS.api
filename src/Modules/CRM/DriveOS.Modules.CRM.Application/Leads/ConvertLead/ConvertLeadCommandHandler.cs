using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
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
    IStudentProvisioningGateway studentProvisioning,
    IClock clock,
    ICrmUnitOfWork unitOfWork
) : ICommandHandler<ConvertLeadCommand, ConvertLeadResponse>
{
    public async Task<Result<ConvertLeadResponse>> Handle(
        ConvertLeadCommand command,
        CancellationToken cancellationToken
    )
    {
        LeadConversion? existing = await conversions.GetByLeadIdAsync(
            command.OrganizationId,
            command.LeadId,
            cancellationToken
        );
        if (existing?.Status == LeadConversionStatus.Completed)
            return Result.Success(ToResponse(existing, true));

        Lead? lead = await leads.GetByIdForUpdateAsync(
            command.OrganizationId,
            command.LeadId,
            cancellationToken
        );
        if (lead is null)
            return Result.Failure<ConvertLeadResponse>(LeadErrors.NotFound);
        if (lead.Status != LeadStatus.Won)
            return Result.Failure<ConvertLeadResponse>(LeadErrors.ConversionRequiresWonStatus);
        if (lead.Qualification is null)
            return Result.Failure<ConvertLeadResponse>(LeadErrors.ConversionRequiresQualification);

        bool alreadyRequested = existing is not null;
        LeadConversion conversion;
        if (existing is null)
        {
            CommercialOffer? offer = await offers.GetByIdAsync(
                command.OrganizationId,
                command.AcceptedOfferId,
                cancellationToken
            );
            if (
                offer is null
                || offer.LeadId != command.LeadId
                || offer.Status != CommercialOfferStatus.Accepted
            )
                return Result.Failure<ConvertLeadResponse>(
                    LeadErrors.ConversionAcceptedOfferRequired
                );
            if (
                !command.IdentityVerified
                || !command.ConsentsVerified
                || !command.DuplicateCheckCompleted
            )
                return Result.Failure<ConvertLeadResponse>(
                    LeadErrors.ConversionPreconditionsIncomplete
                );

            conversion = LeadConversion.Request(
                command.OrganizationId,
                lead,
                command.AcceptedOfferId,
                command.BranchId,
                command.ResponsibleUserId,
                command.TrainingCode,
                command.IdentityVerified,
                command.ConsentsVerified,
                command.DuplicateCheckCompleted,
                command.GuardianSummary,
                command.PayerSummary,
                string.Join(
                    ',',
                    command
                        .RequiredDocumentCodes.Select(x => x.Trim())
                        .Where(x => x.Length > 0)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                )
            );
            await conversions.AddAsync(conversion, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
        }
        else
        {
            conversion = existing;
        }

        Result<StudentProvisioningResult> provisioning = await studentProvisioning.ProvisionAsync(
            new StudentProvisioningRequest(
                conversion.OrganizationId,
                conversion.LeadId,
                conversion.BranchId,
                conversion.FirstName,
                conversion.LastName,
                conversion.Email,
                conversion.Phone,
                conversion.TrainingCode
            ),
            cancellationToken
        );
        if (provisioning.IsFailure)
            return Result.Failure<ConvertLeadResponse>(provisioning.Error);

        LeadConversion? trackedConversion = await conversions.GetByLeadIdForUpdateAsync(
            command.OrganizationId,
            command.LeadId,
            cancellationToken
        );
        if (trackedConversion is null)
            return Result.Failure<ConvertLeadResponse>(LeadConversionErrors.NotFound);
        Result completion = trackedConversion.Complete(
            provisioning.Value.StudentId,
            provisioning.Value.EnrollmentId,
            clock.UtcNow
        );
        if (completion.IsFailure)
            return Result.Failure<ConvertLeadResponse>(completion.Error);
        Result leadCompletion = lead.MarkConverted(
            provisioning.Value.StudentId,
            provisioning.Value.EnrollmentId,
            clock.UtcNow
        );
        if (leadCompletion.IsFailure)
            return Result.Failure<ConvertLeadResponse>(leadCompletion.Error);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(ToResponse(trackedConversion, alreadyRequested));
    }

    private static ConvertLeadResponse ToResponse(LeadConversion value, bool existing) =>
        new(
            value.Id.Value,
            value.Status.ToString(),
            existing,
            value.AcceptedOfferId.Value,
            value.StudentPersonId?.Value,
            value.StudentEnrollmentId?.Value,
            [
                new("identity", value.IdentityVerified),
                new("duplicates", value.DuplicateCheckCompleted),
                new("consents", value.ConsentsVerified),
                new("studentProfile", value.StudentPersonId.HasValue),
                new("enrollment", value.StudentEnrollmentId.HasValue),
                new("contract", false),
                new("documents", !string.IsNullOrWhiteSpace(value.RequiredDocumentCodes)),
            ]
        );
}
