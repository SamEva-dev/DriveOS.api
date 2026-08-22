using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FleetResources.Domain.Vehicles;

/// <summary>
/// Authoritative Fleet aggregate for a physical vehicle. This aggregate owns the technical identity,
/// operational state and compliance facts consumed by Scheduling, Training Delivery and Exams.
/// Calendar availability is deliberately not owned here: BC-09 remains authoritative for bookings,
/// while this aggregate decides whether the vehicle is technically and legally usable.
/// </summary>
public sealed class Vehicle : AggregateRoot<VehicleId>, IAuditableEntity
{
    private Vehicle() { }

    private Vehicle(VehicleId id, OrganizationId organizationId, OrganizationId ownerOrganizationId, BranchId? branchId,
        string registrationNumber, string? vin, string make, string model, string transmissionType, string energyType,
        bool dualControl, string licenseCategoriesCsv, string adaptationsCsv) : base(id)
    {
        OrganizationId = organizationId;
        OwnerOrganizationId = ownerOrganizationId;
        BranchId = branchId;
        RegistrationNumber = registrationNumber;
        Vin = vin;
        Make = make;
        Model = model;
        TransmissionType = transmissionType;
        EnergyType = energyType;
        DualControl = dualControl;
        LicenseCategoriesCsv = licenseCategoriesCsv;
        AdaptationsCsv = adaptationsCsv;
        OperationalStatus = VehicleOperationalStatus.Expected;
    }

    public OrganizationId OrganizationId { get; private set; }
    public OrganizationId OwnerOrganizationId { get; private set; }
    public OrganizationId? ProviderOrganizationId { get; private set; }
    public BranchId? BranchId { get; private set; }
    public string RegistrationNumber { get; private set; } = string.Empty;
    public string? Vin { get; private set; }
    public string Make { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public string TransmissionType { get; private set; } = string.Empty;
    public string EnergyType { get; private set; } = string.Empty;
    public bool DualControl { get; private set; }
    public string LicenseCategoriesCsv { get; private set; } = string.Empty;
    public string AdaptationsCsv { get; private set; } = string.Empty;
    public VehicleOperationalStatus OperationalStatus { get; private set; }
    public bool TechnicalComplianceVerified { get; private set; }
    public bool DocumentsCompliant { get; private set; }
    public DateTimeOffset? InsuranceValidUntilUtc { get; private set; }
    public bool MaintenanceBlocking { get; private set; }
    public DateTimeOffset? NextMaintenanceDueAtUtc { get; private set; }
    public DateTimeOffset? LastComplianceVerifiedAtUtc { get; private set; }
    public string? ComplianceNotes { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<Vehicle> Create(VehicleId id, OrganizationId organizationId, OrganizationId ownerOrganizationId,
        BranchId? branchId, string registrationNumber, string? vin, string make, string model, string transmissionType,
        string energyType, bool dualControl, IReadOnlyCollection<string> licenseCategories, IReadOnlyCollection<string>? adaptations = null)
    {
        if (id.IsEmpty) return Result.Failure<Vehicle>(VehicleErrors.InvalidIdentifier);
        if (organizationId.IsEmpty || ownerOrganizationId.IsEmpty) return Result.Failure<Vehicle>(VehicleErrors.InvalidOrganization);
        if (string.IsNullOrWhiteSpace(registrationNumber)) return Result.Failure<Vehicle>(VehicleErrors.RegistrationRequired);
        if (string.IsNullOrWhiteSpace(transmissionType) || string.IsNullOrWhiteSpace(energyType) || licenseCategories.Count == 0)
            return Result.Failure<Vehicle>(VehicleErrors.TechnicalProfileRequired);

        return Result.Success(new Vehicle(id, organizationId, ownerOrganizationId, branchId, registrationNumber.Trim().ToUpperInvariant(),
            NormalizeOptional(vin), Normalize(make), Normalize(model), Normalize(transmissionType), Normalize(energyType), dualControl,
            NormalizeSet(licenseCategories), NormalizeSet(adaptations ?? [])));
    }

    public Result UpdateCompliance(bool technicalComplianceVerified, bool documentsCompliant, DateTimeOffset? insuranceValidUntilUtc,
        bool maintenanceBlocking, DateTimeOffset? nextMaintenanceDueAtUtc, VehicleOperationalStatus operationalStatus,
        BranchId? branchId, OrganizationId? providerOrganizationId, string? notes, DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (insuranceValidUntilUtc is { } insurance && insurance <= DateTimeOffset.UnixEpoch)
            return Result.Failure(VehicleErrors.InvalidCompliancePeriod);

        TechnicalComplianceVerified = technicalComplianceVerified;
        DocumentsCompliant = documentsCompliant;
        InsuranceValidUntilUtc = insuranceValidUntilUtc?.ToUniversalTime();
        MaintenanceBlocking = maintenanceBlocking;
        NextMaintenanceDueAtUtc = nextMaintenanceDueAtUtc?.ToUniversalTime();
        OperationalStatus = operationalStatus;
        BranchId = branchId;
        ProviderOrganizationId = providerOrganizationId;
        ComplianceNotes = NormalizeOptional(notes);
        LastComplianceVerifiedAtUtc = nowUtc.ToUniversalTime();
        SetModifiedAudit(nowUtc, actorUserId);
        return Result.Success();
    }

    public bool SupportsLicenseCategory(string category) => ContainsToken(LicenseCategoriesCsv, category);
    public bool SupportsAdaptations(IEnumerable<string> required) => required.All(x => ContainsToken(AdaptationsCsv, x));

    public bool IsOperationalFor(DateTimeOffset startAtUtc, DateTimeOffset endAtUtc)
    {
        if (endAtUtc <= startAtUtc) return false;
        if (OperationalStatus is VehicleOperationalStatus.Expected or VehicleOperationalStatus.MaintenanceDue or
            VehicleOperationalStatus.UnderMaintenance or VehicleOperationalStatus.Restricted or VehicleOperationalStatus.Immobilized or
            VehicleOperationalStatus.OutOfService or VehicleOperationalStatus.Returning or VehicleOperationalStatus.Returned) return false;
        if (!TechnicalComplianceVerified || !DocumentsCompliant || MaintenanceBlocking) return false;
        if (InsuranceValidUntilUtc is null || InsuranceValidUntilUtc.Value < endAtUtc.ToUniversalTime()) return false;
        if (NextMaintenanceDueAtUtc is { } due && due <= endAtUtc.ToUniversalTime()) return false;
        return true;
    }

    private static string NormalizeSet(IEnumerable<string> values) => string.Join(',', values.Where(x => !string.IsNullOrWhiteSpace(x))
        .Select(x => x.Trim().ToUpperInvariant()).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x));
    private static bool ContainsToken(string csv, string? value) => !string.IsNullOrWhiteSpace(value) && csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Contains(value.Trim(), StringComparer.OrdinalIgnoreCase);
    private static string Normalize(string? value) => value?.Trim() ?? string.Empty;
    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public void SetCreatedAudit(DateTimeOffset createdAtUtc, UserId? createdByUserId)
    {
        if (CreatedAtUtc != default)
        {
            return;
        }

        CreatedAtUtc = createdAtUtc;
        CreatedByUserId = createdByUserId;
    }

    public void SetModifiedAudit(DateTimeOffset modifiedAtUtc, UserId? modifiedByUserId)
    {
        LastModifiedAtUtc = modifiedAtUtc;
        LastModifiedByUserId = modifiedByUserId;
    }
}
