using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.FundingBilling.Application.Persistence;
using DriveOS.Modules.FundingBilling.Application.Notifications;
using DriveOS.Modules.FundingBilling.Domain.BillingAccounts;
using DriveOS.Modules.FundingBilling.Domain.Payments;
using DriveOS.Modules.FundingBilling.Domain.Refunds;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Application.Refunds.Manage;

public sealed record ApproveRefundCommand(OrganizationId OrganizationId, RefundId RefundId, UserId ActorUserId) : ICommand;
public sealed record MarkRefundProcessingCommand(OrganizationId OrganizationId, RefundId RefundId, UserId ActorUserId) : ICommand;
public sealed record CompleteRefundCommand(OrganizationId OrganizationId, RefundId RefundId, string? ProviderReference, UserId ActorUserId) : ICommand;
public sealed record RejectRefundCommand(OrganizationId OrganizationId, RefundId RefundId, string Reason, UserId ActorUserId) : ICommand;
public sealed record FailRefundCommand(OrganizationId OrganizationId, RefundId RefundId, string Reason, UserId ActorUserId) : ICommand;
public sealed record CancelRefundCommand(OrganizationId OrganizationId, RefundId RefundId, UserId ActorUserId) : ICommand;

internal static class RefundHandlerSupport
{
    public static async Task<Result<Refund>> LoadAsync(IRefundRepository repo, OrganizationId org, RefundId id, CancellationToken ct)
    { var x=await repo.GetByIdAsync(id,ct); return x is null||x.OrganizationId!=org?Result.Failure<Refund>(RefundErrors.NotFound):Result.Success(x); }
}
internal sealed class ApproveRefundCommandHandler(IRefundRepository repo, IFundingBillingUnitOfWork uow, IClock clock) : ICommandHandler<ApproveRefundCommand>
{ public async Task<Result> Handle(ApproveRefundCommand c,CancellationToken ct){var x=await RefundHandlerSupport.LoadAsync(repo,c.OrganizationId,c.RefundId,ct);if(x.IsFailure)return Result.Failure(x.Error);var now=clock.UtcNow;var r=x.Value.Approve(c.ActorUserId,now);if(r.IsFailure)return r;x.Value.SetModifiedAudit(now,c.ActorUserId);await uow.CommitAsync(ct);return Result.Success();} }
internal sealed class MarkRefundProcessingCommandHandler(IRefundRepository repo, IFundingBillingUnitOfWork uow, IClock clock) : ICommandHandler<MarkRefundProcessingCommand>
{ public async Task<Result> Handle(MarkRefundProcessingCommand c,CancellationToken ct){var x=await RefundHandlerSupport.LoadAsync(repo,c.OrganizationId,c.RefundId,ct);if(x.IsFailure)return Result.Failure(x.Error);var now=clock.UtcNow;var r=x.Value.MarkProcessing(c.ActorUserId,now);if(r.IsFailure)return r;x.Value.SetModifiedAudit(now,c.ActorUserId);await uow.CommitAsync(ct);return Result.Success();} }
internal sealed class RejectRefundCommandHandler(IRefundRepository repo, IFundingBillingUnitOfWork uow, IClock clock) : ICommandHandler<RejectRefundCommand>
{ public async Task<Result> Handle(RejectRefundCommand c,CancellationToken ct){var x=await RefundHandlerSupport.LoadAsync(repo,c.OrganizationId,c.RefundId,ct);if(x.IsFailure)return Result.Failure(x.Error);var now=clock.UtcNow;var r=x.Value.Reject(c.Reason,c.ActorUserId,now);if(r.IsFailure)return r;x.Value.SetModifiedAudit(now,c.ActorUserId);await uow.CommitAsync(ct);return Result.Success();} }
internal sealed class FailRefundCommandHandler(IRefundRepository repo, IFundingBillingUnitOfWork uow, IClock clock) : ICommandHandler<FailRefundCommand>
{ public async Task<Result> Handle(FailRefundCommand c,CancellationToken ct){var x=await RefundHandlerSupport.LoadAsync(repo,c.OrganizationId,c.RefundId,ct);if(x.IsFailure)return Result.Failure(x.Error);var now=clock.UtcNow;var r=x.Value.MarkFailed(c.Reason,c.ActorUserId,now);if(r.IsFailure)return r;x.Value.SetModifiedAudit(now,c.ActorUserId);await uow.CommitAsync(ct);return Result.Success();} }
internal sealed class CancelRefundCommandHandler(IRefundRepository repo, IFundingBillingUnitOfWork uow, IClock clock) : ICommandHandler<CancelRefundCommand>
{ public async Task<Result> Handle(CancelRefundCommand c,CancellationToken ct){var x=await RefundHandlerSupport.LoadAsync(repo,c.OrganizationId,c.RefundId,ct);if(x.IsFailure)return Result.Failure(x.Error);var now=clock.UtcNow;var r=x.Value.Cancel(c.ActorUserId,now);if(r.IsFailure)return r;x.Value.SetModifiedAudit(now,c.ActorUserId);await uow.CommitAsync(ct);return Result.Success();} }
internal sealed class CompleteRefundCommandHandler(IRefundRepository refunds, IPaymentRepository payments, IStudentBillingAccountRepository accounts, IFundingBillingUnitOfWork uow, IFinancialNotificationGateway notifications, IClock clock) : ICommandHandler<CompleteRefundCommand>
{
    public async Task<Result> Handle(CompleteRefundCommand c,CancellationToken ct)
    {
        var loaded=await RefundHandlerSupport.LoadAsync(refunds,c.OrganizationId,c.RefundId,ct); if(loaded.IsFailure)return Result.Failure(loaded.Error); Refund refund=loaded.Value;
        if(!string.IsNullOrWhiteSpace(c.ProviderReference)){Refund? duplicate=await refunds.GetByProviderReferenceAsync(c.OrganizationId,c.ProviderReference.Trim(),ct);if(duplicate is not null&&duplicate.Id!=refund.Id)return Result.Failure(RefundErrors.InvalidProviderReference);}
        Payment? payment=await payments.GetByIdAsync(refund.PaymentId,ct); if(payment is null||payment.OrganizationId!=c.OrganizationId)return Result.Failure(RefundErrors.PaymentNotFound);
        BillingAccount? account=await accounts.GetByIdAsync(refund.BillingAccountId,ct); if(account is null||account.OrganizationId!=c.OrganizationId)return Result.Failure(RefundErrors.BillingAccountNotFound);
        if(payment.Currency!=refund.Currency||account.Currency!=refund.Currency)return Result.Failure(RefundErrors.CurrencyMismatch);
        DateTimeOffset now=clock.UtcNow; Result pr=payment.RecordRefundCompleted(refund.Amount,refund.Currency,c.ActorUserId,now); if(pr.IsFailure)return pr; Result ar=account.RecordRefundCompleted(refund.Amount,refund.Currency,c.ActorUserId,now); if(ar.IsFailure)return ar; Result rr=refund.Complete(c.ProviderReference,c.ActorUserId,now); if(rr.IsFailure)return rr;
        payment.SetModifiedAudit(now,c.ActorUserId); account.SetModifiedAudit(now,c.ActorUserId); refund.SetModifiedAudit(now,c.ActorUserId); await uow.CommitAsync(ct);
        await notifications.QueueRefundCompletedAsync(c.OrganizationId, refund.BillingAccountId, refund.Amount, refund.Currency, ct);
        return Result.Success();
    }
}
