using DomainRelay.Abstractions;
using DriveOS.Api.Contracts;
using DriveOS.Api.Errors;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Application.Abstractions.Pagination;
using DriveOS.Application.Abstractions.Sorting;
using DriveOS.Modules.Students.Application.Administration;
using DriveOS.Modules.Students.Application.Branches;
using DriveOS.Modules.Students.Application.Checklists;
using DriveOS.Modules.Students.Application.Closures;
using DriveOS.Modules.Students.Application.Dashboard.GetDashboard;
using DriveOS.Modules.Students.Application.Documents;
using DriveOS.Modules.Students.Application.Enrollments.StartDirectEnrollment;
using DriveOS.Modules.Students.Application.ExternalTransfers;
using DriveOS.Modules.Students.Application.Guardians;
using DriveOS.Modules.Students.Application.Instructors;
using DriveOS.Modules.Students.Application.Reactivations;
using DriveOS.Modules.Students.Application.Relationships;
using DriveOS.Modules.Students.Application.Statuses;
using DriveOS.Modules.Students.Application.Students.GetStudentOverview;
using DriveOS.Modules.Students.Application.Students.GetStudents;
using DriveOS.Modules.Students.Application.Students.Identity;
using DriveOS.Modules.Students.Application.Suspensions;
using DriveOS.Modules.Students.Application.Transfers;
using DriveOS.Modules.Students.Domain.Administration;
using DriveOS.Modules.Students.Domain.Branches;
using DriveOS.Modules.Students.Domain.Checklists;
using DriveOS.Modules.Students.Domain.Closures;
using DriveOS.Modules.Students.Domain.Documents;
using DriveOS.Modules.Students.Domain.Enrollments;
using DriveOS.Modules.Students.Domain.ExternalTransfers;
using DriveOS.Modules.Students.Domain.Guardians;
using DriveOS.Modules.Students.Domain.Instructors;
using DriveOS.Modules.Students.Domain.Relationships;
using DriveOS.Modules.Students.Domain.Statuses;
using DriveOS.Modules.Students.Domain.Students;
using DriveOS.Modules.Students.Domain.Suspensions;
using DriveOS.Modules.Students.Domain.Transfers;
using DriveOS.Security.Contracts;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.Students;

public static class StudentDashboardEndpoints
{
    public static IEndpointRouteBuilder MapStudentDashboardEndpoints(
        this IEndpointRouteBuilder endpoints
    )
    {
        endpoints
            .MapPost("/api/students/enrollments/direct", StartDirectEnrollmentAsync)
            .WithTags("Students")
            .WithName("StartDirectEnrollment")
            .Accepts<StartDirectEnrollmentRequest>("application/json")
            .Produces<StartDirectEnrollmentResponse>(StatusCodes.Status201Created)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .RequireAuthorization("Students.Create", "Enrollments.Create");
        endpoints
            .MapGet("/api/students", GetStudentsAsync)
            .WithTags("Students")
            .WithName("GetStudents")
            .Produces<PagedResponse<StudentListItem>>()
            .RequireAuthorization("Students.Read");
        endpoints
            .MapGet("/api/students/{studentId:guid}/overview", GetStudentOverviewAsync)
            .WithTags("Students")
            .WithName("GetStudentOverview")
            .Produces<StudentOverviewResponse>()
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .RequireAuthorization("Students.Read");
        endpoints
            .MapGet("/api/students/{studentId:guid}/identity", GetStudentIdentityAsync)
            .WithTags("Students - Identity")
            .WithName("GetStudentIdentity")
            .Produces<StudentIdentityResponse>()
            .RequireAuthorization("Students.Identity.Read");
        endpoints
            .MapPut("/api/students/{studentId:guid}/identity", UpdateStudentIdentityAsync)
            .WithTags("Students - Identity")
            .WithName("UpdateStudentIdentity")
            .Accepts<UpdateStudentIdentityRequest>("application/json")
            .Produces<UpdateStudentIdentityResponse>()
            .RequireAuthorization("Students.Identity.Update");
        endpoints
            .MapPost("/api/students/{studentId:guid}/identity/verify", VerifyStudentIdentityAsync)
            .WithTags("Students - Identity")
            .WithName("VerifyStudentIdentity")
            .Accepts<VerifyStudentIdentityRequest>("application/json")
            .Produces<StudentIdentityResponse>()
            .RequireAuthorization("Students.Identity.Verify");
        endpoints
            .MapPatch("/api/students/{studentId:guid}/identity/self-service", UpdateOwnContactAsync)
            .WithTags("Students - Identity")
            .WithName("UpdateOwnStudentContact")
            .Accepts<UpdateOwnStudentContactRequest>("application/json")
            .Produces<UpdateStudentIdentityResponse>()
            .RequireAuthorization("OwnProfile.UpdateAllowedFields");
        RouteGroupBuilder administration = endpoints
            .MapGroup("/api/students/{studentId:guid}/administration")
            .WithTags("Students - Administration");
        administration
            .MapGet("/", GetAdministrationAsync)
            .WithName("GetStudentAdministration")
            .Produces<AdministrationResponse>()
            .RequireAuthorization("Students.Administration.Read");
        administration
            .MapPost("/requirements", CreateRequirementAsync)
            .WithName("CreateAdministrativeRequirement")
            .RequireAuthorization("Students.Administration.Update");
        administration
            .MapPost("/requirements/synchronize", SynchronizeAdministrativeRequirementsAsync)
            .WithName("SynchronizeAdministrativeRequirements")
            .RequireAuthorization("Students.Administration.Update");
        administration
            .MapPut("/requirements/{requirementId:guid}", UpdateRequirementAsync)
            .WithName("UpdateAdministrativeRequirement")
            .RequireAuthorization("Students.Administration.Update");
        administration
            .MapPost("/requirements/{requirementId:guid}/status", DecideRequirementAsync)
            .WithName("DecideAdministrativeRequirement")
            .RequireAuthorization("Documents.Validate");
        administration
            .MapPost("/blocks", AddAdministrativeBlockAsync)
            .WithName("AddAdministrativeBlock")
            .RequireAuthorization("Students.Administration.Update");
        administration
            .MapPost("/blocks/{blockId:guid}/release", ReleaseAdministrativeBlockAsync)
            .WithName("ReleaseAdministrativeBlock")
            .RequireAuthorization("Students.Administration.Update");
        administration
            .MapPost(
                "/requirements/{requirementId:guid}/exceptions",
                RequestComplianceExceptionAsync
            )
            .WithName("RequestComplianceException")
            .RequireAuthorization("Compliance.Exceptions.Request");
        administration
            .MapPost("/exceptions/{exceptionId:guid}/decision", DecideComplianceExceptionAsync)
            .WithName("DecideComplianceException")
            .RequireAuthorization("Compliance.Exceptions.Approve");
        RouteGroupBuilder guardians = endpoints
            .MapGroup("/api/students/{studentId:guid}/guardians")
            .WithTags("Students - Guardians");
        guardians
            .MapGet("/", GetGuardiansAsync)
            .WithName("GetStudentGuardians")
            .Produces<GuardianListResponse>()
            .RequireAuthorization("Guardians.Read");
        guardians
            .MapPost("/", CreateGuardianAsync)
            .WithName("CreateStudentGuardian")
            .Produces<Guid>(StatusCodes.Status201Created)
            .RequireAuthorization("Guardians.Create");
        guardians
            .MapPut("/{relationshipId:guid}", UpdateGuardianAsync)
            .WithName("UpdateStudentGuardian")
            .RequireAuthorization("Guardians.Update");
        guardians
            .MapPost("/{relationshipId:guid}/revoke", RevokeGuardianAsync)
            .WithName("RevokeStudentGuardian")
            .RequireAuthorization("Guardians.Revoke");
        guardians
            .MapPost("/{relationshipId:guid}/invite", InviteGuardianAsync)
            .WithName("InviteStudentGuardian")
            .RequireAuthorization("Guardians.Invite");
        RouteGroupBuilder relationships = endpoints
            .MapGroup("/api/students/{studentId:guid}/relationships")
            .WithTags("Students - Relationships");
        relationships
            .MapGet("/", GetRelationshipsAsync)
            .WithName("GetStudentRelationships")
            .Produces<StudentRelationshipListResponse>()
            .RequireAuthorization("StudentRelationships.Read");
        relationships
            .MapPost("/", CreateRelationshipAsync)
            .WithName("CreateStudentRelationship")
            .Produces<Guid>(StatusCodes.Status201Created)
            .RequireAuthorization("StudentRelationships.Create");
        relationships
            .MapPut("/{relationshipId:guid}", UpdateRelationshipAsync)
            .WithName("UpdateStudentRelationship")
            .RequireAuthorization("StudentRelationships.Update");
        relationships
            .MapPost("/{relationshipId:guid}/invite", InviteRelationshipAsync)
            .WithName("InviteStudentRelationship")
            .RequireAuthorization("StudentRelationships.Update");
        relationships
            .MapPost("/{relationshipId:guid}/suspend", SuspendRelationshipAsync)
            .WithName("SuspendStudentRelationship")
            .RequireAuthorization("StudentRelationships.Update");
        relationships
            .MapPost("/{relationshipId:guid}/revoke", RevokeRelationshipAsync)
            .WithName("RevokeStudentRelationship")
            .RequireAuthorization("StudentRelationships.Revoke");
        RouteGroupBuilder checklist = endpoints
            .MapGroup("/api/students/{studentId:guid}/enrollment-checklist")
            .WithTags("Students - Enrollment checklist");
        checklist
            .MapGet("/", GetChecklistAsync)
            .WithName("GetEnrollmentChecklist")
            .Produces<EnrollmentChecklistResponse>()
            .RequireAuthorization("Enrollments.Checklist.Read");
        checklist
            .MapPost("/synchronize", SynchronizeChecklistAsync)
            .WithName("SynchronizeEnrollmentChecklist")
            .RequireAuthorization("Enrollments.Checklist.Update");
        checklist
            .MapPost("/{enrollmentId:guid}/items/{itemId:guid}/status", ChangeChecklistStatusAsync)
            .WithName("ChangeEnrollmentChecklistItemStatus")
            .RequireAuthorization("Enrollments.Checklist.Update");
        checklist
            .MapPost("/{enrollmentId:guid}/items/{itemId:guid}/assign", AssignChecklistItemAsync)
            .WithName("AssignEnrollmentChecklistItem")
            .RequireAuthorization("Enrollments.Checklist.Update");
        checklist
            .MapPost("/{enrollmentId:guid}/items/{itemId:guid}/remind", RemindChecklistItemAsync)
            .WithName("RemindEnrollmentChecklistItem")
            .RequireAuthorization("Enrollments.Checklist.Update");
        checklist
            .MapPost("/{enrollmentId:guid}/activate", ActivateEnrollmentAsync)
            .WithName("ActivateStudentEnrollment")
            .RequireAuthorization("Enrollments.Activate");
        endpoints
            .MapPost("/api/students/enrollment-checklist/rules", ConfigureChecklistRuleAsync)
            .WithTags("Students - Enrollment checklist")
            .WithName("ConfigureEnrollmentChecklistRule")
            .RequireAuthorization("Enrollments.Checklist.Update");
        RouteGroupBuilder documents = endpoints
            .MapGroup("/api/students/{studentId:guid}/documents")
            .WithTags("Students - Documents");
        documents
            .MapGet("/", GetStudentDocumentsAsync)
            .WithName("GetStudentDocuments")
            .Produces<StudentDocumentListResponse>()
            .RequireAuthorization("StudentDocuments.Read");
        documents
            .MapPost("/requests", RequestStudentDocumentAsync)
            .WithName("RequestStudentDocument")
            .RequireAuthorization("StudentDocuments.Request");
        documents
            .MapPost("/{documentId:guid}/versions", UploadStudentDocumentAsync)
            .WithName("UploadStudentDocument")
            .DisableAntiforgery()
            .RequireAuthorization("StudentDocuments.Upload");
        documents
            .MapPost("/{documentId:guid}/validation", ValidateStudentDocumentAsync)
            .WithName("ValidateStudentDocument")
            .RequireAuthorization("StudentDocuments.Validate");
        documents
            .MapPost("/{documentId:guid}/share", ShareStudentDocumentAsync)
            .WithName("ShareStudentDocument")
            .RequireAuthorization("StudentDocuments.Share");
        documents
            .MapGet("/{documentId:guid}/download", DownloadStudentDocumentAsync)
            .WithName("DownloadStudentDocument")
            .RequireAuthorization("StudentDocuments.Download");
        documents
            .MapPost("/{documentId:guid}/archive", ArchiveStudentDocumentAsync)
            .WithName("ArchiveStudentDocument")
            .RequireAuthorization("StudentDocuments.Validate");
        RouteGroupBuilder statuses = endpoints
            .MapGroup("/api/students/{studentId:guid}/statuses")
            .WithTags("Students - Statuses and blocks");
        statuses
            .MapGet("/", GetStudentStatusesAsync)
            .WithName("GetStudentStatuses")
            .Produces<StudentStatusesResponse>()
            .RequireAuthorization("StudentStatuses.Read");
        statuses
            .MapPost("/blocks", ApplyStudentBlockAsync)
            .WithName("ApplyStudentBlock")
            .Produces<Guid>(StatusCodes.Status201Created)
            .RequireAuthorization("StudentBlocks.Apply");
        statuses
            .MapPost("/blocks/{blockId:guid}/release", ReleaseStudentOperationalBlockAsync)
            .WithName("ReleaseStudentOperationalBlock")
            .RequireAuthorization("StudentBlocks.Release");
        statuses
            .MapPost("/blocks/{blockId:guid}/override", OverrideStudentBlockAsync)
            .WithName("OverrideStudentBlock")
            .RequireAuthorization("StudentBlocks.Override");
        RouteGroupBuilder branches = endpoints
            .MapGroup("/api/students/{studentId:guid}/branches")
            .WithTags("Students - Branch assignments");
        branches
            .MapGet("/", GetStudentBranchesAsync)
            .WithName("GetStudentBranches")
            .Produces<StudentBranchesResponse>()
            .RequireAuthorization("Students.Branches.Read");
        branches
            .MapPost("/assignments", AssignStudentBranchAsync)
            .WithName("AssignStudentBranch")
            .Produces<Guid>(StatusCodes.Status201Created)
            .RequireAuthorization("Students.Branches.Assign");
        branches
            .MapPost("/primary-change/analysis", AnalyzePrimaryBranchChangeAsync)
            .WithName("AnalyzePrimaryBranchChange")
            .Produces<PrimaryBranchChangeAnalysisResponse>()
            .RequireAuthorization("Students.Branches.ChangePrimary");
        branches
            .MapPost("/primary-change", ChangePrimaryStudentBranchAsync)
            .WithName("ChangePrimaryStudentBranch")
            .RequireAuthorization("Students.Branches.ChangePrimary");
        branches
            .MapPost("/assignments/{assignmentId:guid}/end", EndStudentBranchAssignmentAsync)
            .WithName("EndStudentBranchAssignment")
            .RequireAuthorization("Students.Branches.Assign");
        RouteGroupBuilder instructors = endpoints
            .MapGroup("/api/students/{studentId:guid}/instructors")
            .WithTags("Students - Instructor assignments");
        instructors
            .MapGet("/", GetStudentInstructorsAsync)
            .WithName("GetStudentInstructors")
            .Produces<StudentInstructorsResponse>()
            .RequireAuthorization("Students.Instructors.Read");
        instructors
            .MapGet("/suggestions", GetInstructorSuggestionsAsync)
            .WithName("GetInstructorSuggestions")
            .Produces<IReadOnlyList<InstructorSuggestionItem>>()
            .RequireAuthorization("Students.Instructors.Read");
        instructors
            .MapPost("/assignments", AssignStudentInstructorAsync)
            .WithName("AssignStudentInstructor")
            .Produces<Guid>(StatusCodes.Status201Created)
            .RequireAuthorization("Students.Instructors.Assign");
        instructors
            .MapPost("/primary/replace", ReplacePrimaryInstructorAsync)
            .WithName("ReplacePrimaryInstructor")
            .RequireAuthorization("Students.Instructors.Replace");
        instructors
            .MapPost("/assignments/{assignmentId:guid}/end", EndStudentInstructorAssignmentAsync)
            .WithName("EndStudentInstructorAssignment")
            .RequireAuthorization("Students.Instructors.Assign");
        RouteGroupBuilder internalTransfers = endpoints
            .MapGroup("/api/students/{studentId:guid}/transfers/internal")
            .WithTags("Students - Internal transfers");
        internalTransfers
            .MapGet("/", GetInternalTransfersAsync)
            .WithName("GetInternalTransfers")
            .Produces<IReadOnlyList<InternalTransferResponse>>()
            .RequireAuthorization("Students.TransferInternal", "Branches.Read");
        internalTransfers
            .MapPost("/analysis", AnalyzeInternalTransferAsync)
            .WithName("AnalyzeInternalTransfer")
            .Produces<InternalTransferResponse>(StatusCodes.Status201Created)
            .RequireAuthorization("Students.TransferInternal", "Branches.Read");
        internalTransfers
            .MapPost("/{transferId:guid}/validate", ValidateInternalTransferAsync)
            .WithName("ValidateInternalTransfer")
            .Produces<InternalTransferResponse>()
            .RequireAuthorization(
                "Students.TransferInternal",
                "Branches.Read",
                "Planning.Reassign",
                "Finance.TransferReview"
            );
        RouteGroupBuilder externalTransfers = endpoints
            .MapGroup("/api/students/{studentId:guid}/transfers/external")
            .WithTags("Students - External transfers");
        externalTransfers
            .MapGet("/", GetExternalTransfersAsync)
            .WithName("GetExternalTransfers")
            .Produces<IReadOnlyList<ExternalTransferResponse>>()
            .RequireAuthorization("Students.TransferExternal");
        externalTransfers
            .MapPost("/", CreateExternalTransferAsync)
            .WithName("CreateExternalTransfer")
            .Produces<Guid>(StatusCodes.Status201Created)
            .RequireAuthorization("Students.TransferExternal");
        externalTransfers
            .MapPost("/{transferId:guid}/consent", VerifyExternalTransferConsentAsync)
            .WithName("VerifyExternalTransferConsent")
            .RequireAuthorization("Students.TransferExternal");
        externalTransfers
            .MapPost("/{transferId:guid}/finance-review", ReviewExternalTransferFinanceAsync)
            .WithName("ReviewExternalTransferFinance")
            .RequireAuthorization("Finance.TransferResolution");
        externalTransfers
            .MapPost("/{transferId:guid}/submit", SubmitExternalTransferAsync)
            .WithName("SubmitExternalTransfer")
            .Produces<ExternalTransferPreconditions>()
            .RequireAuthorization("Students.TransferExternal");
        externalTransfers
            .MapPost("/{transferId:guid}/decision", DecideExternalTransferAsync)
            .WithName("DecideExternalTransfer")
            .RequireAuthorization("Partners.Students.Transfer", "StudentDataGrants.Create");
        externalTransfers
            .MapPost("/{transferId:guid}/complete", CompleteExternalTransferAsync)
            .WithName("CompleteExternalTransfer")
            .RequireAuthorization("Students.TransferExternal", "Partners.Students.Transfer");
        RouteGroupBuilder suspensions = endpoints
            .MapGroup("/api/students/{studentId:guid}/suspension")
            .WithTags("Students - Suspensions");
        suspensions
            .MapGet("/", GetEnrollmentSuspensionsAsync)
            .WithName("GetEnrollmentSuspensions")
            .Produces<IReadOnlyList<EnrollmentSuspensionResponse>>()
            .RequireAuthorization("Students.Suspend");
        suspensions
            .MapPost("/", SuspendEnrollmentAsync)
            .WithName("SuspendEnrollment")
            .Produces<Guid>(StatusCodes.Status201Created)
            .RequireAuthorization("Students.Suspend");
        RouteGroupBuilder reactivations = endpoints
            .MapGroup("/api/students/{studentId:guid}/reactivate")
            .WithTags("Students - Reactivations");
        reactivations
            .MapGet("/", GetEnrollmentReactivationsAsync)
            .WithName("GetEnrollmentReactivations")
            .Produces<IReadOnlyList<EnrollmentReactivationResponse>>()
            .RequireAuthorization("Students.Reactivate", "Compliance.Read");
        reactivations
            .MapPost("/", CreateEnrollmentReactivationAsync)
            .WithName("CreateEnrollmentReactivation")
            .Produces<Guid>(StatusCodes.Status201Created)
            .RequireAuthorization("Students.Reactivate", "Compliance.Read");
        reactivations
            .MapPut(
                "/{reactivationId:guid}/checks/{checkType}",
                ReviewEnrollmentReactivationCheckAsync
            )
            .WithName("ReviewEnrollmentReactivationCheck")
            .RequireAuthorization("Students.Reactivate", "Compliance.Read");
        reactivations
            .MapPost("/{reactivationId:guid}/apply", ApplyEnrollmentReactivationAsync)
            .WithName("ApplyEnrollmentReactivation")
            .RequireAuthorization("Students.Reactivate", "Enrollments.Reactivate");
        RouteGroupBuilder closures = endpoints
            .MapGroup("/api/students/{studentId:guid}/close")
            .WithTags("Students - Closure and archive");
        closures
            .MapGet("/", GetEnrollmentClosuresAsync)
            .WithName("GetEnrollmentClosures")
            .Produces<IReadOnlyList<EnrollmentClosureResponse>>()
            .RequireAuthorization("Students.Read");
        closures
            .MapPost("/", CreateEnrollmentClosureAsync)
            .WithName("CreateEnrollmentClosure")
            .Produces<Guid>(StatusCodes.Status201Created)
            .RequireAuthorization("Students.Close");
        closures
            .MapPut("/{closureId:guid}/checks/{checkType}", ReviewEnrollmentClosureCheckAsync)
            .WithName("ReviewEnrollmentClosureCheck")
            .RequireAuthorization("Students.Close");
        closures
            .MapPost("/{closureId:guid}/complete", CloseEnrollmentAsync)
            .WithName("CloseStudentEnrollment")
            .RequireAuthorization(
                "Students.Close",
                "Finance.CloseStudentAccount",
                "Contracts.Terminate"
            );
        closures
            .MapPost("/{closureId:guid}/archive", ArchiveStudentAsync)
            .WithName("ArchiveStudent")
            .RequireAuthorization("Students.Archive");
        closures
            .MapPost("/{closureId:guid}/reopen", ReopenEnrollmentAsync)
            .WithName("ReopenClosedEnrollment")
            .RequireAuthorization("Students.Reopen");
        endpoints
            .MapGet("/api/students/dashboard", GetAsync)
            .WithTags("Students - Dashboard")
            .WithName("GetStudentDashboard")
            .Produces<StudentDashboardResponse>()
            .RequireAuthorization("Students.Dashboard.Read");
        return endpoints;
    }

    private static async Task<IResult> GetGuardiansAsync(
        Guid studentId,
        IMediator mediator,
        ICurrentTenant tenant,
        CancellationToken ct
    )
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return Results.Problem(statusCode: 401, title: "errors.currentTenant.required");
        var r = await mediator.Send(
            new GetGuardiansQuery(tenant.OrganizationId.Value, new PersonId(studentId)),
            ct
        );
        return GuardianResult(r);
    }

    private static async Task<IResult> CreateGuardianAsync(
        Guid studentId,
        GuardianRequest request,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        var r = await mediator.Send(
            new CreateGuardianCommand(
                org,
                new PersonId(studentId),
                new PersonId(request.GuardianPersonId),
                request.FirstName,
                request.LastName,
                request.Email,
                request.Phone,
                request.RelationshipType,
                request.LegalBasis,
                request.ParentalAuthorityStatus,
                request.Permissions,
                request.EffectiveFrom,
                request.EffectiveTo,
                request.FinancialRights,
                request.SignatureRights,
                request.NotificationPreferences,
                actor
            ),
            ct
        );
        return r.IsSuccess
            ? Results.Created($"/api/students/{studentId}/guardians/{r.Value}", r.Value)
            : GuardianResult(r);
    }

    private static async Task<IResult> UpdateGuardianAsync(
        Guid studentId,
        Guid relationshipId,
        UpdateGuardianRequest request,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        var r = await mediator.Send(
            new UpdateGuardianCommand(
                org,
                new PersonId(studentId),
                relationshipId,
                request.RelationshipType,
                request.LegalBasis,
                request.ParentalAuthorityStatus,
                request.Permissions,
                request.EffectiveFrom,
                request.EffectiveTo,
                request.FinancialRights,
                request.SignatureRights,
                request.NotificationPreferences,
                actor
            ),
            ct
        );
        return GuardianResult(r);
    }

    private static async Task<IResult> RevokeGuardianAsync(
        Guid studentId,
        Guid relationshipId,
        ReasonRequest request,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        return GuardianResult(
            await mediator.Send(
                new RevokeGuardianCommand(
                    org,
                    new PersonId(studentId),
                    relationshipId,
                    request.Reason,
                    actor
                ),
                ct
            )
        );
    }

    private static async Task<IResult> InviteGuardianAsync(
        Guid studentId,
        Guid relationshipId,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        return GuardianResult(
            await mediator.Send(
                new InviteGuardianCommand(org, new PersonId(studentId), relationshipId, actor),
                ct
            )
        );
    }

    private static bool TryGuardianActor(
        ICurrentTenant tenant,
        ICurrentUser user,
        out OrganizationId org,
        out UserId actor,
        out IResult? failure
    )
    {
        org = default;
        actor = default;
        if (!tenant.HasTenant || tenant.OrganizationId is null || user.UserId is null)
        {
            failure = Results.Problem(statusCode: 401, title: "errors.currentUser.required");
            return false;
        }
        org = tenant.OrganizationId.Value;
        actor = user.UserId.Value;
        failure = null;
        return true;
    }

    private static IResult GuardianResult<T>(Result<T> r) =>
        r.IsSuccess
            ? Results.Ok(r.Value)
            : Results.Problem(
                statusCode: AdministrationStatusCode(r.Error.Type),
                title: r.Error.Code,
                detail: r.Error.MessageKey
            );

    private static IResult GuardianResult(Result r) =>
        r.IsSuccess
            ? Results.NoContent()
            : Results.Problem(
                statusCode: AdministrationStatusCode(r.Error.Type),
                title: r.Error.Code,
                detail: r.Error.MessageKey
            );

    private static async Task<IResult> GetRelationshipsAsync(
        Guid studentId,
        IMediator mediator,
        ICurrentTenant tenant,
        CancellationToken ct
    )
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return Results.Problem(statusCode: 401, title: "errors.currentTenant.required");
        return RelationshipResult(
            await mediator.Send(
                new GetStudentRelationshipsQuery(
                    tenant.OrganizationId.Value,
                    new PersonId(studentId)
                ),
                ct
            )
        );
    }

    private static async Task<IResult> CreateRelationshipAsync(
        Guid studentId,
        StudentRelationshipRequest q,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        var r = await mediator.Send(
            new CreateStudentRelationshipCommand(
                org,
                new PersonId(studentId),
                q.PersonOrOrganizationId,
                q.PartyKind,
                q.DisplayName,
                q.Email,
                q.Phone,
                q.RelationshipType,
                q.Permissions,
                q.FinancialScope,
                q.CommunicationScope,
                q.EffectiveFrom,
                q.EffectiveTo,
                q.IsPrimaryPayer,
                user.HasPermission("Finance.Payers.Manage"),
                actor
            ),
            ct
        );
        return r.IsSuccess
            ? Results.Created($"/api/students/{studentId}/relationships/{r.Value}", r.Value)
            : RelationshipResult(r);
    }

    private static async Task<IResult> UpdateRelationshipAsync(
        Guid studentId,
        Guid relationshipId,
        UpdateStudentRelationshipRequest q,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        return RelationshipResult(
            await mediator.Send(
                new UpdateStudentRelationshipCommand(
                    org,
                    new PersonId(studentId),
                    relationshipId,
                    q.RelationshipType,
                    q.Permissions,
                    q.FinancialScope,
                    q.CommunicationScope,
                    q.EffectiveFrom,
                    q.EffectiveTo,
                    q.IsPrimaryPayer,
                    user.HasPermission("Finance.Payers.Manage"),
                    actor
                ),
                ct
            )
        );
    }

    private static async Task<IResult> InviteRelationshipAsync(
        Guid studentId,
        Guid relationshipId,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        return RelationshipResult(
            await mediator.Send(
                new InviteStudentRelationshipCommand(
                    org,
                    new PersonId(studentId),
                    relationshipId,
                    actor
                ),
                ct
            )
        );
    }

    private static async Task<IResult> SuspendRelationshipAsync(
        Guid studentId,
        Guid relationshipId,
        ReasonRequest q,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        return RelationshipResult(
            await mediator.Send(
                new SuspendStudentRelationshipCommand(
                    org,
                    new PersonId(studentId),
                    relationshipId,
                    q.Reason,
                    actor
                ),
                ct
            )
        );
    }

    private static async Task<IResult> RevokeRelationshipAsync(
        Guid studentId,
        Guid relationshipId,
        ReasonRequest q,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        return RelationshipResult(
            await mediator.Send(
                new RevokeStudentRelationshipCommand(
                    org,
                    new PersonId(studentId),
                    relationshipId,
                    q.Reason,
                    actor
                ),
                ct
            )
        );
    }

    private static IResult RelationshipResult<T>(Result<T> r) =>
        r.IsSuccess
            ? Results.Ok(r.Value)
            : Results.Problem(
                statusCode: AdministrationStatusCode(r.Error.Type),
                title: r.Error.Code,
                detail: r.Error.MessageKey
            );

    private static IResult RelationshipResult(Result r) =>
        r.IsSuccess
            ? Results.NoContent()
            : Results.Problem(
                statusCode: AdministrationStatusCode(r.Error.Type),
                title: r.Error.Code,
                detail: r.Error.MessageKey
            );

    private static async Task<IResult> GetChecklistAsync(
        Guid studentId,
        Guid? enrollmentId,
        IMediator mediator,
        ICurrentTenant tenant,
        CancellationToken ct
    )
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return Results.Problem(statusCode: 401, title: "errors.currentTenant.required");
        var q = new GetEnrollmentChecklistQuery(
            tenant.OrganizationId.Value,
            new PersonId(studentId),
            enrollmentId.HasValue ? new DraftEnrollmentId(enrollmentId.Value) : null
        );
        return ChecklistResult(await mediator.Send(q, ct));
    }

    private static async Task<IResult> SynchronizeChecklistAsync(
        Guid studentId,
        EnrollmentReferenceRequest q,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        return ChecklistResult(
            await mediator.Send(
                new SynchronizeEnrollmentChecklistCommand(
                    org,
                    new PersonId(studentId),
                    new DraftEnrollmentId(q.EnrollmentId),
                    actor
                ),
                ct
            )
        );
    }

    private static async Task<IResult> ChangeChecklistStatusAsync(
        Guid studentId,
        Guid enrollmentId,
        Guid itemId,
        ChecklistStatusRequest q,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        return ChecklistResult(
            await mediator.Send(
                new ChangeChecklistItemStatusCommand(
                    org,
                    new PersonId(studentId),
                    new DraftEnrollmentId(enrollmentId),
                    itemId,
                    q.Status,
                    q.Reason,
                    user.HasPermission("Compliance.Exceptions.Approve"),
                    actor
                ),
                ct
            )
        );
    }

    private static async Task<IResult> AssignChecklistItemAsync(
        Guid studentId,
        Guid enrollmentId,
        Guid itemId,
        AssignChecklistItemRequest q,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        return ChecklistResult(
            await mediator.Send(
                new AssignChecklistItemCommand(
                    org,
                    new PersonId(studentId),
                    new DraftEnrollmentId(enrollmentId),
                    itemId,
                    q.ResponsibleUserId,
                    actor
                ),
                ct
            )
        );
    }

    private static async Task<IResult> RemindChecklistItemAsync(
        Guid studentId,
        Guid enrollmentId,
        Guid itemId,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        return ChecklistResult(
            await mediator.Send(
                new RemindChecklistItemCommand(
                    org,
                    new PersonId(studentId),
                    new DraftEnrollmentId(enrollmentId),
                    itemId,
                    actor
                ),
                ct
            )
        );
    }

    private static async Task<IResult> ActivateEnrollmentAsync(
        Guid studentId,
        Guid enrollmentId,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        return ChecklistResult(
            await mediator.Send(
                new ActivateEnrollmentCommand(
                    org,
                    new PersonId(studentId),
                    new DraftEnrollmentId(enrollmentId),
                    actor
                ),
                ct
            )
        );
    }

    private static async Task<IResult> ConfigureChecklistRuleAsync(
        ChecklistRuleRequest q,
        IMediator mediator,
        ICurrentTenant tenant,
        CancellationToken ct
    )
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return Results.Problem(statusCode: 401, title: "errors.currentTenant.required");
        return ChecklistResult(
            await mediator.Send(
                new ConfigureChecklistRuleCommand(
                    tenant.OrganizationId.Value,
                    q.RuleId,
                    q.TrainingCode,
                    q.Code,
                    q.LabelKey,
                    q.Category,
                    q.IsBlocking,
                    q.TargetRoute,
                    q.DueInDays,
                    q.IsActive
                ),
                ct
            )
        );
    }

    private static IResult ChecklistResult<T>(Result<T> r) =>
        r.IsSuccess
            ? Results.Ok(r.Value)
            : Results.Problem(
                statusCode: AdministrationStatusCode(r.Error.Type),
                title: r.Error.Code,
                detail: r.Error.MessageKey
            );

    private static IResult ChecklistResult(Result r) =>
        r.IsSuccess
            ? Results.NoContent()
            : Results.Problem(
                statusCode: AdministrationStatusCode(r.Error.Type),
                title: r.Error.Code,
                detail: r.Error.MessageKey
            );

    private static async Task<IResult> GetStudentDocumentsAsync(
        Guid studentId,
        Guid? enrollmentId,
        IMediator mediator,
        ICurrentTenant tenant,
        CancellationToken ct
    )
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return Results.Problem(statusCode: 401, title: "errors.currentTenant.required");
        return DocumentResult(
            await mediator.Send(
                new GetStudentDocumentsQuery(
                    tenant.OrganizationId.Value,
                    new PersonId(studentId),
                    enrollmentId.HasValue ? new DraftEnrollmentId(enrollmentId.Value) : null
                ),
                ct
            )
        );
    }

    private static async Task<IResult> RequestStudentDocumentAsync(
        Guid studentId,
        StudentDocumentRequest q,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        var r = await mediator.Send(
            new RequestStudentDocumentCommand(
                org,
                new PersonId(studentId),
                q.EnrollmentId.HasValue ? new DraftEnrollmentId(q.EnrollmentId.Value) : null,
                q.DocumentType,
                q.Category,
                q.Visibility,
                q.ExpiresOn,
                actor
            ),
            ct
        );
        return r.IsSuccess
            ? Results.Created($"/api/students/{studentId}/documents/{r.Value}", r.Value)
            : DocumentResult(r);
    }

    private static async Task<IResult> UploadStudentDocumentAsync(
        Guid studentId,
        Guid documentId,
        IFormFile file,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        await using Stream content = file.OpenReadStream();
        var r = await mediator.Send(
            new UploadStudentDocumentCommand(
                org,
                new PersonId(studentId),
                documentId,
                file.FileName,
                file.ContentType,
                file.Length,
                content,
                actor
            ),
            ct
        );
        return DocumentResult(r);
    }

    private static async Task<IResult> ValidateStudentDocumentAsync(
        Guid studentId,
        Guid documentId,
        DocumentValidationRequest q,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        return DocumentResult(
            await mediator.Send(
                new ValidateStudentDocumentCommand(
                    org,
                    new PersonId(studentId),
                    documentId,
                    q.Approve,
                    q.Reason,
                    actor
                ),
                ct
            )
        );
    }

    private static async Task<IResult> ShareStudentDocumentAsync(
        Guid studentId,
        Guid documentId,
        DocumentShareRequest q,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        return DocumentResult(
            await mediator.Send(
                new ShareStudentDocumentCommand(
                    org,
                    new PersonId(studentId),
                    documentId,
                    q.Visibility,
                    actor
                ),
                ct
            )
        );
    }

    private static async Task<IResult> ArchiveStudentDocumentAsync(
        Guid studentId,
        Guid documentId,
        ReasonRequest q,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        return DocumentResult(
            await mediator.Send(
                new ArchiveStudentDocumentCommand(
                    org,
                    new PersonId(studentId),
                    documentId,
                    q.Reason,
                    actor
                ),
                ct
            )
        );
    }

    private static async Task<IResult> DownloadStudentDocumentAsync(
        Guid studentId,
        Guid documentId,
        int? version,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        var r = await mediator.Send(
            new DownloadStudentDocumentQuery(
                org,
                new PersonId(studentId),
                documentId,
                version,
                actor
            ),
            ct
        );
        return r.IsSuccess
            ? Results.File(
                r.Value.Content,
                r.Value.ContentType,
                r.Value.FileName,
                enableRangeProcessing: false
            )
            : DocumentResult(r);
    }

    private static IResult DocumentResult<T>(Result<T> r) =>
        r.IsSuccess
            ? Results.Ok(r.Value)
            : Results.Problem(
                statusCode: AdministrationStatusCode(r.Error.Type),
                title: r.Error.Code,
                detail: r.Error.MessageKey
            );

    private static IResult DocumentResult(Result r) =>
        r.IsSuccess
            ? Results.NoContent()
            : Results.Problem(
                statusCode: AdministrationStatusCode(r.Error.Type),
                title: r.Error.Code,
                detail: r.Error.MessageKey
            );

    private static async Task<IResult> GetStudentStatusesAsync(
        Guid studentId,
        IMediator mediator,
        ICurrentTenant tenant,
        CancellationToken ct
    )
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return Results.Problem(statusCode: 401, title: "errors.currentTenant.required");
        return StatusResult(
            await mediator.Send(
                new GetStudentStatusesQuery(tenant.OrganizationId.Value, new PersonId(studentId)),
                ct
            )
        );
    }

    private static async Task<IResult> ApplyStudentBlockAsync(
        Guid studentId,
        ApplyStudentBlockRequest q,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        var r = await mediator.Send(
            new ApplyStudentBlockCommand(
                org,
                new PersonId(studentId),
                q.BlockType,
                q.Reason,
                q.SourceDomain,
                q.BlockingActions,
                q.Severity,
                q.ExpectedResolution,
                actor
            ),
            ct
        );
        return r.IsSuccess
            ? Results.Created($"/api/students/{studentId}/statuses/blocks/{r.Value}", r.Value)
            : StatusResult(r);
    }

    private static async Task<IResult> ReleaseStudentOperationalBlockAsync(
        Guid studentId,
        Guid blockId,
        ReleaseStudentBlockRequest q,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        return StatusResult(
            await mediator.Send(
                new ReleaseStudentBlockCommand(
                    org,
                    new PersonId(studentId),
                    blockId,
                    q.ResolutionType,
                    q.Reason,
                    actor
                ),
                ct
            )
        );
    }

    private static async Task<IResult> OverrideStudentBlockAsync(
        Guid studentId,
        Guid blockId,
        OverrideStudentBlockRequest q,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        return StatusResult(
            await mediator.Send(
                new OverrideStudentBlockCommand(
                    org,
                    new PersonId(studentId),
                    blockId,
                    q.Reason,
                    q.UntilUtc,
                    actor
                ),
                ct
            )
        );
    }

    private static IResult StatusResult<T>(Result<T> r) =>
        r.IsSuccess
            ? Results.Ok(r.Value)
            : Results.Problem(
                statusCode: AdministrationStatusCode(r.Error.Type),
                title: r.Error.Code,
                detail: r.Error.MessageKey
            );

    private static IResult StatusResult(Result r) =>
        r.IsSuccess
            ? Results.NoContent()
            : Results.Problem(
                statusCode: AdministrationStatusCode(r.Error.Type),
                title: r.Error.Code,
                detail: r.Error.MessageKey
            );

    private static async Task<IResult> GetStudentBranchesAsync(
        Guid studentId,
        IMediator mediator,
        ICurrentTenant tenant,
        CancellationToken ct
    )
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return Results.Problem(statusCode: 401, title: "errors.currentTenant.required");
        return BranchResult(
            await mediator.Send(
                new GetStudentBranchesQuery(tenant.OrganizationId.Value, new PersonId(studentId)),
                ct
            )
        );
    }

    private static async Task<IResult> AssignStudentBranchAsync(
        Guid studentId,
        AssignStudentBranchRequest q,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        var r = await mediator.Send(
            new AssignStudentBranchCommand(
                org,
                new PersonId(studentId),
                new BranchId(q.BranchId),
                q.Type,
                q.ServicesAllowed,
                q.EffectiveFrom,
                q.EffectiveTo,
                q.Reason,
                actor
            ),
            ct
        );
        return r.IsSuccess
            ? Results.Created($"/api/students/{studentId}/branches/assignments/{r.Value}", r.Value)
            : BranchResult(r);
    }

    private static async Task<IResult> AnalyzePrimaryBranchChangeAsync(
        Guid studentId,
        AnalyzePrimaryBranchChangeRequest q,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        return BranchResult(
            await mediator.Send(
                new AnalyzePrimaryBranchChangeCommand(
                    org,
                    new PersonId(studentId),
                    new BranchId(q.TargetBranchId),
                    actor
                ),
                ct
            )
        );
    }

    private static async Task<IResult> ChangePrimaryStudentBranchAsync(
        Guid studentId,
        ChangePrimaryBranchRequest q,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        return BranchResult(
            await mediator.Send(
                new ChangePrimaryStudentBranchCommand(
                    org,
                    new PersonId(studentId),
                    q.AnalysisId,
                    q.Reason,
                    actor
                ),
                ct
            )
        );
    }

    private static async Task<IResult> EndStudentBranchAssignmentAsync(
        Guid studentId,
        Guid assignmentId,
        ReasonRequest q,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        return BranchResult(
            await mediator.Send(
                new EndStudentBranchAssignmentCommand(
                    org,
                    new PersonId(studentId),
                    assignmentId,
                    q.Reason,
                    actor
                ),
                ct
            )
        );
    }

    private static IResult BranchResult<T>(Result<T> r) =>
        r.IsSuccess
            ? Results.Ok(r.Value)
            : Results.Problem(
                statusCode: AdministrationStatusCode(r.Error.Type),
                title: r.Error.Code,
                detail: r.Error.MessageKey
            );

    private static IResult BranchResult(Result r) =>
        r.IsSuccess
            ? Results.NoContent()
            : Results.Problem(
                statusCode: AdministrationStatusCode(r.Error.Type),
                title: r.Error.Code,
                detail: r.Error.MessageKey
            );

    private static async Task<IResult> GetStudentInstructorsAsync(
        Guid studentId,
        IMediator mediator,
        ICurrentTenant tenant,
        CancellationToken ct
    )
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return Results.Problem(statusCode: 401, title: "errors.currentTenant.required");
        return InstructorResult(
            await mediator.Send(
                new GetStudentInstructorsQuery(
                    tenant.OrganizationId.Value,
                    new PersonId(studentId)
                ),
                ct
            )
        );
    }

    private static async Task<IResult> GetInstructorSuggestionsAsync(
        Guid studentId,
        Guid? branchId,
        string trainingCategory,
        IMediator mediator,
        ICurrentTenant tenant,
        CancellationToken ct
    )
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return Results.Problem(statusCode: 401, title: "errors.currentTenant.required");
        return InstructorResult(
            await mediator.Send(
                new GetInstructorSuggestionsQuery(
                    tenant.OrganizationId.Value,
                    new PersonId(studentId),
                    branchId.HasValue ? new BranchId(branchId.Value) : null,
                    trainingCategory
                ),
                ct
            )
        );
    }

    private static async Task<IResult> AssignStudentInstructorAsync(
        Guid studentId,
        AssignStudentInstructorRequest q,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        var r = await mediator.Send(
            new AssignStudentInstructorCommand(
                org,
                new PersonId(studentId),
                new UserId(q.InstructorId),
                q.Type,
                q.EffectiveFrom,
                q.EffectiveTo,
                q.TrainingCategory,
                q.MaximumScope,
                q.Reason,
                actor
            ),
            ct
        );
        return r.IsSuccess
            ? Results.Created(
                $"/api/students/{studentId}/instructors/assignments/{r.Value}",
                r.Value
            )
            : InstructorResult(r);
    }

    private static async Task<IResult> ReplacePrimaryInstructorAsync(
        Guid studentId,
        ReplacePrimaryInstructorRequest q,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        return InstructorResult(
            await mediator.Send(
                new ReplacePrimaryInstructorCommand(
                    org,
                    new PersonId(studentId),
                    new UserId(q.InstructorId),
                    q.EffectiveFrom,
                    q.EffectiveTo,
                    q.TrainingCategory,
                    q.MaximumScope,
                    q.Reason,
                    actor
                ),
                ct
            )
        );
    }

    private static async Task<IResult> EndStudentInstructorAssignmentAsync(
        Guid studentId,
        Guid assignmentId,
        ReasonRequest q,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        return InstructorResult(
            await mediator.Send(
                new EndStudentInstructorAssignmentCommand(
                    org,
                    new PersonId(studentId),
                    assignmentId,
                    q.Reason,
                    actor
                ),
                ct
            )
        );
    }

    private static IResult InstructorResult<T>(Result<T> r) =>
        r.IsSuccess
            ? Results.Ok(r.Value)
            : Results.Problem(
                statusCode: AdministrationStatusCode(r.Error.Type),
                title: r.Error.Code,
                detail: r.Error.MessageKey
            );

    private static IResult InstructorResult(Result r) =>
        r.IsSuccess
            ? Results.NoContent()
            : Results.Problem(
                statusCode: AdministrationStatusCode(r.Error.Type),
                title: r.Error.Code,
                detail: r.Error.MessageKey
            );

    private static async Task<IResult> GetInternalTransfersAsync(
        Guid studentId,
        IMediator mediator,
        ICurrentTenant tenant,
        CancellationToken ct
    )
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return Results.Problem(statusCode: 401, title: "errors.currentTenant.required");
        return InternalTransferResult(
            await mediator.Send(
                new GetInternalTransfersQuery(tenant.OrganizationId.Value, new PersonId(studentId)),
                ct
            )
        );
    }

    private static async Task<IResult> AnalyzeInternalTransferAsync(
        Guid studentId,
        AnalyzeInternalTransferRequest q,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        var result = await mediator.Send(
            new AnalyzeInternalTransferCommand(
                org,
                new PersonId(studentId),
                new BranchId(q.TargetBranchId),
                q.Mode,
                q.Elements,
                q.EffectiveOn,
                q.TemporaryUntil,
                q.Reason,
                actor
            ),
            ct
        );
        return result.IsSuccess
            ? Results.Created(
                $"/api/students/{studentId}/transfers/internal/{result.Value.TransferId}",
                result.Value
            )
            : InternalTransferResult(result);
    }

    private static async Task<IResult> ValidateInternalTransferAsync(
        Guid studentId,
        Guid transferId,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        return InternalTransferResult(
            await mediator.Send(
                new ValidateInternalTransferCommand(
                    org,
                    new PersonId(studentId),
                    transferId,
                    actor
                ),
                ct
            )
        );
    }

    private static async Task<IResult> GetExternalTransfersAsync(
        Guid studentId,
        IMediator mediator,
        ICurrentTenant tenant,
        CancellationToken ct
    )
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return Results.Problem(statusCode: 401, title: "errors.currentTenant.required");
        return ExternalTransferResult(
            await mediator.Send(
                new GetExternalTransfersQuery(tenant.OrganizationId.Value, new PersonId(studentId)),
                ct
            )
        );
    }

    private static async Task<IResult> CreateExternalTransferAsync(
        Guid studentId,
        CreateExternalTransferRequest q,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        var r = await mediator.Send(
            new CreateExternalTransferCommand(
                org,
                new PersonId(studentId),
                new OrganizationId(q.TargetOrganizationId),
                q.Type,
                q.DataScope,
                q.EffectiveOn,
                q.TemporaryUntil,
                q.CountryCode,
                q.Reason,
                q.Responsibilities,
                actor
            ),
            ct
        );
        return r.IsSuccess
            ? Results.Created($"/api/students/{studentId}/transfers/external/{r.Value}", r.Value)
            : ExternalTransferResult(r);
    }

    private static async Task<IResult> VerifyExternalTransferConsentAsync(
        Guid studentId,
        Guid transferId,
        ConsentEvidenceRequest q,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        return ExternalTransferResult(
            await mediator.Send(
                new VerifyExternalTransferConsentCommand(
                    org,
                    new PersonId(studentId),
                    transferId,
                    q.EvidenceReference,
                    actor
                ),
                ct
            )
        );
    }

    private static async Task<IResult> ReviewExternalTransferFinanceAsync(
        Guid studentId,
        Guid transferId,
        ExternalTransferFinanceRequest q,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        return ExternalTransferResult(
            await mediator.Send(
                new ReviewExternalTransferFinanceCommand(
                    org,
                    new PersonId(studentId),
                    transferId,
                    q.Status,
                    q.Resolution,
                    actor
                ),
                ct
            )
        );
    }

    private static async Task<IResult> SubmitExternalTransferAsync(
        Guid studentId,
        Guid transferId,
        SubmitExternalTransferRequest q,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        return ExternalTransferResult(
            await mediator.Send(
                new SubmitExternalTransferCommand(
                    org,
                    new PersonId(studentId),
                    transferId,
                    q.RequestInvitationIfMissing,
                    actor
                ),
                ct
            )
        );
    }

    private static async Task<IResult> DecideExternalTransferAsync(
        Guid studentId,
        Guid transferId,
        ExternalTransferDecisionRequest q,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        return ExternalTransferResult(
            await mediator.Send(
                new DecideExternalTransferCommand(
                    org,
                    new PersonId(studentId),
                    transferId,
                    q.Accept,
                    q.Reason,
                    actor
                ),
                ct
            )
        );
    }

    private static async Task<IResult> CompleteExternalTransferAsync(
        Guid studentId,
        Guid transferId,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        return ExternalTransferResult(
            await mediator.Send(
                new CompleteExternalTransferCommand(
                    org,
                    new PersonId(studentId),
                    transferId,
                    actor
                ),
                ct
            )
        );
    }

    private static IResult ExternalTransferResult<T>(Result<T> r) =>
        r.IsSuccess
            ? Results.Ok(r.Value)
            : Results.Problem(
                statusCode: AdministrationStatusCode(r.Error.Type),
                title: r.Error.Code,
                detail: r.Error.MessageKey
            );

    private static IResult ExternalTransferResult(Result r) =>
        r.IsSuccess
            ? Results.NoContent()
            : Results.Problem(
                statusCode: AdministrationStatusCode(r.Error.Type),
                title: r.Error.Code,
                detail: r.Error.MessageKey
            );

    private static async Task<IResult> GetEnrollmentSuspensionsAsync(
        Guid studentId,
        IMediator mediator,
        ICurrentTenant tenant,
        CancellationToken ct
    )
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return Results.Problem(statusCode: 401, title: "errors.currentTenant.required");
        return SuspensionResult(
            await mediator.Send(
                new GetEnrollmentSuspensionsQuery(
                    tenant.OrganizationId.Value,
                    new PersonId(studentId)
                ),
                ct
            )
        );
    }

    private static async Task<IResult> SuspendEnrollmentAsync(
        Guid studentId,
        SuspendEnrollmentRequest q,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        bool full = q.Scope.HasFlag(EnrollmentSuspensionScope.FullEnrollment);
        if (
            (full || q.Scope.HasFlag(EnrollmentSuspensionScope.FinanceActions))
            && !user.HasPermission("Students.SuspendFinancial")
        )
            return Results.Problem(statusCode: 403, title: "errors.authorization.permissionDenied");
        if (
            (
                full
                || q.Scope.HasFlag(EnrollmentSuspensionScope.TrainingDelivery)
                || q.Scope.HasFlag(EnrollmentSuspensionScope.ExamRegistration)
            ) && !user.HasPermission("Students.SuspendPedagogical")
        )
            return Results.Problem(statusCode: 403, title: "errors.authorization.permissionDenied");
        var r = await mediator.Send(
            new SuspendEnrollmentCommand(
                org,
                new PersonId(studentId),
                q.Reason,
                q.Scope,
                q.StartDate,
                q.ExpectedEndDate,
                q.ImmediateActions,
                q.BookingsDecision,
                q.FutureBookingsCount,
                q.CreditDecision,
                q.NotificationPlan,
                q.ReviewDate,
                actor
            ),
            ct
        );
        return r.IsSuccess
            ? Results.Created($"/api/students/{studentId}/suspension/{r.Value}", r.Value)
            : SuspensionResult(r);
    }

    private static IResult SuspensionResult<T>(Result<T> r) =>
        r.IsSuccess
            ? Results.Ok(r.Value)
            : Results.Problem(
                statusCode: AdministrationStatusCode(r.Error.Type),
                title: r.Error.Code,
                detail: r.Error.MessageKey
            );

    private static async Task<IResult> GetEnrollmentReactivationsAsync(
        Guid studentId,
        IMediator mediator,
        ICurrentTenant tenant,
        CancellationToken ct
    )
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return Results.Problem(statusCode: 401, title: "errors.currentTenant.required");
        return ReactivationResult(
            await mediator.Send(
                new GetEnrollmentReactivationsQuery(
                    tenant.OrganizationId.Value,
                    new PersonId(studentId)
                ),
                ct
            )
        );
    }

    private static async Task<IResult> CreateEnrollmentReactivationAsync(
        Guid studentId,
        CreateEnrollmentReactivationRequest q,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        if (q.PedagogyReviewRequested && !user.HasPermission("Pedagogy.ReviewRequest"))
            return Results.Problem(statusCode: 403, title: "errors.authorization.permissionDenied");
        var checks = q
            .Checks.Select(x => new EnrollmentReactivationCheckSeed(x.Type, x.Status, x.Detail))
            .ToArray();
        var r = await mediator.Send(
            new CreateEnrollmentReactivationCommand(
                org,
                new PersonId(studentId),
                q.SuspensionId,
                q.Mode,
                q.ResumeDate,
                q.Conditions,
                q.PedagogyReviewRequested,
                checks,
                actor
            ),
            ct
        );
        return r.IsSuccess
            ? Results.Created($"/api/students/{studentId}/reactivate/{r.Value}", r.Value)
            : ReactivationResult(r);
    }

    private static async Task<IResult> ReviewEnrollmentReactivationCheckAsync(
        Guid studentId,
        Guid reactivationId,
        ReactivationCheckType checkType,
        ReviewReactivationCheckRequest q,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        return ReactivationResult(
            await mediator.Send(
                new ReviewEnrollmentReactivationCheckCommand(
                    org,
                    new PersonId(studentId),
                    reactivationId,
                    checkType,
                    q.Status,
                    q.Detail,
                    actor
                ),
                ct
            )
        );
    }

    private static async Task<IResult> ApplyEnrollmentReactivationAsync(
        Guid studentId,
        Guid reactivationId,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        return ReactivationResult(
            await mediator.Send(
                new ApplyEnrollmentReactivationCommand(
                    org,
                    new PersonId(studentId),
                    reactivationId,
                    actor
                ),
                ct
            )
        );
    }

    private static IResult ReactivationResult<T>(Result<T> r) =>
        r.IsSuccess
            ? Results.Ok(r.Value)
            : Results.Problem(
                statusCode: AdministrationStatusCode(r.Error.Type),
                title: r.Error.Code,
                detail: r.Error.MessageKey
            );

    private static IResult ReactivationResult(Result r) =>
        r.IsSuccess
            ? Results.NoContent()
            : Results.Problem(
                statusCode: AdministrationStatusCode(r.Error.Type),
                title: r.Error.Code,
                detail: r.Error.MessageKey
            );

    private static async Task<IResult> GetEnrollmentClosuresAsync(
        Guid studentId,
        IMediator mediator,
        ICurrentTenant tenant,
        CancellationToken ct
    )
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return Results.Problem(statusCode: 401, title: "errors.currentTenant.required");
        return ClosureResult(
            await mediator.Send(
                new GetEnrollmentClosuresQuery(
                    tenant.OrganizationId.Value,
                    new PersonId(studentId)
                ),
                ct
            )
        );
    }

    private static async Task<IResult> CreateEnrollmentClosureAsync(
        Guid studentId,
        CreateEnrollmentClosureRequest q,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        var checks = q
            .Checks.Select(x => new EnrollmentClosureCheckSeed(x.Type, x.Status, x.Detail))
            .ToArray();
        var r = await mediator.Send(
            new CreateEnrollmentClosureCommand(
                org,
                new PersonId(studentId),
                q.EnrollmentId,
                q.Reason,
                q.ClosureDate,
                q.ReasonDetail,
                checks,
                actor
            ),
            ct
        );
        return r.IsSuccess
            ? Results.Created($"/api/students/{studentId}/close/{r.Value}", r.Value)
            : ClosureResult(r);
    }

    private static async Task<IResult> ReviewEnrollmentClosureCheckAsync(
        Guid studentId,
        Guid closureId,
        EnrollmentClosureCheckType checkType,
        ReviewEnrollmentClosureCheckRequest q,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        return ClosureResult(
            await mediator.Send(
                new ReviewEnrollmentClosureCheckCommand(
                    org,
                    new PersonId(studentId),
                    closureId,
                    checkType,
                    q.Status,
                    q.Detail,
                    actor
                ),
                ct
            )
        );
    }

    private static async Task<IResult> CloseEnrollmentAsync(
        Guid studentId,
        Guid closureId,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        return ClosureResult(
            await mediator.Send(
                new CloseEnrollmentCommand(org, new PersonId(studentId), closureId, actor),
                ct
            )
        );
    }

    private static async Task<IResult> ArchiveStudentAsync(
        Guid studentId,
        Guid closureId,
        ArchiveStudentRequest q,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        return ClosureResult(
            await mediator.Send(
                new ArchiveStudentCommand(
                    org,
                    new PersonId(studentId),
                    closureId,
                    q.RetainUntil,
                    q.RetentionLegalBasis,
                    q.RetentionScope,
                    actor
                ),
                ct
            )
        );
    }

    private static async Task<IResult> ReopenEnrollmentAsync(
        Guid studentId,
        Guid closureId,
        ReopenEnrollmentRequest q,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryGuardianActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        return ClosureResult(
            await mediator.Send(
                new ReopenEnrollmentCommand(
                    org,
                    new PersonId(studentId),
                    closureId,
                    q.Justification,
                    actor
                ),
                ct
            )
        );
    }

    private static IResult ClosureResult<T>(Result<T> r) =>
        r.IsSuccess
            ? Results.Ok(r.Value)
            : Results.Problem(
                statusCode: AdministrationStatusCode(r.Error.Type),
                title: r.Error.Code,
                detail: r.Error.MessageKey
            );

    private static IResult ClosureResult(Result r) =>
        r.IsSuccess
            ? Results.NoContent()
            : Results.Problem(
                statusCode: AdministrationStatusCode(r.Error.Type),
                title: r.Error.Code,
                detail: r.Error.MessageKey
            );

    private static IResult InternalTransferResult<T>(Result<T> r) =>
        r.IsSuccess
            ? Results.Ok(r.Value)
            : Results.Problem(
                statusCode: AdministrationStatusCode(r.Error.Type),
                title: r.Error.Code,
                detail: r.Error.MessageKey
            );

    private static async Task<IResult> GetAdministrationAsync(
        Guid studentId,
        IMediator mediator,
        ICurrentTenant tenant,
        CancellationToken ct
    )
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return Results.Problem(statusCode: 401, title: "errors.currentTenant.required");
        var r = await mediator.Send(
            new GetAdministrationQuery(tenant.OrganizationId.Value, new PersonId(studentId)),
            ct
        );
        return AdministrationResult(r);
    }

    private static Task<IResult> CreateRequirementAsync(
        Guid studentId,
        ConfigureRequirementRequest request,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    ) => ConfigureRequirementCore(studentId, null, request, mediator, tenant, user, ct);

    private static async Task<IResult> SynchronizeAdministrativeRequirementsAsync(
        Guid studentId,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryAdministrationActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        var r = await mediator.Send(
            new SynchronizeAdministrativeRequirementsCommand(org, new PersonId(studentId), actor),
            ct
        );
        return AdministrationResult(r);
    }

    private static Task<IResult> UpdateRequirementAsync(
        Guid studentId,
        Guid requirementId,
        ConfigureRequirementRequest request,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    ) => ConfigureRequirementCore(studentId, requirementId, request, mediator, tenant, user, ct);

    private static async Task<IResult> ConfigureRequirementCore(
        Guid studentId,
        Guid? requirementId,
        ConfigureRequirementRequest request,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryAdministrationActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        var r = await mediator.Send(
            new ConfigureRequirementCommand(
                org,
                new PersonId(studentId),
                requirementId,
                request.Code,
                request.LabelKey,
                request.IsBlocking,
                request.DueAtUtc,
                request.PolicySource,
                actor
            ),
            ct
        );
        return AdministrationResult(r);
    }

    private static async Task<IResult> DecideRequirementAsync(
        Guid studentId,
        Guid requirementId,
        DecideRequirementRequest request,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryAdministrationActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        var r = await mediator.Send(
            new DecideRequirementCommand(
                org,
                new PersonId(studentId),
                requirementId,
                request.Status,
                request.Reason,
                actor
            ),
            ct
        );
        return AdministrationResult(r);
    }

    private static async Task<IResult> AddAdministrativeBlockAsync(
        Guid studentId,
        AddBlockRequest request,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryAdministrationActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        var r = await mediator.Send(
            new AddAdministrativeBlockCommand(
                org,
                new PersonId(studentId),
                request.Code,
                request.Reason,
                actor
            ),
            ct
        );
        return AdministrationResult(r);
    }

    private static async Task<IResult> ReleaseAdministrativeBlockAsync(
        Guid studentId,
        Guid blockId,
        ReasonRequest request,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryAdministrationActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        var r = await mediator.Send(
            new ReleaseAdministrativeBlockCommand(
                org,
                new PersonId(studentId),
                blockId,
                request.Reason,
                actor
            ),
            ct
        );
        return AdministrationResult(r);
    }

    private static async Task<IResult> RequestComplianceExceptionAsync(
        Guid studentId,
        Guid requirementId,
        ReasonRequest request,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryAdministrationActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        var r = await mediator.Send(
            new RequestComplianceExceptionCommand(
                org,
                new PersonId(studentId),
                requirementId,
                request.Reason,
                actor
            ),
            ct
        );
        return AdministrationResult(r);
    }

    private static async Task<IResult> DecideComplianceExceptionAsync(
        Guid studentId,
        Guid exceptionId,
        ExceptionDecisionRequest request,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser user,
        CancellationToken ct
    )
    {
        if (!TryAdministrationActor(tenant, user, out var org, out var actor, out var failure))
            return failure!;
        var r = await mediator.Send(
            new DecideComplianceExceptionCommand(
                org,
                new PersonId(studentId),
                exceptionId,
                request.Approve,
                request.Reason,
                actor
            ),
            ct
        );
        return AdministrationResult(r);
    }

    private static bool TryAdministrationActor(
        ICurrentTenant tenant,
        ICurrentUser user,
        out OrganizationId organizationId,
        out UserId actor,
        out IResult? failure
    )
    {
        organizationId = default;
        actor = default;
        if (!tenant.HasTenant || tenant.OrganizationId is null || user.UserId is null)
        {
            failure = Results.Problem(statusCode: 401, title: "errors.currentUser.required");
            return false;
        }
        organizationId = tenant.OrganizationId.Value;
        actor = user.UserId.Value;
        failure = null;
        return true;
    }

    private static IResult AdministrationResult<T>(Result<T> r) =>
        r.IsSuccess
            ? Results.Ok(r.Value)
            : Results.Problem(
                statusCode: AdministrationStatusCode(r.Error.Type),
                title: r.Error.Code,
                detail: r.Error.MessageKey
            );

    private static IResult AdministrationResult(Result r) =>
        r.IsSuccess
            ? Results.NoContent()
            : Results.Problem(
                statusCode: AdministrationStatusCode(r.Error.Type),
                title: r.Error.Code,
                detail: r.Error.MessageKey
            );

    private static int AdministrationStatusCode(ErrorType type) =>
        type switch
        {
            ErrorType.NotFound => 404,
            ErrorType.Conflict => 409,
            ErrorType.Forbidden => 403,
            _ => 400,
        };

    private static async Task<IResult> GetStudentIdentityAsync(
        Guid studentId,
        IMediator mediator,
        ICurrentTenant tenant,
        CancellationToken cancellationToken
    )
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return Results.Problem(statusCode: 401, title: "errors.currentTenant.required");
        Result<StudentIdentityResponse> result = await mediator.Send(
            new GetStudentIdentityQuery(tenant.OrganizationId.Value, new PersonId(studentId)),
            cancellationToken
        );
        return StudentIdentityResult(result);
    }

    private static async Task<IResult> UpdateStudentIdentityAsync(
        Guid studentId,
        UpdateStudentIdentityRequest request,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser currentUser,
        CancellationToken cancellationToken
    )
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null || currentUser.UserId is null)
            return Results.Problem(statusCode: 401, title: "errors.currentUser.required");
        var identity = new StudentIdentityData(
            request.LegalFirstName,
            request.LegalLastName,
            request.PreferredName,
            request.BirthDate,
            request.BirthPlace,
            request.Nationality,
            request.Email,
            request.Phone,
            request.AddressLine1,
            request.AddressLine2,
            request.PostalCode,
            request.City,
            request.CountryCode,
            request.PreferredLanguage,
            request.TimeZone,
            request.AllowEmail,
            request.AllowSms,
            request.AllowPhone
        );
        Result<UpdateStudentIdentityResponse> result = await mediator.Send(
            new UpdateStudentIdentityCommand(
                tenant.OrganizationId.Value,
                new PersonId(studentId),
                identity,
                request.Justification,
                currentUser.UserId.Value
            ),
            cancellationToken
        );
        return StudentIdentityResult(result);
    }

    private static async Task<IResult> VerifyStudentIdentityAsync(
        Guid studentId,
        VerifyStudentIdentityRequest request,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser currentUser,
        CancellationToken cancellationToken
    )
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null || currentUser.UserId is null)
            return Results.Problem(statusCode: 401, title: "errors.currentUser.required");
        Result<StudentIdentityResponse> result = await mediator.Send(
            new VerifyStudentIdentityCommand(
                tenant.OrganizationId.Value,
                new PersonId(studentId),
                request.Status,
                request.Justification,
                currentUser.UserId.Value
            ),
            cancellationToken
        );
        return StudentIdentityResult(result);
    }

    private static async Task<IResult> UpdateOwnContactAsync(
        Guid studentId,
        UpdateOwnStudentContactRequest request,
        HttpContext httpContext,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser currentUser,
        CancellationToken cancellationToken
    )
    {
        string? personClaim = httpContext.User.FindFirst("person_id")?.Value;
        if (!Guid.TryParse(personClaim, out Guid personId) || personId != studentId)
            return Results.Problem(
                statusCode: 403,
                title: "Students.Identity.SelfService.Forbidden",
                detail: "errors.students.identity.selfService.forbidden"
            );
        if (!tenant.HasTenant || tenant.OrganizationId is null || currentUser.UserId is null)
            return Results.Problem(statusCode: 401, title: "errors.currentUser.required");
        Result<UpdateStudentIdentityResponse> result = await mediator.Send(
            new UpdateOwnStudentContactCommand(
                tenant.OrganizationId.Value,
                new PersonId(studentId),
                request.Email,
                request.Phone,
                request.AddressLine1,
                request.AddressLine2,
                request.PostalCode,
                request.City,
                request.CountryCode,
                request.PreferredLanguage,
                request.TimeZone,
                request.AllowEmail,
                request.AllowSms,
                request.AllowPhone,
                currentUser.UserId.Value
            ),
            cancellationToken
        );
        return StudentIdentityResult(result);
    }

    private static IResult StudentIdentityResult<T>(Result<T> result) =>
        result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(
                statusCode: result.Error.Type == ErrorType.NotFound ? 404 : 400,
                title: result.Error.Code,
                detail: result.Error.MessageKey
            );

    private static async Task<IResult> GetStudentOverviewAsync(
        Guid studentId,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser currentUser,
        CancellationToken cancellationToken
    )
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return Results.Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "errors.currentTenant.required"
            );
        var scope = new StudentOverviewReadScope(
            currentUser.HasPermission("Enrollments.Read"),
            currentUser.HasPermission("Students.Administration.Read"),
            currentUser.HasPermission("Finance.Summary.Read"),
            currentUser.HasPermission("Pedagogy.Summary.Read"),
            currentUser.HasPermission(DriveOsPermissionCodes.Scheduling.BookingsRead),
            currentUser.HasPermission(DriveOsPermissionCodes.Exams.DashboardRead),
            currentUser.HasPermission(DriveOsPermissionCodes.StudentDocuments.Read),
            currentUser.HasPermission(DriveOsPermissionCodes.Communication.Notifications.Read),
            currentUser.HasPermission(DriveOsPermissionCodes.TrainingDelivery.IncidentsRead),
            currentUser.HasPermission(DriveOsPermissionCodes.Partners.Read),
            currentUser.HasPermission(DriveOsPermissionCodes.Students.HistoryRead),
            currentUser.HasPermission(DriveOsPermissionCodes.Scheduling.BookingsCreate),
            currentUser.HasPermission("Finance.Payments.Create"),
            currentUser.HasPermission(DriveOsPermissionCodes.StudentDocuments.Upload),
            currentUser.HasPermission(DriveOsPermissionCodes.Communication.Notifications.Manage)
        );
        Result<StudentOverviewResponse> result = await mediator.Send(
            new GetStudentOverviewQuery(
                tenant.OrganizationId.Value,
                new PersonId(studentId),
                scope
            ),
            cancellationToken
        );
        if (result.IsSuccess)
            return Results.Ok(result.Value);
        return Results.Problem(
            statusCode: result.Error.Type == ErrorType.NotFound
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest,
            title: result.Error.Code,
            detail: result.Error.MessageKey
        );
    }

    private static async Task<IResult> StartDirectEnrollmentAsync(
        StartDirectEnrollmentRequest request,
        HttpRequest httpRequest,
        IMediator mediator,
        ICurrentTenant tenant,
        CancellationToken cancellationToken
    )
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return Results.Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "errors.currentTenant.required"
            );
        string idempotencyKey = httpRequest.Headers["Idempotency-Key"].ToString();
        var command = new StartDirectEnrollmentCommand(
            tenant.OrganizationId.Value,
            idempotencyKey,
            request.ExistingStudentId.HasValue
                ? new PersonId(request.ExistingStudentId.Value)
                : null,
            new BranchId(request.BranchId),
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone,
            request.TrainingCode,
            request.Source,
            request.RegulatoryCountryCode,
            request.PreferredLanguageCode,
            request.RequiredConsentsAccepted
        );
        Result<StartDirectEnrollmentResponse> result = await mediator.Send(
            command,
            cancellationToken
        );
        if (result.IsFailure)
        {
            int status = result.Error.Type switch
            {
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status400BadRequest,
            };
            return Results.Problem(
                statusCode: status,
                title: result.Error.Code,
                detail: result.Error.MessageKey
            );
        }
        return result.Value.IdempotentReplay
            ? Results.Ok(result.Value)
            : Results.Created($"/api/students/{result.Value.StudentId}", result.Value);
    }

    private static async Task<IResult> GetStudentsAsync(
        [AsParameters] GetStudentsRequest request,
        IMediator mediator,
        ICurrentTenant tenant,
        CancellationToken cancellationToken
    )
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return Results.Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "errors.currentTenant.required"
            );
        var query = new GetStudentsQuery(
            tenant.OrganizationId.Value,
            request.PageNumber,
            request.PageSize,
            request.Search,
            request.BranchId.HasValue ? new BranchId(request.BranchId.Value) : null,
            request.Status,
            request.EnrollmentStatus,
            request.SortBy,
            request.SortDirection
        );
        Result<PagedResult<StudentListItem>> result = await mediator.Send(query, cancellationToken);
        if (result.IsFailure)
            return Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: result.Error.Code
            );
        PagedResult<StudentListItem> page = result.Value;
        return Results.Ok(
            new PagedResponse<StudentListItem>(
                page.Items,
                page.PageNumber,
                page.PageSize,
                page.TotalCount,
                page.TotalPages,
                page.HasPreviousPage,
                page.HasNextPage
            )
        );
    }

    private static async Task<IResult> GetAsync(
        Guid? branchId,
        IMediator mediator,
        ICurrentTenant tenant,
        CancellationToken cancellationToken
    )
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return Results.Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "errors.currentTenant.required"
            );
        Result<StudentDashboardResponse> result = await mediator.Send(
            new GetStudentDashboardQuery(
                tenant.OrganizationId.Value,
                branchId.HasValue ? new BranchId(branchId.Value) : null
            ),
            cancellationToken
        );
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: result.Error.Code
            );
    }

    public sealed record GetStudentsRequest(
        int PageNumber = 1,
        int PageSize = 20,
        string? Search = null,
        Guid? BranchId = null,
        StudentStatus? Status = null,
        EnrollmentStatus? EnrollmentStatus = null,
        StudentSortField SortBy = StudentSortField.Name,
        SortDirection SortDirection = SortDirection.Ascending
    );

    public sealed record StartDirectEnrollmentRequest(
        Guid? ExistingStudentId,
        Guid BranchId,
        string FirstName,
        string LastName,
        string? Email,
        string? Phone,
        string TrainingCode,
        EnrollmentSource Source,
        string RegulatoryCountryCode,
        string PreferredLanguageCode,
        bool RequiredConsentsAccepted
    );

    public sealed record UpdateStudentIdentityRequest(
        string LegalFirstName,
        string LegalLastName,
        string? PreferredName,
        DateOnly? BirthDate,
        string? BirthPlace,
        string? Nationality,
        string? Email,
        string? Phone,
        string? AddressLine1,
        string? AddressLine2,
        string? PostalCode,
        string? City,
        string? CountryCode,
        string? PreferredLanguage,
        string? TimeZone,
        bool AllowEmail,
        bool AllowSms,
        bool AllowPhone,
        string? Justification
    );

    public sealed record VerifyStudentIdentityRequest(
        IdentityVerificationStatus Status,
        string Justification
    );

    public sealed record UpdateOwnStudentContactRequest(
        string? Email,
        string? Phone,
        string? AddressLine1,
        string? AddressLine2,
        string? PostalCode,
        string? City,
        string? CountryCode,
        string? PreferredLanguage,
        string? TimeZone,
        bool AllowEmail,
        bool AllowSms,
        bool AllowPhone
    );

    public sealed record ConfigureRequirementRequest(
        string Code,
        string LabelKey,
        bool IsBlocking,
        DateTimeOffset? DueAtUtc,
        string PolicySource
    );

    public sealed record DecideRequirementRequest(
        AdministrativeRequirementStatus Status,
        string Reason
    );

    public sealed record AddBlockRequest(string Code, string Reason);

    public sealed record ReasonRequest(string Reason);

    public sealed record ExceptionDecisionRequest(bool Approve, string Reason);

    public sealed record GuardianRequest(
        Guid GuardianPersonId,
        string FirstName,
        string LastName,
        string? Email,
        string? Phone,
        GuardianRelationshipType RelationshipType,
        string LegalBasis,
        ParentalAuthorityStatus ParentalAuthorityStatus,
        GuardianPermissions Permissions,
        DateOnly EffectiveFrom,
        DateOnly? EffectiveTo,
        bool FinancialRights,
        bool SignatureRights,
        string NotificationPreferences
    );

    public sealed record UpdateGuardianRequest(
        GuardianRelationshipType RelationshipType,
        string LegalBasis,
        ParentalAuthorityStatus ParentalAuthorityStatus,
        GuardianPermissions Permissions,
        DateOnly EffectiveFrom,
        DateOnly? EffectiveTo,
        bool FinancialRights,
        bool SignatureRights,
        string NotificationPreferences
    );

    public sealed record StudentRelationshipRequest(
        Guid PersonOrOrganizationId,
        RelatedPartyKind PartyKind,
        string DisplayName,
        string? Email,
        string? Phone,
        StudentRelationshipType RelationshipType,
        StudentRelationshipPermissions Permissions,
        FinancialScope FinancialScope,
        CommunicationScope CommunicationScope,
        DateOnly EffectiveFrom,
        DateOnly? EffectiveTo,
        bool IsPrimaryPayer
    );

    public sealed record UpdateStudentRelationshipRequest(
        StudentRelationshipType RelationshipType,
        StudentRelationshipPermissions Permissions,
        FinancialScope FinancialScope,
        CommunicationScope CommunicationScope,
        DateOnly EffectiveFrom,
        DateOnly? EffectiveTo,
        bool IsPrimaryPayer
    );

    public sealed record EnrollmentReferenceRequest(Guid EnrollmentId);

    public sealed record ChecklistStatusRequest(ChecklistItemStatus Status, string? Reason);

    public sealed record AssignChecklistItemRequest(Guid ResponsibleUserId);

    public sealed record ChecklistRuleRequest(
        Guid? RuleId,
        string TrainingCode,
        string Code,
        string LabelKey,
        ChecklistCategory Category,
        bool IsBlocking,
        string TargetRoute,
        int DueInDays,
        bool IsActive = true
    );

    public sealed record StudentDocumentRequest(
        Guid? EnrollmentId,
        string DocumentType,
        StudentDocumentCategory Category,
        StudentDocumentVisibility Visibility,
        DateOnly? ExpiresOn
    );

    public sealed record DocumentValidationRequest(bool Approve, string? Reason);

    public sealed record DocumentShareRequest(StudentDocumentVisibility Visibility);

    public sealed record ApplyStudentBlockRequest(
        string BlockType,
        string Reason,
        string SourceDomain,
        StudentBlockingAction BlockingActions,
        StudentBlockSeverity Severity,
        string ExpectedResolution
    );

    public sealed record ReleaseStudentBlockRequest(
        StudentBlockResolutionType ResolutionType,
        string Reason
    );

    public sealed record OverrideStudentBlockRequest(string Reason, DateTimeOffset UntilUtc);

    public sealed record AssignStudentBranchRequest(
        Guid BranchId,
        StudentBranchAssignmentType Type,
        StudentBranchService ServicesAllowed,
        DateOnly EffectiveFrom,
        DateOnly? EffectiveTo,
        string Reason
    );

    public sealed record AnalyzePrimaryBranchChangeRequest(Guid TargetBranchId);

    public sealed record ChangePrimaryBranchRequest(Guid AnalysisId, string Reason);

    public sealed record AssignStudentInstructorRequest(
        Guid InstructorId,
        StudentInstructorAssignmentType Type,
        DateOnly EffectiveFrom,
        DateOnly? EffectiveTo,
        string TrainingCategory,
        StudentInstructorScope MaximumScope,
        string Reason
    );

    public sealed record ReplacePrimaryInstructorRequest(
        Guid InstructorId,
        DateOnly EffectiveFrom,
        DateOnly? EffectiveTo,
        string TrainingCategory,
        StudentInstructorScope MaximumScope,
        string Reason
    );

    public sealed record AnalyzeInternalTransferRequest(
        Guid TargetBranchId,
        InternalTransferMode Mode,
        InternalTransferElement Elements,
        DateOnly? EffectiveOn,
        DateOnly? TemporaryUntil,
        string Reason
    );

    public sealed record CreateExternalTransferRequest(
        Guid TargetOrganizationId,
        ExternalTransferType Type,
        ExternalTransferDataScope DataScope,
        DateOnly EffectiveOn,
        DateOnly? TemporaryUntil,
        string CountryCode,
        string Reason,
        string Responsibilities
    );

    public sealed record ConsentEvidenceRequest(string EvidenceReference);

    public sealed record ExternalTransferFinanceRequest(
        TransferFinancialStatus Status,
        string? Resolution
    );

    public sealed record SubmitExternalTransferRequest(bool RequestInvitationIfMissing);

    public sealed record ExternalTransferDecisionRequest(bool Accept, string Reason);

    public sealed record SuspendEnrollmentRequest(
        EnrollmentSuspensionReason Reason,
        EnrollmentSuspensionScope Scope,
        DateOnly StartDate,
        DateOnly ExpectedEndDate,
        string ImmediateActions,
        ExistingBookingsDecision BookingsDecision,
        int FutureBookingsCount,
        string CreditDecision,
        string NotificationPlan,
        DateOnly ReviewDate
    );

    public sealed record CreateEnrollmentReactivationRequest(
        Guid SuspensionId,
        EnrollmentReactivationMode Mode,
        DateOnly ResumeDate,
        string Conditions,
        bool PedagogyReviewRequested,
        IReadOnlyList<EnrollmentReactivationCheckRequest> Checks
    );

    public sealed record EnrollmentReactivationCheckRequest(
        ReactivationCheckType Type,
        ReactivationCheckStatus Status,
        string Detail
    );

    public sealed record ReviewReactivationCheckRequest(
        ReactivationCheckStatus Status,
        string Detail
    );

    public sealed record CreateEnrollmentClosureRequest(
        Guid EnrollmentId,
        EnrollmentClosureReason Reason,
        DateOnly ClosureDate,
        string ReasonDetail,
        IReadOnlyList<EnrollmentClosureCheckRequest> Checks
    );

    public sealed record EnrollmentClosureCheckRequest(
        EnrollmentClosureCheckType Type,
        EnrollmentClosureCheckStatus Status,
        string Detail
    );

    public sealed record ReviewEnrollmentClosureCheckRequest(
        EnrollmentClosureCheckStatus Status,
        string Detail
    );

    public sealed record ArchiveStudentRequest(
        DateOnly RetainUntil,
        string RetentionLegalBasis,
        StudentDataRetentionScope RetentionScope
    );

    public sealed record ReopenEnrollmentRequest(string Justification);
}
