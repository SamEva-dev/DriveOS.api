using DriveOS.Modules.Contracts.Application.TrainingContracts.Create;
using DriveOS.Modules.CRM.Domain.Offers;
using DriveOS.Modules.CRM.Infrastructure.Persistence;
using DriveOS.Modules.Organizations.Infrastructure.Persistence;
using DriveOS.Modules.Students.Application.References;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Api.Integrations.Contracts;
internal sealed class TrainingContractSourceGateway(CrmDbContext crm, IStudentReferenceReadService students, OrganizationsDbContext organizations) : ITrainingContractSourceGateway
{
    public async Task<Result<TrainingContractSourceSnapshot>> ResolveAsync(OrganizationId organizationId, DraftEnrollmentId enrollmentId, CommercialOfferId offerId, CancellationToken ct = default)
    {
        StudentContractSourceReference? enrollment = await students.GetContractSourceAsync(organizationId, enrollmentId, ct);
        if (enrollment is null) return Result.Failure<TrainingContractSourceSnapshot>(CreateTrainingContractErrors.EnrollmentNotFound);
        var offer = await crm.CommercialOffers.AsNoTracking().SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == offerId, ct);
        if (offer is null) return Result.Failure<TrainingContractSourceSnapshot>(CreateTrainingContractErrors.OfferNotFound);
        if (offer.Status != CommercialOfferStatus.Accepted) return Result.Failure<TrainingContractSourceSnapshot>(CreateTrainingContractErrors.OfferNotAccepted);
        if (enrollment.SourceLeadId is null || enrollment.SourceLeadId.Value != offer.LeadId.Value || !string.Equals(enrollment.TrainingCode, offer.TrainingCode, StringComparison.OrdinalIgnoreCase) || (offer.BranchId.HasValue && offer.BranchId.Value != enrollment.BranchId))
            return Result.Failure<TrainingContractSourceSnapshot>(CreateTrainingContractErrors.SourceMismatch);
        var organization = await organizations.Organizations.AsNoTracking().SingleOrDefaultAsync(x => x.Id == organizationId, ct);
        if (organization is null) return Result.Failure<TrainingContractSourceSnapshot>(CreateTrainingContractErrors.OrganizationNotFound);
        return Result.Success(new TrainingContractSourceSnapshot(organizationId, enrollment.BranchId, enrollment.StudentId,
            enrollment.StudentDisplayName, organization.LegalName, offer.Id, offer.Version,
            offer.TrainingCode, offer.Amount, offer.Currency));
    }
}
