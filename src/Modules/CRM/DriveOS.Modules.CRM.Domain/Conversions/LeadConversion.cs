using DriveOS.Modules.CRM.Domain.Conversions.Events;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Domain.Conversions;

public sealed class LeadConversion : AggregateRoot<LeadConversionId>, IAuditableEntity
{
    private LeadConversion() { }

    private LeadConversion(LeadConversionId id, OrganizationId organizationId, Lead lead,
        CommercialOfferId acceptedOfferId, BranchId branchId, UserId responsibleUserId,
        PersonId? personId, DraftEnrollmentId? draftEnrollmentId,
        string trainingCode, bool identityVerified, bool consentsVerified,
        bool duplicateCheckCompleted, string? guardianSummary, string? payerSummary,
        string? requiredDocumentCodes)
        : base(id)
    {
        OrganizationId = organizationId;
        LeadId = lead.Id;
        AcceptedOfferId = acceptedOfferId;
        BranchId = branchId;
        ResponsibleUserId = responsibleUserId;
        TrainingCode = trainingCode;
        StudentPersonId = personId;
        StudentEnrollmentId = draftEnrollmentId;
        FirstName = lead.Identity.FirstName;
        LastName = lead.Identity.LastName;
        Email = lead.Identity.Email;
        Phone = lead.Identity.Phone;
        IdentityVerified = identityVerified;
        ConsentsVerified = consentsVerified;
        DuplicateCheckCompleted = duplicateCheckCompleted;
        GuardianSummary = Normalize(guardianSummary);
        PayerSummary = Normalize(payerSummary);
        RequiredDocumentCodes = Normalize(requiredDocumentCodes);
        Status = LeadConversionStatus.Requested;
    }

    public OrganizationId OrganizationId { get; private set; }
    public LeadId LeadId { get; private set; }
    public CommercialOfferId AcceptedOfferId { get; private set; }
    public BranchId BranchId { get; private set; }
    public UserId ResponsibleUserId { get; private set; }
    public string TrainingCode { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public bool IdentityVerified { get; private set; }
    public bool ConsentsVerified { get; private set; }
    public bool DuplicateCheckCompleted { get; private set; }
    public string? GuardianSummary { get; private set; }
    public string? PayerSummary { get; private set; }
    public string? RequiredDocumentCodes { get; private set; }
    public LeadConversionStatus Status { get; private set; }
    public PersonId? StudentPersonId { get; private set; }
    public DraftEnrollmentId? StudentEnrollmentId { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static LeadConversion Request(OrganizationId organizationId, Lead lead,
        CommercialOfferId acceptedOfferId, BranchId branchId, UserId responsibleUserId,
        PersonId? personId, DraftEnrollmentId? draftEnrollmentId,
        string trainingCode, bool identityVerified, bool consentsVerified,
        bool duplicateCheckCompleted, string? guardianSummary, string? payerSummary,
        string? requiredDocumentCodes)
    {
        var conversion = new LeadConversion(LeadConversionId.New(), organizationId, lead,
            acceptedOfferId, branchId, responsibleUserId,personId, draftEnrollmentId, trainingCode.Trim(), identityVerified,
            consentsVerified, duplicateCheckCompleted, guardianSummary, payerSummary,
            requiredDocumentCodes);
        conversion.RaiseDomainEvent(new LeadConversionRequestedDomainEvent(
            conversion.Id, conversion.OrganizationId, conversion.LeadId,
            conversion.AcceptedOfferId, conversion.BranchId, conversion.ResponsibleUserId));
        return conversion;
    }

    public static LeadConversion Request(OrganizationId organizationId, Lead lead,
        CommercialOfferId acceptedOfferId, BranchId branchId, UserId responsibleUserId,
        string trainingCode, bool identityVerified, bool consentsVerified,
        bool duplicateCheckCompleted, string? guardianSummary, string? payerSummary,
        string? requiredDocumentCodes) =>
        Request(organizationId, lead, acceptedOfferId, branchId, responsibleUserId,
            null, null, trainingCode, identityVerified, consentsVerified,
            duplicateCheckCompleted, guardianSummary, payerSummary, requiredDocumentCodes);

    public void Complete(PersonId personId, DraftEnrollmentId draftEnrollmentId, DateTimeOffset completedAtUtc)
    {
        StudentPersonId = personId;
        StudentEnrollmentId = draftEnrollmentId;
        CompletedAtUtc = completedAtUtc.ToUniversalTime();
        Status = LeadConversionStatus.Completed;
    }

    public void SetCreatedAudit(DateTimeOffset value, UserId? userId)
    { if (CreatedAtUtc == default) { CreatedAtUtc = value; CreatedByUserId = userId; } }
    public void SetModifiedAudit(DateTimeOffset value, UserId? userId)
    { LastModifiedAtUtc = value; LastModifiedByUserId = userId; }
    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
