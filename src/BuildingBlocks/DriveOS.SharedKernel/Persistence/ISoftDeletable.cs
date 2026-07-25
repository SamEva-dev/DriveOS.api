namespace DriveOS.SharedKernel.Persistence;

public interface ISoftDeletable
{
    bool IsDeleted { get; }

    DateTimeOffset? DeletedAtUtc { get; }

    Guid? DeletedByUserId { get; }
}