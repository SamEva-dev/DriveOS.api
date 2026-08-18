using DriveOS.Modules.Contracts.Domain.ContractDocuments;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DriveOS.Modules.Contracts.Infrastructure.Persistence.Configurations;
internal sealed class ContractDocumentConfiguration:IEntityTypeConfiguration<ContractDocument>
{
 public void Configure(EntityTypeBuilder<ContractDocument>b){b.ToTable("contract_documents");b.HasKey(x=>x.Id);b.Property(x=>x.Id).HasConversion(x=>x.Value,x=>new ContractDocumentId(x));b.Property(x=>x.OrganizationId).HasConversion(x=>x.Value,x=>new OrganizationId(x)).IsRequired();b.Property(x=>x.ContractId).HasConversion(x=>x.Value,x=>new TrainingContractId(x)).IsRequired();b.HasIndex(x=>new{x.OrganizationId,x.ContractId});b.Property(x=>x.Title).HasMaxLength(200).IsRequired();b.Property(x=>x.RetentionLegalBasis).HasMaxLength(300);b.Property(x=>x.CreatedByUserId).HasConversion(x=>x==null?(Guid?)null:x.Value.Value,x=>x.HasValue?new UserId(x.Value):null);b.Property(x=>x.LastModifiedByUserId).HasConversion(x=>x==null?(Guid?)null:x.Value.Value,x=>x.HasValue?new UserId(x.Value):null);b.Property(x=>x.ArchivedByUserId).HasConversion(x=>x==null?(Guid?)null:x.Value.Value,x=>x.HasValue?new UserId(x.Value):null);b.HasMany(x=>x.Versions).WithOne().HasForeignKey(x=>x.DocumentId).OnDelete(DeleteBehavior.Cascade);}
}
internal sealed class ContractDocumentVersionConfiguration:IEntityTypeConfiguration<ContractDocumentVersion>
{public void Configure(EntityTypeBuilder<ContractDocumentVersion>b){b.ToTable("contract_document_versions");b.HasKey(x=>x.Id);b.Property(x=>x.Id).HasConversion(x=>x.Value,x=>new ContractDocumentVersionId(x));b.Property(x=>x.DocumentId).HasConversion(x=>x.Value,x=>new ContractDocumentId(x));b.HasIndex(x=>new{x.DocumentId,x.VersionNumber}).IsUnique();b.Property(x=>x.FileName).HasMaxLength(255).IsRequired();b.Property(x=>x.ContentType).HasMaxLength(120).IsRequired();b.Property(x=>x.StorageReference).HasMaxLength(700).IsRequired();b.Property(x=>x.Sha256).HasMaxLength(64).IsRequired();b.Property(x=>x.UploadedByUserId).HasConversion(x=>x.Value,x=>new UserId(x));}}
