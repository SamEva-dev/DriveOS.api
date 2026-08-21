using DriveOS.Modules.TrainingDelivery.Domain.GroupSessions;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.TrainingDelivery.Infrastructure.Persistence.Configurations;

internal sealed class GroupTrainingSessionConfiguration : IEntityTypeConfiguration<GroupTrainingSession>
{
    public void Configure(EntityTypeBuilder<GroupTrainingSession> b)
    {
        b.ToTable("group_training_sessions"); b.HasKey(x=>x.Id);
        b.Property(x=>x.Id).HasConversion(x=>x.Value,x=>new GroupTrainingSessionId(x));
        b.Property(x=>x.OrganizationId).HasConversion(x=>x.Value,x=>new OrganizationId(x)).IsRequired();
        b.Property(x=>x.SourceBookingId).HasConversion(x=>x.Value,x=>new BookingId(x)).IsRequired();
        b.Property(x=>x.TrainerId).HasConversion(x=>x.Value,x=>new UserId(x)).IsRequired();
        b.Property(x=>x.BranchId).HasConversion(x=>x.HasValue?x.Value.Value:(Guid?)null,x=>x.HasValue?new BranchId(x.Value):null);
        b.Property(x=>x.Program).HasMaxLength(300).IsRequired(); b.Property(x=>x.RoomName).HasMaxLength(300); b.Property(x=>x.SharedObjectives).HasMaxLength(2000); b.Property(x=>x.CollectiveReport).HasMaxLength(4000);
        b.HasMany(x=>x.Participants).WithOne().HasForeignKey(x=>x.GroupTrainingSessionId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x=>x.Operations).WithOne().HasForeignKey(x=>x.GroupTrainingSessionId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x=>new{x.OrganizationId,x.SourceBookingId}).IsUnique(); b.HasIndex(x=>new{x.OrganizationId,x.PlannedStartAtUtc});
    }
}

internal sealed class GroupTrainingSessionParticipantConfiguration : IEntityTypeConfiguration<GroupTrainingSessionParticipant>
{
    public void Configure(EntityTypeBuilder<GroupTrainingSessionParticipant> b)
    {
        b.ToTable("group_training_session_participants"); b.HasKey(x=>x.Id);
        b.Property(x=>x.Id).HasConversion(x=>x.Value,x=>new GroupTrainingSessionParticipantId(x));
        b.Property(x=>x.GroupTrainingSessionId).HasConversion(x=>x.Value,x=>new GroupTrainingSessionId(x));
        b.Property(x=>x.StudentId).HasConversion(x=>x.Value,x=>new PersonId(x));
        b.Property(x=>x.IndividualObservation).HasMaxLength(1000);
        b.HasIndex(x=>new{x.GroupTrainingSessionId,x.StudentId}).IsUnique();
    }
}

internal sealed class GroupTrainingSessionOperationConfiguration : IEntityTypeConfiguration<GroupTrainingSessionOperation>
{
    public void Configure(EntityTypeBuilder<GroupTrainingSessionOperation> b)
    {
        b.ToTable("group_training_session_operations"); b.HasKey(x=>x.Id);
        b.Property(x=>x.Id).HasConversion(x=>x.Value,x=>new GroupTrainingSessionOperationId(x));
        b.Property(x=>x.GroupTrainingSessionId).HasConversion(x=>x.Value,x=>new GroupTrainingSessionId(x));
        b.HasIndex(x=>new{x.GroupTrainingSessionId,x.OperationId}).IsUnique();
    }
}
