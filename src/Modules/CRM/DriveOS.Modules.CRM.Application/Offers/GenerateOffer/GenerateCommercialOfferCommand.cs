using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Domain.Assessments;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.Modules.CRM.Domain.Offers;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Application.Offers.GenerateOffer;

public sealed record GenerateCommercialOfferCommand(
    OrganizationId OrganizationId,
    LeadId LeadId,
    AssessmentSessionId AssessmentSessionId,
    BranchId? BranchId,
    string TrainingCode,
    string Currency,
    DateTimeOffset ValidUntilUtc,
    decimal EstimatedFundingAmount,
    string? FinancingNotes,
    string? Conditions,
    string? InternalNotes,
    IReadOnlyCollection<CommercialOfferLineDraft> Lines
) : ICommand<Guid>;
