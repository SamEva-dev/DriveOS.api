using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CurriculumPedagogy.Domain.Curricula;

public sealed class CurriculumModule : Entity<CurriculumModuleId>
{
    private readonly List<Competency> _competencies = [];

    private CurriculumModule() { }

    private CurriculumModule(
        CurriculumModuleId id,
        CurriculumVersionId curriculumVersionId,
        string code,
        string name,
        string? description,
        int order)
        : base(id)
    {
        CurriculumVersionId = curriculumVersionId;
        Code = code;
        Name = name;
        Description = description;
        Order = order;
    }

    public CurriculumVersionId CurriculumVersionId { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public int Order { get; private set; }

    public IReadOnlyCollection<Competency> Competencies => _competencies.AsReadOnly();

    internal static Result<CurriculumModule> Create(
        CurriculumModuleId id,
        CurriculumVersionId curriculumVersionId,
        string code,
        string name,
        string? description,
        int order)
    {
        if (id.IsEmpty || curriculumVersionId.IsEmpty)
            return Result.Failure<CurriculumModule>(CurriculumErrors.ModuleInvalidIdentifier);

        Result<string> normalizedCode = NormalizeCode(code);
        if (normalizedCode.IsFailure)
            return Result.Failure<CurriculumModule>(normalizedCode.Error);

        Result<string> normalizedName = NormalizeName(name);
        if (normalizedName.IsFailure)
            return Result.Failure<CurriculumModule>(normalizedName.Error);

        Result<string?> normalizedDescription = NormalizeDescription(description);
        if (normalizedDescription.IsFailure)
            return Result.Failure<CurriculumModule>(normalizedDescription.Error);

        if (order <= 0)
            return Result.Failure<CurriculumModule>(CurriculumErrors.ModuleInvalidOrder);

        return Result.Success(new CurriculumModule(
            id,
            curriculumVersionId,
            normalizedCode.Value,
            normalizedName.Value,
            normalizedDescription.Value,
            order));
    }

    internal Result Update(string name, string? description, int order)
    {
        Result<string> normalizedName = NormalizeName(name);
        if (normalizedName.IsFailure)
            return Result.Failure(normalizedName.Error);

        Result<string?> normalizedDescription = NormalizeDescription(description);
        if (normalizedDescription.IsFailure)
            return Result.Failure(normalizedDescription.Error);

        if (order <= 0)
            return Result.Failure(CurriculumErrors.ModuleInvalidOrder);

        Name = normalizedName.Value;
        Description = normalizedDescription.Value;
        Order = order;
        return Result.Success();
    }

    internal Result<Competency> AddCompetency(
        CompetencyId competencyId,
        string code,
        string name,
        string? description,
        string learningObjective,
        int order,
        bool isRequired)
    {
        Result<Competency> result = Competency.Create(
            competencyId,
            Id,
            code,
            name,
            description,
            learningObjective,
            order,
            isRequired);

        if (result.IsFailure)
            return result;

        Competency competency = result.Value;

        if (_competencies.Any(x => string.Equals(x.Code, competency.Code, StringComparison.OrdinalIgnoreCase)))
            return Result.Failure<Competency>(CurriculumErrors.CompetencyCodeAlreadyExists);

        if (_competencies.Any(x => x.Order == competency.Order))
            return Result.Failure<Competency>(CurriculumErrors.CompetencyOrderAlreadyExists);

        _competencies.Add(competency);
        return Result.Success(competency);
    }

    internal Result UpdateCompetency(
        CompetencyId competencyId,
        string name,
        string? description,
        string learningObjective,
        int order,
        bool isRequired)
    {
        Competency? competency = _competencies.FirstOrDefault(x => x.Id == competencyId);
        if (competency is null)
            return Result.Failure(CurriculumErrors.CompetencyNotFound);

        if (_competencies.Any(x => x.Id != competencyId && x.Order == order))
            return Result.Failure(CurriculumErrors.CompetencyOrderAlreadyExists);

        return competency.Update(name, description, learningObjective, order, isRequired);
    }

    internal Result RemoveCompetency(CompetencyId competencyId)
    {
        Competency? competency = _competencies.FirstOrDefault(x => x.Id == competencyId);
        if (competency is null)
            return Result.Failure(CurriculumErrors.CompetencyNotFound);

        _competencies.Remove(competency);
        return Result.Success();
    }

    internal Competency? FindCompetency(CompetencyId competencyId) =>
        _competencies.FirstOrDefault(x => x.Id == competencyId);

    private static Result<string> NormalizeCode(string code)
    {
        string normalized = (code ?? string.Empty).Trim().ToUpperInvariant();
        if (normalized.Length is < 1 or > 40 || !normalized.All(IsCodeCharacter))
            return Result.Failure<string>(CurriculumErrors.ModuleInvalidCode);

        return Result.Success(normalized);
    }

    private static Result<string> NormalizeName(string name)
    {
        string normalized = (name ?? string.Empty).Trim();
        if (normalized.Length is < 2 or > 160)
            return Result.Failure<string>(CurriculumErrors.ModuleInvalidName);

        return Result.Success(normalized);
    }

    private static Result<string?> NormalizeDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return Result.Success<string?>(null);

        string normalized = description.Trim();
        if (normalized.Length > 2000)
            return Result.Failure<string?>(CurriculumErrors.ModuleInvalidDescription);

        return Result.Success<string?>(normalized);
    }

    private static bool IsCodeCharacter(char value) =>
        char.IsLetterOrDigit(value) || value is '-' or '_' or '.';
}
