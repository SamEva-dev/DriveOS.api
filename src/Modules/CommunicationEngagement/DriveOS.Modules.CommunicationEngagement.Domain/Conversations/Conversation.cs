using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CommunicationEngagement.Domain.Conversations;

/// <summary>
/// BC-15 conversation aggregate. Business modules only provide a RelatedEntity reference;
/// BC-15 owns participants, visibility, read state and lifecycle.
/// </summary>
public sealed class Conversation : AggregateRoot<ConversationId>, IAuditableEntity
{
    private Conversation() { }

    private Conversation(
        ConversationId id,
        OrganizationId organizationId,
        string relatedEntityType,
        Guid relatedEntityId,
        ConversationParticipant[] participants,
        ConversationVisibility visibility) : base(id)
    {
        OrganizationId = organizationId;
        RelatedEntityType = Token(relatedEntityType);
        RelatedEntityId = relatedEntityId;
        Participants = participants;
        Visibility = visibility;
        Status = ConversationStatus.Active;
    }

    public OrganizationId OrganizationId { get; private set; }
    public string RelatedEntityType { get; private set; } = string.Empty;
    public Guid RelatedEntityId { get; private set; }
    public ConversationParticipant[] Participants { get; private set; } = [];
    public ConversationVisibility Visibility { get; private set; }
    public ConversationStatus Status { get; private set; }
    public DateTimeOffset? LastMessageAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<Conversation> Create(
        ConversationId id,
        OrganizationId organizationId,
        string relatedEntityType,
        Guid relatedEntityId,
        IEnumerable<ConversationParticipant> participants,
        ConversationVisibility visibility,
        DateTimeOffset now,
        UserId actor)
    {
        if (id.IsEmpty || organizationId.IsEmpty || relatedEntityId == Guid.Empty)
            return Result.Failure<Conversation>(ConversationErrors.InvalidIdentifier);

        string relation = Token(relatedEntityType);
        if (relation.Length is < 2 or > 80)
            return Result.Failure<Conversation>(ConversationErrors.InvalidRelatedEntity);

        ConversationParticipant[] normalized = participants
            .Where(x => x.PrincipalId != Guid.Empty)
            .GroupBy(x => new { x.Type, x.PrincipalId })
            .Select(g => g.First() with { LastReadAtUtc = null })
            .ToArray();

        if (normalized.Length < 2 ||
            !normalized.Any(x => x.Type == ConversationParticipantType.Organization && x.PrincipalId == organizationId.Value) ||
            !normalized.Any(x => x.Type == ConversationParticipantType.User))
            return Result.Failure<Conversation>(ConversationErrors.InvalidParticipants);

        var conversation = new Conversation(id, organizationId, relation, relatedEntityId, normalized, visibility);
        conversation.SetCreatedAudit(now, actor);
        return Result.Success(conversation);
    }

    public bool HasParticipant(ConversationParticipantType type, Guid principalId) =>
        Participants.Any(x => x.Type == type && x.PrincipalId == principalId);

    public Result MarkMessageSent(DateTimeOffset sentAtUtc, UserId actor)
    {
        if (Status != ConversationStatus.Active)
            return Result.Failure(ConversationErrors.Closed);
        LastMessageAtUtc = sentAtUtc.ToUniversalTime();
        SetModifiedAudit(sentAtUtc, actor);
        return Result.Success();
    }

    public Result MarkRead(ConversationParticipantType type, Guid principalId, DateTimeOffset readAtUtc, UserId actor)
    {
        int index = Array.FindIndex(Participants, x => x.Type == type && x.PrincipalId == principalId);
        if (index < 0) return Result.Failure(ConversationErrors.ParticipantNotFound);
        Participants[index] = Participants[index] with { LastReadAtUtc = readAtUtc.ToUniversalTime() };
        SetModifiedAudit(readAtUtc, actor);
        return Result.Success();
    }

    public Result Close(DateTimeOffset now, UserId actor)
    {
        if (Status == ConversationStatus.Closed) return Result.Failure(ConversationErrors.Closed);
        Status = ConversationStatus.Closed;
        SetModifiedAudit(now, actor);
        return Result.Success();
    }

    public void SetCreatedAudit(DateTimeOffset at, UserId? actor) { CreatedAtUtc = at.ToUniversalTime(); CreatedByUserId = actor; }
    public void SetModifiedAudit(DateTimeOffset at, UserId? actor) { LastModifiedAtUtc = at.ToUniversalTime(); LastModifiedByUserId = actor; }
    private static string Token(string? value) => (value ?? string.Empty).Trim().ToUpperInvariant();
}

public sealed record ConversationParticipant(
    ConversationParticipantType Type,
    Guid PrincipalId,
    DateTimeOffset? LastReadAtUtc);

public enum ConversationParticipantType { User = 1, Organization = 2 }
public enum ConversationVisibility { ParticipantsOnly = 1 }
public enum ConversationStatus { Active = 1, Closed = 2 }
