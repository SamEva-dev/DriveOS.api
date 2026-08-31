using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CommunicationEngagement.Application.Surveys;

public sealed record EnqueueCommunicationSurveyRequest(
    UserId RecipientUserId,
    OrganizationId OrganizationId,
    string SurveyType,
    string DeduplicationKey,
    string RelatedEntityType,
    Guid RelatedEntityId,
    IReadOnlyDictionary<string,string?> Parameters);

public interface ICommunicationSurveyRequestWriter
{
    Task<bool> TryEnqueueAsync(
        EnqueueCommunicationSurveyRequest request,
        CancellationToken cancellationToken=default);
}
