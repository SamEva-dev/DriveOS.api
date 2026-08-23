using DriveOS.Modules.Workforce.Domain.Offboarding;using DriveOS.SharedKernel.Identifiers;using Microsoft.EntityFrameworkCore;using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DriveOS.Modules.Workforce.Infrastructure.Persistence.Configurations;
internal sealed class OffboardingProcessConfiguration:IEntityTypeConfiguration<OffboardingProcess>
{
 public void Configure(EntityTypeBuilder<OffboardingProcess> b)
 {
  b.ToTable("offboarding_processes");b.HasKey(x=>x.Id);b.Property(x=>x.Id).HasConversion(x=>x.Value,x=>new OffboardingProcessId(x)).ValueGeneratedNever();b.Property(x=>x.OrganizationId).HasConversion(x=>x.Value,x=>new OrganizationId(x));b.Property(x=>x.EmployeeId).HasConversion(x=>x.Value,x=>new EmployeeId(x));b.Property(x=>x.Status).HasConversion<string>().HasMaxLength(32);b.Property(x=>x.Reason).HasMaxLength(1000).IsRequired();b.Property(x=>x.CompletedByUserId).HasConversion(x=>x.HasValue?x.Value.Value:(Guid?)null,x=>x.HasValue?new UserId(x.Value):null);b.Property(x=>x.CancelledByUserId).HasConversion(x=>x.HasValue?x.Value.Value:(Guid?)null,x=>x.HasValue?new UserId(x.Value):null);b.Property(x=>x.CreatedByUserId).HasConversion(x=>x.HasValue?x.Value.Value:(Guid?)null,x=>x.HasValue?new UserId(x.Value):null);b.Property(x=>x.LastModifiedByUserId).HasConversion(x=>x.HasValue?x.Value.Value:(Guid?)null,x=>x.HasValue?new UserId(x.Value):null);b.Property(x=>x.CancellationReason).HasMaxLength(1000);
  b.HasIndex(x=>new{x.OrganizationId,x.EmployeeId,x.Status});
  b.OwnsMany(x=>x.Items,i=>{i.ToTable("offboarding_checklist_items");i.WithOwner().HasForeignKey("offboarding_process_id");i.HasKey(x=>x.Id);i.Property(x=>x.Id).HasConversion(x=>x.Value,x=>new OffboardingChecklistItemId(x)).ValueGeneratedNever();i.Property(x=>x.Kind).HasConversion<string>().HasMaxLength(64);i.Property(x=>x.Status).HasConversion<string>().HasMaxLength(32);i.Property(x=>x.Note).HasMaxLength(1000);i.Property(x=>x.WaiverReason).HasMaxLength(1000);i.Property(x=>x.ResolvedByUserId).HasConversion(x=>x.HasValue?x.Value.Value:(Guid?)null,x=>x.HasValue?new UserId(x.Value):null);i.HasIndex("offboarding_process_id",nameof(OffboardingChecklistItem.Kind)).IsUnique();});
  b.Navigation(x=>x.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
 }
}
