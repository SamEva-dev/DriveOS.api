using DriveOS.Application.Abstractions.Pagination;
using DriveOS.Application.Abstractions.Sorting;
using DriveOS.Modules.Contracts.Application.TrainingContracts.Read;
using DriveOS.Modules.Contracts.Domain.TrainingContracts;
using DriveOS.Modules.Contracts.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Contracts.Infrastructure.Read;

internal sealed class TrainingContractReadService(ContractsDbContext db) : ITrainingContractReadService
{
    public async Task<TrainingContractDetailResponse?> GetAsync(
        OrganizationId organizationId,
        TrainingContractId contractId,
        CancellationToken ct = default)
    {
        var contract = await db.TrainingContracts
            .AsNoTracking()
            .Include(x => x.Parties)
            .Include(x => x.Signatories)
            .Include(x => x.Versions)
            .SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == contractId, ct);

        if (contract is null)
            return null;

        var process = await db.SignatureProcesses
            .AsNoTracking()
            .Include(x => x.Recipients)
            .Include(x => x.Evidence)
            .Where(x => x.OrganizationId == organizationId && x.ContractId == contractId && x.ContractVersionNumber == contract.CurrentVersionNumber)
            .SingleOrDefaultAsync(ct);

        var amendments = await db.ContractAmendments
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId && x.ContractId == contractId)
            .OrderByDescending(x => x.AmendmentNumber)
            .Select(x => new ContractAmendmentResponse(
                x.Id.Value, x.AmendmentNumber, x.BaseContractVersionNumber, x.Reason, x.EffectiveDate,
                x.StartDate, x.EndDate, x.TotalAmount, x.Currency, x.Status.ToString(),
                x.SignedDocumentReference, x.SignedDocumentSha256, x.SignedAtUtc, x.AppliedAtUtc,
                x.CancellationReason, x.CreatedAtUtc))
            .ToArrayAsync(ct);

        SignatureProcessResponse? processResponse = process is null
            ? null
            : new SignatureProcessResponse(
                process.Id.Value,
                process.ContractVersionNumber,
                process.DocumentSha256,
                process.SignatureOrder,
                process.Status.ToString(),
                process.RequestedAtUtc,
                process.RequestedByUserId.Value,
                process.CompletedAtUtc,
                process.Recipients.OrderBy(x => x.SigningOrder).ThenBy(x => x.DisplayName).Select(x =>
                    new SignatureProcessRecipientResponse(
                        x.SignatoryId.Value,
                        x.Kind,
                        x.PersonId.Value,
                        x.RepresentedOrganizationId?.Value,
                        x.DisplayName,
                        x.SigningOrder,
                        x.IsRequired,
                        process.Evidence.Any(e => e.SignatoryId == x.SignatoryId))).ToArray(),
                process.Evidence.OrderBy(x => x.SignedAtUtc).Select(x =>
                    new SignatureEvidenceResponse(
                        x.Id.Value,
                        x.SignatoryId.Value,
                        x.PersonId.Value,
                        x.DocumentSha256,
                        x.SignatureMethod,
                        x.AuthenticationMethod,
                        x.Provider,
                        x.ProviderSignatureReference,
                        x.CertificateReference,
                        x.IpAddress,
                        x.UserAgent,
                        x.SignedAtUtc,
                        x.ReceivedAtUtc,
                        x.RecordedByUserId.Value)).ToArray());

        return new TrainingContractDetailResponse(
            contract.Id.Value,
            contract.OrganizationId.Value,
            contract.BranchId.Value,
            contract.StudentId.Value,
            contract.SourceOfferId.Value,
            contract.SourceOfferVersion,
            contract.ContractNumber,
            contract.StartDate,
            contract.EndDate,
            contract.TotalAmount,
            contract.Currency,
            contract.CurrentVersionNumber,
            contract.Status.ToString(),
            new TrainingContractTermsResponse(
                contract.TermsSnapshot.TrainingCode,
                contract.TermsSnapshot.PracticalHours,
                contract.TermsSnapshot.ServicesSnapshot,
                contract.TermsSnapshot.PaymentScheduleSnapshot,
                contract.TermsSnapshot.CancellationTerms,
                contract.TermsSnapshot.BookingRules,
                contract.TermsSnapshot.StudentObligations,
                contract.TermsSnapshot.ProviderObligations,
                contract.TermsSnapshot.ExamPresentationTerms,
                contract.TermsSnapshot.DataProcessingTerms),
            contract.Parties.Select(x => new TrainingContractPartyResponse(
                x.Kind.ToString(), x.PersonId?.Value, x.OrganizationId?.Value, x.DisplayName, x.LegalReference)).ToArray(),
            contract.Versions.OrderByDescending(x => x.VersionNumber).Select(x => new TrainingContractVersionResponse(
                x.Id.Value, x.VersionNumber, x.SourceOfferId.Value, x.SourceOfferVersion, x.StartDate, x.EndDate,
                x.TotalAmount, x.Currency, x.RevisionReason, x.CreatedByUserId?.Value, x.CreatedAtUtc)).ToArray(),
            contract.Signatories.OrderBy(x => x.SigningOrder).ThenBy(x => x.DisplayName).Select(x => new TrainingContractSignatoryResponse(
                x.Id.Value, x.Kind.ToString(), x.PersonId.Value, x.RepresentedOrganizationId?.Value, x.DisplayName,
                x.SigningOrder, x.IsRequired, x.AuthorityReference, x.AuthorityStatus.ToString(),
                x.AuthorityVerifiedByUserId?.Value, x.AuthorityVerifiedAtUtc, x.AuthorityRejectionReason, x.Status.ToString())).ToArray(),
            amendments,
            processResponse,
            contract.GeneratedDocumentFileName,
            contract.GeneratedDocumentContentType,
            contract.GeneratedDocumentSha256,
            contract.GeneratedDocumentVersionNumber,
            contract.GeneratedAtUtc,
            contract.GeneratedByUserId?.Value,
            contract.CreatedAtUtc,
            contract.CreatedByUserId?.Value,
            contract.LastModifiedAtUtc,
            contract.LastModifiedByUserId?.Value,
            contract.ActivatedAtUtc,
            contract.ActivatedByUserId?.Value,
            contract.SuspensionReason,
            contract.SuspensionEffectiveDate,
            contract.SuspensionExpectedResumeDate,
            contract.SuspendedAtUtc,
            contract.SuspendedByUserId?.Value,
            contract.TerminationReason,
            contract.TerminationEffectiveDate,
            contract.TerminatedAtUtc,
            contract.TerminatedByUserId?.Value,
            contract.CompletionNote,
            contract.CompletionEffectiveDate,
            contract.CompletedAtUtc,
            contract.CompletedByUserId?.Value,
            contract.ExpirationEffectiveDate,
            contract.ExpiredAtUtc,
            contract.ExpiredByUserId?.Value);
    }

    public async Task<IReadOnlyList<TrainingContractListItemResponse>> ListAsync(
        OrganizationId organizationId,
        PersonId? studentId,
        CancellationToken ct = default)
    {
        var query = db.TrainingContracts.AsNoTracking().Where(x => x.OrganizationId == organizationId);
        if (studentId is not null)
            query = query.Where(x => x.StudentId == studentId);

        return await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new TrainingContractListItemResponse(
                x.Id.Value,
                x.ContractNumber,
                x.StudentId.Value,
                x.BranchId.Value,
                x.CurrentVersionNumber,
                x.Status.ToString(),
                x.StartDate,
                x.EndDate,
                x.TotalAmount,
                x.Currency,
                x.TermsSnapshot.TrainingCode,
                x.CreatedAtUtc))
            .ToListAsync(ct);
    }
    public async Task<PagedResult<TrainingContractListItemResponse>> SearchAsync(
        SearchTrainingContractsQuery request,
        CancellationToken ct = default)
    {
        int pageNumber = Math.Max(1, request.PageNumber);
        int pageSize = Math.Clamp(request.PageSize, 10, 100);

        IQueryable<TrainingContract> query = db.TrainingContracts
            .AsNoTracking()
            .Where(x => x.OrganizationId == request.OrganizationId);

        if (request.StudentId is not null)
            query = query.Where(x => x.StudentId == request.StudentId.Value);

        if (request.BranchId is not null)
            query = query.Where(x => x.BranchId == request.BranchId.Value);

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            string pattern = $"%{request.Search.Trim()}%";
            query = query.Where(x =>
                EF.Functions.ILike(x.ContractNumber, pattern)
                || EF.Functions.ILike(x.Currency, pattern));
        }

        if (request.StartsFrom.HasValue)
            query = query.Where(x => x.StartDate >= request.StartsFrom.Value);
        if (request.StartsTo.HasValue)
            query = query.Where(x => x.StartDate <= request.StartsTo.Value);
        if (request.EndsFrom.HasValue)
            query = query.Where(x => x.EndDate != null && x.EndDate >= request.EndsFrom.Value);
        if (request.EndsTo.HasValue)
            query = query.Where(x => x.EndDate != null && x.EndDate <= request.EndsTo.Value);

        long totalCount = await query.LongCountAsync(ct);
        query = ApplySorting(query, request.SortBy, request.SortDirection);

        TrainingContractListItemResponse[] items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new TrainingContractListItemResponse(
                x.Id.Value,
                x.ContractNumber,
                x.StudentId.Value,
                x.BranchId.Value,
                x.CurrentVersionNumber,
                x.Status.ToString(),
                x.StartDate,
                x.EndDate,
                x.TotalAmount,
                x.Currency,
                x.TermsSnapshot.TrainingCode,
                x.CreatedAtUtc))
            .ToArrayAsync(ct);

        return new PagedResult<TrainingContractListItemResponse>(items, pageNumber, pageSize, totalCount);
    }

    private static IQueryable<TrainingContract> ApplySorting(
        IQueryable<TrainingContract> query,
        TrainingContractSortField field,
        SortDirection direction) =>
        (field, direction) switch
        {
            (TrainingContractSortField.ContractNumber, SortDirection.Ascending) => query.OrderBy(x => x.ContractNumber),
            (TrainingContractSortField.ContractNumber, _) => query.OrderByDescending(x => x.ContractNumber),
            (TrainingContractSortField.StartDate, SortDirection.Ascending) => query.OrderBy(x => x.StartDate),
            (TrainingContractSortField.StartDate, _) => query.OrderByDescending(x => x.StartDate),
            (TrainingContractSortField.EndDate, SortDirection.Ascending) => query.OrderBy(x => x.EndDate),
            (TrainingContractSortField.EndDate, _) => query.OrderByDescending(x => x.EndDate),
            (TrainingContractSortField.Status, SortDirection.Ascending) => query.OrderBy(x => x.Status).ThenByDescending(x => x.CreatedAtUtc),
            (TrainingContractSortField.Status, _) => query.OrderByDescending(x => x.Status).ThenByDescending(x => x.CreatedAtUtc),
            (TrainingContractSortField.TotalAmount, SortDirection.Ascending) => query.OrderBy(x => x.TotalAmount),
            (TrainingContractSortField.TotalAmount, _) => query.OrderByDescending(x => x.TotalAmount),
            (TrainingContractSortField.CreatedAt, SortDirection.Ascending) => query.OrderBy(x => x.CreatedAtUtc),
            _ => query.OrderByDescending(x => x.CreatedAtUtc),
        };

}
