using System.Text.RegularExpressions;
using DriveOS.Modules.Organizations.Domain.OrganizationSequences.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.OrganizationSequences;

public sealed class OrganizationSequence :
    AggregateRoot<OrganizationSequenceId>,
    IAuditableEntity
{
    public const int CodeMaximumLength = 30;
    public const int MinimumPadding = 1;
    public const int MaximumPadding = 18;

    private static readonly Regex CodePattern = new(
        "^[A-Z0-9][A-Z0-9_-]*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private OrganizationSequence() { }

    private OrganizationSequence(
        OrganizationSequenceId id,
        OrganizationId organizationId,
        BranchId? branchId,
        OrganizationSequenceScope scope,
        string code,
        SequencePattern pattern,
        int padding,
        long nextValue,
        OrganizationSequenceResetPolicy resetPolicy)
        : base(id)
    {
        OrganizationId = organizationId;
        BranchId = branchId;
        Scope = scope;
        Code = code;
        Pattern = pattern;
        Padding = padding;
        NextValue = nextValue;
        ResetPolicy = resetPolicy;
        Status = OrganizationSequenceStatus.Active;
        Revision = 1;
    }

    public OrganizationId OrganizationId { get; private set; }
    public BranchId? BranchId { get; private set; }
    public OrganizationSequenceScope Scope { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public SequencePattern Pattern { get; private set; } = null!;
    public int Padding { get; private set; }
    public long NextValue { get; private set; }
    public OrganizationSequenceResetPolicy ResetPolicy { get; private set; }
    public int? LastResetYear { get; private set; }
    public int? LastResetMonth { get; private set; }
    public OrganizationSequenceStatus Status { get; private set; }
    public int Revision { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<OrganizationSequence> Create(
        OrganizationSequenceId id,
        OrganizationId organizationId,
        BranchId? branchId,
        OrganizationSequenceScope scope,
        string? code,
        SequencePattern pattern,
        int padding,
        long initialValue,
        OrganizationSequenceResetPolicy resetPolicy)
    {
        if (id.IsEmpty)
            return Result.Failure<OrganizationSequence>(OrganizationSequenceErrors.EmptyId);

        if (organizationId.IsEmpty)
            return Result.Failure<OrganizationSequence>(OrganizationSequenceErrors.EmptyOrganizationId);

        if (!Enum.IsDefined(scope))
            return Result.Failure<OrganizationSequence>(OrganizationSequenceErrors.InvalidScope);

        if (scope == OrganizationSequenceScope.Branch &&
            (!branchId.HasValue || branchId.Value.IsEmpty))
        {
            return Result.Failure<OrganizationSequence>(OrganizationSequenceErrors.EmptyBranchId);
        }

        if (scope == OrganizationSequenceScope.Organization && branchId.HasValue)
            return Result.Failure<OrganizationSequence>(OrganizationSequenceErrors.BranchNotAllowed);

        string normalizedCode = code?.Trim().ToUpperInvariant() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedCode))
            return Result.Failure<OrganizationSequence>(OrganizationSequenceErrors.EmptyCode);

        if (normalizedCode.Length > CodeMaximumLength)
            return Result.Failure<OrganizationSequence>(OrganizationSequenceErrors.CodeTooLong(CodeMaximumLength));

        if (!CodePattern.IsMatch(normalizedCode))
            return Result.Failure<OrganizationSequence>(OrganizationSequenceErrors.InvalidCode);

        ArgumentNullException.ThrowIfNull(pattern);

        if (padding is < MinimumPadding or > MaximumPadding)
            return Result.Failure<OrganizationSequence>(OrganizationSequenceErrors.InvalidPadding);

        if (initialValue <= 0)
            return Result.Failure<OrganizationSequence>(OrganizationSequenceErrors.InvalidInitialValue);

        if (!Enum.IsDefined(resetPolicy))
            return Result.Failure<OrganizationSequence>(OrganizationSequenceErrors.InvalidResetPolicy);

        var sequence = new OrganizationSequence(
            id,
            organizationId,
            branchId,
            scope,
            normalizedCode,
            pattern,
            padding,
            initialValue,
            resetPolicy);

        sequence.RaiseDomainEvent(new OrganizationSequenceCreatedDomainEvent(
            sequence.Id,
            sequence.OrganizationId,
            sequence.BranchId,
            sequence.Code));

        return Result.Success(sequence);
    }

    public Result<string> ReserveNext(DateTimeOffset instantUtc)
    {
        if (Status != OrganizationSequenceStatus.Active)
            return Result.Failure<string>(OrganizationSequenceErrors.ActiveRequired);

        ApplyResetIfRequired(instantUtc);

        long reservedValue = NextValue;
        string formattedValue = Pattern.Format(
            Code,
            reservedValue,
            Padding,
            instantUtc);

        checked
        {
            NextValue++;
        }

        Revision++;

        RaiseDomainEvent(new OrganizationSequenceNumberReservedDomainEvent(
            Id,
            OrganizationId,
            BranchId,
            Code,
            reservedValue,
            formattedValue));

        return Result.Success(formattedValue);
    }

    public Result Suspend()
    {
        if (Status == OrganizationSequenceStatus.Archived)
            return Result.Failure(OrganizationSequenceErrors.ArchivedSequence);

        if (Status == OrganizationSequenceStatus.Suspended)
            return Result.Failure(OrganizationSequenceErrors.AlreadySuspended);

        Status = OrganizationSequenceStatus.Suspended;
        Revision++;
        return Result.Success();
    }

    public Result Reactivate()
    {
        if (Status == OrganizationSequenceStatus.Archived)
            return Result.Failure(OrganizationSequenceErrors.ArchivedSequence);

        if (Status == OrganizationSequenceStatus.Active)
            return Result.Success();

        Status = OrganizationSequenceStatus.Active;
        Revision++;
        return Result.Success();
    }

    public Result Archive()
    {
        if (Status == OrganizationSequenceStatus.Archived)
            return Result.Success();

        Status = OrganizationSequenceStatus.Archived;
        Revision++;
        return Result.Success();
    }

    public void SetCreatedAudit(DateTimeOffset createdAtUtc, UserId? createdByUserId)
    {
        if (CreatedAtUtc != default)
            return;

        CreatedAtUtc = createdAtUtc;
        CreatedByUserId = createdByUserId;
    }

    public void SetModifiedAudit(DateTimeOffset modifiedAtUtc, UserId? modifiedByUserId)
    {
        LastModifiedAtUtc = modifiedAtUtc;
        LastModifiedByUserId = modifiedByUserId;
    }

    private void ApplyResetIfRequired(DateTimeOffset instantUtc)
    {
        bool resetRequired = ResetPolicy switch
        {
            OrganizationSequenceResetPolicy.Never => false,
            OrganizationSequenceResetPolicy.Yearly =>
                LastResetYear.HasValue && LastResetYear.Value != instantUtc.Year,
            OrganizationSequenceResetPolicy.Monthly =>
                LastResetYear.HasValue &&
                (LastResetYear.Value != instantUtc.Year || LastResetMonth != instantUtc.Month),
            _ => false,
        };

        if (resetRequired)
            NextValue = 1;

        if (ResetPolicy is OrganizationSequenceResetPolicy.Yearly or
            OrganizationSequenceResetPolicy.Monthly)
        {
            LastResetYear = instantUtc.Year;
        }

        if (ResetPolicy == OrganizationSequenceResetPolicy.Monthly)
            LastResetMonth = instantUtc.Month;
        else
            LastResetMonth = null;
    }
}
