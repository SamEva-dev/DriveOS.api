using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Domain.TrainingCredits.Events;

public sealed record TrainingCreditAccountCreatedDomainEvent(
    TrainingCreditAccountId TrainingCreditAccountId,
    BillingAccountId BillingAccountId,
    string CreditType,
    DateOnly? ExpirationDate) : DomainEvent;
