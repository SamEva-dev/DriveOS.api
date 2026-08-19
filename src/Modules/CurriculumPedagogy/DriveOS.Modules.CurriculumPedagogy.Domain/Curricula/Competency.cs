using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CurriculumPedagogy.Domain.Curricula;

public sealed class Competency : Entity<CompetencyId>
{
    private Competency() { }

    private Competency(
        CompetencyId id,
        CurriculumModuleId curriculumModuleId,
        string code,
        string name,
        string? description,
        string learningObjective,
        int order,
        bool isRequired)
        : base(id)
    {
        CurriculumModuleId = curriculumModuleId;
        Code = code;
        Name = name;
        Description = description;
        LearningObjective = learningObjective;
        Order = order;
        IsRequired = isRequired;
    }

    public CurriculumModuleId CurriculumModuleId { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string LearningObjective { get; private set; } = string.Empty;

    public int Order { get; private set; }

    public bool IsRequired { get; private set; }

    internal static Result<Competency> Create(
        CompetencyId id,
        CurriculumModuleId curriculumModuleId,
        string code,
        string name,
        string? description,
        string learningObjective,
        int order,
        bool isRequired)
    {
        if (id.IsEmpty || curriculumModuleId.IsEmpty)
            return Result.Failure<Competency>(CurriculumErrors.CompetencyInvalidIdentifier);

        Result<string> normalizedCode = NormalizeCode(code);
        if (normalizedCode.IsFailure)
            return Result.Failure<Competency>(normalizedCode.Error);

        Result<string> normalizedName = NormalizeName(name);
        if (normalizedName.IsFailure)
            return Result.Failure<Competency>(normalizedName.Error);

        Result<string?> normalizedDescription = NormalizeDescription(description);
        if (normalizedDescription.IsFailure)
            return Result.Failure<Competency>(normalizedDescription.Error);

        Result<string> normalizedObjective = NormalizeObjective(learningObjective);
        if (normalizedObjective.IsFailure)
            return Result.Failure<Competency>(normalizedObjective.Error);

        if (order <= 0)
            return Result.Failure<Competency>(CurriculumErrors.CompetencyInvalidOrder);

        return Result.Success(new Competency(
            id,
            curriculumModuleId,
            normalizedCode.Value,
            normalizedName.Value,
            normalizedDescription.Value,
            normalizedObjective.Value,
            order,
            isRequired));
    }

    internal Result Update(
        string name,
        string? description,
        string learningObjective,
        int order,
        bool isRequired)
    {
        Result<string> normalizedName = NormalizeName(name);
        if (normalizedName.IsFailure)
            return Result.Failure(normalizedName.Error);

        Result<string?> normalizedDescription = NormalizeDescription(description);
        if (normalizedDescription.IsFailure)
            return Result.Failure(normalizedDescription.Error);

        Result<string> normalizedObjective = NormalizeObjective(learningObjective);
        if (normalizedObjective.IsFailure)
            return Result.Failure(normalizedObjective.Error);

        if (order <= 0)
            return Result.Failure(CurriculumErrors.CompetencyInvalidOrder);

        Name = normalizedName.Value;
        Description = normalizedDescription.Value;
        LearningObjective = normalizedObjective.Value;
        Order = order;
        IsRequired = isRequired;
        return Result.Success();
    }

    private static Result<string> NormalizeCode(string code)
    {
        string normalized = (code ?? string.Empty).Trim().ToUpperInvariant();
        if (normalized.Length is < 1 or > 60 || !normalized.All(IsCodeCharacter))
            return Result.Failure<string>(CurriculumErrors.CompetencyInvalidCode);

        return Result.Success(normalized);
    }

    private static Result<string> NormalizeName(string name)
    {
        string normalized = (name ?? string.Empty).Trim();
        if (normalized.Length is < 2 or > 200)
            return Result.Failure<string>(CurriculumErrors.CompetencyInvalidName);

        return Result.Success(normalized);
    }

    private static Result<string?> NormalizeDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return Result.Success<string?>(null);

        string normalized = description.Trim();
        if (normalized.Length > 3000)
            return Result.Failure<string?>(CurriculumErrors.CompetencyInvalidDescription);

        return Result.Success<string?>(normalized);
    }

    private static Result<string> NormalizeObjective(string objective)
    {
        string normalized = (objective ?? string.Empty).Trim();
        if (normalized.Length is < 3 or > 2000)
            return Result.Failure<string>(CurriculumErrors.CompetencyInvalidLearningObjective);

        return Result.Success(normalized);
    }

    private static bool IsCodeCharacter(char value) =>
        char.IsLetterOrDigit(value) || value is '-' or '_' or '.';
}
