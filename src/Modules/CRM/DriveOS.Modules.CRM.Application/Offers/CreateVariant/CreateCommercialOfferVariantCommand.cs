using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Domain.Offers;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Application.Offers.CreateVariant;

public sealed record CreateCommercialOfferVariantCommand(
    OrganizationId OrganizationId,
    CommercialOfferId SourceOfferId,
    string TrainingCode,
    DateTimeOffset ValidUntilUtc,
    decimal EstimatedFundingAmount,
    string? FinancingNotes,
    string? Conditions,
    string? InternalNotes,
    IReadOnlyCollection<CommercialOfferLineDraft> Lines
) : ICommand<Guid>;
