using System.Text.Json;
using DriveOS.Modules.CommunicationEngagement.Application.Persistence;
using DriveOS.Modules.CommunicationEngagement.Application.Surveys;
using DriveOS.Modules.CommunicationEngagement.Domain.Surveys;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CommunicationEngagement.Infrastructure.Surveys;

internal sealed class CommunicationSurveyRequestWriter(
    ICommunicationSurveyRequestRepository repository,
    ICommunicationEngagementUnitOfWork uow):ICommunicationSurveyRequestWriter
{
    public async Task<bool> TryEnqueueAsync(
        EnqueueCommunicationSurveyRequest request,
        CancellationToken cancellationToken=default)
    {
        if(await repository.ExistsByDeduplicationKeyAsync(request.DeduplicationKey,cancellationToken))
            return true;

        var created=CommunicationSurveyRequest.Create(
            new CommunicationSurveyRequestId(Guid.NewGuid()),
            request.RecipientUserId,
            request.OrganizationId,
            request.SurveyType,
            request.DeduplicationKey,
            request.RelatedEntityType,
            request.RelatedEntityId,
            JsonSerializer.Serialize(request.Parameters),
            DateTimeOffset.UtcNow);

        if(created.IsFailure)
            return false;

        repository.Add(created.Value);
        await uow.CommitAsync(cancellationToken);
        return true;
    }
}
