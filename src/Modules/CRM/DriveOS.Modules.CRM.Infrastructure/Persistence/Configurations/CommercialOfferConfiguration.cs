using DriveOS.Modules.CRM.Domain.Assessments;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.Modules.CRM.Domain.Offers;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.CRM.Infrastructure.Persistence.Configurations;

internal sealed class CommercialOfferConfiguration : IEntityTypeConfiguration<CommercialOffer>
{
    public void Configure(EntityTypeBuilder<CommercialOffer> b)
    {
        b.ToTable("commercial_offers");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id)
            .HasConversion(x => x.Value, x => new CommercialOfferId(x))
            .HasColumnName("id")
            .ValueGeneratedNever();
        b.Property(x => x.OrganizationId)
            .HasConversion(x => x.Value, x => new OrganizationId(x))
            .HasColumnName("organization_id");
        b.Property(x => x.LeadId)
            .HasConversion(x => x.Value, x => new LeadId(x))
            .HasColumnName("lead_id");
        b.Property(x => x.AssessmentSessionId)
            .HasConversion(x => x.Value, x => new AssessmentSessionId(x))
            .HasColumnName("assessment_session_id");
        b.Property(x => x.AssessmentRevision).HasColumnName("assessment_revision");
        b.Property(x => x.BranchId)
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new BranchId(x.Value) : null
            )
            .HasColumnName("branch_id");
        b.Property(x => x.Version).HasColumnName("version");
        b.Property(x => x.TrainingCode).HasColumnName("training_code").HasMaxLength(100);
        b.Property(x => x.CatalogAmount).HasColumnName("catalog_amount").HasPrecision(18, 2);
        b.Property(x => x.DiscountAmount).HasColumnName("discount_amount").HasPrecision(18, 2);
        b.Property(x => x.TaxAmount).HasColumnName("tax_amount").HasPrecision(18, 2);
        b.Property(x => x.Amount).HasColumnName("amount").HasPrecision(18, 2);
        b.Property(x => x.EstimatedFundingAmount)
            .HasColumnName("estimated_funding_amount")
            .HasPrecision(18, 2);
        b.Property(x => x.ProspectRemainingAmount)
            .HasColumnName("prospect_remaining_amount")
            .HasPrecision(18, 2);
        b.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3);
        b.Property(x => x.ValidUntilUtc).HasColumnName("valid_until_utc");
        b.Property(x => x.FinancingNotes).HasColumnName("financing_notes").HasMaxLength(2000);
        b.Property(x => x.Conditions).HasColumnName("conditions").HasMaxLength(8000);
        b.Property(x => x.InternalNotes).HasColumnName("internal_notes").HasMaxLength(4000);
        b.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(24);
        b.Property(x => x.SentAtUtc).HasColumnName("sent_at_utc");
        b.Property(x => x.DeliveryStatus)
            .HasColumnName("delivery_status")
            .HasConversion<string>()
            .HasMaxLength(24);
        b.Property(x => x.DeliveryChannel)
            .HasColumnName("delivery_channel")
            .HasConversion<string>()
            .HasMaxLength(24);
        b.Property(x => x.RecipientSnapshotJson)
            .HasColumnName("recipient_snapshot_json")
            .HasColumnType("jsonb");
        b.Property(x => x.DeliverySubject).HasColumnName("delivery_subject").HasMaxLength(250);
        b.Property(x => x.DeliveryMessage).HasColumnName("delivery_message").HasMaxLength(8000);
        b.Property(x => x.DeliveryLanguage).HasColumnName("delivery_language").HasMaxLength(10);
        b.Property(x => x.DocumentReference).HasColumnName("document_reference").HasMaxLength(500);
        b.Property(x => x.AttachmentSnapshotJson)
            .HasColumnName("attachment_snapshot_json")
            .HasColumnType("jsonb");
        b.Property(x => x.SecureLinkTokenHash)
            .HasColumnName("secure_link_token_hash")
            .HasMaxLength(64);
        b.Property(x => x.SecureLinkExpiresAtUtc).HasColumnName("secure_link_expires_at_utc");
        b.Property(x => x.SecureLinkRevokedAtUtc).HasColumnName("secure_link_revoked_at_utc");
        b.Property(x => x.DeliveryAttemptCount).HasColumnName("delivery_attempt_count");
        b.Property(x => x.ViewedAtUtc).HasColumnName("viewed_at_utc");
        b.Property(x => x.LastViewedAtUtc).HasColumnName("last_viewed_at_utc");
        b.Property(x => x.ViewCount).HasColumnName("view_count");
        b.Property(x => x.LastContactAtUtc).HasColumnName("last_contact_at_utc");
        b.Property(x => x.NextFollowUpAtUtc).HasColumnName("next_follow_up_at_utc");
        b.Property(x => x.DecidedAtUtc).HasColumnName("decided_at_utc");
        b.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        b.Property(x => x.CreatedByUserId)
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null
            )
            .HasColumnName("created_by_user_id");
        b.Property(x => x.LastModifiedAtUtc).HasColumnName("last_modified_at_utc");
        b.Property(x => x.LastModifiedByUserId)
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null
            )
            .HasColumnName("last_modified_by_user_id");
        b.HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey(x => x.OfferId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.HasMany(x => x.Interactions)
            .WithOne()
            .HasForeignKey(x => x.OfferId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Interactions).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.HasIndex(x => new
            {
                x.OrganizationId,
                x.LeadId,
                x.Version,
            })
            .IsUnique();
        b.HasIndex(x => new { x.OrganizationId, x.AssessmentSessionId });
        b.HasIndex(x => x.SecureLinkTokenHash)
            .IsUnique()
            .HasFilter("secure_link_token_hash IS NOT NULL");
        b.Ignore(x => x.DomainEvents);
    }
}

internal sealed class OfferInteractionConfiguration : IEntityTypeConfiguration<OfferInteraction>
{
    public void Configure(EntityTypeBuilder<OfferInteraction> b)
    {
        b.ToTable("commercial_offer_interactions");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id)
            .HasConversion(x => x.Value, x => new OfferInteractionId(x))
            .HasColumnName("id")
            .ValueGeneratedNever();
        b.Property(x => x.OfferId)
            .HasConversion(x => x.Value, x => new CommercialOfferId(x))
            .HasColumnName("offer_id");
        b.Property(x => x.Type)
            .HasConversion<string>()
            .HasColumnName("interaction_type")
            .HasMaxLength(40);
        b.Property(x => x.OccurredAtUtc).HasColumnName("occurred_at_utc");
        b.Property(x => x.ActorUserId)
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null
            )
            .HasColumnName("actor_user_id");
        b.Property(x => x.Summary).HasColumnName("summary").HasMaxLength(4000);
        b.Property(x => x.MetadataJson).HasColumnName("metadata_json").HasColumnType("jsonb");
        b.HasIndex(x => new { x.OfferId, x.OccurredAtUtc });
    }
}

internal sealed class CommercialOfferLineConfiguration
    : IEntityTypeConfiguration<CommercialOfferLine>
{
    public void Configure(EntityTypeBuilder<CommercialOfferLine> b)
    {
        b.ToTable("commercial_offer_lines");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(x => x.Value, x => new CommercialOfferLineId(x))
            .ValueGeneratedNever();
        b.Property(x => x.OfferId)
            .HasConversion(x => x.Value, x => new CommercialOfferId(x))
            .HasColumnName("offer_id");
        b.Property(x => x.Type).HasConversion<string>().HasColumnName("line_type").HasMaxLength(40);
        b.Property(x => x.ServiceId)
            .HasColumnName("service_id")
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new ServiceId(x.Value) : null
            );
        b.Property(x => x.Description).HasColumnName("description").HasMaxLength(500);
        b.Property(x => x.Quantity).HasColumnName("quantity").HasPrecision(18, 2);
        b.Property(x => x.Unit).HasColumnName("unit").HasMaxLength(30);
        b.Property(x => x.UnitPrice).HasColumnName("unit_price").HasPrecision(18, 2);
        b.Property(x => x.DiscountAmount).HasColumnName("discount_amount").HasPrecision(18, 2);
        b.Property(x => x.TaxRate).HasColumnName("tax_rate").HasPrecision(7, 4);
        b.Property(x => x.NetAmount).HasColumnName("net_amount").HasPrecision(18, 2);
        b.Property(x => x.TaxAmount).HasColumnName("tax_amount").HasPrecision(18, 2);
        b.Property(x => x.TotalAmount).HasColumnName("total_amount").HasPrecision(18, 2);
        b.Property(x => x.Mandatory).HasColumnName("mandatory");
        b.Property(x => x.PriceSource)
            .HasConversion<string>()
            .HasColumnName("price_source")
            .HasMaxLength(32);
        b.Property(x => x.ManualOverrideReason)
            .HasColumnName("manual_override_reason")
            .HasMaxLength(1000);
        b.HasIndex(x => new { x.OfferId, x.Type });
    }
}
