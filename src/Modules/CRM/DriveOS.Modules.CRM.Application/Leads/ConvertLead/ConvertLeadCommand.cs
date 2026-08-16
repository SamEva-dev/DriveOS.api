using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Application.Leads.ConvertLead;

public sealed record ConvertLeadCommand(
    OrganizationId OrganizationId,
    LeadId LeadId,
    CommercialOfferId AcceptedOfferId,
    BranchId BranchId,
    UserId ResponsibleUserId,
    string TrainingCode,
    bool IdentityVerified,
    bool ConsentsVerified,
    bool DuplicateCheckCompleted,
    string? GuardianSummary,
    string? PayerSummary,
    IReadOnlyCollection<string> RequiredDocumentCodes
) : ICommand<ConvertLeadResponse>;

public sealed record ConversionChecklistItem(string Code, bool Completed);

public sealed record ConvertLeadResponse(
    Guid ConversionId,
    string Status,
    bool AlreadyRequested,
    Guid AcceptedOfferId,
    Guid? StudentPersonId,
    Guid? StudentEnrollmentId,
    IReadOnlyCollection<ConversionChecklistItem> Checklist
);
