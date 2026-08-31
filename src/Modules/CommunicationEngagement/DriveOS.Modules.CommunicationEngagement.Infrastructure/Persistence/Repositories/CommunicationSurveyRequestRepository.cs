using DriveOS.Modules.CommunicationEngagement.Domain.Surveys;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.CommunicationEngagement.Infrastructure.Persistence.Repositories;

internal sealed class CommunicationSurveyRequestRepository(
    CommunicationEngagementDbContext db):ICommunicationSurveyRequestRepository
{
    public Task<bool> ExistsByDeduplicationKeyAsync(string deduplicationKey,CancellationToken ct=default)=>
        db.SurveyRequests.AsNoTracking().AnyAsync(x=>x.DeduplicationKey==deduplicationKey,ct);

    public void Add(CommunicationSurveyRequest request)=>db.SurveyRequests.Add(request);
}
