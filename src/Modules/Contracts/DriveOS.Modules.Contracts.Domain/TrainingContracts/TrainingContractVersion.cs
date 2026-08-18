using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Contracts.Domain.TrainingContracts;

public sealed class TrainingContractVersion : Entity<TrainingContractVersionId>
{
    private readonly List<TrainingContractParty> _parties = [];

    private TrainingContractVersion() { }

    private TrainingContractVersion(
        TrainingContractVersionId id,
        TrainingContractId contractId,
        int versionNumber,
        CommercialOfferId sourceOfferId,
        int sourceOfferVersion,
        DateOnly startDate,
        DateOnly? endDate,
        decimal totalAmount,
        string currency,
        TrainingContractTermsSnapshot termsSnapshot,
        IEnumerable<TrainingContractParty> parties,
        string? revisionReason,
        UserId? createdByUserId,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        ContractId = contractId;
        VersionNumber = versionNumber;
        SourceOfferId = sourceOfferId;
        SourceOfferVersion = sourceOfferVersion;
        StartDate = startDate;
        EndDate = endDate;
        TotalAmount = totalAmount;
        Currency = currency;
        TermsSnapshot = termsSnapshot;
        RevisionReason = revisionReason;
        CreatedByUserId = createdByUserId;
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
        _parties.AddRange(parties);
    }

    public TrainingContractId ContractId { get; private set; }
    public int VersionNumber { get; private set; }
    public CommercialOfferId SourceOfferId { get; private set; }
    public int SourceOfferVersion { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public TrainingContractTermsSnapshot TermsSnapshot { get; private set; } = null!;
    public string? RevisionReason { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public IReadOnlyCollection<TrainingContractParty> Parties => _parties.AsReadOnly();

    internal static Result<TrainingContractVersion> Create(
        TrainingContractId contractId,
        int versionNumber,
        CommercialOfferId sourceOfferId,
        int sourceOfferVersion,
        DateOnly startDate,
        DateOnly? endDate,
        decimal totalAmount,
        string currency,
        TrainingContractTermsSnapshot termsSnapshot,
        IReadOnlyCollection<TrainingContractParty> parties,
        string? revisionReason,
        UserId? createdByUserId,
        DateTimeOffset createdAtUtc)
    {
        if (contractId.IsEmpty || versionNumber < 1)
            return Result.Failure<TrainingContractVersion>(TrainingContractErrors.InvalidVersion);

        if (sourceOfferId.IsEmpty || sourceOfferVersion < 1)
            return Result.Failure<TrainingContractVersion>(TrainingContractErrors.InvalidSourceOffer);

        if (endDate is not null && endDate < startDate)
            return Result.Failure<TrainingContractVersion>(TrainingContractErrors.InvalidEffectivePeriod);

        if (totalAmount < 0 || totalAmount > 100_000_000m)
            return Result.Failure<TrainingContractVersion>(TrainingContractErrors.InvalidAmount);

        string normalizedCurrency = currency?.Trim().ToUpperInvariant() ?? string.Empty;
        if (normalizedCurrency.Length != 3 || normalizedCurrency.Any(c => c is < 'A' or > 'Z'))
            return Result.Failure<TrainingContractVersion>(TrainingContractErrors.InvalidCurrency);

        if (termsSnapshot is null)
            return Result.Failure<TrainingContractVersion>(TrainingContractErrors.InvalidTermsSnapshot);

        if (parties is null || parties.Count == 0)
            return Result.Failure<TrainingContractVersion>(TrainingContractErrors.InvalidParty);

        string? normalizedReason = Normalize(revisionReason);
        if (versionNumber > 1 && (normalizedReason is null || normalizedReason.Length > 500))
            return Result.Failure<TrainingContractVersion>(TrainingContractErrors.InvalidRevisionReason);

        return Result.Success(new TrainingContractVersion(
            TrainingContractVersionId.New(),
            contractId,
            versionNumber,
            sourceOfferId,
            sourceOfferVersion,
            startDate,
            endDate,
            decimal.Round(totalAmount, 2),
            normalizedCurrency,
            termsSnapshot,
            parties,
            normalizedReason,
            createdByUserId,
            createdAtUtc));
    }

    internal void StampCreation(DateTimeOffset createdAtUtc, UserId? createdByUserId)
    {
        if (CreatedAtUtc != default)
            return;

        CreatedAtUtc = createdAtUtc.ToUniversalTime();
        CreatedByUserId = createdByUserId;
    }

    private static string? Normalize(string? value)
    {
        string normalized = value?.Trim() ?? string.Empty;
        return normalized.Length == 0 ? null : normalized;
    }
}
