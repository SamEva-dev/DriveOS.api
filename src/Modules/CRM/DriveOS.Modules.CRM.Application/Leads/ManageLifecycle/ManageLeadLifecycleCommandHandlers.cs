using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.CRM.Application.Abstractions.Persistence;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Application.Leads.ManageLifecycle;

internal abstract class LeadLifecycleHandler(ILeadRepository leads, ICrmUnitOfWork unitOfWork)
{
    protected async Task<Result> Execute(
        OrganizationId organizationId,
        LeadId leadId,
        Func<Lead, Result> mutation,
        CancellationToken cancellationToken
    )
    {
        Lead? lead = await leads.GetByIdForUpdateAsync(organizationId, leadId, cancellationToken);
        if (lead is null)
            return Result.Failure(LeadErrors.NotFound);
        Result result = mutation(lead);
        if (result.IsFailure)
            return result;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class CloseLeadCommandHandler(
    ILeadRepository leads,
    ICrmUnitOfWork uow,
    IClock clock
) : LeadLifecycleHandler(leads, uow), ICommandHandler<CloseLeadCommand>
{
    public Task<Result> Handle(CloseLeadCommand c, CancellationToken ct) =>
        Execute(
            c.OrganizationId,
            c.LeadId,
            l => l.Close(c.Decision, c.Reason, c.Comment, clock.UtcNow),
            ct
        );
}

internal sealed class SetLeadDormantCommandHandler(
    ILeadRepository leads,
    ICrmUnitOfWork uow,
    IClock clock
) : LeadLifecycleHandler(leads, uow), ICommandHandler<SetLeadDormantCommand>
{
    public Task<Result> Handle(SetLeadDormantCommand c, CancellationToken ct) =>
        Execute(
            c.OrganizationId,
            c.LeadId,
            l =>
                l.SetDormant(
                    c.Reason,
                    c.ResumeAtUtc,
                    c.ResponsibleUserId,
                    c.CampaignCode,
                    c.Comment,
                    clock.UtcNow
                ),
            ct
        );
}

internal sealed class ReferLeadToPartnerCommandHandler(
    ILeadRepository leads,
    ICrmUnitOfWork uow,
    IClock clock
) : LeadLifecycleHandler(leads, uow), ICommandHandler<ReferLeadToPartnerCommand>
{
    public Task<Result> Handle(ReferLeadToPartnerCommand c, CancellationToken ct) =>
        Execute(
            c.OrganizationId,
            c.LeadId,
            l =>
                l.ReferToPartner(
                    c.PartnerName,
                    c.SharedDataDescription,
                    c.ConsentCollectedAtUtc,
                    c.Comment,
                    clock.UtcNow
                ),
            ct
        );
}

internal sealed class ReopenLeadCommandHandler(
    ILeadRepository leads,
    ICrmUnitOfWork uow,
    IClock clock
) : LeadLifecycleHandler(leads, uow), ICommandHandler<ReopenLeadCommand>
{
    public Task<Result> Handle(ReopenLeadCommand c, CancellationToken ct) =>
        Execute(c.OrganizationId, c.LeadId, l => l.Reopen(c.Comment, clock.UtcNow), ct);
}
