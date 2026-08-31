using DriveOS.Modules.ProfessionalMarketplace.Domain.Invitations;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Configurations;
internal sealed class FreelanceInvitationConfiguration:IEntityTypeConfiguration<FreelanceInvitation>
{
    public void Configure(EntityTypeBuilder<FreelanceInvitation>b)
    {
        b.ToTable("freelance_invitations");b.HasKey(x=>x.Id);
        b.Property(x=>x.Id).HasConversion(x=>x.Value,x=>new FreelanceInvitationId(x)).ValueGeneratedNever();
        b.Property(x=>x.ClientOrganizationId).HasConversion(x=>x.Value,x=>new OrganizationId(x)).IsRequired();
        b.Property(x=>x.BranchId).HasConversion(x=>x==null?(Guid?)null:x.Value.Value,x=>x==null?null:new BranchId(x.Value));
        b.Property(x=>x.MissionId).HasConversion(x=>x==null?(Guid?)null:x.Value.Value,x=>x==null?null:new ProfessionalMissionId(x.Value));
        b.Property(x=>x.ProfessionalProfileId).HasConversion(x=>x==null?(Guid?)null:x.Value.Value,x=>x==null?null:new ProfessionalProfileId(x.Value));
        b.Property(x=>x.InvitedUserId).HasConversion(x=>x==null?(Guid?)null:x.Value.Value,x=>x==null?null:new UserId(x.Value));
        b.Property(x=>x.InvitedByUserId).HasConversion(x=>x.Value,x=>new UserId(x)).IsRequired();
        b.Property(x=>x.AcceptedByUserId).HasConversion(x=>x==null?(Guid?)null:x.Value.Value,x=>x==null?null:new UserId(x.Value));
        b.Property(x=>x.Email).HasMaxLength(320);
        b.Property(x=>x.Phone).HasMaxLength(40);
        b.Property(x=>x.Message).HasMaxLength(2000);
        b.Property(x=>x.TokenHash).HasMaxLength(64);
        b.Property(x=>x.DeclineReason).HasMaxLength(512);
        b.Property(x=>x.Status).HasConversion<string>().HasMaxLength(24).IsRequired();
        b.HasIndex(x=>x.TokenHash).IsUnique();
        b.HasIndex(x=>new{x.ClientOrganizationId,x.Status,x.ExpirationDate});
        b.HasIndex(x=>new{x.Email,x.Status});
        b.HasIndex(x=>new{x.InvitedUserId,x.Status});
        b.Ignore(x=>x.DomainEvents);
    }
}
