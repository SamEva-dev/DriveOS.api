using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CurriculumPedagogy.Domain.Curricula;

public sealed class CurriculumVersion : Entity<CurriculumVersionId>
{
    private readonly List<CurriculumModule> _modules = [];

    private CurriculumVersion() { }

    internal CurriculumVersion(
        CurriculumVersionId id,
        CurriculumId curriculumId,
        int versionNumber,
        CurriculumVersionId? sourceVersionId,
        string nameSnapshot,
        string? descriptionSnapshot,
        string countryCodeSnapshot,
        string licenseCategoryCodeSnapshot,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        string? changeSummary,
        UserId createdByUserId,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        CurriculumId = curriculumId;
        VersionNumber = versionNumber;
        SourceVersionId = sourceVersionId;
        NameSnapshot = nameSnapshot;
        DescriptionSnapshot = descriptionSnapshot;
        CountryCodeSnapshot = countryCodeSnapshot;
        LicenseCategoryCodeSnapshot = licenseCategoryCodeSnapshot;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        ChangeSummary = changeSummary;
        Status = CurriculumVersionStatus.Draft;
        CreatedByUserId = createdByUserId;
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
    }

    public CurriculumId CurriculumId { get; private set; }
    public int VersionNumber { get; private set; }
    public CurriculumVersionId? SourceVersionId { get; private set; }
    public string NameSnapshot { get; private set; } = string.Empty;
    public string? DescriptionSnapshot { get; private set; }
    public string CountryCodeSnapshot { get; private set; } = string.Empty;
    public string LicenseCategoryCodeSnapshot { get; private set; } = string.Empty;
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }
    public string? ChangeSummary { get; private set; }
    public CurriculumVersionStatus Status { get; private set; }
    public UserId CreatedByUserId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public IReadOnlyCollection<CurriculumModule> Modules => _modules.AsReadOnly();

    internal static Result<CurriculumVersion> Create(
        CurriculumVersionId id,
        CurriculumId curriculumId,
        int versionNumber,
        CurriculumVersionId? sourceVersionId,
        string nameSnapshot,
        string? descriptionSnapshot,
        string countryCodeSnapshot,
        string licenseCategoryCodeSnapshot,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        string? changeSummary,
        UserId createdByUserId,
        DateTimeOffset createdAtUtc)
    {
        if (id.IsEmpty || curriculumId.IsEmpty || versionNumber <= 0)
            return Result.Failure<CurriculumVersion>(CurriculumErrors.VersionInvalid);

        if (createdByUserId.IsEmpty || createdAtUtc == default)
            return Result.Failure<CurriculumVersion>(CurriculumErrors.VersionInvalid);

        if (effectiveTo.HasValue && effectiveTo.Value < effectiveFrom)
            return Result.Failure<CurriculumVersion>(CurriculumErrors.VersionEffectivePeriodInvalid);

        string? normalizedSummary = string.IsNullOrWhiteSpace(changeSummary)
            ? null
            : changeSummary.Trim();

        if (normalizedSummary?.Length > 1000)
            return Result.Failure<CurriculumVersion>(CurriculumErrors.VersionChangeSummaryInvalid);

        return Result.Success(new CurriculumVersion(
            id,
            curriculumId,
            versionNumber,
            sourceVersionId,
            nameSnapshot,
            descriptionSnapshot,
            countryCodeSnapshot,
            licenseCategoryCodeSnapshot,
            effectiveFrom,
            effectiveTo,
            normalizedSummary,
            createdByUserId,
            createdAtUtc));
    }

    internal Result<CurriculumModule> AddModule(
        CurriculumModuleId moduleId,
        string code,
        string name,
        string? description,
        int order)
    {
        Result editable = EnsureDraftStructure();
        if (editable.IsFailure)
            return Result.Failure<CurriculumModule>(editable.Error);

        Result<CurriculumModule> result = CurriculumModule.Create(moduleId, Id, code, name, description, order);
        if (result.IsFailure)
            return result;

        CurriculumModule module = result.Value;

        if (_modules.Any(x => string.Equals(x.Code, module.Code, StringComparison.OrdinalIgnoreCase)))
            return Result.Failure<CurriculumModule>(CurriculumErrors.ModuleCodeAlreadyExists);

        if (_modules.Any(x => x.Order == module.Order))
            return Result.Failure<CurriculumModule>(CurriculumErrors.ModuleOrderAlreadyExists);

        _modules.Add(module);
        return Result.Success(module);
    }

    internal Result UpdateModule(CurriculumModuleId moduleId, string name, string? description, int order)
    {
        Result editable = EnsureDraftStructure();
        if (editable.IsFailure)
            return editable;

        CurriculumModule? module = _modules.FirstOrDefault(x => x.Id == moduleId);
        if (module is null)
            return Result.Failure(CurriculumErrors.ModuleNotFound);

        if (_modules.Any(x => x.Id != moduleId && x.Order == order))
            return Result.Failure(CurriculumErrors.ModuleOrderAlreadyExists);

        Result result = module.Update(name, description, order);
        if (result.IsFailure)
            return result;

        return Result.Success();
    }

    internal Result RemoveModule(CurriculumModuleId moduleId)
    {
        Result editable = EnsureDraftStructure();
        if (editable.IsFailure)
            return editable;

        CurriculumModule? module = _modules.FirstOrDefault(x => x.Id == moduleId);
        if (module is null)
            return Result.Failure(CurriculumErrors.ModuleNotFound);

        if (module.Competencies.Count > 0)
            return Result.Failure(CurriculumErrors.ModuleHasCompetencies);

        _modules.Remove(module);
        return Result.Success();
    }

    internal Result<Competency> AddCompetency(
        CurriculumModuleId moduleId,
        CompetencyId competencyId,
        string code,
        string name,
        string? description,
        string learningObjective,
        int order,
        bool isRequired = true)
    {
        Result editable = EnsureDraftStructure();
        if (editable.IsFailure)
            return Result.Failure<Competency>(editable.Error);

        CurriculumModule? module = _modules.FirstOrDefault(x => x.Id == moduleId);
        if (module is null)
            return Result.Failure<Competency>(CurriculumErrors.ModuleNotFound);

        Result<Competency> result = module.AddCompetency(
            competencyId,
            code,
            name,
            description,
            learningObjective,
            order,
            isRequired);

        if (result.IsFailure)
            return result;

        return Result.Success(result.Value);
    }

    internal Result UpdateCompetency(
        CurriculumModuleId moduleId,
        CompetencyId competencyId,
        string name,
        string? description,
        string learningObjective,
        int order,
        bool isRequired)
    {
        Result editable = EnsureDraftStructure();
        if (editable.IsFailure)
            return editable;

        CurriculumModule? module = _modules.FirstOrDefault(x => x.Id == moduleId);
        if (module is null)
            return Result.Failure(CurriculumErrors.ModuleNotFound);

        Result result = module.UpdateCompetency(
            competencyId,
            name,
            description,
            learningObjective,
            order,
            isRequired);

        if (result.IsFailure)
            return result;

        return Result.Success();
    }

    internal Result RemoveCompetency(CurriculumModuleId moduleId, CompetencyId competencyId)
    {
        Result editable = EnsureDraftStructure();
        if (editable.IsFailure)
            return editable;

        CurriculumModule? module = _modules.FirstOrDefault(x => x.Id == moduleId);
        if (module is null)
            return Result.Failure(CurriculumErrors.ModuleNotFound);

        Result result = module.RemoveCompetency(competencyId);
        if (result.IsFailure)
            return result;

        return Result.Success();
    }

    internal Result Publish(UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status != CurriculumVersionStatus.Draft || actorUserId.IsEmpty || occurredAtUtc == default)
            return Result.Failure(CurriculumErrors.VersionPublishNotAllowed);
        if (_modules.Count == 0 || _modules.Any(x => x.Competencies.Count == 0))
            return Result.Failure(CurriculumErrors.VersionEmpty);
        Status = CurriculumVersionStatus.Published;
        PublishedByUserId = actorUserId;
        PublishedAtUtc = occurredAtUtc.ToUniversalTime();
        return Result.Success();
    }

    internal Result Supersede(DateOnly replacementEffectiveFrom)
    {
        if (Status != CurriculumVersionStatus.Published || replacementEffectiveFrom <= EffectiveFrom)
            return Result.Failure(CurriculumErrors.VersionEffectivePeriodOverlaps);

        DateOnly previousEnd = replacementEffectiveFrom.AddDays(-1);
        if (!EffectiveTo.HasValue || EffectiveTo.Value >= replacementEffectiveFrom)
            EffectiveTo = previousEnd;
        Status = CurriculumVersionStatus.Superseded;
        return Result.Success();
    }

    public UserId? PublishedByUserId { get; private set; }
    public DateTimeOffset? PublishedAtUtc { get; private set; }

    private Result EnsureDraftStructure() =>
        Status == CurriculumVersionStatus.Draft
            ? Result.Success()
            : Result.Failure(CurriculumErrors.VersionStructureModificationNotAllowed);
}
