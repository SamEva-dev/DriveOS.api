using DriveOS.Modules.CurriculumPedagogy.Domain.Curricula.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CurriculumPedagogy.Domain.Curricula;

public sealed class Curriculum : AggregateRoot<CurriculumId>, IAuditableEntity
{
    private readonly List<CurriculumVersion> _versions = [];

    private Curriculum() { }

    private Curriculum(
        CurriculumId id,
        OrganizationId organizationId,
        string code,
        string name,
        string? description,
        string countryCode,
        string licenseCategoryCode)
        : base(id)
    {
        OrganizationId = organizationId;
        Code = code;
        Name = name;
        Description = description;
        CountryCode = countryCode;
        LicenseCategoryCode = licenseCategoryCode;
        Status = CurriculumStatus.Draft;
    }

    public OrganizationId OrganizationId { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    /// <summary>
    /// ISO 3166-1 alpha-2 target country. PED-004 will enrich the country/rule catalogue;
    /// the curriculum only owns the immutable target code at this stage.
    /// </summary>
    public string CountryCode { get; private set; } = string.Empty;

    /// <summary>
    /// Stable business code of the driving licence category (for example B, A2, C).
    /// The catalogue and compatibility rules are introduced in PED-004.
    /// </summary>
    public string LicenseCategoryCode { get; private set; } = string.Empty;

    public CurriculumStatus Status { get; private set; }

    public IReadOnlyCollection<CurriculumVersion> Versions => _versions.AsReadOnly();

    public int LatestVersionNumber => _versions.Count == 0 ? 0 : _versions.Max(x => x.VersionNumber);

    public DateTimeOffset? ArchivedAtUtc { get; private set; }

    public UserId? ArchivedByUserId { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public UserId? CreatedByUserId { get; private set; }

    public DateTimeOffset? LastModifiedAtUtc { get; private set; }

    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<Curriculum> Create(
        CurriculumId id,
        OrganizationId organizationId,
        string code,
        string name,
        string? description,
        string countryCode,
        string licenseCategoryCode)
    {
        if (id.IsEmpty)
            return Result.Failure<Curriculum>(CurriculumErrors.InvalidIdentifier);

        if (organizationId.IsEmpty)
            return Result.Failure<Curriculum>(CurriculumErrors.InvalidOrganization);

        Result<string> normalizedCode = NormalizeCode(code);
        if (normalizedCode.IsFailure)
            return Result.Failure<Curriculum>(normalizedCode.Error);

        Result<string> normalizedName = NormalizeName(name);
        if (normalizedName.IsFailure)
            return Result.Failure<Curriculum>(normalizedName.Error);

        Result<string?> normalizedDescription = NormalizeDescription(description);
        if (normalizedDescription.IsFailure)
            return Result.Failure<Curriculum>(normalizedDescription.Error);

        Result<CurriculumScope> scopeResult = CurriculumScope.Create(countryCode, licenseCategoryCode);
        if (scopeResult.IsFailure)
            return Result.Failure<Curriculum>(scopeResult.Error);

        var curriculum = new Curriculum(
            id,
            organizationId,
            normalizedCode.Value,
            normalizedName.Value,
            normalizedDescription.Value,
            scopeResult.Value.CountryCode,
            scopeResult.Value.LicenseCategoryCode);

        curriculum.RaiseDomainEvent(new CurriculumCreatedDomainEvent(
            curriculum.Id,
            curriculum.OrganizationId,
            curriculum.Code,
            curriculum.CountryCode,
            curriculum.LicenseCategoryCode));

        return Result.Success(curriculum);
    }

    public Result UpdateMetadata(string name, string? description)
    {
        if (Status != CurriculumStatus.Draft)
            return Result.Failure(CurriculumErrors.ModificationNotAllowed);

        Result<string> normalizedName = NormalizeName(name);
        if (normalizedName.IsFailure)
            return Result.Failure(normalizedName.Error);

        Result<string?> normalizedDescription = NormalizeDescription(description);
        if (normalizedDescription.IsFailure)
            return Result.Failure(normalizedDescription.Error);

        Name = normalizedName.Value;
        Description = normalizedDescription.Value;

        RaiseDomainEvent(new CurriculumMetadataUpdatedDomainEvent(Id, OrganizationId, Name));
        return Result.Success();
    }

    public Result<CurriculumVersion> CreateVersion(
        CurriculumVersionId versionId,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        string? changeSummary,
        UserId actorUserId,
        DateTimeOffset occurredAtUtc)
    {
        if (Status == CurriculumStatus.Archived)
            return Result.Failure<CurriculumVersion>(CurriculumErrors.VersionCreationNotAllowed);

        int nextVersionNumber = LatestVersionNumber + 1;
        CurriculumVersion? sourceVersion = _versions
            .OrderByDescending(x => x.VersionNumber)
            .FirstOrDefault();

        Result<CurriculumVersion> versionResult = CurriculumVersion.Create(
            versionId,
            Id,
            nextVersionNumber,
            sourceVersion?.Id,
            Name,
            Description,
            CountryCode,
            LicenseCategoryCode,
            effectiveFrom,
            effectiveTo,
            changeSummary,
            actorUserId,
            occurredAtUtc);

        if (versionResult.IsFailure)
            return versionResult;

        CurriculumVersion version = versionResult.Value;
        _versions.Add(version);

        RaiseDomainEvent(new CurriculumVersionCreatedDomainEvent(
            Id,
            OrganizationId,
            version.Id,
            version.VersionNumber,
            version.SourceVersionId,
            version.EffectiveFrom,
            version.EffectiveTo,
            version.CreatedByUserId,
            version.CreatedAtUtc));

        return Result.Success(version);
    }

    public Result<CurriculumModule> AddModule(
        CurriculumVersionId versionId,
        CurriculumModuleId moduleId,
        string code,
        string name,
        string? description,
        int order)
    {
        CurriculumVersion? version = _versions.FirstOrDefault(x => x.Id == versionId);
        if (version is null)
            return Result.Failure<CurriculumModule>(CurriculumErrors.VersionNotFound);

        Result<CurriculumModule> result = version.AddModule(moduleId, code, name, description, order);
        if (result.IsFailure)
            return result;

        CurriculumModule module = result.Value;
        RaiseDomainEvent(new CurriculumModuleAddedDomainEvent(Id, version.Id, module.Id, module.Code, module.Order));
        return Result.Success(module);
    }

    public Result UpdateModule(
        CurriculumVersionId versionId,
        CurriculumModuleId moduleId,
        string name,
        string? description,
        int order)
    {
        CurriculumVersion? version = _versions.FirstOrDefault(x => x.Id == versionId);
        if (version is null)
            return Result.Failure(CurriculumErrors.VersionNotFound);

        Result result = version.UpdateModule(moduleId, name, description, order);
        if (result.IsFailure)
            return result;

        CurriculumModule module = version.Modules.Single(x => x.Id == moduleId);
        RaiseDomainEvent(new CurriculumModuleUpdatedDomainEvent(Id, version.Id, module.Id, module.Name, module.Order));
        return Result.Success();
    }

    public Result RemoveModule(CurriculumVersionId versionId, CurriculumModuleId moduleId)
    {
        CurriculumVersion? version = _versions.FirstOrDefault(x => x.Id == versionId);
        if (version is null)
            return Result.Failure(CurriculumErrors.VersionNotFound);

        Result result = version.RemoveModule(moduleId);
        if (result.IsFailure)
            return result;

        RaiseDomainEvent(new CurriculumModuleRemovedDomainEvent(Id, version.Id, moduleId));
        return Result.Success();
    }

    public Result<Competency> AddCompetency(
        CurriculumVersionId versionId,
        CurriculumModuleId moduleId,
        CompetencyId competencyId,
        string code,
        string name,
        string? description,
        string learningObjective,
        int order,
        bool isRequired = true)
    {
        CurriculumVersion? version = _versions.FirstOrDefault(x => x.Id == versionId);
        if (version is null)
            return Result.Failure<Competency>(CurriculumErrors.VersionNotFound);

        Result<Competency> result = version.AddCompetency(
            moduleId,
            competencyId,
            code,
            name,
            description,
            learningObjective,
            order,
            isRequired);

        if (result.IsFailure)
            return result;

        Competency competency = result.Value;
        RaiseDomainEvent(new CompetencyAddedDomainEvent(
            Id,
            version.Id,
            moduleId,
            competency.Id,
            competency.Code,
            competency.Order));

        return Result.Success(competency);
    }

    public Result UpdateCompetency(
        CurriculumVersionId versionId,
        CurriculumModuleId moduleId,
        CompetencyId competencyId,
        string name,
        string? description,
        string learningObjective,
        int order,
        bool isRequired)
    {
        CurriculumVersion? version = _versions.FirstOrDefault(x => x.Id == versionId);
        if (version is null)
            return Result.Failure(CurriculumErrors.VersionNotFound);

        Result result = version.UpdateCompetency(
            moduleId,
            competencyId,
            name,
            description,
            learningObjective,
            order,
            isRequired);

        if (result.IsFailure)
            return result;

        CurriculumModule module = version.Modules.Single(x => x.Id == moduleId);
        Competency competency = module.Competencies.Single(x => x.Id == competencyId);
        RaiseDomainEvent(new CompetencyUpdatedDomainEvent(
            Id,
            version.Id,
            module.Id,
            competency.Id,
            competency.Name,
            competency.Order,
            competency.IsRequired));

        return Result.Success();
    }

    public Result RemoveCompetency(
        CurriculumVersionId versionId,
        CurriculumModuleId moduleId,
        CompetencyId competencyId)
    {
        CurriculumVersion? version = _versions.FirstOrDefault(x => x.Id == versionId);
        if (version is null)
            return Result.Failure(CurriculumErrors.VersionNotFound);

        Result result = version.RemoveCompetency(moduleId, competencyId);
        if (result.IsFailure)
            return result;

        RaiseDomainEvent(new CompetencyRemovedDomainEvent(Id, version.Id, moduleId, competencyId));
        return Result.Success();
    }

    public Result PublishVersion(CurriculumVersionId versionId, UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status == CurriculumStatus.Archived)
            return Result.Failure(CurriculumErrors.VersionPublishNotAllowed);

        CurriculumVersion? version = _versions.FirstOrDefault(x => x.Id == versionId);
        if (version is null)
            return Result.Failure(CurriculumErrors.VersionNotFound);

        CurriculumVersion? currentPublished = _versions
            .Where(x => x.Id != versionId && x.Status == CurriculumVersionStatus.Published)
            .OrderByDescending(x => x.EffectiveFrom)
            .FirstOrDefault();

        if (currentPublished is not null)
        {
            Result superseded = currentPublished.Supersede(version.EffectiveFrom);
            if (superseded.IsFailure)
                return superseded;
            RaiseDomainEvent(new CurriculumVersionSupersededDomainEvent(
                Id, OrganizationId, currentPublished.Id, version.Id, currentPublished.EffectiveTo!.Value, occurredAtUtc.ToUniversalTime()));
        }

        bool overlapsHistorical = _versions.Any(x => x.Id != versionId && x != currentPublished &&
            x.EffectiveFrom <= (version.EffectiveTo ?? DateOnly.MaxValue) &&
            (x.EffectiveTo ?? DateOnly.MaxValue) >= version.EffectiveFrom);
        if (overlapsHistorical)
            return Result.Failure(CurriculumErrors.VersionEffectivePeriodOverlaps);

        Result published = version.Publish(actorUserId, occurredAtUtc);
        if (published.IsFailure)
            return published;

        Status = CurriculumStatus.Published;
        RaiseDomainEvent(new CurriculumVersionPublishedDomainEvent(
            Id, OrganizationId, version.Id, version.VersionNumber, actorUserId, occurredAtUtc.ToUniversalTime()));
        return Result.Success();
    }

    public Result Archive(UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status == CurriculumStatus.Archived)
            return Result.Failure(CurriculumErrors.ArchiveNotAllowed);

        if (actorUserId.IsEmpty || occurredAtUtc == default)
            return Result.Failure(CurriculumErrors.ArchiveNotAllowed);

        Status = CurriculumStatus.Archived;
        ArchivedByUserId = actorUserId;
        ArchivedAtUtc = occurredAtUtc.ToUniversalTime();

        RaiseDomainEvent(new CurriculumArchivedDomainEvent(
            Id,
            OrganizationId,
            actorUserId,
            ArchivedAtUtc.Value));

        return Result.Success();
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

    private static Result<string> NormalizeCode(string code)
    {
        string normalized = (code ?? string.Empty).Trim().ToUpperInvariant();
        if (normalized.Length is < 2 or > 40 || !normalized.All(IsCodeCharacter))
            return Result.Failure<string>(CurriculumErrors.InvalidCode);

        return Result.Success(normalized);
    }

    private static Result<string> NormalizeName(string name)
    {
        string normalized = (name ?? string.Empty).Trim();
        if (normalized.Length is < 3 or > 160)
            return Result.Failure<string>(CurriculumErrors.InvalidName);

        return Result.Success(normalized);
    }

    private static Result<string?> NormalizeDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return Result.Success<string?>(null);

        string normalized = description.Trim();
        if (normalized.Length > 2000)
            return Result.Failure<string?>(CurriculumErrors.InvalidDescription);

        return Result.Success<string?>(normalized);
    }

    private static bool IsCodeCharacter(char value) =>
        char.IsAsciiLetterOrDigit(value) || value is '-' or '_' or '.';
}
