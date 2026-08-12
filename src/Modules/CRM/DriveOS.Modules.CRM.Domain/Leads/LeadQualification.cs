using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Domain.Leads;

public sealed record LeadQualification
{
    private LeadQualification() { }

    private LeadQualification(string need, string licenseCategory, string availability,
        DateOnly? targetDate, FinancingOption financing, string? notes)
    {
        Need = need;
        LicenseCategory = licenseCategory;
        Availability = availability;
        TargetDate = targetDate;
        Financing = financing;
        Notes = notes;
    }

    public string Need { get; private init; } = string.Empty;
    public string LicenseCategory { get; private init; } = string.Empty;
    public string Availability { get; private init; } = string.Empty;
    public DateOnly? TargetDate { get; private init; }
    public FinancingOption Financing { get; private init; }
    public string? Notes { get; private init; }

    public static Result<LeadQualification> Create(string need, string licenseCategory,
        string availability, DateOnly? targetDate, FinancingOption financing, string? notes)
    {
        string normalizedNeed = need?.Trim() ?? string.Empty;
        string normalizedCategory = licenseCategory?.Trim().ToUpperInvariant() ?? string.Empty;
        string normalizedAvailability = availability?.Trim() ?? string.Empty;
        string? normalizedNotes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();

        if (normalizedNeed.Length is 0 or > 1000) return Result.Failure<LeadQualification>(LeadErrors.QualificationNeedInvalid);
        if (normalizedCategory.Length is 0 or > 30) return Result.Failure<LeadQualification>(LeadErrors.QualificationCategoryInvalid);
        if (normalizedAvailability.Length is 0 or > 500) return Result.Failure<LeadQualification>(LeadErrors.QualificationAvailabilityInvalid);
        if (!Enum.IsDefined(financing)) return Result.Failure<LeadQualification>(LeadErrors.QualificationFinancingInvalid);
        if (normalizedNotes?.Length > 2000) return Result.Failure<LeadQualification>(LeadErrors.QualificationNotesTooLong);

        return Result.Success(new LeadQualification(normalizedNeed, normalizedCategory,
            normalizedAvailability, targetDate, financing, normalizedNotes));
    }
}
