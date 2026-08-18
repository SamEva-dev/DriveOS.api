using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.FundingBilling.Application.Persistence;
using DriveOS.Modules.FundingBilling.Application.Notifications;
using DriveOS.Modules.FundingBilling.Domain.BillingAccounts;
using DriveOS.Modules.FundingBilling.Domain.Payments;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Application.Payments.Record;

public sealed record MarkPaymentProcessingCommand(OrganizationId OrganizationId, PaymentId PaymentId, UserId ActorUserId) : ICommand;
public sealed record RecordPaymentReceivedCommand(OrganizationId OrganizationId, PaymentId PaymentId, string? ExternalReference, UserId ActorUserId) : ICommand;
public sealed record MarkPaymentFailedCommand(OrganizationId OrganizationId, PaymentId PaymentId, string Reason, UserId ActorUserId) : ICommand;
public sealed record CancelPaymentCommand(OrganizationId OrganizationId, PaymentId PaymentId, UserId ActorUserId) : ICommand;

internal sealed class MarkPaymentProcessingCommandHandler(IPaymentRepository payments, IFundingBillingUnitOfWork uow, IClock clock) : ICommandHandler<MarkPaymentProcessingCommand>
{
    public async Task<Result> Handle(MarkPaymentProcessingCommand c, CancellationToken ct) { Payment? p=await payments.GetByIdAsync(c.PaymentId,ct); if(p is null||p.OrganizationId!=c.OrganizationId)return Result.Failure(PaymentErrors.NotFound); Result r=p.MarkProcessing(c.ActorUserId,clock.UtcNow); if(r.IsFailure)return r; p.SetModifiedAudit(clock.UtcNow,c.ActorUserId); await uow.CommitAsync(ct); return Result.Success(); }
}
internal sealed class RecordPaymentReceivedCommandHandler(IPaymentRepository payments, IStudentBillingAccountRepository accounts, IFundingBillingUnitOfWork uow, IFinancialNotificationGateway notifications, IClock clock) : ICommandHandler<RecordPaymentReceivedCommand>
{
    public async Task<Result> Handle(RecordPaymentReceivedCommand c, CancellationToken ct)
    {
        Payment? p=await payments.GetByIdAsync(c.PaymentId,ct); if(p is null||p.OrganizationId!=c.OrganizationId)return Result.Failure(PaymentErrors.NotFound);
        BillingAccount? a=await accounts.GetByIdAsync(p.BillingAccountId,ct); if(a is null||a.OrganizationId!=c.OrganizationId)return Result.Failure(PaymentErrors.BillingAccountNotFound);
        DateTimeOffset now=clock.UtcNow; Result r=p.RecordPaid(c.ExternalReference,c.ActorUserId,now); if(r.IsFailure)return r;
        Result ar=a.RecordPaymentReceived(p.Amount,p.Currency,c.ActorUserId,now); if(ar.IsFailure)return ar;
        p.SetModifiedAudit(now,c.ActorUserId); a.SetModifiedAudit(now,c.ActorUserId); await uow.CommitAsync(ct);
        await notifications.QueuePaymentReceivedAsync(c.OrganizationId, p.BillingAccountId, p.Amount, p.Currency, ct);
        return Result.Success();
    }
}
internal sealed class MarkPaymentFailedCommandHandler(IPaymentRepository payments, IFundingBillingUnitOfWork uow, IFinancialNotificationGateway notifications, IClock clock) : ICommandHandler<MarkPaymentFailedCommand>
{
    public async Task<Result> Handle(MarkPaymentFailedCommand c,CancellationToken ct){Payment? p=await payments.GetByIdAsync(c.PaymentId,ct);if(p is null||p.OrganizationId!=c.OrganizationId)return Result.Failure(PaymentErrors.NotFound);var now=clock.UtcNow;Result r=p.MarkFailed(c.Reason,c.ActorUserId,now);if(r.IsFailure)return r;p.SetModifiedAudit(now,c.ActorUserId);await uow.CommitAsync(ct);await notifications.QueuePaymentFailedAsync(c.OrganizationId,p.BillingAccountId,p.Amount,p.Currency,c.Reason,ct);return Result.Success();}
}
internal sealed class CancelPaymentCommandHandler(IPaymentRepository payments, IFundingBillingUnitOfWork uow, IClock clock) : ICommandHandler<CancelPaymentCommand>
{
    public async Task<Result> Handle(CancelPaymentCommand c,CancellationToken ct){Payment? p=await payments.GetByIdAsync(c.PaymentId,ct);if(p is null||p.OrganizationId!=c.OrganizationId)return Result.Failure(PaymentErrors.NotFound);var now=clock.UtcNow;Result r=p.Cancel(c.ActorUserId,now);if(r.IsFailure)return r;p.SetModifiedAudit(now,c.ActorUserId);await uow.CommitAsync(ct);return Result.Success();}
}
