using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Application.Abstractions.Persistence;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Application.Leads.GetLead;

public sealed class GetLeadQueryHandler(ILeadReadService leadReadService)
    : IQueryHandler<GetLeadQuery, LeadResponse>
{
    public async Task<Result<LeadResponse>> Handle(
        GetLeadQuery query,
        CancellationToken cancellationToken)
    {
        LeadResponse? lead = await leadReadService.GetByIdAsync(
            query.OrganizationId,
            query.LeadId,
            cancellationToken);

        return lead is null
            ? Result.Failure<LeadResponse>(LeadErrors.NotFound)
            : Result.Success(lead);
    }
}
