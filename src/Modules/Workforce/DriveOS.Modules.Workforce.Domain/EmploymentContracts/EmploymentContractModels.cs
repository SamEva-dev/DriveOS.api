using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Workforce.Domain.EmploymentContracts;

public enum EmploymentContractStatus { Draft=0, PendingSignature=1, Signed=2, Active=3, Suspended=4, Ending=5, Terminated=6, Completed=7, Cancelled=8 }
public enum EmploymentContractType { Permanent=0, FixedTerm=1, Apprenticeship=2, Professionalization=3, Temporary=4, Internship=5, Other=99 }

/// <summary>
/// Workforce-owned HR contract record. It owns employment terms and lifecycle only.
/// Signed document bytes, versions, hashes, signature evidence and proof remain owned by BC-06 Contracts & Documents.
/// </summary>
public sealed class EmploymentContract
{
    private EmploymentContract() { }
    private EmploymentContract(EmploymentContractId id, EmploymentContractType contractType, DateOnly startDate, DateOnly? endDate, decimal? contractualWeeklyHours, JobPositionId? primaryJobPositionId, DateTimeOffset nowUtc, UserId actorUserId)
    {
        Id=id; ContractType=contractType; StartDate=startDate; EndDate=endDate; ContractualWeeklyHours=contractualWeeklyHours; PrimaryJobPositionId=primaryJobPositionId;
        Status=EmploymentContractStatus.Draft; CreatedAtUtc=nowUtc.ToUniversalTime(); CreatedByUserId=actorUserId;
    }
    public EmploymentContractId Id { get; private set; }
    public EmploymentContractType ContractType { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public decimal? ContractualWeeklyHours { get; private set; }
    public JobPositionId? PrimaryJobPositionId { get; private set; }
    public EmploymentContractStatus Status { get; private set; }
    public ContractDocumentId? ContractDocumentId { get; private set; }
    public SignatureProcessId? SignatureProcessId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<EmploymentContract> Create(EmploymentContractId id,EmploymentContractType contractType,DateOnly startDate,DateOnly? endDate,decimal? contractualWeeklyHours,JobPositionId? primaryJobPositionId,DateTimeOffset nowUtc,UserId actorUserId)
    {
        if(id.IsEmpty)return Result.Failure<EmploymentContract>(EmploymentContractErrors.InvalidIdentifier);
        if(endDate.HasValue&&endDate.Value<startDate)return Result.Failure<EmploymentContract>(EmploymentContractErrors.InvalidPeriod);
        if(contractualWeeklyHours is <=0 or >168)return Result.Failure<EmploymentContract>(EmploymentContractErrors.InvalidWeeklyHours);
        return Result.Success(new EmploymentContract(id,contractType,startDate,endDate,contractualWeeklyHours,primaryJobPositionId,nowUtc,actorUserId));
    }
    public Result UpdateTerms(DateOnly startDate,DateOnly? endDate,decimal? contractualWeeklyHours,JobPositionId? primaryJobPositionId,DateTimeOffset nowUtc,UserId actorUserId)
    {
        if(Status!=EmploymentContractStatus.Draft)return Result.Failure(EmploymentContractErrors.ImmutableAfterSignatureFlow);
        if(endDate.HasValue&&endDate.Value<startDate)return Result.Failure(EmploymentContractErrors.InvalidPeriod);
        if(contractualWeeklyHours is <=0 or >168)return Result.Failure(EmploymentContractErrors.InvalidWeeklyHours);
        StartDate=startDate;EndDate=endDate;ContractualWeeklyHours=contractualWeeklyHours;PrimaryJobPositionId=primaryJobPositionId;Touch(nowUtc,actorUserId);return Result.Success();
    }
    public Result LinkDocument(ContractDocumentId documentId,SignatureProcessId? signatureProcessId,DateTimeOffset nowUtc,UserId actorUserId)
    {
        if(documentId.IsEmpty)return Result.Failure(EmploymentContractErrors.InvalidDocumentReference);
        if(Status is not EmploymentContractStatus.Draft and not EmploymentContractStatus.PendingSignature)return Result.Failure(EmploymentContractErrors.InvalidLifecycleTransition);
        ContractDocumentId=documentId;SignatureProcessId=signatureProcessId;Status=signatureProcessId.HasValue?EmploymentContractStatus.PendingSignature:EmploymentContractStatus.Draft;Touch(nowUtc,actorUserId);return Result.Success();
    }
    public Result MarkSigned(SignatureProcessId signatureProcessId,DateTimeOffset nowUtc,UserId actorUserId)
    {
        if(Status!=EmploymentContractStatus.PendingSignature||ContractDocumentId is null)return Result.Failure(EmploymentContractErrors.InvalidLifecycleTransition);
        SignatureProcessId=signatureProcessId;Status=EmploymentContractStatus.Signed;Touch(nowUtc,actorUserId);return Result.Success();
    }
    public Result Activate(DateOnly atDate,DateTimeOffset nowUtc,UserId actorUserId)
    {
        if(Status!=EmploymentContractStatus.Signed)return Result.Failure(EmploymentContractErrors.InvalidLifecycleTransition);
        if(atDate<StartDate)return Result.Failure(EmploymentContractErrors.ActivationBeforeStartDate);
        Status=EmploymentContractStatus.Active;Touch(nowUtc,actorUserId);return Result.Success();
    }
    public Result Terminate(DateOnly endDate,DateTimeOffset nowUtc,UserId actorUserId)
    {
        if(Status is not EmploymentContractStatus.Active and not EmploymentContractStatus.Suspended and not EmploymentContractStatus.Signed)return Result.Failure(EmploymentContractErrors.InvalidLifecycleTransition);
        if(endDate<StartDate)return Result.Failure(EmploymentContractErrors.InvalidPeriod);
        EndDate=endDate;Status=EmploymentContractStatus.Terminated;Touch(nowUtc,actorUserId);return Result.Success();
    }
    public Result Cancel(DateTimeOffset nowUtc,UserId actorUserId)
    {
        if(Status is not EmploymentContractStatus.Draft and not EmploymentContractStatus.PendingSignature)return Result.Failure(EmploymentContractErrors.InvalidLifecycleTransition);
        Status=EmploymentContractStatus.Cancelled;Touch(nowUtc,actorUserId);return Result.Success();
    }
    private void Touch(DateTimeOffset nowUtc,UserId actorUserId){LastModifiedAtUtc=nowUtc.ToUniversalTime();LastModifiedByUserId=actorUserId;}
}
