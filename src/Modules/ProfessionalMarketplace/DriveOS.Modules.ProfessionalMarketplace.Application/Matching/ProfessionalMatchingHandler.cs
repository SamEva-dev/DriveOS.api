using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.Matching;

public sealed class MatchProfessionalsForOpportunityQueryHandler(IProfessionalMatchingReadService matching)
    :IQueryHandler<MatchProfessionalsForOpportunityQuery,ProfessionalMatchResult[]>
{
    public async Task<Result<ProfessionalMatchResult[]>> Handle(MatchProfessionalsForOpportunityQuery query,CancellationToken ct)
        =>Result.Success(await matching.MatchAsync(query.OpportunityId,query.OrganizationId,query.Limit,ct));
}
