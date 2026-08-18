using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Application.Installments.Create;

public sealed record CreatePaymentInstallmentInput(
    DateOnly DueDate,
    decimal ExpectedAmount,
    Guid? FinancingPersonId,
    Guid? FinancingOrganizationId);

public sealed record CreatePaymentScheduleCommand(
    OrganizationId OrganizationId,
    BillingAccountId BillingAccountId,
    IReadOnlyCollection<CreatePaymentInstallmentInput> Installments,
    UserId ActorUserId) : ICommand<IReadOnlyCollection<PaymentInstallmentId>>;
