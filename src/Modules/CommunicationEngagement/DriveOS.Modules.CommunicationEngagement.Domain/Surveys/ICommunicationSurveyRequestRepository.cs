using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CommunicationEngagement.Domain.Surveys;

public interface ICommunicationSurveyRequestRepository
{
    Task<bool> ExistsByDeduplicationKeyAsync(string deduplicationKey,CancellationToken ct=default);
    void Add(CommunicationSurveyRequest request);
}
