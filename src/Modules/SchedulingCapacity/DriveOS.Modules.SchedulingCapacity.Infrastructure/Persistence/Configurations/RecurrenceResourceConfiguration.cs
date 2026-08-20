using DriveOS.Modules.SchedulingCapacity.Domain.Recurrences;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence.Configurations;
internal sealed class RecurrenceResourceConfiguration : IEntityTypeConfiguration<RecurrenceResource>
{
    public void Configure(EntityTypeBuilder<RecurrenceResource> b) { b.ToTable("recurrence_resources"); b.HasKey(x=>x.Id); b.Property(x=>x.Id).HasConversion(x=>x.Value,x=>new RecurrenceResourceId(x)); b.Property(x=>x.SeriesId).HasConversion(x=>x.Value,x=>new RecurrenceSeriesId(x)); b.Property(x=>x.CalendarResourceId).HasConversion(x=>x.Value,x=>new CalendarResourceId(x)); b.HasIndex(x=>new{x.SeriesId,x.CalendarResourceId}).IsUnique(); }
}
