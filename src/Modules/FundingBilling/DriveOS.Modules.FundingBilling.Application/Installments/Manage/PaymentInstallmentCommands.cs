using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Application.Installments.Manage;

public sealed record ReschedulePaymentInstallmentCommand(
    OrganizationId OrganizationId,
    PaymentInstallmentId PaymentInstallmentId,
    DateOnly NewDueDate,
    string Reason,
    UserId ActorUserId) : ICommand;

public sealed record CancelPaymentInstallmentCommand(
    OrganizationId OrganizationId,
    PaymentInstallmentId PaymentInstallmentId,
    string Reason,
    UserId ActorUserId) : ICommand;

public sealed record WaivePaymentInstallmentCommand(
    OrganizationId OrganizationId,
    PaymentInstallmentId PaymentInstallmentId,
    string Reason,
    UserId ActorUserId) : ICommand;
