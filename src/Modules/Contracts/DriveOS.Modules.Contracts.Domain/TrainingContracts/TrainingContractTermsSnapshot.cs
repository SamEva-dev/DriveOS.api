using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Contracts.Domain.TrainingContracts;

public sealed record TrainingContractTermsSnapshot
{
    // Required by EF Core when this value object is materialized from JSON-owned mappings.
    // Keep it private so domain creation still goes through Create(...).
    private TrainingContractTermsSnapshot()
    {
    }

    private TrainingContractTermsSnapshot(
        string trainingCode,
        decimal practicalHours,
        string servicesSnapshot,
        string paymentScheduleSnapshot,
        string cancellationTerms,
        string bookingRules,
        string studentObligations,
        string providerObligations,
        string examPresentationTerms,
        string dataProcessingTerms)
    {
        TrainingCode = trainingCode;
        PracticalHours = practicalHours;
        ServicesSnapshot = servicesSnapshot;
        PaymentScheduleSnapshot = paymentScheduleSnapshot;
        CancellationTerms = cancellationTerms;
        BookingRules = bookingRules;
        StudentObligations = studentObligations;
        ProviderObligations = providerObligations;
        ExamPresentationTerms = examPresentationTerms;
        DataProcessingTerms = dataProcessingTerms;
    }

    public string TrainingCode { get; private set; } = string.Empty;
    public decimal PracticalHours { get; private set; }
    public string ServicesSnapshot { get; private set; } = string.Empty;
    public string PaymentScheduleSnapshot { get; private set; } = string.Empty;
    public string CancellationTerms { get; private set; } = string.Empty;
    public string BookingRules { get; private set; } = string.Empty;
    public string StudentObligations { get; private set; } = string.Empty;
    public string ProviderObligations { get; private set; } = string.Empty;
    public string ExamPresentationTerms { get; private set; } = string.Empty;
    public string DataProcessingTerms { get; private set; } = string.Empty;

    public static Result<TrainingContractTermsSnapshot> Create(
        string trainingCode,
        decimal practicalHours,
        string servicesSnapshot,
        string paymentScheduleSnapshot,
        string cancellationTerms,
        string bookingRules,
        string studentObligations,
        string providerObligations,
        string examPresentationTerms,
        string dataProcessingTerms)
    {
        string normalizedTrainingCode = trainingCode?.Trim() ?? string.Empty;
        if (normalizedTrainingCode.Length is < 1 or > 100)
            return Result.Failure<TrainingContractTermsSnapshot>(TrainingContractErrors.InvalidTermsSnapshot);

        if (practicalHours < 0 || practicalHours > 1000)
            return Result.Failure<TrainingContractTermsSnapshot>(TrainingContractErrors.InvalidTermsSnapshot);

        string[] requiredSections =
        [
            servicesSnapshot?.Trim() ?? string.Empty,
            paymentScheduleSnapshot?.Trim() ?? string.Empty,
            cancellationTerms?.Trim() ?? string.Empty,
            bookingRules?.Trim() ?? string.Empty,
            studentObligations?.Trim() ?? string.Empty,
            providerObligations?.Trim() ?? string.Empty,
            examPresentationTerms?.Trim() ?? string.Empty,
            dataProcessingTerms?.Trim() ?? string.Empty
        ];

        if (requiredSections.Any(section => section.Length == 0 || section.Length > 20_000))
            return Result.Failure<TrainingContractTermsSnapshot>(TrainingContractErrors.InvalidTermsSnapshot);

        return Result.Success(new TrainingContractTermsSnapshot(
            normalizedTrainingCode,
            decimal.Round(practicalHours, 2),
            requiredSections[0],
            requiredSections[1],
            requiredSections[2],
            requiredSections[3],
            requiredSections[4],
            requiredSections[5],
            requiredSections[6],
            requiredSections[7]));
    }
}
