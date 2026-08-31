using DriveOS.SharedKernel.Identifiers;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Engagements;
using DriveOS.Modules.ProfessionalMarketplace.Application.Notifications;
using DriveOS.Modules.ProfessionalMarketplace.Application.Engagements;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.ProfessionalMarketplace.Application.Persistence;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Invoices;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ServiceStatements;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.Invoices;

public sealed class CreateProfessionalInvoiceCommandHandler(
    IProfessionalInvoiceRepository invoices,IServiceStatementRepository statements,
    IProfessionalMarketplaceUnitOfWork uow,IClock clock):ICommandHandler<CreateProfessionalInvoiceCommand,ProfessionalInvoiceId>
{
    public async Task<Result<ProfessionalInvoiceId>> Handle(CreateProfessionalInvoiceCommand c,CancellationToken ct)
    {
        ServiceStatement? statement=await statements.GetAsync(c.ServiceStatementId,false,ct);
        if(statement is null||statement.ClientOrganizationId!=c.OrganizationId)
            return Result.Failure<ProfessionalInvoiceId>(ServiceStatementErrors.NotFound);

        if(await invoices.ExistsForStatementAsync(statement.Id,ct))
            return Result.Failure<ProfessionalInvoiceId>(ProfessionalInvoiceErrors.DuplicateStatement);

        var created=ProfessionalInvoice.Create(c.Id,statement,c.Mode,c.IssueDate,c.DueDate,c.TaxAmount,
            c.InvoiceNumber,c.BankReference,clock.UtcNow,c.ActorUserId);
        if(created.IsFailure)return Result.Failure<ProfessionalInvoiceId>(created.Error);

        invoices.Add(created.Value);await uow.CommitAsync(ct);return Result.Success(created.Value.Id);
    }
}

public sealed class UpdateProfessionalInvoiceDraftCommandHandler(
    IProfessionalInvoiceRepository invoices,IProfessionalMarketplaceUnitOfWork uow,IClock clock)
    :ICommandHandler<UpdateProfessionalInvoiceDraftCommand>
{
    public async Task<Result> Handle(UpdateProfessionalInvoiceDraftCommand c,CancellationToken ct)
    {
        var invoice=await invoices.GetAsync(c.Id,true,ct);
        if(invoice is null||invoice.ProfessionalProfileId!=c.ProfileId)return Result.Failure(ProfessionalInvoiceErrors.NotFound);
        var r=invoice.UpdateDraft(c.IssueDate,c.DueDate,c.TaxAmount,c.InvoiceNumber,c.BankReference,clock.UtcNow,c.ActorUserId);
        if(r.IsFailure)return r;await uow.CommitAsync(ct);return Result.Success();
    }
}

public sealed class ValidateProfessionalInvoiceCommandHandler(
    IProfessionalInvoiceRepository invoices,IProfessionalMarketplaceUnitOfWork uow,IClock clock)
    :ICommandHandler<ValidateProfessionalInvoiceCommand>
{
    public async Task<Result> Handle(ValidateProfessionalInvoiceCommand c,CancellationToken ct)
    {
        var invoice=await invoices.GetAsync(c.Id,true,ct);
        if(invoice is null||invoice.ProfessionalProfileId!=c.ProfileId)return Result.Failure(ProfessionalInvoiceErrors.NotFound);
        var r=invoice.Validate(clock.UtcNow,c.ActorUserId);
        if(r.IsFailure)return r;await uow.CommitAsync(ct);return Result.Success();
    }
}

public sealed class RequestProfessionalInvoiceCommandHandler(
    IProfessionalInvoiceRepository invoices,
    IServiceStatementRepository statements,
    IProfessionalInvoiceFinanceGateway finance,
    IProfessionalMarketplaceUnitOfWork uow,
    IClock clock):ICommandHandler<RequestProfessionalInvoiceCommand>
{
    public async Task<Result> Handle(RequestProfessionalInvoiceCommand c,CancellationToken ct)
    {
        var invoice=await invoices.GetAsync(c.Id,true,ct);
        if(invoice is null||invoice.ClientOrganizationId!=c.OrganizationId)
            return Result.Failure(ProfessionalInvoiceErrors.NotFound);

        var statement=await statements.GetAsync(invoice.ServiceStatementId,true,ct);
        if(statement is null||statement.ClientOrganizationId!=c.OrganizationId)
            return Result.Failure(ServiceStatementErrors.NotFound);

        // Finance is created first. The operation is idempotent on ProfessionalInvoiceId.
        // If the BC-13 commit fails, retry reuses the same SupplierInvoice and completes safely.
        ProfessionalInvoiceFinanceSnapshot supplier=await finance.EnsureSupplierInvoiceAsync(
            new ProfessionalInvoiceFinanceRequest(
                invoice.Id,
                invoice.ServiceStatementId,
                invoice.ProviderOrganizationId,
                invoice.ClientOrganizationId,
                invoice.InvoiceNumber,
                invoice.IssueDate,
                invoice.DueDate,
                invoice.Currency,
                invoice.Subtotal,
                invoice.TaxAmount,
                invoice.Total,
                invoice.Mode.ToString(),
                c.ActorUserId),
            ct);

        var requested=invoice.RequestFinance(supplier.SupplierInvoiceId,supplier.Status,clock.UtcNow,c.ActorUserId);
        if(requested.IsFailure)return requested;

        var marked=statement.MarkInvoiced(clock.UtcNow,c.ActorUserId);
        if(marked.IsFailure)return marked;

        await uow.CommitAsync(ct);
        return Result.Success();
    }
}

public sealed class SyncProfessionalInvoiceFinanceStatusCommandHandler(
    IProfessionalInvoiceRepository invoices,
    IProfessionalInvoiceFinanceGateway finance,
    IProfessionalEngagementRepository engagements,
    IProfessionalProfileRepository profiles,
    IMarketplaceNotificationGateway notifications,
    IMarketplaceSatisfactionGateway satisfaction,
    IProfessionalMarketplaceUnitOfWork uow,
    IClock clock):ICommandHandler<SyncProfessionalInvoiceFinanceStatusCommand,ProfessionalInvoiceFinanceSnapshot>
{
    public async Task<Result<ProfessionalInvoiceFinanceSnapshot>> Handle(
        SyncProfessionalInvoiceFinanceStatusCommand c,
        CancellationToken ct)
    {
        ProfessionalInvoice? invoice=await invoices.GetAsync(c.Id,true,ct);
        if(invoice is null||invoice.ClientOrganizationId!=c.OrganizationId)
            return Result.Failure<ProfessionalInvoiceFinanceSnapshot>(ProfessionalInvoiceErrors.NotFound);

        ProfessionalInvoiceFinanceSnapshot? snapshot=await finance.GetSupplierInvoiceAsync(invoice.Id,ct);
        if(snapshot is null)
            return Result.Failure<ProfessionalInvoiceFinanceSnapshot>(ProfessionalInvoiceErrors.InvalidFinanceReference);

        Result synced=invoice.SyncFinanceStatus(
            snapshot.Status,
            snapshot.LatestPaymentStatus,
            clock.UtcNow,
            c.ActorUserId);

        if(synced.IsFailure)
            return Result.Failure<ProfessionalInvoiceFinanceSnapshot>(synced.Error);

        // Persist Finance truth first so first-payment discovery is deterministic and retry-safe.
        await uow.CommitAsync(ct);

        if(invoice.PaymentStatus==ProfessionalInvoicePaymentStatus.Paid)
            await HandleInitialIntegrationCompletionAsync(invoice,snapshot,c.ActorUserId,ct);

        return Result.Success(snapshot);
    }

    private async Task HandleInitialIntegrationCompletionAsync(
        ProfessionalInvoice syncedInvoice,
        ProfessionalInvoiceFinanceSnapshot syncedSnapshot,
        UserId actor,
        CancellationToken ct)
    {
        ProfessionalInvoice? firstPaid=await invoices.GetEarliestPaidAsync(syncedInvoice.EngagementId,ct);
        if(firstPaid is null)return;

        ProfessionalEngagement? engagement=await engagements.GetAsync(firstPaid.EngagementId,true,ct);
        if(engagement is null)return;

        ProfessionalInvoiceFinanceSnapshot? firstSnapshot=
            firstPaid.Id==syncedInvoice.Id
                ?syncedSnapshot
                :await finance.GetSupplierInvoiceAsync(firstPaid.Id,ct);

        if(firstSnapshot is null)return;

        ProfessionalInvoicePaymentTimelineItem? paidAttempt=firstSnapshot.PaymentTimeline
            .Where(x=>string.Equals(x.Status,"Paid",StringComparison.OrdinalIgnoreCase))
            .OrderBy(x=>x.PaidAtUtc??DateTimeOffset.MaxValue)
            .ThenBy(x=>x.CreatedAtUtc)
            .FirstOrDefault();

        bool newlyCompleted=engagement.InitialIntegrationCompletedAtUtc is null;

        Result milestone=engagement.CompleteInitialIntegration(
            firstPaid.Id,
            firstSnapshot.SupplierInvoiceId,
            paidAttempt?.AttemptId,
            paidAttempt?.PaymentMethod,
            clock.UtcNow);

        if(milestone.IsFailure)return;

        if(newlyCompleted)
            await uow.CommitAsync(ct);

        ProfessionalProfile? profile=await profiles.GetByIdAsync(engagement.ProfessionalProfileId,ct);
        if(profile is null||profile.UserId is not UserId userId||userId.IsEmpty)
            return;

        await notifications.TryEnqueueAsync(new(
            "User",
            userId.Value,
            engagement.OrganizationId,
            "ENGAGEMENT",
            "professionalMarketplace.notifications.initialIntegrationCompleted",
            $"initial-integration-completed:{engagement.Id.Value}",
            new Dictionary<string,string?>
            {
                ["engagementId"]=engagement.Id.Value.ToString(),
                ["firstPaidInvoiceId"]=firstPaid.Id.Value.ToString(),
                ["paymentMethod"]=engagement.ConfirmedPaymentMethod,
                ["reliableRelationship"]="true"
            },
            "PROFESSIONAL_ENGAGEMENT",
            engagement.Id.Value,
            profile.ProfessionalEmail,
            profile.Languages.FirstOrDefault(x=>x.StartsWith("fr",StringComparison.OrdinalIgnoreCase))??"en",
            actor),ct);

        if(engagement.SatisfactionRequestedAtUtc is null)
        {
            bool requested=await satisfaction.TryRequestPartnerFeedbackAsync(new(
                userId,
                engagement.OrganizationId,
                engagement.Id,
                firstPaid.Id,
                engagement.ConfirmedPaymentMethod,
                profile.Languages.FirstOrDefault(x=>x.StartsWith("fr",StringComparison.OrdinalIgnoreCase))??"en"),ct);

            if(requested)
            {
                ProfessionalEngagement? tracked=await engagements.GetAsync(engagement.Id,true,ct);
                if(tracked is not null&&tracked.SatisfactionRequestedAtUtc is null)
                {
                    tracked.MarkSatisfactionRequested(clock.UtcNow);
                    await uow.CommitAsync(ct);
                }
            }
        }
    }
}


public sealed class ListOrganizationProfessionalInvoicesQueryHandler(
    IProfessionalInvoiceRepository invoices,IProfessionalEngagementRepository engagements)
    :IQueryHandler<ListOrganizationProfessionalInvoicesQuery,IReadOnlyList<ProfessionalInvoiceResponse>>
{
    public async Task<Result<IReadOnlyList<ProfessionalInvoiceResponse>>> Handle(ListOrganizationProfessionalInvoicesQuery q,CancellationToken ct)
    {
        ProfessionalEngagement? engagement=await engagements.GetAsync(q.EngagementId,false,ct);
        if(engagement is null||engagement.OrganizationId!=q.OrganizationId)
            return Result.Failure<IReadOnlyList<ProfessionalInvoiceResponse>>(ProfessionalEngagementErrors.NotFound);
        IReadOnlyList<ProfessionalInvoice> rows=await invoices.ListByEngagementAsync(q.EngagementId,ct);
        return Result.Success<IReadOnlyList<ProfessionalInvoiceResponse>>(rows.Select(Map).ToArray());
    }
    internal static ProfessionalInvoiceResponse Map(ProfessionalInvoice x)=>new(
        x.Id.Value,x.EngagementId.Value,x.ProfessionalProfileId.Value,x.ServiceStatementId.Value,x.ProviderOrganizationId,x.ClientOrganizationId.Value,
        x.Mode.ToString(),x.InvoiceNumber,x.IssueDate,x.DueDate,x.Currency,x.Subtotal,x.TaxAmount,x.Total,x.BankReference,x.Status.ToString(),x.PaymentStatus.ToString(),
        x.FinanceSupplierInvoiceId,x.FinanceSupplierInvoiceStatus,x.FinanceStatusSyncedAtUtc,x.ValidatedAtUtc,x.ValidatedByUserId?.Value,x.RequestedAtUtc,x.CreatedAtUtc);
}

public sealed class GetOrganizationProfessionalInvoiceQueryHandler(IProfessionalInvoiceRepository invoices)
    :IQueryHandler<GetOrganizationProfessionalInvoiceQuery,ProfessionalInvoiceResponse>
{
    public async Task<Result<ProfessionalInvoiceResponse>> Handle(GetOrganizationProfessionalInvoiceQuery q,CancellationToken ct)
    {
        ProfessionalInvoice? x=await invoices.GetAsync(q.Id,false,ct);
        if(x is null||x.ClientOrganizationId!=q.OrganizationId)return Result.Failure<ProfessionalInvoiceResponse>(ProfessionalInvoiceErrors.NotFound);
        return Result.Success(ListOrganizationProfessionalInvoicesQueryHandler.Map(x));
    }
}

public sealed class UpdateOrganizationProfessionalInvoiceDraftCommandHandler(
    IProfessionalInvoiceRepository invoices,IProfessionalMarketplaceUnitOfWork uow,IClock clock)
    :ICommandHandler<UpdateOrganizationProfessionalInvoiceDraftCommand>
{
    public async Task<Result> Handle(UpdateOrganizationProfessionalInvoiceDraftCommand c,CancellationToken ct)
    {
        ProfessionalInvoice? invoice=await invoices.GetAsync(c.Id,true,ct);
        if(invoice is null||invoice.ClientOrganizationId!=c.OrganizationId)return Result.Failure(ProfessionalInvoiceErrors.NotFound);
        Result r=invoice.UpdateDraft(c.IssueDate,c.DueDate,c.TaxAmount,c.InvoiceNumber,c.BankReference,clock.UtcNow,c.ActorUserId);
        if(r.IsFailure)return r;await uow.CommitAsync(ct);return Result.Success();
    }
}

public sealed class ValidateOrganizationProfessionalInvoiceCommandHandler(
    IProfessionalInvoiceRepository invoices,IProfessionalMarketplaceUnitOfWork uow,IClock clock)
    :ICommandHandler<ValidateOrganizationProfessionalInvoiceCommand>
{
    public async Task<Result> Handle(ValidateOrganizationProfessionalInvoiceCommand c,CancellationToken ct)
    {
        ProfessionalInvoice? invoice=await invoices.GetAsync(c.Id,true,ct);
        if(invoice is null||invoice.ClientOrganizationId!=c.OrganizationId)return Result.Failure(ProfessionalInvoiceErrors.NotFound);
        Result r=invoice.Validate(clock.UtcNow,c.ActorUserId);
        if(r.IsFailure)return r;await uow.CommitAsync(ct);return Result.Success();
    }
}

public sealed class UpdateCurrentProfessionalInvoiceDraftCommandHandler(
    IProfessionalInvoiceRepository invoices,IProfessionalProfileRepository profiles,IProfessionalMarketplaceUnitOfWork uow,IClock clock)
    :ICommandHandler<UpdateCurrentProfessionalInvoiceDraftCommand>
{
    public async Task<Result> Handle(UpdateCurrentProfessionalInvoiceDraftCommand c,CancellationToken ct)
    {
        ProfessionalProfile? profile=await profiles.FindByUserAsync(c.UserId,ct);
        if(profile is null)return Result.Failure(ProfessionalProfileErrors.NotFound);
        ProfessionalInvoice? invoice=await invoices.GetAsync(c.Id,true,ct);
        if(invoice is null||invoice.ProfessionalProfileId!=profile.Id)return Result.Failure(ProfessionalInvoiceErrors.NotFound);
        Result r=invoice.UpdateDraft(c.IssueDate,c.DueDate,c.TaxAmount,c.InvoiceNumber,c.BankReference,clock.UtcNow,c.UserId);
        if(r.IsFailure)return r;await uow.CommitAsync(ct);return Result.Success();
    }
}

public sealed class ValidateCurrentProfessionalInvoiceCommandHandler(
    IProfessionalInvoiceRepository invoices,IProfessionalProfileRepository profiles,IProfessionalMarketplaceUnitOfWork uow,IClock clock)
    :ICommandHandler<ValidateCurrentProfessionalInvoiceCommand>
{
    public async Task<Result> Handle(ValidateCurrentProfessionalInvoiceCommand c,CancellationToken ct)
    {
        ProfessionalProfile? profile=await profiles.FindByUserAsync(c.UserId,ct);
        if(profile is null)return Result.Failure(ProfessionalProfileErrors.NotFound);
        ProfessionalInvoice? invoice=await invoices.GetAsync(c.Id,true,ct);
        if(invoice is null||invoice.ProfessionalProfileId!=profile.Id)return Result.Failure(ProfessionalInvoiceErrors.NotFound);
        Result r=invoice.Validate(clock.UtcNow,c.UserId);
        if(r.IsFailure)return r;await uow.CommitAsync(ct);return Result.Success();
    }
}
