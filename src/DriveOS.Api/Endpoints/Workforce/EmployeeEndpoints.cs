using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.Workforce.Application.Employees;
using DriveOS.Modules.Workforce.Application.BranchAssignments;
using DriveOS.Modules.Workforce.Application.JobPositions;
using DriveOS.Modules.Workforce.Application.Qualifications;
using DriveOS.Modules.Workforce.Application.EmploymentContracts;
using DriveOS.Modules.Workforce.Domain.EmploymentContracts;
using DriveOS.Modules.Workforce.Domain.Qualifications;
using DriveOS.Modules.Workforce.Domain.Employees;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
namespace DriveOS.Api.Endpoints.Workforce;
internal static class EmployeeEndpoints
{
    internal static IEndpointRouteBuilder MapEmployeeEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/workforce/employees").WithTags("Workforce - Employees");
        group.MapGet("/", GetEmployees).RequireAuthorization("Workforce.Employees.Read");
        group.MapGet("/{employeeId:guid}", GetEmployee).RequireAuthorization("Workforce.Employees.Read");
        group.MapGet("/{employeeId:guid}/history", GetEmploymentHistory).RequireAuthorization("Workforce.Employees.Read");
        group.MapPost("/", CreateEmployee).RequireAuthorization("Workforce.Employees.Create");
        group.MapPost("/{employeeId:guid}/rehire", RehireEmployee).RequireAuthorization("Workforce.Employees.Rehire");
        group.MapPut("/{employeeId:guid}/identity", UpdateEmployeeIdentity).RequireAuthorization("Workforce.Employees.Update");
        group.MapPost("/{employeeId:guid}/onboarding/start", StartOnboarding).RequireAuthorization("Workforce.Employees.Onboard");
        group.MapPost("/{employeeId:guid}/activate", Activate).RequireAuthorization("Workforce.Employees.Activate");
        group.MapPost("/{employeeId:guid}/suspend", Suspend).RequireAuthorization("Workforce.Employees.Suspend");
        group.MapPost("/{employeeId:guid}/reactivate", Reactivate).RequireAuthorization("Workforce.Employees.Reactivate");
        group.MapPost("/{employeeId:guid}/termination/start", StartTermination).RequireAuthorization("Workforce.Employees.Terminate");
        group.MapPost("/{employeeId:guid}/termination/complete", EndEmployment).RequireAuthorization("Workforce.Employees.Terminate");
        group.MapGet("/{employeeId:guid}/branch-assignments", GetBranchAssignments).RequireAuthorization("Workforce.BranchAssignments.Read");
        group.MapPost("/{employeeId:guid}/branch-assignments", AddBranchAssignment).RequireAuthorization("Workforce.BranchAssignments.Manage");
        group.MapPut("/{employeeId:guid}/branch-assignments/{assignmentId:guid}", UpdateBranchAssignment).RequireAuthorization("Workforce.BranchAssignments.Manage");
        group.MapPost("/{employeeId:guid}/branch-assignments/{assignmentId:guid}/end", EndBranchAssignment).RequireAuthorization("Workforce.BranchAssignments.Manage");
        group.MapPost("/{employeeId:guid}/branch-assignments/{assignmentId:guid}/cancel", CancelBranchAssignment).RequireAuthorization("Workforce.BranchAssignments.Manage");
        group.MapGet("/{employeeId:guid}/job-position-assignments", GetJobPositionAssignments).RequireAuthorization("Workforce.JobPositions.Read");
        group.MapPost("/{employeeId:guid}/job-position-assignments", AddJobPositionAssignment).RequireAuthorization("Workforce.JobPositions.Assign");
        group.MapPut("/{employeeId:guid}/job-position-assignments/{assignmentId:guid}", UpdateJobPositionAssignment).RequireAuthorization("Workforce.JobPositions.Assign");
        group.MapPost("/{employeeId:guid}/job-position-assignments/{assignmentId:guid}/end", EndJobPositionAssignment).RequireAuthorization("Workforce.JobPositions.Assign");
        group.MapPost("/{employeeId:guid}/job-position-assignments/{assignmentId:guid}/cancel", CancelJobPositionAssignment).RequireAuthorization("Workforce.JobPositions.Assign");
        group.MapGet("/{employeeId:guid}/qualifications", GetQualifications).RequireAuthorization("Workforce.Qualifications.Read");
        group.MapPost("/{employeeId:guid}/qualifications", DeclareQualification).RequireAuthorization("Workforce.Qualifications.Manage");
        group.MapPost("/{employeeId:guid}/qualifications/{qualificationId:guid}/verify", VerifyQualification).RequireAuthorization("Workforce.Qualifications.Verify");
        group.MapPost("/{employeeId:guid}/qualifications/{qualificationId:guid}/reject", RejectQualification).RequireAuthorization("Workforce.Qualifications.Verify");
        group.MapGet("/{employeeId:guid}/instructor-authorizations", GetInstructorAuthorizations).RequireAuthorization("Workforce.InstructorAuthorizations.Read");
        group.MapPost("/{employeeId:guid}/instructor-authorizations", DeclareInstructorAuthorization).RequireAuthorization("Workforce.InstructorAuthorizations.Manage");
        group.MapPost("/{employeeId:guid}/instructor-authorizations/{authorizationId:guid}/verify", VerifyInstructorAuthorization).RequireAuthorization("Workforce.InstructorAuthorizations.Verify");
        group.MapPost("/{employeeId:guid}/instructor-authorizations/{authorizationId:guid}/reject", RejectInstructorAuthorization).RequireAuthorization("Workforce.InstructorAuthorizations.Verify");
        group.MapGet("/{employeeId:guid}/employment-contracts", GetEmploymentContracts).RequireAuthorization("Workforce.EmploymentContracts.Read");
        group.MapPost("/{employeeId:guid}/employment-contracts", AddEmploymentContract).RequireAuthorization("Workforce.EmploymentContracts.Manage");
        group.MapPut("/{employeeId:guid}/employment-contracts/{contractId:guid}", UpdateEmploymentContract).RequireAuthorization("Workforce.EmploymentContracts.Manage");
        group.MapPost("/{employeeId:guid}/employment-contracts/{contractId:guid}/document", LinkEmploymentContractDocument).RequireAuthorization("Workforce.EmploymentContracts.Manage");
        group.MapPost("/{employeeId:guid}/employment-contracts/{contractId:guid}/signed", MarkEmploymentContractSigned).RequireAuthorization("Workforce.EmploymentContracts.Sign");
        group.MapPost("/{employeeId:guid}/employment-contracts/{contractId:guid}/activate", ActivateEmploymentContract).RequireAuthorization("Workforce.EmploymentContracts.Manage");
        group.MapPost("/{employeeId:guid}/employment-contracts/{contractId:guid}/terminate", TerminateEmploymentContract).RequireAuthorization("Workforce.EmploymentContracts.Manage");
        group.MapPost("/{employeeId:guid}/employment-contracts/{contractId:guid}/cancel", CancelEmploymentContract).RequireAuthorization("Workforce.EmploymentContracts.Manage");
        return app;
    }
    private static async Task<IResult> GetEmployees(string? status, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } org) return Results.Unauthorized();
        EmploymentStatus? parsed = null;
        if (!string.IsNullOrWhiteSpace(status)) { if (!Enum.TryParse<EmploymentStatus>(status, true, out var value)) return Results.BadRequest(new { code = "Workforce.Employee.InvalidStatus", messageKey = "errors.workforce.employee.invalidStatus" }); parsed = value; }
        Result<IReadOnlyList<EmployeeResponse>> r = await mediator.Send(new GetEmployeesQuery(org, parsed), ct);
        return r.IsSuccess ? Results.Ok(r.Value) : ToProblem(r.Error);
    }
    private static async Task<IResult> GetEmployee(Guid employeeId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } org) return Results.Unauthorized();
        Result<EmployeeResponse> r = await mediator.Send(new GetEmployeeQuery(org, new EmployeeId(employeeId)), ct);
        return r.IsSuccess ? Results.Ok(r.Value) : ToProblem(r.Error);
    }
    private static async Task<IResult> GetEmploymentHistory(Guid employeeId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } org) return Results.Unauthorized();
        Result<IReadOnlyList<EmployeeResponse>> r = await mediator.Send(new GetEmployeeEmploymentHistoryQuery(org, new EmployeeId(employeeId)), ct);
        return r.IsSuccess ? Results.Ok(r.Value) : ToProblem(r.Error);
    }
    private static async Task<IResult> CreateEmployee(CreateEmployeeRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } org || user.UserId is not { } actor) return Results.Unauthorized();
        EmployeeId id = request.EmployeeId is { } raw && raw != Guid.Empty ? new EmployeeId(raw) : EmployeeId.New();
        UserId? linked = request.UserId is { } uid && uid != Guid.Empty ? new UserId(uid) : null;
        Result<EmployeeId> r = await mediator.Send(new CreateEmployeeCommand(org, id, new PersonId(request.PersonId), linked, request.EmployeeNumber, request.EmploymentStartDate, request.EmploymentEndDate, actor), ct);
        return r.IsSuccess ? Results.Created($"/api/workforce/employees/{r.Value.Value}", new { id = r.Value.Value }) : ToProblem(r.Error);
    }
    private static async Task<IResult> RehireEmployee(Guid employeeId, RehireEmployeeRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } org || user.UserId is not { } actor) return Results.Unauthorized();
        EmployeeId newId = request.EmployeeId is { } raw && raw != Guid.Empty ? new EmployeeId(raw) : EmployeeId.New();
        UserId? linked = request.UserId is { } uid && uid != Guid.Empty ? new UserId(uid) : null;
        Result<EmployeeId> r = await mediator.Send(new RehireEmployeeCommand(org, new EmployeeId(employeeId), newId, linked, request.ReusePreviousUserLink, request.EmployeeNumber, request.EmploymentStartDate, request.EmploymentEndDate, actor), ct);
        return r.IsSuccess ? Results.Created($"/api/workforce/employees/{r.Value.Value}", new { id = r.Value.Value, rehiredFromEmployeeId = employeeId }) : ToProblem(r.Error);
    }
    private static async Task<IResult> UpdateEmployeeIdentity(Guid employeeId, UpdateEmployeeIdentityRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } org || user.UserId is not { } actor) return Results.Unauthorized();
        UserId? linked = request.UserId is { } uid && uid != Guid.Empty ? new UserId(uid) : null;
        Result r = await mediator.Send(new UpdateEmployeeIdentityCommand(org, new EmployeeId(employeeId), linked, request.EmployeeNumber, request.EmploymentStartDate, request.EmploymentEndDate, actor), ct);
        return r.IsSuccess ? Results.NoContent() : ToProblem(r.Error);
    }
    private static async Task<IResult> StartOnboarding(Guid employeeId, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } org || user.UserId is not { } actor) return Results.Unauthorized();
        Result r = await mediator.Send(new StartEmployeeOnboardingCommand(org, new EmployeeId(employeeId), actor), ct);
        return r.IsSuccess ? Results.NoContent() : ToProblem(r.Error);
    }

    private static async Task<IResult> Activate(Guid employeeId, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } org || user.UserId is not { } actor) return Results.Unauthorized();
        Result r = await mediator.Send(new ActivateEmployeeCommand(org, new EmployeeId(employeeId), actor), ct);
        return r.IsSuccess ? Results.NoContent() : ToProblem(r.Error);
    }

    private static async Task<IResult> Suspend(Guid employeeId, EmployeeLifecycleReasonRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } org || user.UserId is not { } actor) return Results.Unauthorized();
        Result r = await mediator.Send(new SuspendEmployeeCommand(org, new EmployeeId(employeeId), request.Reason, actor), ct);
        return r.IsSuccess ? Results.NoContent() : ToProblem(r.Error);
    }

    private static async Task<IResult> Reactivate(Guid employeeId, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } org || user.UserId is not { } actor) return Results.Unauthorized();
        Result r = await mediator.Send(new ReactivateEmployeeCommand(org, new EmployeeId(employeeId), actor), ct);
        return r.IsSuccess ? Results.NoContent() : ToProblem(r.Error);
    }

    private static async Task<IResult> StartTermination(Guid employeeId, StartEmploymentTerminationRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } org || user.UserId is not { } actor) return Results.Unauthorized();
        Result r = await mediator.Send(new StartEmploymentTerminationCommand(org, new EmployeeId(employeeId), request.PlannedEndDate, request.Reason, actor), ct);
        return r.IsSuccess ? Results.NoContent() : ToProblem(r.Error);
    }

    private static async Task<IResult> EndEmployment(Guid employeeId, EndEmploymentRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } org || user.UserId is not { } actor) return Results.Unauthorized();
        Result r = await mediator.Send(new EndEmploymentCommand(org, new EmployeeId(employeeId), request.EndDate, request.Reason, actor), ct);
        return r.IsSuccess ? Results.NoContent() : ToProblem(r.Error);
    }


    private static async Task<IResult> GetBranchAssignments(Guid employeeId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } org) return Results.Unauthorized();
        Result<IReadOnlyList<EmployeeBranchAssignmentResponse>> r = await mediator.Send(new GetEmployeeBranchAssignmentsQuery(org, new EmployeeId(employeeId)), ct);
        return r.IsSuccess ? Results.Ok(r.Value) : ToProblem(r.Error);
    }

    private static async Task<IResult> AddBranchAssignment(Guid employeeId, AddEmployeeBranchAssignmentRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } org || user.UserId is not { } actor) return Results.Unauthorized();
        EmployeeBranchAssignmentId assignmentId = request.AssignmentId is { } raw && raw != Guid.Empty ? new EmployeeBranchAssignmentId(raw) : EmployeeBranchAssignmentId.New();
        Result<EmployeeBranchAssignmentId> r = await mediator.Send(new AddEmployeeBranchAssignmentCommand(org, new EmployeeId(employeeId), assignmentId, new BranchId(request.BranchId), request.StartDate, request.EndDate, request.IsPrimary, actor), ct);
        return r.IsSuccess ? Results.Created($"/api/workforce/employees/{employeeId}/branch-assignments/{r.Value.Value}", new { id = r.Value.Value }) : ToProblem(r.Error);
    }

    private static async Task<IResult> UpdateBranchAssignment(Guid employeeId, Guid assignmentId, UpdateEmployeeBranchAssignmentRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } org || user.UserId is not { } actor) return Results.Unauthorized();
        Result r = await mediator.Send(new UpdateEmployeeBranchAssignmentCommand(org, new EmployeeId(employeeId), new EmployeeBranchAssignmentId(assignmentId), request.StartDate, request.EndDate, request.IsPrimary, actor), ct);
        return r.IsSuccess ? Results.NoContent() : ToProblem(r.Error);
    }

    private static async Task<IResult> EndBranchAssignment(Guid employeeId, Guid assignmentId, EndEmployeeBranchAssignmentRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } org || user.UserId is not { } actor) return Results.Unauthorized();
        Result r = await mediator.Send(new EndEmployeeBranchAssignmentCommand(org, new EmployeeId(employeeId), new EmployeeBranchAssignmentId(assignmentId), request.EndDate, actor), ct);
        return r.IsSuccess ? Results.NoContent() : ToProblem(r.Error);
    }

    private static async Task<IResult> CancelBranchAssignment(Guid employeeId, Guid assignmentId, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } org || user.UserId is not { } actor) return Results.Unauthorized();
        Result r = await mediator.Send(new CancelEmployeeBranchAssignmentCommand(org, new EmployeeId(employeeId), new EmployeeBranchAssignmentId(assignmentId), actor), ct);
        return r.IsSuccess ? Results.NoContent() : ToProblem(r.Error);
    }


    private static async Task<IResult> GetJobPositionAssignments(Guid employeeId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } org) return Results.Unauthorized();
        Result<IReadOnlyList<EmployeeJobPositionAssignmentResponse>> r = await mediator.Send(new GetEmployeeJobPositionAssignmentsQuery(org, new EmployeeId(employeeId)), ct);
        return r.IsSuccess ? Results.Ok(r.Value) : ToProblem(r.Error);
    }
    private static async Task<IResult> AddJobPositionAssignment(Guid employeeId, AddEmployeeJobPositionAssignmentRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } org || user.UserId is not { } actor) return Results.Unauthorized();
        EmployeeJobPositionAssignmentId id=request.AssignmentId is { } raw && raw!=Guid.Empty?new EmployeeJobPositionAssignmentId(raw):EmployeeJobPositionAssignmentId.New();
        BranchId? branch=request.BranchId is { } bid && bid!=Guid.Empty?new BranchId(bid):null;
        Result<EmployeeJobPositionAssignmentId> r=await mediator.Send(new AddEmployeeJobPositionAssignmentCommand(org,new EmployeeId(employeeId),id,new JobPositionId(request.JobPositionId),branch,request.StartDate,request.EndDate,request.IsPrimary,actor),ct);
        return r.IsSuccess?Results.Created($"/api/workforce/employees/{employeeId}/job-position-assignments/{r.Value.Value}",new{id=r.Value.Value}):ToProblem(r.Error);
    }
    private static async Task<IResult> UpdateJobPositionAssignment(Guid employeeId,Guid assignmentId,UpdateEmployeeJobPositionAssignmentRequest request,IMediator mediator,ICurrentTenant tenant,ICurrentUser user,CancellationToken ct)
    {if(tenant.OrganizationId is not { } org||user.UserId is not { } actor)return Results.Unauthorized();Result r=await mediator.Send(new UpdateEmployeeJobPositionAssignmentCommand(org,new EmployeeId(employeeId),new EmployeeJobPositionAssignmentId(assignmentId),request.StartDate,request.EndDate,request.IsPrimary,actor),ct);return r.IsSuccess?Results.NoContent():ToProblem(r.Error);}
    private static async Task<IResult> EndJobPositionAssignment(Guid employeeId,Guid assignmentId,EndEmployeeJobPositionAssignmentRequest request,IMediator mediator,ICurrentTenant tenant,ICurrentUser user,CancellationToken ct)
    {if(tenant.OrganizationId is not { } org||user.UserId is not { } actor)return Results.Unauthorized();Result r=await mediator.Send(new EndEmployeeJobPositionAssignmentCommand(org,new EmployeeId(employeeId),new EmployeeJobPositionAssignmentId(assignmentId),request.EndDate,actor),ct);return r.IsSuccess?Results.NoContent():ToProblem(r.Error);}
    private static async Task<IResult> CancelJobPositionAssignment(Guid employeeId,Guid assignmentId,IMediator mediator,ICurrentTenant tenant,ICurrentUser user,CancellationToken ct)
    {if(tenant.OrganizationId is not { } org||user.UserId is not { } actor)return Results.Unauthorized();Result r=await mediator.Send(new CancelEmployeeJobPositionAssignmentCommand(org,new EmployeeId(employeeId),new EmployeeJobPositionAssignmentId(assignmentId),actor),ct);return r.IsSuccess?Results.NoContent():ToProblem(r.Error);}


    private static async Task<IResult> GetQualifications(Guid employeeId,IMediator mediator,ICurrentTenant tenant,CancellationToken ct){if(tenant.OrganizationId is not { } org)return Results.Unauthorized();var r=await mediator.Send(new GetEmployeeQualificationsQuery(org,new EmployeeId(employeeId)),ct);return r.IsSuccess?Results.Ok(r.Value):ToProblem(r.Error);}
    private static async Task<IResult> DeclareQualification(Guid employeeId,DeclareQualificationRequest x,IMediator mediator,ICurrentTenant tenant,ICurrentUser user,CancellationToken ct){if(tenant.OrganizationId is not { } org||user.UserId is not { } actor)return Results.Unauthorized();var id=EmployeeQualificationId.New();var r=await mediator.Send(new DeclareEmployeeQualificationCommand(org,new EmployeeId(employeeId),id,x.CountryCode,x.QualificationType,x.Title,x.Identifier,x.IssuingAuthority,x.IssuedOn,x.ExpiresOn,x.Source,actor),ct);return r.IsSuccess?Results.Created($"/api/workforce/employees/{employeeId}/qualifications/{id.Value}",new{id=id.Value}):ToProblem(r.Error);}
    private static async Task<IResult> VerifyQualification(Guid employeeId,Guid qualificationId,VerifyCredentialRequest x,IMediator mediator,ICurrentTenant tenant,ICurrentUser user,CancellationToken ct){if(tenant.OrganizationId is not { } org||user.UserId is not { } actor)return Results.Unauthorized();var r=await mediator.Send(new VerifyEmployeeQualificationCommand(org,new EmployeeId(employeeId),new EmployeeQualificationId(qualificationId),x.VerificationMethod,x.Reason,actor),ct);return r.IsSuccess?Results.NoContent():ToProblem(r.Error);}
    private static async Task<IResult> RejectQualification(Guid employeeId,Guid qualificationId,RejectCredentialRequest x,IMediator mediator,ICurrentTenant tenant,ICurrentUser user,CancellationToken ct){if(tenant.OrganizationId is not { } org||user.UserId is not { } actor)return Results.Unauthorized();var r=await mediator.Send(new RejectEmployeeQualificationCommand(org,new EmployeeId(employeeId),new EmployeeQualificationId(qualificationId),x.Reason,actor),ct);return r.IsSuccess?Results.NoContent():ToProblem(r.Error);}
    private static async Task<IResult> GetInstructorAuthorizations(Guid employeeId,IMediator mediator,ICurrentTenant tenant,CancellationToken ct){if(tenant.OrganizationId is not { } org)return Results.Unauthorized();var r=await mediator.Send(new GetInstructorAuthorizationsQuery(org,new EmployeeId(employeeId)),ct);return r.IsSuccess?Results.Ok(r.Value):ToProblem(r.Error);}
    private static async Task<IResult> DeclareInstructorAuthorization(Guid employeeId,DeclareInstructorAuthorizationRequest x,IMediator mediator,ICurrentTenant tenant,ICurrentUser user,CancellationToken ct){if(tenant.OrganizationId is not { } org||user.UserId is not { } actor)return Results.Unauthorized();var id=InstructorAuthorizationId.New();var r=await mediator.Send(new DeclareInstructorAuthorizationCommand(org,new EmployeeId(employeeId),id,x.CountryCode,x.AuthorizationType,x.Identifier,x.IssuingAuthority,x.JurisdictionCode,x.LicenseCategoryCode,x.IssuedOn,x.ExpiresOn,x.Source,actor),ct);return r.IsSuccess?Results.Created($"/api/workforce/employees/{employeeId}/instructor-authorizations/{id.Value}",new{id=id.Value}):ToProblem(r.Error);}
    private static async Task<IResult> VerifyInstructorAuthorization(Guid employeeId,Guid authorizationId,VerifyCredentialRequest x,IMediator mediator,ICurrentTenant tenant,ICurrentUser user,CancellationToken ct){if(tenant.OrganizationId is not { } org||user.UserId is not { } actor)return Results.Unauthorized();var r=await mediator.Send(new VerifyInstructorAuthorizationCommand(org,new EmployeeId(employeeId),new InstructorAuthorizationId(authorizationId),x.VerificationMethod,x.Reason,actor),ct);return r.IsSuccess?Results.NoContent():ToProblem(r.Error);}
    private static async Task<IResult> RejectInstructorAuthorization(Guid employeeId,Guid authorizationId,RejectCredentialRequest x,IMediator mediator,ICurrentTenant tenant,ICurrentUser user,CancellationToken ct){if(tenant.OrganizationId is not { } org||user.UserId is not { } actor)return Results.Unauthorized();var r=await mediator.Send(new RejectInstructorAuthorizationCommand(org,new EmployeeId(employeeId),new InstructorAuthorizationId(authorizationId),x.Reason,actor),ct);return r.IsSuccess?Results.NoContent():ToProblem(r.Error);}


    private static async Task<IResult> GetEmploymentContracts(Guid employeeId,IMediator mediator,ICurrentTenant tenant,CancellationToken ct)
    {if(tenant.OrganizationId is not { } org)return Results.Unauthorized();var r=await mediator.Send(new GetEmploymentContractsQuery(org,new EmployeeId(employeeId)),ct);return r.IsSuccess?Results.Ok(r.Value):ToProblem(r.Error);}
    private static async Task<IResult> AddEmploymentContract(Guid employeeId,AddEmploymentContractRequest x,IMediator mediator,ICurrentTenant tenant,ICurrentUser user,CancellationToken ct)
    {if(tenant.OrganizationId is not { } org||user.UserId is not { } actor)return Results.Unauthorized();if(!Enum.TryParse<EmploymentContractType>(x.ContractType,true,out var type))return Results.BadRequest(new{code="Workforce.EmploymentContract.InvalidType",messageKey="errors.workforce.employmentContract.invalidType"});var id=EmploymentContractId.New();JobPositionId? pos=x.PrimaryJobPositionId is { } p&&p!=Guid.Empty?new JobPositionId(p):null;var r=await mediator.Send(new AddEmploymentContractCommand(org,new EmployeeId(employeeId),id,type,x.StartDate,x.EndDate,x.ContractualWeeklyHours,pos,actor),ct);return r.IsSuccess?Results.Created($"/api/workforce/employees/{employeeId}/employment-contracts/{id.Value}",new{id=id.Value}):ToProblem(r.Error);}
    private static async Task<IResult> UpdateEmploymentContract(Guid employeeId,Guid contractId,UpdateEmploymentContractRequest x,IMediator mediator,ICurrentTenant tenant,ICurrentUser user,CancellationToken ct)
    {if(tenant.OrganizationId is not { } org||user.UserId is not { } actor)return Results.Unauthorized();JobPositionId? pos=x.PrimaryJobPositionId is { } p&&p!=Guid.Empty?new JobPositionId(p):null;var r=await mediator.Send(new UpdateEmploymentContractTermsCommand(org,new EmployeeId(employeeId),new EmploymentContractId(contractId),x.StartDate,x.EndDate,x.ContractualWeeklyHours,pos,actor),ct);return r.IsSuccess?Results.NoContent():ToProblem(r.Error);}
    private static async Task<IResult> LinkEmploymentContractDocument(Guid employeeId,Guid contractId,LinkEmploymentContractDocumentRequest x,IMediator mediator,ICurrentTenant tenant,ICurrentUser user,CancellationToken ct)
    {if(tenant.OrganizationId is not { } org||user.UserId is not { } actor)return Results.Unauthorized();SignatureProcessId? sp=x.SignatureProcessId is { } p&&p!=Guid.Empty?new SignatureProcessId(p):null;var r=await mediator.Send(new LinkEmploymentContractDocumentCommand(org,new EmployeeId(employeeId),new EmploymentContractId(contractId),new ContractDocumentId(x.ContractDocumentId),sp,actor),ct);return r.IsSuccess?Results.NoContent():ToProblem(r.Error);}
    private static async Task<IResult> MarkEmploymentContractSigned(Guid employeeId,Guid contractId,MarkEmploymentContractSignedRequest x,IMediator mediator,ICurrentTenant tenant,ICurrentUser user,CancellationToken ct)
    {if(tenant.OrganizationId is not { } org||user.UserId is not { } actor)return Results.Unauthorized();var r=await mediator.Send(new MarkEmploymentContractSignedCommand(org,new EmployeeId(employeeId),new EmploymentContractId(contractId),new SignatureProcessId(x.SignatureProcessId),actor),ct);return r.IsSuccess?Results.NoContent():ToProblem(r.Error);}
    private static async Task<IResult> ActivateEmploymentContract(Guid employeeId,Guid contractId,ActivateEmploymentContractRequest x,IMediator mediator,ICurrentTenant tenant,ICurrentUser user,CancellationToken ct)
    {if(tenant.OrganizationId is not { } org||user.UserId is not { } actor)return Results.Unauthorized();var r=await mediator.Send(new ActivateEmploymentContractCommand(org,new EmployeeId(employeeId),new EmploymentContractId(contractId),x.ActivationDate,actor),ct);return r.IsSuccess?Results.NoContent():ToProblem(r.Error);}
    private static async Task<IResult> TerminateEmploymentContract(Guid employeeId,Guid contractId,TerminateEmploymentContractRequest x,IMediator mediator,ICurrentTenant tenant,ICurrentUser user,CancellationToken ct)
    {if(tenant.OrganizationId is not { } org||user.UserId is not { } actor)return Results.Unauthorized();var r=await mediator.Send(new TerminateEmploymentContractCommand(org,new EmployeeId(employeeId),new EmploymentContractId(contractId),x.EndDate,actor),ct);return r.IsSuccess?Results.NoContent():ToProblem(r.Error);}
    private static async Task<IResult> CancelEmploymentContract(Guid employeeId,Guid contractId,IMediator mediator,ICurrentTenant tenant,ICurrentUser user,CancellationToken ct)
    {if(tenant.OrganizationId is not { } org||user.UserId is not { } actor)return Results.Unauthorized();var r=await mediator.Send(new CancelEmploymentContractCommand(org,new EmployeeId(employeeId),new EmploymentContractId(contractId),actor),ct);return r.IsSuccess?Results.NoContent():ToProblem(r.Error);}

    private static IResult ToProblem(Error error) => Results.Problem(statusCode: error.Type switch { ErrorType.NotFound => 404, ErrorType.Conflict => 409, ErrorType.Validation => 400, _ => 400 }, extensions: new Dictionary<string, object?> { ["code"] = error.Code, ["messageKey"] = error.MessageKey });
}
public sealed record CreateEmployeeRequest(Guid? EmployeeId, Guid PersonId, Guid? UserId, string EmployeeNumber, DateOnly EmploymentStartDate, DateOnly? EmploymentEndDate);
public sealed record RehireEmployeeRequest(Guid? EmployeeId, Guid? UserId, bool ReusePreviousUserLink, string EmployeeNumber, DateOnly EmploymentStartDate, DateOnly? EmploymentEndDate);
public sealed record UpdateEmployeeIdentityRequest(Guid? UserId, string EmployeeNumber, DateOnly EmploymentStartDate, DateOnly? EmploymentEndDate);

public sealed record EmployeeLifecycleReasonRequest(string Reason);
public sealed record StartEmploymentTerminationRequest(DateOnly PlannedEndDate, string Reason);
public sealed record EndEmploymentRequest(DateOnly EndDate, string Reason);

public sealed record AddEmployeeBranchAssignmentRequest(Guid? AssignmentId, Guid BranchId, DateOnly StartDate, DateOnly? EndDate, bool IsPrimary);
public sealed record UpdateEmployeeBranchAssignmentRequest(DateOnly StartDate, DateOnly? EndDate, bool IsPrimary);
public sealed record EndEmployeeBranchAssignmentRequest(DateOnly EndDate);

public sealed record AddEmployeeJobPositionAssignmentRequest(Guid? AssignmentId, Guid JobPositionId, Guid? BranchId, DateOnly StartDate, DateOnly? EndDate, bool IsPrimary);
public sealed record UpdateEmployeeJobPositionAssignmentRequest(DateOnly StartDate, DateOnly? EndDate, bool IsPrimary);
public sealed record EndEmployeeJobPositionAssignmentRequest(DateOnly EndDate);

public sealed record DeclareQualificationRequest(string CountryCode,string QualificationType,string Title,string? Identifier,string? IssuingAuthority,DateOnly? IssuedOn,DateOnly? ExpiresOn,QualificationSource Source);
public sealed record DeclareInstructorAuthorizationRequest(string CountryCode,string AuthorizationType,string Identifier,string IssuingAuthority,string? JurisdictionCode,string LicenseCategoryCode,DateOnly? IssuedOn,DateOnly? ExpiresOn,QualificationSource Source);
public sealed record VerifyCredentialRequest(string VerificationMethod,string? Reason);
public sealed record RejectCredentialRequest(string Reason);

public sealed record AddEmploymentContractRequest(string ContractType,DateOnly StartDate,DateOnly? EndDate,decimal? ContractualWeeklyHours,Guid? PrimaryJobPositionId);
public sealed record UpdateEmploymentContractRequest(DateOnly StartDate,DateOnly? EndDate,decimal? ContractualWeeklyHours,Guid? PrimaryJobPositionId);
public sealed record LinkEmploymentContractDocumentRequest(Guid ContractDocumentId,Guid? SignatureProcessId);
public sealed record MarkEmploymentContractSignedRequest(Guid SignatureProcessId);
public sealed record ActivateEmploymentContractRequest(DateOnly ActivationDate);
public sealed record TerminateEmploymentContractRequest(DateOnly EndDate);
