using DriveOS.Modules.Workforce.Domain.Events;
using DriveOS.Modules.Workforce.Domain.BranchAssignments;
using DriveOS.Modules.Workforce.Domain.JobPositions;
using DriveOS.Modules.Workforce.Domain.Qualifications;
using DriveOS.Modules.Workforce.Domain.EmploymentContracts;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.Workforce.Domain.Employees;

/// <summary>
/// Authoritative Workforce aggregate for the employment relationship between one person and one employer organization.
/// It does not own authentication, security roles, branch assignments, job positions, qualifications or contract documents.
/// A rehire creates a new Employee aggregate; historical employment is never overwritten.
/// </summary>
public sealed class Employee : AggregateRoot<EmployeeId>, IAuditableEntity
{
    private readonly List<EmployeeBranchAssignment> _branchAssignments = [];
    private readonly List<EmployeeJobPositionAssignment> _jobPositionAssignments = [];
    private readonly List<EmployeeQualification> _qualifications = [];
    private readonly List<InstructorAuthorization> _instructorAuthorizations = [];
    private readonly List<EmploymentContract> _employmentContracts = [];

    private Employee() { }
    private Employee(EmployeeId id, OrganizationId employerOrganizationId, PersonId personId, UserId? userId, string employeeNumber, DateOnly employmentStartDate, DateOnly? employmentEndDate, DateTimeOffset nowUtc) : base(id)
    {
        EmployerOrganizationId = employerOrganizationId;
        PersonId = personId;
        UserId = userId;
        EmployeeNumber = NormalizeEmployeeNumber(employeeNumber);
        EmploymentStartDate = employmentStartDate;
        EmploymentEndDate = employmentEndDate;
        Status = EmploymentStatus.Draft;
        RehiredFromEmployeeId = null;
        RaiseDomainEvent(new EmployeeCreatedDomainEvent(Guid.NewGuid(), nowUtc.ToUniversalTime(), Id, EmployerOrganizationId, PersonId));
    }
    public OrganizationId EmployerOrganizationId { get; private set; }
    public PersonId PersonId { get; private set; }
    /// <summary>Optional authentication-account link. Ending employment must not delete the global account.</summary>
    public UserId? UserId { get; private set; }
    public string EmployeeNumber { get; private set; } = string.Empty;
    public DateOnly EmploymentStartDate { get; private set; }
    public DateOnly? EmploymentEndDate { get; private set; }
    public EmploymentStatus Status { get; private set; }
    /// <summary>Previous employment period when this aggregate was created through a rehire. Historical aggregates are never reactivated.</summary>
    public EmployeeId? RehiredFromEmployeeId { get; private set; }
    public IReadOnlyCollection<EmployeeBranchAssignment> BranchAssignments => _branchAssignments;
    public IReadOnlyCollection<EmployeeJobPositionAssignment> JobPositionAssignments => _jobPositionAssignments;
    public IReadOnlyCollection<EmployeeQualification> Qualifications => _qualifications;
    public IReadOnlyCollection<InstructorAuthorization> InstructorAuthorizations => _instructorAuthorizations;
    public IReadOnlyCollection<EmploymentContract> EmploymentContracts => _employmentContracts;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<Employee> RehireFrom(Employee previousEmployment, EmployeeId newEmployeeId, UserId? userId, string employeeNumber, DateOnly employmentStartDate, DateOnly? employmentEndDate, DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (previousEmployment.Status != EmploymentStatus.Ended || !previousEmployment.EmploymentEndDate.HasValue)
            return Result.Failure<Employee>(EmployeeErrors.RehireRequiresEndedEmployment);
        if (employmentStartDate <= previousEmployment.EmploymentEndDate.Value)
            return Result.Failure<Employee>(EmployeeErrors.RehireMustStartAfterPreviousEmployment);

        Result<Employee> created = Create(newEmployeeId, previousEmployment.EmployerOrganizationId, previousEmployment.PersonId, userId, employeeNumber, employmentStartDate, employmentEndDate, nowUtc);
        if (created.IsFailure) return created;

        created.Value.RehiredFromEmployeeId = previousEmployment.Id;
        created.Value.RaiseDomainEvent(new EmployeeRehiredDomainEvent(
            Guid.NewGuid(), nowUtc.ToUniversalTime(), previousEmployment.Id, created.Value.Id, previousEmployment.EmployerOrganizationId,
            previousEmployment.PersonId, userId, employmentStartDate, actorUserId));
        return created;
    }

    public static Result<Employee> Create(EmployeeId id, OrganizationId employerOrganizationId, PersonId personId, UserId? userId, string employeeNumber, DateOnly employmentStartDate, DateOnly? employmentEndDate, DateTimeOffset nowUtc)
    {
        if (id.IsEmpty) return Result.Failure<Employee>(EmployeeErrors.InvalidIdentifier);
        if (employerOrganizationId.IsEmpty) return Result.Failure<Employee>(EmployeeErrors.InvalidEmployer);
        if (personId.IsEmpty) return Result.Failure<Employee>(EmployeeErrors.PersonRequired);
        if (string.IsNullOrWhiteSpace(employeeNumber)) return Result.Failure<Employee>(EmployeeErrors.EmployeeNumberRequired);
        if (employeeNumber.Trim().Length > 64) return Result.Failure<Employee>(EmployeeErrors.EmployeeNumberTooLong);
        if (employmentEndDate.HasValue && employmentEndDate.Value < employmentStartDate) return Result.Failure<Employee>(EmployeeErrors.InvalidEmploymentPeriod);
        return Result.Success(new Employee(id, employerOrganizationId, personId, userId, employeeNumber, employmentStartDate, employmentEndDate, nowUtc));
    }

    public Result UpdateIdentity(UserId? userId, string employeeNumber, DateOnly employmentStartDate, DateOnly? employmentEndDate, DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (Status == EmploymentStatus.Ended) return Result.Failure(EmployeeErrors.EndedEmploymentImmutable);
        if (string.IsNullOrWhiteSpace(employeeNumber)) return Result.Failure(EmployeeErrors.EmployeeNumberRequired);
        if (employeeNumber.Trim().Length > 64) return Result.Failure(EmployeeErrors.EmployeeNumberTooLong);
        if (employmentEndDate.HasValue && employmentEndDate.Value < employmentStartDate) return Result.Failure(EmployeeErrors.InvalidEmploymentPeriod);
        UserId = userId;
        EmployeeNumber = NormalizeEmployeeNumber(employeeNumber);
        EmploymentStartDate = employmentStartDate;
        EmploymentEndDate = employmentEndDate;
        SetModifiedAudit(nowUtc, actorUserId);
        RaiseDomainEvent(new EmployeeIdentityUpdatedDomainEvent(Guid.NewGuid(), nowUtc.ToUniversalTime(), Id, EmployerOrganizationId));
        return Result.Success();
    }
    /// <summary>Moves a draft employment relationship into the onboarding phase.</summary>
    public Result StartOnboarding(DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (Status != EmploymentStatus.Draft)
            return Result.Failure(EmployeeErrors.InvalidLifecycleTransition);

        Status = EmploymentStatus.Onboarding;
        SetModifiedAudit(nowUtc, actorUserId);
        RaiseDomainEvent(new EmployeeOnboardingStartedDomainEvent(
            Guid.NewGuid(), nowUtc.ToUniversalTime(), Id, EmployerOrganizationId, actorUserId));

        return Result.Success();
    }

    /// <summary>Activates an onboarded employee. Access provisioning is intentionally handled outside Workforce.</summary>
    public Result Activate(DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (Status != EmploymentStatus.Onboarding)
            return Result.Failure(EmployeeErrors.InvalidLifecycleTransition);

        Status = EmploymentStatus.Active;
        SetModifiedAudit(nowUtc, actorUserId);
        RaiseDomainEvent(new EmployeeActivatedDomainEvent(
            Guid.NewGuid(), nowUtc.ToUniversalTime(), Id, EmployerOrganizationId, UserId, actorUserId));

        return Result.Success();
    }

    /// <summary>Temporarily suspends the employment relationship without ending it.</summary>
    public Result Suspend(string reason, DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (Status != EmploymentStatus.Active)
            return Result.Failure(EmployeeErrors.InvalidLifecycleTransition);
        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(EmployeeErrors.LifecycleReasonRequired);

        Status = EmploymentStatus.Suspended;
        SetModifiedAudit(nowUtc, actorUserId);
        RaiseDomainEvent(new EmployeeSuspendedDomainEvent(
            Guid.NewGuid(), nowUtc.ToUniversalTime(), Id, EmployerOrganizationId, reason.Trim(), actorUserId));

        return Result.Success();
    }

    /// <summary>Returns a suspended employee to active employment.</summary>
    public Result Reactivate(DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (Status != EmploymentStatus.Suspended)
            return Result.Failure(EmployeeErrors.InvalidLifecycleTransition);

        Status = EmploymentStatus.Active;
        SetModifiedAudit(nowUtc, actorUserId);
        RaiseDomainEvent(new EmployeeReactivatedDomainEvent(
            Guid.NewGuid(), nowUtc.ToUniversalTime(), Id, EmployerOrganizationId, UserId, actorUserId));

        return Result.Success();
    }

    /// <summary>Starts an orderly employment termination. The relationship remains current until EndEmployment is completed.</summary>
    public Result StartTermination(DateOnly plannedEndDate, string reason, DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (Status is not (EmploymentStatus.Active or EmploymentStatus.Suspended or EmploymentStatus.OnLeave))
            return Result.Failure(EmployeeErrors.InvalidLifecycleTransition);
        if (plannedEndDate < EmploymentStartDate)
            return Result.Failure(EmployeeErrors.InvalidTerminationDate);
        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(EmployeeErrors.LifecycleReasonRequired);

        EmploymentEndDate = plannedEndDate;
        Status = EmploymentStatus.Ending;
        SetModifiedAudit(nowUtc, actorUserId);
        RaiseDomainEvent(new EmploymentTerminationStartedDomainEvent(
            Guid.NewGuid(), nowUtc.ToUniversalTime(), Id, EmployerOrganizationId, plannedEndDate, reason.Trim(), actorUserId));

        return Result.Success();
    }

    /// <summary>Closes the employment relationship permanently. Rehire must create a new Employee aggregate.</summary>
    public Result EndEmployment(DateOnly endDate, string reason, DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (Status != EmploymentStatus.Ending)
            return Result.Failure(EmployeeErrors.InvalidLifecycleTransition);
        if (endDate < EmploymentStartDate)
            return Result.Failure(EmployeeErrors.InvalidTerminationDate);
        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(EmployeeErrors.LifecycleReasonRequired);

        EmploymentEndDate = endDate;
        Status = EmploymentStatus.Ended;
        SetModifiedAudit(nowUtc, actorUserId);
        RaiseDomainEvent(new EmploymentEndedDomainEvent(
            Guid.NewGuid(), nowUtc.ToUniversalTime(), Id, EmployerOrganizationId, endDate, UserId, reason.Trim(), actorUserId));

        return Result.Success();
    }


    public Result<EmployeeBranchAssignmentId> AddBranchAssignment(EmployeeBranchAssignmentId assignmentId, BranchId branchId, DateOnly startDate, DateOnly? endDate, bool isPrimary, DateOnly today, DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (Status == EmploymentStatus.Ended)
            return Result.Failure<EmployeeBranchAssignmentId>(EmployeeBranchAssignmentErrors.EmployeeEnded);

        if (HasSameBranchOverlap(branchId, startDate, endDate, null))
            return Result.Failure<EmployeeBranchAssignmentId>(EmployeeBranchAssignmentErrors.SameBranchPeriodOverlap);
        if (isPrimary && HasPrimaryOverlap(startDate, endDate, null))
            return Result.Failure<EmployeeBranchAssignmentId>(EmployeeBranchAssignmentErrors.PrimaryPeriodOverlap);

        Result<EmployeeBranchAssignment> created = EmployeeBranchAssignment.Create(assignmentId, branchId, startDate, endDate, isPrimary, today, nowUtc, actorUserId);
        if (created.IsFailure) return Result.Failure<EmployeeBranchAssignmentId>(created.Error);

        _branchAssignments.Add(created.Value);
        SetModifiedAudit(nowUtc, actorUserId);
        RaiseDomainEvent(new EmployeeBranchAssignedDomainEvent(Guid.NewGuid(), nowUtc.ToUniversalTime(), Id, EmployerOrganizationId, assignmentId, branchId, startDate, endDate, isPrimary, actorUserId));
        return Result.Success(assignmentId);
    }

    public Result UpdateBranchAssignment(EmployeeBranchAssignmentId assignmentId, DateOnly startDate, DateOnly? endDate, bool isPrimary, DateOnly today, DateTimeOffset nowUtc, UserId actorUserId)
    {
        EmployeeBranchAssignment? assignment = _branchAssignments.SingleOrDefault(x => x.Id == assignmentId);
        if (assignment is null) return Result.Failure(EmployeeBranchAssignmentErrors.NotFound);
        if (HasSameBranchOverlap(assignment.BranchId, startDate, endDate, assignmentId)) return Result.Failure(EmployeeBranchAssignmentErrors.SameBranchPeriodOverlap);
        if (isPrimary && HasPrimaryOverlap(startDate, endDate, assignmentId)) return Result.Failure(EmployeeBranchAssignmentErrors.PrimaryPeriodOverlap);
        if (WouldOrphanJobPositionAssignments(assignment.BranchId, startDate, endDate)) return Result.Failure(EmployeeBranchAssignmentErrors.JobPositionDependsOnAssignment);

        Result updated = assignment.Update(startDate, endDate, isPrimary, today, nowUtc, actorUserId);
        if (updated.IsFailure) return updated;
        SetModifiedAudit(nowUtc, actorUserId);
        RaiseDomainEvent(new EmployeeBranchAssignmentUpdatedDomainEvent(Guid.NewGuid(), nowUtc.ToUniversalTime(), Id, EmployerOrganizationId, assignmentId, assignment.BranchId, startDate, endDate, isPrimary, actorUserId));
        return Result.Success();
    }

    public Result EndBranchAssignment(EmployeeBranchAssignmentId assignmentId, DateOnly endDate, DateOnly today, DateTimeOffset nowUtc, UserId actorUserId)
    {
        EmployeeBranchAssignment? assignment = _branchAssignments.SingleOrDefault(x => x.Id == assignmentId);
        if (assignment is null) return Result.Failure(EmployeeBranchAssignmentErrors.NotFound);
        if (WouldOrphanJobPositionAssignments(assignment.BranchId, assignment.StartDate, endDate)) return Result.Failure(EmployeeBranchAssignmentErrors.JobPositionDependsOnAssignment);
        Result ended = assignment.End(endDate, today, nowUtc, actorUserId);
        if (ended.IsFailure) return ended;
        SetModifiedAudit(nowUtc, actorUserId);
        RaiseDomainEvent(new EmployeeBranchAssignmentEndedDomainEvent(Guid.NewGuid(), nowUtc.ToUniversalTime(), Id, EmployerOrganizationId, assignmentId, assignment.BranchId, endDate, actorUserId));
        return Result.Success();
    }

    public Result CancelBranchAssignment(EmployeeBranchAssignmentId assignmentId, DateTimeOffset nowUtc, UserId actorUserId)
    {
        EmployeeBranchAssignment? assignment = _branchAssignments.SingleOrDefault(x => x.Id == assignmentId);
        if (assignment is null) return Result.Failure(EmployeeBranchAssignmentErrors.NotFound);
        if (_jobPositionAssignments.Any(x => x.BranchId == assignment.BranchId && x.Status != EmployeeJobPositionAssignmentStatus.Cancelled)) return Result.Failure(EmployeeBranchAssignmentErrors.JobPositionDependsOnAssignment);
        Result cancelled = assignment.Cancel(nowUtc, actorUserId);
        if (cancelled.IsFailure) return cancelled;
        SetModifiedAudit(nowUtc, actorUserId);
        RaiseDomainEvent(new EmployeeBranchAssignmentCancelledDomainEvent(Guid.NewGuid(), nowUtc.ToUniversalTime(), Id, EmployerOrganizationId, assignmentId, assignment.BranchId, actorUserId));
        return Result.Success();
    }


    public Result<EmployeeJobPositionAssignmentId> AddJobPositionAssignment(EmployeeJobPositionAssignmentId assignmentId, JobPositionId jobPositionId, BranchId? branchId, DateOnly startDate, DateOnly? endDate, bool isPrimary, DateOnly today, DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (Status == EmploymentStatus.Ended) return Result.Failure<EmployeeJobPositionAssignmentId>(EmployeeJobPositionAssignmentErrors.EmployeeEnded);
        if (HasJobPositionOverlap(jobPositionId, branchId, startDate, endDate, null)) return Result.Failure<EmployeeJobPositionAssignmentId>(EmployeeJobPositionAssignmentErrors.PeriodOverlap);
        if (isPrimary && HasPrimaryJobPositionOverlap(startDate, endDate, null)) return Result.Failure<EmployeeJobPositionAssignmentId>(EmployeeJobPositionAssignmentErrors.PrimaryPeriodOverlap);
        if (branchId is { } scopedBranch && !HasBranchCoverage(scopedBranch, startDate, endDate)) return Result.Failure<EmployeeJobPositionAssignmentId>(EmployeeJobPositionAssignmentErrors.BranchAssignmentRequired);

        Result<EmployeeJobPositionAssignment> created = EmployeeJobPositionAssignment.Create(assignmentId, jobPositionId, branchId, startDate, endDate, isPrimary, today, nowUtc, actorUserId);
        if (created.IsFailure) return Result.Failure<EmployeeJobPositionAssignmentId>(created.Error);
        _jobPositionAssignments.Add(created.Value);
        SetModifiedAudit(nowUtc, actorUserId);
        RaiseDomainEvent(new EmployeeJobPositionAssignedDomainEvent(Guid.NewGuid(), nowUtc.ToUniversalTime(), Id, EmployerOrganizationId, assignmentId, jobPositionId, branchId, startDate, endDate, isPrimary, actorUserId));
        return Result.Success(assignmentId);
    }

    public Result UpdateJobPositionAssignment(EmployeeJobPositionAssignmentId assignmentId, DateOnly startDate, DateOnly? endDate, bool isPrimary, DateOnly today, DateTimeOffset nowUtc, UserId actorUserId)
    {
        EmployeeJobPositionAssignment? assignment = _jobPositionAssignments.SingleOrDefault(x => x.Id == assignmentId);
        if (assignment is null) return Result.Failure(EmployeeJobPositionAssignmentErrors.NotFound);
        if (HasJobPositionOverlap(assignment.JobPositionId, assignment.BranchId, startDate, endDate, assignmentId)) return Result.Failure(EmployeeJobPositionAssignmentErrors.PeriodOverlap);
        if (isPrimary && HasPrimaryJobPositionOverlap(startDate, endDate, assignmentId)) return Result.Failure(EmployeeJobPositionAssignmentErrors.PrimaryPeriodOverlap);
        if (assignment.BranchId is { } scopedBranch && !HasBranchCoverage(scopedBranch, startDate, endDate)) return Result.Failure(EmployeeJobPositionAssignmentErrors.BranchAssignmentRequired);
        Result updated = assignment.Update(startDate, endDate, isPrimary, today, nowUtc, actorUserId);
        if (updated.IsFailure) return updated;
        SetModifiedAudit(nowUtc, actorUserId);
        RaiseDomainEvent(new EmployeeJobPositionAssignmentUpdatedDomainEvent(Guid.NewGuid(), nowUtc.ToUniversalTime(), Id, EmployerOrganizationId, assignmentId, assignment.JobPositionId, assignment.BranchId, startDate, endDate, isPrimary, actorUserId));
        return Result.Success();
    }

    public Result EndJobPositionAssignment(EmployeeJobPositionAssignmentId assignmentId, DateOnly endDate, DateTimeOffset nowUtc, UserId actorUserId)
    {
        EmployeeJobPositionAssignment? assignment = _jobPositionAssignments.SingleOrDefault(x => x.Id == assignmentId);
        if (assignment is null) return Result.Failure(EmployeeJobPositionAssignmentErrors.NotFound);
        Result ended = assignment.End(endDate, DateOnly.FromDateTime(nowUtc.UtcDateTime), nowUtc, actorUserId);
        if (ended.IsFailure) return ended;
        SetModifiedAudit(nowUtc, actorUserId);
        RaiseDomainEvent(new EmployeeJobPositionAssignmentEndedDomainEvent(Guid.NewGuid(), nowUtc.ToUniversalTime(), Id, EmployerOrganizationId, assignmentId, assignment.JobPositionId, endDate, actorUserId));
        return Result.Success();
    }

    public Result CancelJobPositionAssignment(EmployeeJobPositionAssignmentId assignmentId, DateTimeOffset nowUtc, UserId actorUserId)
    {
        EmployeeJobPositionAssignment? assignment = _jobPositionAssignments.SingleOrDefault(x => x.Id == assignmentId);
        if (assignment is null) return Result.Failure(EmployeeJobPositionAssignmentErrors.NotFound);
        Result cancelled = assignment.Cancel(nowUtc, actorUserId);
        if (cancelled.IsFailure) return cancelled;
        SetModifiedAudit(nowUtc, actorUserId);
        RaiseDomainEvent(new EmployeeJobPositionAssignmentCancelledDomainEvent(Guid.NewGuid(), nowUtc.ToUniversalTime(), Id, EmployerOrganizationId, assignmentId, assignment.JobPositionId, actorUserId));
        return Result.Success();
    }



    public Result<EmployeeQualificationId> DeclareQualification(EmployeeQualificationId id, string countryCode, string qualificationType, string title, string? identifier, string? issuingAuthority, DateOnly? issuedOn, DateOnly? expiresOn, QualificationSource source, DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (Status == EmploymentStatus.Ended) return Result.Failure<EmployeeQualificationId>(QualificationErrors.EmployeeNotEligible);
        Result<EmployeeQualification> created = EmployeeQualification.Declare(id, countryCode, qualificationType, title, identifier, issuingAuthority, issuedOn, expiresOn, source, nowUtc, actorUserId);
        if (created.IsFailure) return Result.Failure<EmployeeQualificationId>(created.Error);
        EmployeeQualification? current = _qualifications.FirstOrDefault(x => x.CountryCode == created.Value.CountryCode && x.QualificationType == created.Value.QualificationType && x.Status is EmployeeQualificationStatus.Declared or EmployeeQualificationStatus.Verified);
        if (current is not null) current.Supersede(id);
        _qualifications.Add(created.Value);
        SetModifiedAudit(nowUtc, actorUserId);
        return Result.Success(id);
    }

    public Result VerifyQualification(EmployeeQualificationId id, string method, string? reason, DateTimeOffset nowUtc, UserId actorUserId)
    {
        EmployeeQualification? q = _qualifications.SingleOrDefault(x => x.Id == id);
        if (q is null) return Result.Failure(QualificationErrors.NotFound);
        Result r = q.Verify(method, reason, nowUtc, actorUserId); if (r.IsFailure) return r; SetModifiedAudit(nowUtc, actorUserId); return Result.Success();
    }

    public Result RejectQualification(EmployeeQualificationId id, string reason, DateTimeOffset nowUtc, UserId actorUserId)
    {
        EmployeeQualification? q = _qualifications.SingleOrDefault(x => x.Id == id);
        if (q is null) return Result.Failure(QualificationErrors.NotFound);
        Result r = q.Reject(reason, nowUtc, actorUserId); if (r.IsFailure) return r; SetModifiedAudit(nowUtc, actorUserId); return Result.Success();
    }

    public Result<InstructorAuthorizationId> DeclareInstructorAuthorization(InstructorAuthorizationId id, string countryCode, string authorizationType, string identifier, string issuingAuthority, string? jurisdictionCode, string licenseCategoryCode, DateOnly? issuedOn, DateOnly? expiresOn, QualificationSource source, DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (Status is not (EmploymentStatus.Active or EmploymentStatus.Onboarding or EmploymentStatus.Suspended)) return Result.Failure<InstructorAuthorizationId>(QualificationErrors.EmployeeNotEligible);
        Result<InstructorAuthorization> created = InstructorAuthorization.Declare(id, countryCode, authorizationType, identifier, issuingAuthority, jurisdictionCode, licenseCategoryCode, issuedOn, expiresOn, source, nowUtc, actorUserId);
        if (created.IsFailure) return Result.Failure<InstructorAuthorizationId>(created.Error);
        InstructorAuthorization? current = _instructorAuthorizations.FirstOrDefault(x => x.CountryCode == created.Value.CountryCode && x.AuthorizationType == created.Value.AuthorizationType && x.LicenseCategoryCode == created.Value.LicenseCategoryCode && x.Status is EmployeeQualificationStatus.Declared or EmployeeQualificationStatus.Verified);
        if (current is not null) current.Supersede(id);
        _instructorAuthorizations.Add(created.Value);
        SetModifiedAudit(nowUtc, actorUserId);
        return Result.Success(id);
    }

    public Result VerifyInstructorAuthorization(InstructorAuthorizationId id, string method, string? reason, DateTimeOffset nowUtc, UserId actorUserId)
    {
        InstructorAuthorization? a = _instructorAuthorizations.SingleOrDefault(x => x.Id == id); if (a is null) return Result.Failure(QualificationErrors.NotFound); Result r=a.Verify(method, reason, nowUtc, actorUserId); if(r.IsFailure)return r; SetModifiedAudit(nowUtc,actorUserId); return Result.Success();
    }
    public Result RejectInstructorAuthorization(InstructorAuthorizationId id, string reason, DateTimeOffset nowUtc, UserId actorUserId)
    {
        InstructorAuthorization? a = _instructorAuthorizations.SingleOrDefault(x => x.Id == id); if (a is null) return Result.Failure(QualificationErrors.NotFound); Result r=a.Reject(reason, nowUtc, actorUserId); if(r.IsFailure)return r; SetModifiedAudit(nowUtc,actorUserId); return Result.Success();
    }


    public Result<EmploymentContractId> AddEmploymentContract(EmploymentContractId contractId, EmploymentContractType contractType, DateOnly startDate, DateOnly? endDate, decimal? contractualWeeklyHours, JobPositionId? primaryJobPositionId, DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (Status == EmploymentStatus.Ended) return Result.Failure<EmploymentContractId>(EmploymentContractErrors.EmployeeEnded);
        if (_employmentContracts.Any(x => x.Status is not EmploymentContractStatus.Cancelled and not EmploymentContractStatus.Terminated and not EmploymentContractStatus.Completed && PeriodsOverlap(x.StartDate, x.EndDate, startDate, endDate)))
            return Result.Failure<EmploymentContractId>(EmploymentContractErrors.PeriodOverlap);
        var created = EmploymentContract.Create(contractId, contractType, startDate, endDate, contractualWeeklyHours, primaryJobPositionId, nowUtc, actorUserId);
        if (created.IsFailure) return Result.Failure<EmploymentContractId>(created.Error);
        _employmentContracts.Add(created.Value); SetModifiedAudit(nowUtc, actorUserId); return Result.Success(contractId);
    }

    public Result UpdateEmploymentContractTerms(EmploymentContractId contractId, DateOnly startDate, DateOnly? endDate, decimal? contractualWeeklyHours, JobPositionId? primaryJobPositionId, DateTimeOffset nowUtc, UserId actorUserId)
    {
        var contract = _employmentContracts.SingleOrDefault(x => x.Id == contractId);
        if (contract is null) return Result.Failure(EmploymentContractErrors.NotFound);
        if (_employmentContracts.Any(x => x.Id != contractId && x.Status is not EmploymentContractStatus.Cancelled and not EmploymentContractStatus.Terminated and not EmploymentContractStatus.Completed && PeriodsOverlap(x.StartDate, x.EndDate, startDate, endDate)))
            return Result.Failure(EmploymentContractErrors.PeriodOverlap);
        var r=contract.UpdateTerms(startDate,endDate,contractualWeeklyHours,primaryJobPositionId,nowUtc,actorUserId); if(r.IsSuccess)SetModifiedAudit(nowUtc,actorUserId); return r;
    }

    public Result LinkEmploymentContractDocument(EmploymentContractId contractId, ContractDocumentId documentId, SignatureProcessId? signatureProcessId, DateTimeOffset nowUtc, UserId actorUserId)
    { var c=_employmentContracts.SingleOrDefault(x=>x.Id==contractId); if(c is null)return Result.Failure(EmploymentContractErrors.NotFound);var r=c.LinkDocument(documentId,signatureProcessId,nowUtc,actorUserId);if(r.IsSuccess)SetModifiedAudit(nowUtc,actorUserId);return r; }
    public Result MarkEmploymentContractSigned(EmploymentContractId contractId, SignatureProcessId signatureProcessId, DateTimeOffset nowUtc, UserId actorUserId)
    { var c=_employmentContracts.SingleOrDefault(x=>x.Id==contractId); if(c is null)return Result.Failure(EmploymentContractErrors.NotFound);var r=c.MarkSigned(signatureProcessId,nowUtc,actorUserId);if(r.IsSuccess)SetModifiedAudit(nowUtc,actorUserId);return r; }
    public Result ActivateEmploymentContract(EmploymentContractId contractId, DateOnly atDate, DateTimeOffset nowUtc, UserId actorUserId)
    { var c=_employmentContracts.SingleOrDefault(x=>x.Id==contractId); if(c is null)return Result.Failure(EmploymentContractErrors.NotFound);var r=c.Activate(atDate,nowUtc,actorUserId);if(r.IsSuccess)SetModifiedAudit(nowUtc,actorUserId);return r; }
    public Result TerminateEmploymentContract(EmploymentContractId contractId, DateOnly endDate, DateTimeOffset nowUtc, UserId actorUserId)
    { var c=_employmentContracts.SingleOrDefault(x=>x.Id==contractId); if(c is null)return Result.Failure(EmploymentContractErrors.NotFound);var r=c.Terminate(endDate,nowUtc,actorUserId);if(r.IsSuccess)SetModifiedAudit(nowUtc,actorUserId);return r; }
    public Result CancelEmploymentContract(EmploymentContractId contractId, DateTimeOffset nowUtc, UserId actorUserId)
    { var c=_employmentContracts.SingleOrDefault(x=>x.Id==contractId); if(c is null)return Result.Failure(EmploymentContractErrors.NotFound);var r=c.Cancel(nowUtc,actorUserId);if(r.IsSuccess)SetModifiedAudit(nowUtc,actorUserId);return r; }

    public InstructorAuthorization? ResolveInstructorAuthorization(string countryCode, string authorizationType, string licenseCategoryCode, DateOnly atDate)
        => _instructorAuthorizations.Where(x => x.CountryCode == EmployeeQualification.NormalizeToken(countryCode) && x.AuthorizationType == EmployeeQualification.NormalizeToken(authorizationType) && x.LicenseCategoryCode == EmployeeQualification.NormalizeToken(licenseCategoryCode) && x.IsVerifiedAt(atDate)).OrderByDescending(x => x.IssuedOn).FirstOrDefault();

    private bool HasJobPositionOverlap(JobPositionId jobPositionId, BranchId? branchId, DateOnly startDate, DateOnly? endDate, EmployeeJobPositionAssignmentId? ignoredId)
        => _jobPositionAssignments.Any(x => x.Id != ignoredId && x.JobPositionId == jobPositionId && x.BranchId == branchId && x.Status != EmployeeJobPositionAssignmentStatus.Cancelled && PeriodsOverlap(x.StartDate, x.EndDate, startDate, endDate));

    private bool HasPrimaryJobPositionOverlap(DateOnly startDate, DateOnly? endDate, EmployeeJobPositionAssignmentId? ignoredId)
        => _jobPositionAssignments.Any(x => x.Id != ignoredId && x.IsPrimary && x.Status != EmployeeJobPositionAssignmentStatus.Cancelled && PeriodsOverlap(x.StartDate, x.EndDate, startDate, endDate));

    private bool HasBranchCoverage(BranchId branchId, DateOnly startDate, DateOnly? endDate)
        => _branchAssignments.Any(x => x.BranchId == branchId && x.Status != EmployeeBranchAssignmentStatus.Cancelled && x.StartDate <= startDate && (!x.EndDate.HasValue || (endDate.HasValue ? x.EndDate.Value >= endDate.Value : false)));

    private bool WouldOrphanJobPositionAssignments(BranchId branchId, DateOnly newStartDate, DateOnly? newEndDate)
        => _jobPositionAssignments.Any(x => x.BranchId == branchId && x.Status != EmployeeJobPositionAssignmentStatus.Cancelled && (x.StartDate < newStartDate || (!newEndDate.HasValue ? false : !x.EndDate.HasValue || x.EndDate.Value > newEndDate.Value)));

    private bool HasSameBranchOverlap(BranchId branchId, DateOnly startDate, DateOnly? endDate, EmployeeBranchAssignmentId? ignoredId)
        => _branchAssignments.Any(x => x.Id != ignoredId && x.BranchId == branchId && x.Status != EmployeeBranchAssignmentStatus.Cancelled && PeriodsOverlap(x.StartDate, x.EndDate, startDate, endDate));

    private bool HasPrimaryOverlap(DateOnly startDate, DateOnly? endDate, EmployeeBranchAssignmentId? ignoredId)
        => _branchAssignments.Any(x => x.Id != ignoredId && x.IsPrimary && x.Status != EmployeeBranchAssignmentStatus.Cancelled && PeriodsOverlap(x.StartDate, x.EndDate, startDate, endDate));

    private static bool PeriodsOverlap(DateOnly leftStart, DateOnly? leftEnd, DateOnly rightStart, DateOnly? rightEnd)
        => (!leftEnd.HasValue || rightStart <= leftEnd.Value) && (!rightEnd.HasValue || leftStart <= rightEnd.Value);

    private static string NormalizeEmployeeNumber(string value) => value.Trim().ToUpperInvariant();
    public void SetCreatedAudit(DateTimeOffset createdAtUtc, UserId? createdByUserId) { if (CreatedAtUtc != default) return; CreatedAtUtc = createdAtUtc.ToUniversalTime(); CreatedByUserId = createdByUserId; }
    public void SetModifiedAudit(DateTimeOffset modifiedAtUtc, UserId? modifiedByUserId) { LastModifiedAtUtc = modifiedAtUtc.ToUniversalTime(); LastModifiedByUserId = modifiedByUserId; }
}
