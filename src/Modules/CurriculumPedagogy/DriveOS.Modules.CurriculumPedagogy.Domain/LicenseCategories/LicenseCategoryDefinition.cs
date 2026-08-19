using DriveOS.Modules.CurriculumPedagogy.Domain.Curricula;
using DriveOS.Modules.CurriculumPedagogy.Domain.LicenseCategories.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CurriculumPedagogy.Domain.LicenseCategories;

public sealed class LicenseCategoryDefinition : AggregateRoot<LicenseCategoryDefinitionId>, IAuditableEntity
{
    private LicenseCategoryDefinition() { }

    private LicenseCategoryDefinition(
        LicenseCategoryDefinitionId id,
        OrganizationId organizationId,
        CurriculumScope scope,
        string name,
        string? description)
        : base(id)
    {
        OrganizationId = organizationId;
        CountryCode = scope.CountryCode;
        Code = scope.LicenseCategoryCode;
        Name = name;
        Description = description;
        Status = LicenseCategoryDefinitionStatus.Draft;
    }

    public OrganizationId OrganizationId { get; private set; }
    public string CountryCode { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public LicenseCategoryDefinitionStatus Status { get; private set; }
    public DateTimeOffset? ActivatedAtUtc { get; private set; }
    public UserId? ActivatedByUserId { get; private set; }
    public DateTimeOffset? ArchivedAtUtc { get; private set; }
    public UserId? ArchivedByUserId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<LicenseCategoryDefinition> Create(
        LicenseCategoryDefinitionId id,
        OrganizationId organizationId,
        string countryCode,
        string code,
        string name,
        string? description)
    {
        if (id.IsEmpty)
            return Result.Failure<LicenseCategoryDefinition>(LicenseCategoryDefinitionErrors.InvalidIdentifier);

        if (organizationId.IsEmpty)
            return Result.Failure<LicenseCategoryDefinition>(LicenseCategoryDefinitionErrors.InvalidOrganization);

        Result<CurriculumScope> scopeResult = CurriculumScope.Create(countryCode, code);
        if (scopeResult.IsFailure)
            return Result.Failure<LicenseCategoryDefinition>(scopeResult.Error);

        string normalizedName = (name ?? string.Empty).Trim();
        if (normalizedName.Length is < 1 or > 160)
            return Result.Failure<LicenseCategoryDefinition>(LicenseCategoryDefinitionErrors.InvalidName);

        string? normalizedDescription = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        if (normalizedDescription?.Length > 2000)
            return Result.Failure<LicenseCategoryDefinition>(LicenseCategoryDefinitionErrors.InvalidDescription);

        var definition = new LicenseCategoryDefinition(
            id,
            organizationId,
            scopeResult.Value,
            normalizedName,
            normalizedDescription);

        definition.RaiseDomainEvent(new LicenseCategoryDefinitionCreatedDomainEvent(
            definition.Id,
            definition.OrganizationId,
            definition.CountryCode,
            definition.Code,
            definition.Name));

        return Result.Success(definition);
    }

    public Result UpdateMetadata(string name, string? description)
    {
        if (Status != LicenseCategoryDefinitionStatus.Draft)
            return Result.Failure(LicenseCategoryDefinitionErrors.ModificationNotAllowed);

        string normalizedName = (name ?? string.Empty).Trim();
        if (normalizedName.Length is < 1 or > 160)
            return Result.Failure(LicenseCategoryDefinitionErrors.InvalidName);

        string? normalizedDescription = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        if (normalizedDescription?.Length > 2000)
            return Result.Failure(LicenseCategoryDefinitionErrors.InvalidDescription);

        Name = normalizedName;
        Description = normalizedDescription;
        return Result.Success();
    }

    public Result Activate(UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status != LicenseCategoryDefinitionStatus.Draft || actorUserId.IsEmpty || occurredAtUtc == default)
            return Result.Failure(LicenseCategoryDefinitionErrors.ActivationNotAllowed);

        Status = LicenseCategoryDefinitionStatus.Active;
        ActivatedByUserId = actorUserId;
        ActivatedAtUtc = occurredAtUtc.ToUniversalTime();

        RaiseDomainEvent(new LicenseCategoryDefinitionActivatedDomainEvent(
            Id,
            OrganizationId,
            actorUserId,
            ActivatedAtUtc.Value));

        return Result.Success();
    }

    public Result Archive(UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status == LicenseCategoryDefinitionStatus.Archived || actorUserId.IsEmpty || occurredAtUtc == default)
            return Result.Failure(LicenseCategoryDefinitionErrors.ArchiveNotAllowed);

        Status = LicenseCategoryDefinitionStatus.Archived;
        ArchivedByUserId = actorUserId;
        ArchivedAtUtc = occurredAtUtc.ToUniversalTime();

        RaiseDomainEvent(new LicenseCategoryDefinitionArchivedDomainEvent(
            Id,
            OrganizationId,
            actorUserId,
            ArchivedAtUtc.Value));

        return Result.Success();
    }

    public bool Matches(string countryCode, string licenseCategoryCode)
    {
        Result<CurriculumScope> scopeResult = CurriculumScope.Create(countryCode, licenseCategoryCode);
        return scopeResult.IsSuccess &&
               string.Equals(CountryCode, scopeResult.Value.CountryCode, StringComparison.Ordinal) &&
               string.Equals(Code, scopeResult.Value.LicenseCategoryCode, StringComparison.Ordinal);
    }

    public void SetCreatedAudit(DateTimeOffset createdAtUtc, UserId? createdByUserId)
    {
        if (CreatedAtUtc != default)
            return;

        CreatedAtUtc = createdAtUtc.ToUniversalTime();
        CreatedByUserId = createdByUserId;
    }

    public void SetModifiedAudit(DateTimeOffset modifiedAtUtc, UserId? modifiedByUserId)
    {
        LastModifiedAtUtc = modifiedAtUtc.ToUniversalTime();
        LastModifiedByUserId = modifiedByUserId;
    }
}
