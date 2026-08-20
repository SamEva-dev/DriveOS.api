using DriveOS.Modules.SchedulingCapacity.Domain.Recurrences;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence.Configurations;
internal sealed class RecurrenceOccurrenceConfiguration : IEntityTypeConfiguration<RecurrenceOccurrence>
{
    public void Configure(EntityTypeBuilder<RecurrenceOccurrence> b) { b.ToTable("recurrence_occurrences"); b.HasKey(x=>x.Id); b.Property(x=>x.Id).HasConversion(x=>x.Value,x=>new RecurrenceOccurrenceId(x)); b.Property(x=>x.SeriesId).HasConversion(x=>x.Value,x=>new RecurrenceSeriesId(x)); b.Property(x=>x.Status).HasConversion<string>().HasMaxLength(24); b.Property(x=>x.ExceptionReason).HasMaxLength(500); b.HasIndex(x=>new{x.SeriesId,x.ScheduledDate,x.Revision}).IsUnique(); }
}
