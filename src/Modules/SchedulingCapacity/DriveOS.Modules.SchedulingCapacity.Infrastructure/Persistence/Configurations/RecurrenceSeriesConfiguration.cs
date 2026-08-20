using DriveOS.Modules.SchedulingCapacity.Domain.Recurrences;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence.Configurations;

internal sealed class RecurrenceSeriesConfiguration : IEntityTypeConfiguration<RecurrenceSeries>
{
    public void Configure(EntityTypeBuilder<RecurrenceSeries> builder)
    {
        builder.ToTable("recurrence_series"); builder.HasKey(x=>x.Id);
        builder.Property(x=>x.Id).HasConversion(x=>x.Value,x=>new RecurrenceSeriesId(x));
        builder.Property(x=>x.OrganizationId).HasConversion(x=>x.Value,x=>new OrganizationId(x)).IsRequired();
        builder.Property(x=>x.BranchId).HasConversion(x=>x.HasValue?x.Value.Value:(Guid?)null,x=>x.HasValue?new BranchId(x.Value):null);
        builder.Property(x=>x.TargetType).HasConversion<string>().HasMaxLength(32); builder.Property(x=>x.Frequency).HasConversion<string>().HasMaxLength(16);
        builder.Property(x=>x.ResourceSelectionPolicy).HasConversion<string>().HasMaxLength(32); builder.Property(x=>x.TimeZoneId).HasMaxLength(100); builder.Property(x=>x.Title).HasMaxLength(200); builder.Property(x=>x.DaysOfWeek).HasMaxLength(32);
        builder.Property(x=>x.CreatedByUserId).HasConversion(x=>x.HasValue?x.Value.Value:(Guid?)null,x=>x.HasValue?new UserId(x.Value):null);
        builder.Property(x=>x.LastModifiedByUserId).HasConversion(x=>x.HasValue?x.Value.Value:(Guid?)null,x=>x.HasValue?new UserId(x.Value):null);
        builder.HasMany(x=>x.Occurrences).WithOne().HasForeignKey(x=>x.SeriesId).OnDelete(DeleteBehavior.Cascade); builder.Navigation(x=>x.Occurrences).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasMany(x=>x.Resources).WithOne().HasForeignKey(x=>x.SeriesId).OnDelete(DeleteBehavior.Cascade); builder.Navigation(x=>x.Resources).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(x=>new{x.OrganizationId,x.StartDate,x.EndDate});
    }
}
