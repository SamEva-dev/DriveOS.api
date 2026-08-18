using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Contracts.Domain.TrainingContracts;

public enum TrainingContractPartyKind
{
    TrainingProvider = 0,
    Student = 1,
    Guardian = 2,
    Funder = 3
}

public sealed record TrainingContractParty
{
    // Required by EF Core for owned-collection materialization.
    private TrainingContractParty() { }

    private TrainingContractParty(
        TrainingContractPartyKind kind,
        PersonId? personId,
        OrganizationId? organizationId,
        string displayName,
        string? legalReference)
    {
        Kind = kind;
        PersonId = personId;
        OrganizationId = organizationId;
        DisplayName = displayName;
        LegalReference = legalReference;
    }

    public TrainingContractPartyKind Kind { get; private set; }
    public PersonId? PersonId { get; private set; }
    public OrganizationId? OrganizationId { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public string? LegalReference { get; private set; }

    public static Result<TrainingContractParty> ForPerson(
        TrainingContractPartyKind kind,
        PersonId personId,
        string displayName,
        string? legalReference = null)
    {
        if (personId.IsEmpty)
            return Result.Failure<TrainingContractParty>(TrainingContractErrors.InvalidParty);

        if (kind == TrainingContractPartyKind.TrainingProvider)
            return Result.Failure<TrainingContractParty>(TrainingContractErrors.InvalidParty);

        return Create(kind, personId, null, displayName, legalReference);
    }

    public static Result<TrainingContractParty> ForOrganization(
        TrainingContractPartyKind kind,
        OrganizationId organizationId,
        string displayName,
        string? legalReference = null)
    {
        if (organizationId.IsEmpty)
            return Result.Failure<TrainingContractParty>(TrainingContractErrors.InvalidParty);

        if (kind is TrainingContractPartyKind.Student or TrainingContractPartyKind.Guardian)
            return Result.Failure<TrainingContractParty>(TrainingContractErrors.InvalidParty);

        return Create(kind, null, organizationId, displayName, legalReference);
    }

    private static Result<TrainingContractParty> Create(
        TrainingContractPartyKind kind,
        PersonId? personId,
        OrganizationId? organizationId,
        string displayName,
        string? legalReference)
    {
        string normalizedName = displayName?.Trim() ?? string.Empty;
        string? normalizedReference = Normalize(legalReference);

        if (normalizedName.Length is < 2 or > 250)
            return Result.Failure<TrainingContractParty>(TrainingContractErrors.InvalidParty);

        if (normalizedReference is { Length: > 150 })
            return Result.Failure<TrainingContractParty>(TrainingContractErrors.InvalidParty);

        return Result.Success(new TrainingContractParty(
            kind,
            personId,
            organizationId,
            normalizedName,
            normalizedReference));
    }

    private static string? Normalize(string? value)
    {
        string normalized = value?.Trim() ?? string.Empty;
        return normalized.Length == 0 ? null : normalized;
    }
}
