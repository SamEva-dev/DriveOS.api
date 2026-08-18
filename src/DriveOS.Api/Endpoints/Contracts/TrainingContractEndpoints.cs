using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Application.Abstractions.Pagination;
using DriveOS.Application.Abstractions.Sorting;
using DriveOS.Modules.Contracts.Application.TrainingContracts.Activate;
using DriveOS.Modules.Contracts.Application.Auditing;
using DriveOS.Security.Contracts;
using DriveOS.Modules.Contracts.Application.ContractAmendments;
using DriveOS.Modules.Contracts.Application.ContractDocuments;
using DriveOS.Modules.Contracts.Application.TrainingContracts.Create;
using DriveOS.Modules.Contracts.Application.TrainingContracts.Complete;
using DriveOS.Modules.Contracts.Application.TrainingContracts.Expire;
using DriveOS.Modules.Contracts.Application.TrainingContracts.Generate;
using DriveOS.Modules.Contracts.Application.TrainingContracts.Read;
using DriveOS.Modules.Contracts.Domain.TrainingContracts;
using DriveOS.Modules.Contracts.Application.TrainingContracts.Signatories;
using DriveOS.Modules.Contracts.Application.TrainingContracts.Suspend;
using DriveOS.Modules.Contracts.Application.TrainingContracts.Terminate;
using DriveOS.Modules.Contracts.Application.TrainingContracts.Signature;
using DriveOS.Modules.Contracts.Application.TrainingContracts.Signature.Record;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.Contracts;

public sealed record AddTrainingContractSignatoryRequest(string Kind, Guid PersonId, Guid? RepresentedOrganizationId, string DisplayName, int SigningOrder, bool IsRequired, string? AuthorityReference);
public sealed record UpdateTrainingContractSignatoryRequest(int SigningOrder, bool IsRequired, string DisplayName, string? AuthorityReference);
public sealed record DecideTrainingContractSignatoryAuthorityRequest(bool Approved, string? Reason);

public sealed record RecordTrainingContractSignatureRequest(
    Guid SignatoryId,
    string DocumentSha256,
    string SignatureMethod,
    string AuthenticationMethod,
    string Provider,
    string ProviderSignatureReference,
    string? CertificateReference,
    string? IpAddress,
    string? UserAgent,
    DateTimeOffset SignedAtUtc);


public sealed record CreateContractAmendmentRequest(
    string Reason, DateOnly EffectiveDate, DateOnly StartDate, DateOnly? EndDate, decimal TotalAmount, string Currency,
    decimal PracticalHours, string ServicesSnapshot, string PaymentScheduleSnapshot, string CancellationTerms,
    string BookingRules, string StudentObligations, string ProviderObligations, string ExamPresentationTerms, string DataProcessingTerms);
public sealed record RecordContractAmendmentSignedProofRequest(string SignedDocumentReference, string DocumentSha256, DateTimeOffset SignedAtUtc);
public sealed record CancelContractAmendmentRequest(string Reason);
public sealed record SuspendTrainingContractRequest(string Reason, DateOnly EffectiveDate, DateOnly? ExpectedResumeDate);
public sealed record TerminateTrainingContractRequest(string Reason, DateOnly EffectiveDate);
public sealed record CompleteTrainingContractRequest(string Note, DateOnly EffectiveDate);

public sealed record CreateTrainingContractRequest(Guid EnrollmentId, Guid SourceOfferId, string ContractNumber, DateOnly StartDate, DateOnly? EndDate, decimal PracticalHours, string ServicesSnapshot, string PaymentScheduleSnapshot, string CancellationTerms, string BookingRules, string StudentObligations, string ProviderObligations, string ExamPresentationTerms, string DataProcessingTerms, string? ProviderLegalReference, string? StudentLegalReference);

public static class TrainingContractEndpoints
{
    public static IEndpointRouteBuilder MapTrainingContractEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder contracts = endpoints.MapGroup("/api/contracts/training").WithTags("Contracts - Training");

        contracts.MapGet("", GetListAsync)
            .WithName("GetTrainingContracts")
            .Produces<IReadOnlyList<TrainingContractListItemResponse>>()
            .RequireAuthorization(DriveOsPermissionCodes.Contracts.Read);

        contracts.MapGet("/search", SearchAsync)
            .WithName("SearchTrainingContracts")
            .Produces<PagedResult<TrainingContractListItemResponse>>()
            .RequireAuthorization(DriveOsPermissionCodes.Contracts.Read);

        contracts.MapGet("/{contractId:guid}", GetByIdAsync)
            .WithName("GetTrainingContract")
            .Produces<TrainingContractDetailResponse>()
            .RequireAuthorization(DriveOsPermissionCodes.Contracts.Read);

        contracts.MapPost("", CreateAsync)
            .WithName("CreateTrainingContract")
            .Produces<Guid>(StatusCodes.Status201Created)
            .RequireAuthorization(DriveOsPermissionCodes.Contracts.Create);


        contracts.MapPost("/{contractId:guid}/signatories", AddSignatoryAsync)
            .WithName("AddTrainingContractSignatory")
            .Produces<Guid>(StatusCodes.Status201Created)
            .RequireAuthorization(DriveOsPermissionCodes.Contracts.SignatoriesManage);

        contracts.MapPut("/{contractId:guid}/signatories/{signatoryId:guid}", UpdateSignatoryAsync)
            .WithName("UpdateTrainingContractSignatory")
            .RequireAuthorization(DriveOsPermissionCodes.Contracts.SignatoriesManage);

        contracts.MapDelete("/{contractId:guid}/signatories/{signatoryId:guid}", RemoveSignatoryAsync)
            .WithName("RemoveTrainingContractSignatory")
            .RequireAuthorization(DriveOsPermissionCodes.Contracts.SignatoriesManage);

        contracts.MapPost("/{contractId:guid}/signatories/{signatoryId:guid}/authority", DecideSignatoryAuthorityAsync)
            .WithName("DecideTrainingContractSignatoryAuthority")
            .RequireAuthorization(DriveOsPermissionCodes.Contracts.SignatoriesVerify);

        contracts.MapPost("/{contractId:guid}/generate", GenerateAsync)
            .WithName("GenerateTrainingContract")
            .Produces<GeneratedTrainingContractResponse>()
            .RequireAuthorization(DriveOsPermissionCodes.Contracts.Generate);

        contracts.MapPost("/{contractId:guid}/send-for-signature", SendForSignatureAsync)
            .WithName("SendTrainingContractForSignature")
            .Produces<SendTrainingContractForSignatureResponse>()
            .RequireAuthorization(DriveOsPermissionCodes.Contracts.SignatureSend);

        contracts.MapPost("/{contractId:guid}/activate", ActivateAsync)
            .WithName("ActivateTrainingContract")
            .Produces<ActivateTrainingContractResponse>()
            .RequireAuthorization(DriveOsPermissionCodes.Contracts.Activate);

        contracts.MapPost("/{contractId:guid}/suspend", SuspendAsync)
            .WithName("SuspendTrainingContract")
            .Produces<SuspendTrainingContractResponse>()
            .RequireAuthorization(DriveOsPermissionCodes.Contracts.Suspend);

        contracts.MapPost("/{contractId:guid}/terminate", TerminateAsync)
            .WithName("TerminateTrainingContract")
            .Produces<TerminateTrainingContractResponse>()
            .RequireAuthorization(DriveOsPermissionCodes.Contracts.Terminate);

        contracts.MapPost("/{contractId:guid}/complete", CompleteAsync)
            .WithName("CompleteTrainingContract")
            .Produces<CompleteTrainingContractResponse>()
            .RequireAuthorization(DriveOsPermissionCodes.Contracts.Complete);

        contracts.MapPost("/{contractId:guid}/expire", ExpireAsync)
            .WithName("ExpireTrainingContract")
            .Produces<ExpireTrainingContractResponse>()
            .RequireAuthorization(DriveOsPermissionCodes.Contracts.Expire);

        contracts.MapPost("/{contractId:guid}/signature-processes/{signatureProcessId:guid}/signatures", RecordSignatureAsync)
            .WithName("RecordTrainingContractSignature")
            .Produces<RecordTrainingContractSignatureResponse>()
            .RequireAuthorization(DriveOsPermissionCodes.Contracts.SignatureRecord);

        contracts.MapPost("/{contractId:guid}/amendments", CreateAmendmentAsync)
            .WithName("CreateContractAmendment")
            .Produces<CreateContractAmendmentResponse>(StatusCodes.Status201Created)
            .RequireAuthorization(DriveOsPermissionCodes.Contracts.AmendmentsManage);

        contracts.MapPost("/{contractId:guid}/amendments/{amendmentId:guid}/signed-proof", RecordAmendmentSignedProofAsync)
            .WithName("RecordContractAmendmentSignedProof")
            .RequireAuthorization(DriveOsPermissionCodes.Contracts.AmendmentsSignatureRecord);

        contracts.MapPost("/{contractId:guid}/amendments/{amendmentId:guid}/apply", ApplyAmendmentAsync)
            .WithName("ApplyContractAmendment")
            .Produces<ApplyContractAmendmentResponse>()
            .RequireAuthorization(DriveOsPermissionCodes.Contracts.AmendmentsApply);

        contracts.MapPost("/{contractId:guid}/amendments/{amendmentId:guid}/cancel", CancelAmendmentAsync)
            .WithName("CancelContractAmendment")
            .RequireAuthorization(DriveOsPermissionCodes.Contracts.AmendmentsManage);

        contracts.MapGet("/{contractId:guid}/documents", GetDocumentsAsync)
            .WithName("GetTrainingContractDocuments")
            .Produces<IReadOnlyList<ContractDocumentResponse>>()
            .RequireAuthorization(DriveOsPermissionCodes.Contracts.DocumentsRead);

        contracts.MapPost("/{contractId:guid}/documents", UploadDocumentAsync)
            .WithName("UploadTrainingContractDocument")
            .DisableAntiforgery()
            .Produces<ContractDocumentResponse>(StatusCodes.Status201Created)
            .RequireAuthorization(DriveOsPermissionCodes.Contracts.DocumentsUpload);

        contracts.MapPost("/{contractId:guid}/documents/{documentId:guid}/versions", AddDocumentVersionAsync)
            .WithName("AddTrainingContractDocumentVersion")
            .DisableAntiforgery()
            .Produces<ContractDocumentResponse>()
            .RequireAuthorization(DriveOsPermissionCodes.Contracts.DocumentsUpload);

        contracts.MapPost("/{contractId:guid}/documents/{documentId:guid}/archive", ArchiveDocumentAsync)
            .WithName("ArchiveTrainingContractDocument")
            .RequireAuthorization(DriveOsPermissionCodes.Contracts.DocumentsArchive);

        contracts.MapGet("/{contractId:guid}/audit", GetAuditAsync)
            .WithName("GetTrainingContractAudit")
            .Produces<IReadOnlyList<ContractAuditEntryResponse>>()
            .RequireAuthorization(DriveOsPermissionCodes.Contracts.AuditRead);

        contracts.MapGet("/{contractId:guid}/history", GetHistoryAsync)
            .WithName("GetTrainingContractHistory")
            .Produces<TrainingContractHistoryResponse>()
            .RequireAuthorization(DriveOsPermissionCodes.Contracts.AuditRead);

        return endpoints;
    }

    private static async Task<IResult> GetListAsync(Guid? studentId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return Results.Problem(statusCode: 401, title: "errors.currentTenant.required");

        PersonId? personId = studentId.HasValue ? new PersonId(studentId.Value) : null;
        Result<IReadOnlyList<TrainingContractListItemResponse>> result = await mediator.Send(
            new GetTrainingContractsQuery(tenant.OrganizationId.Value, personId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Problem(result.Error);
    }

    private static async Task<IResult> SearchAsync(
        int? pageNumber,
        int? pageSize,
        string? search,
        Guid? studentId,
        Guid? branchId,
        string? status,
        DateOnly? startsFrom,
        DateOnly? startsTo,
        DateOnly? endsFrom,
        DateOnly? endsTo,
        string? sortBy,
        string? sortDirection,
        IMediator mediator,
        ICurrentTenant tenant,
        CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return Results.Problem(statusCode: 401, title: "errors.currentTenant.required");

        TrainingContractStatus parsedStatus = default;
        if (!string.IsNullOrWhiteSpace(status)
            && !Enum.TryParse(status, true, out parsedStatus))
            return Results.Problem(statusCode: 400, title: "Contracts.TrainingContract.Status.Invalid", detail: "errors.contracts.trainingContract.status.invalid");

        TrainingContractStatus? statusValue = string.IsNullOrWhiteSpace(status) ? null : parsedStatus;

        TrainingContractSortField sortField = TrainingContractSortField.CreatedAt;
        if (!string.IsNullOrWhiteSpace(sortBy)
            && !Enum.TryParse(sortBy, true, out sortField))
            return Results.Problem(statusCode: 400, title: "Contracts.TrainingContract.Sort.Invalid", detail: "errors.contracts.trainingContract.sort.invalid");

        SortDirection direction = SortDirection.Descending;
        if (!string.IsNullOrWhiteSpace(sortDirection)
            && !Enum.TryParse(sortDirection, true, out direction))
            return Results.Problem(statusCode: 400, title: "Contracts.TrainingContract.SortDirection.Invalid", detail: "errors.contracts.trainingContract.sortDirection.invalid");

        if (startsFrom.HasValue && startsTo.HasValue && startsFrom.Value > startsTo.Value)
            return Results.Problem(statusCode: 400, title: "Contracts.TrainingContract.StartRange.Invalid", detail: "errors.contracts.trainingContract.startRange.invalid");
        if (endsFrom.HasValue && endsTo.HasValue && endsFrom.Value > endsTo.Value)
            return Results.Problem(statusCode: 400, title: "Contracts.TrainingContract.EndRange.Invalid", detail: "errors.contracts.trainingContract.endRange.invalid");

        Result<PagedResult<TrainingContractListItemResponse>> result = await mediator.Send(
            new SearchTrainingContractsQuery(
                tenant.OrganizationId.Value,
                pageNumber ?? 1,
                pageSize ?? 20,
                search,
                studentId.HasValue ? new PersonId(studentId.Value) : null,
                branchId.HasValue ? new BranchId(branchId.Value) : null,
                statusValue,
                startsFrom,
                startsTo,
                endsFrom,
                endsTo,
                sortField,
                direction),
            ct);

        return result.IsSuccess ? Results.Ok(result.Value) : Problem(result.Error);
    }

    private static async Task<IResult> GetByIdAsync(Guid contractId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return Results.Problem(statusCode: 401, title: "errors.currentTenant.required");

        Result<TrainingContractDetailResponse> result = await mediator.Send(
            new GetTrainingContractQuery(tenant.OrganizationId.Value, new TrainingContractId(contractId)), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Problem(result.Error);
    }

    private static async Task<IResult> CreateAsync(CreateTrainingContractRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null || user.UserId is null)
            return Results.Problem(statusCode: 401, title: "errors.currentUser.required");

        var result = await mediator.Send(new CreateTrainingContractCommand(
            tenant.OrganizationId.Value,
            new DraftEnrollmentId(request.EnrollmentId),
            new CommercialOfferId(request.SourceOfferId),
            request.ContractNumber,
            request.StartDate,
            request.EndDate,
            request.PracticalHours,
            request.ServicesSnapshot,
            request.PaymentScheduleSnapshot,
            request.CancellationTerms,
            request.BookingRules,
            request.StudentObligations,
            request.ProviderObligations,
            request.ExamPresentationTerms,
            request.DataProcessingTerms,
            request.ProviderLegalReference,
            request.StudentLegalReference,
            user.UserId.Value), ct);

        return result.IsSuccess
            ? Results.Created($"/api/contracts/training/{result.Value.Value}", result.Value.Value)
            : Problem(result.Error);
    }

    private static async Task<IResult> AddSignatoryAsync(Guid contractId, AddTrainingContractSignatoryRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null || user.UserId is null) return Results.Problem(statusCode: 401, title: "errors.currentUser.required");
        Result<Guid> result = await mediator.Send(new AddTrainingContractSignatoryCommand(
            tenant.OrganizationId.Value, new TrainingContractId(contractId), request.Kind, new PersonId(request.PersonId),
            request.RepresentedOrganizationId.HasValue ? new OrganizationId(request.RepresentedOrganizationId.Value) : null,
            request.DisplayName, request.SigningOrder, request.IsRequired, request.AuthorityReference, user.UserId.Value), ct);
        return result.IsSuccess ? Results.Created($"/api/contracts/training/{contractId}/signatories/{result.Value}", result.Value) : Problem(result.Error);
    }

    private static async Task<IResult> UpdateSignatoryAsync(Guid contractId, Guid signatoryId, UpdateTrainingContractSignatoryRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null || user.UserId is null) return Results.Problem(statusCode: 401, title: "errors.currentUser.required");
        Result result = await mediator.Send(new UpdateTrainingContractSignatoryCommand(tenant.OrganizationId.Value, new TrainingContractId(contractId), new TrainingContractSignatoryId(signatoryId), request.SigningOrder, request.IsRequired, request.DisplayName, request.AuthorityReference, user.UserId.Value), ct);
        return result.IsSuccess ? Results.NoContent() : Problem(result.Error);
    }

    private static async Task<IResult> RemoveSignatoryAsync(Guid contractId, Guid signatoryId, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null || user.UserId is null) return Results.Problem(statusCode: 401, title: "errors.currentUser.required");
        Result result = await mediator.Send(new RemoveTrainingContractSignatoryCommand(tenant.OrganizationId.Value, new TrainingContractId(contractId), new TrainingContractSignatoryId(signatoryId), user.UserId.Value), ct);
        return result.IsSuccess ? Results.NoContent() : Problem(result.Error);
    }

    private static async Task<IResult> DecideSignatoryAuthorityAsync(Guid contractId, Guid signatoryId, DecideTrainingContractSignatoryAuthorityRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null || user.UserId is null) return Results.Problem(statusCode: 401, title: "errors.currentUser.required");
        Result result = await mediator.Send(new DecideTrainingContractSignatoryAuthorityCommand(tenant.OrganizationId.Value, new TrainingContractId(contractId), new TrainingContractSignatoryId(signatoryId), request.Approved, request.Reason, user.UserId.Value), ct);
        return result.IsSuccess ? Results.NoContent() : Problem(result.Error);
    }

    private static async Task<IResult> GenerateAsync(Guid contractId, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null || user.UserId is null)
            return Results.Problem(statusCode: 401, title: "errors.currentUser.required");

        Result<GeneratedTrainingContractResponse> result = await mediator.Send(
            new GenerateTrainingContractCommand(tenant.OrganizationId.Value, new TrainingContractId(contractId), user.UserId.Value), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Problem(result.Error);
    }


    private static async Task<IResult> SendForSignatureAsync(Guid contractId, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null || user.UserId is null)
            return Results.Problem(statusCode: 401, title: "errors.currentUser.required");
        Result<SendTrainingContractForSignatureResponse> result = await mediator.Send(
            new SendTrainingContractForSignatureCommand(tenant.OrganizationId.Value, new TrainingContractId(contractId), user.UserId.Value), ct);
        return result.IsSuccess ? Results.Accepted($"/api/contracts/training/{contractId}", result.Value) : Problem(result.Error);
    }



    private static async Task<IResult> ActivateAsync(
        Guid contractId,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null || user.UserId is null)
            return Results.Problem(statusCode: 401, title: "errors.currentUser.required");

        Result<ActivateTrainingContractResponse> result = await mediator.Send(
            new ActivateTrainingContractCommand(
                tenant.OrganizationId.Value,
                new TrainingContractId(contractId),
                user.UserId.Value),
            ct);

        return result.IsSuccess ? Results.Ok(result.Value) : Problem(result.Error);
    }

    private static async Task<IResult> SuspendAsync(
        Guid contractId,
        SuspendTrainingContractRequest request,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null || user.UserId is null)
            return Results.Problem(statusCode: 401, title: "errors.currentUser.required");

        Result<SuspendTrainingContractResponse> result = await mediator.Send(
            new SuspendTrainingContractCommand(
                tenant.OrganizationId.Value, new TrainingContractId(contractId), request.Reason,
                request.EffectiveDate, request.ExpectedResumeDate, user.UserId.Value), ct);

        return result.IsSuccess ? Results.Ok(result.Value) : Problem(result.Error);
    }

    private static async Task<IResult> TerminateAsync(
        Guid contractId,
        TerminateTrainingContractRequest request,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null || user.UserId is null)
            return Results.Problem(statusCode: 401, title: "errors.currentUser.required");

        Result<TerminateTrainingContractResponse> result = await mediator.Send(
            new TerminateTrainingContractCommand(
                tenant.OrganizationId.Value, new TrainingContractId(contractId), request.Reason,
                request.EffectiveDate, user.UserId.Value), ct);

        return result.IsSuccess ? Results.Ok(result.Value) : Problem(result.Error);
    }

    private static async Task<IResult> RecordSignatureAsync(
        Guid contractId,
        Guid signatureProcessId,
        RecordTrainingContractSignatureRequest request,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null || user.UserId is null)
            return Results.Problem(statusCode: 401, title: "errors.currentUser.required");

        Result<RecordTrainingContractSignatureResponse> result = await mediator.Send(
            new RecordTrainingContractSignatureCommand(
                tenant.OrganizationId.Value,
                new TrainingContractId(contractId),
                new SignatureProcessId(signatureProcessId),
                new TrainingContractSignatoryId(request.SignatoryId),
                request.DocumentSha256,
                request.SignatureMethod,
                request.AuthenticationMethod,
                request.Provider,
                request.ProviderSignatureReference,
                request.CertificateReference,
                request.IpAddress,
                request.UserAgent,
                request.SignedAtUtc,
                user.UserId.Value),
            ct);

        return result.IsSuccess ? Results.Ok(result.Value) : Problem(result.Error);
    }


    private static async Task<IResult> CreateAmendmentAsync(Guid contractId, CreateContractAmendmentRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null || user.UserId is null) return Results.Problem(statusCode: 401, title: "errors.currentUser.required");
        Result<CreateContractAmendmentResponse> result = await mediator.Send(new CreateContractAmendmentCommand(
            tenant.OrganizationId.Value, new TrainingContractId(contractId), request.Reason, request.EffectiveDate,
            request.StartDate, request.EndDate, request.TotalAmount, request.Currency, request.PracticalHours,
            request.ServicesSnapshot, request.PaymentScheduleSnapshot, request.CancellationTerms, request.BookingRules,
            request.StudentObligations, request.ProviderObligations, request.ExamPresentationTerms, request.DataProcessingTerms,
            user.UserId.Value), ct);
        return result.IsSuccess ? Results.Created($"/api/contracts/training/{contractId}/amendments/{result.Value.AmendmentId}", result.Value) : Problem(result.Error);
    }

    private static async Task<IResult> RecordAmendmentSignedProofAsync(Guid contractId, Guid amendmentId, RecordContractAmendmentSignedProofRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null || user.UserId is null) return Results.Problem(statusCode: 401, title: "errors.currentUser.required");
        Result result = await mediator.Send(new RecordContractAmendmentSignedProofCommand(tenant.OrganizationId.Value, new TrainingContractId(contractId), new ContractAmendmentId(amendmentId), request.SignedDocumentReference, request.DocumentSha256, request.SignedAtUtc, user.UserId.Value), ct);
        return result.IsSuccess ? Results.NoContent() : Problem(result.Error);
    }

    private static async Task<IResult> ApplyAmendmentAsync(Guid contractId, Guid amendmentId, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null || user.UserId is null) return Results.Problem(statusCode: 401, title: "errors.currentUser.required");
        Result<ApplyContractAmendmentResponse> result = await mediator.Send(new ApplyContractAmendmentCommand(tenant.OrganizationId.Value, new TrainingContractId(contractId), new ContractAmendmentId(amendmentId), user.UserId.Value), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Problem(result.Error);
    }

    private static async Task<IResult> CancelAmendmentAsync(Guid contractId, Guid amendmentId, CancelContractAmendmentRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null || user.UserId is null) return Results.Problem(statusCode: 401, title: "errors.currentUser.required");
        Result result = await mediator.Send(new CancelContractAmendmentCommand(tenant.OrganizationId.Value, new TrainingContractId(contractId), new ContractAmendmentId(amendmentId), request.Reason, user.UserId.Value), ct);
        return result.IsSuccess ? Results.NoContent() : Problem(result.Error);
    }

    private static async Task<IResult> CompleteAsync(Guid contractId, CompleteTrainingContractRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null || user.UserId is null)
            return Results.Problem(statusCode: 401, title: "errors.currentUser.required");
        Result<CompleteTrainingContractResponse> result = await mediator.Send(new CompleteTrainingContractCommand(
            tenant.OrganizationId.Value, new TrainingContractId(contractId), request.Note, request.EffectiveDate, user.UserId.Value), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Problem(result.Error);
    }

    private static async Task<IResult> ExpireAsync(Guid contractId, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null || user.UserId is null)
            return Results.Problem(statusCode: 401, title: "errors.currentUser.required");
        Result<ExpireTrainingContractResponse> result = await mediator.Send(new ExpireTrainingContractCommand(
            tenant.OrganizationId.Value, new TrainingContractId(contractId), user.UserId.Value), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Problem(result.Error);
    }

    private static async Task<IResult> GetDocumentsAsync(Guid contractId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null) return Results.Problem(statusCode: 401, title: "errors.currentTenant.required");
        Result<IReadOnlyList<ContractDocumentResponse>> result = await mediator.Send(new GetContractDocumentsQuery(tenant.OrganizationId.Value, new TrainingContractId(contractId)), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Problem(result.Error);
    }

    private static async Task<IResult> UploadDocumentAsync(Guid contractId, HttpRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null || user.UserId is null) return Results.Problem(statusCode: 401, title: "errors.currentUser.required");
        if (!request.HasFormContentType) return Results.Problem(statusCode: 400, title: "errors.contracts.document.form.required");
        IFormCollection form = await request.ReadFormAsync(ct); IFormFile? file = form.Files.GetFile("file");
        if (file is null || file.Length == 0 || file.Length > 50 * 1024 * 1024) return Results.Problem(statusCode: 400, title: "errors.contracts.document.file.invalid");
        await using var ms = new MemoryStream(); await file.CopyToAsync(ms, ct);
        DateOnly? retainUntil = DateOnly.TryParse(form["retainUntil"].ToString(), out DateOnly parsed) ? parsed : null;
        Result<ContractDocumentResponse> result = await mediator.Send(new UploadContractDocumentCommand(tenant.OrganizationId.Value, new TrainingContractId(contractId), form["documentType"].ToString(), form["title"].ToString(), form["visibility"].ToString(), retainUntil, form["retentionLegalBasis"].ToString(), file.FileName, file.ContentType, ms.ToArray(), user.UserId.Value), ct);
        return result.IsSuccess ? Results.Created($"/api/contracts/training/{contractId}/documents/{result.Value.Id}", result.Value) : Problem(result.Error);
    }

    private static async Task<IResult> AddDocumentVersionAsync(Guid contractId, Guid documentId, HttpRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null || user.UserId is null) return Results.Problem(statusCode: 401, title: "errors.currentUser.required");
        if (!request.HasFormContentType) return Results.Problem(statusCode: 400, title: "errors.contracts.document.form.required");
        IFormCollection form = await request.ReadFormAsync(ct); IFormFile? file = form.Files.GetFile("file");
        if (file is null || file.Length == 0 || file.Length > 50 * 1024 * 1024) return Results.Problem(statusCode: 400, title: "errors.contracts.document.file.invalid");
        await using var ms = new MemoryStream(); await file.CopyToAsync(ms, ct);
        Result<ContractDocumentResponse> result = await mediator.Send(new AddContractDocumentVersionCommand(tenant.OrganizationId.Value, new TrainingContractId(contractId), new ContractDocumentId(documentId), file.FileName, file.ContentType, ms.ToArray(), user.UserId.Value), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Problem(result.Error);
    }

    private static async Task<IResult> ArchiveDocumentAsync(Guid contractId, Guid documentId, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null || user.UserId is null) return Results.Problem(statusCode: 401, title: "errors.currentUser.required");
        Result result = await mediator.Send(new ArchiveContractDocumentCommand(tenant.OrganizationId.Value, new TrainingContractId(contractId), new ContractDocumentId(documentId), user.UserId.Value), ct);
        return result.IsSuccess ? Results.NoContent() : Problem(result.Error);
    }

    private static async Task<IResult> GetHistoryAsync(
        Guid contractId,
        IMediator mediator,
        ICurrentTenant tenant,
        CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return Results.Problem(statusCode: 401, title: "errors.currentTenant.required");

        Result<TrainingContractHistoryResponse> result = await mediator.Send(
            new GetTrainingContractHistoryQuery(
                tenant.OrganizationId.Value,
                new TrainingContractId(contractId)),
            ct);

        return result.IsSuccess ? Results.Ok(result.Value) : Problem(result.Error);
    }

    private static async Task<IResult> GetAuditAsync(
        Guid contractId,
        IMediator mediator,
        ICurrentTenant tenant,
        CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return Results.Problem(statusCode: 401, title: "errors.currentTenant.required");

        Result<IReadOnlyList<ContractAuditEntryResponse>> result = await mediator.Send(
            new GetContractAuditQuery(
                tenant.OrganizationId.Value,
                new TrainingContractId(contractId)),
            ct);

        return result.IsSuccess ? Results.Ok(result.Value) : Problem(result.Error);
    }

    private static IResult Problem(Error error) => Results.Problem(
        statusCode: error.Type switch { ErrorType.NotFound => 404, ErrorType.Conflict => 409, _ => 400 },
        title: error.Code,
        detail: error.MessageKey);
}
