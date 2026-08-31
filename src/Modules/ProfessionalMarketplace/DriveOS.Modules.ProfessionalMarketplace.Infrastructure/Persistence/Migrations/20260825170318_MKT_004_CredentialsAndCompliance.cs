using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MKT_004_CredentialsAndCompliance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ComplianceEvaluatedAtUtc",
                schema: "professional",
                table: "professional_profiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MarketplaceVisibility",
                schema: "professional",
                table: "professional_profiles",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TeachingCapabilities",
                schema: "professional",
                table: "professional_profiles",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VerificationBadge",
                schema: "professional",
                table: "professional_profiles",
                type: "character varying(24)",
                maxLength: 24,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "professional_compliance_requirements",
                schema: "professional",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequirementCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    ProfessionalType = table.Column<string>(type: "character varying(48)", maxLength: 48, nullable: false),
                    EvidenceKind = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    EvidenceTypeCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Mandatory = table.Column<bool>(type: "boolean", nullable: false),
                    Blocking = table.Column<bool>(type: "boolean", nullable: false),
                    ApplicableCategoryCodes = table.Column<string[]>(type: "text[]", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_professional_compliance_requirements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "professional_credentials",
                schema: "professional",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessionalProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    CredentialTypeCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    IssuingAuthority = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ReferenceNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ValidUntil = table.Column<DateOnly>(type: "date", nullable: true),
                    CategoryCodes = table.Column<string[]>(type: "text[]", nullable: false),
                    EvidenceDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    VerificationMethod = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    VerifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_professional_credentials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "professional_documents",
                schema: "professional",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessionalProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentTypeCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Mandatory = table.Column<bool>(type: "boolean", nullable: false),
                    IssueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ExpirationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    VerificationMethod = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    VerifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    SupersededById = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_professional_documents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_professional_compliance_requirements_CountryCode_Profession~",
                schema: "professional",
                table: "professional_compliance_requirements",
                columns: new[] { "CountryCode", "ProfessionalType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_professional_compliance_requirements_RequirementCode_Countr~",
                schema: "professional",
                table: "professional_compliance_requirements",
                columns: new[] { "RequirementCode", "CountryCode", "ProfessionalType", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_professional_credentials_ProfessionalProfileId_CredentialTy~",
                schema: "professional",
                table: "professional_credentials",
                columns: new[] { "ProfessionalProfileId", "CredentialTypeCode", "CountryCode" });

            migrationBuilder.CreateIndex(
                name: "IX_professional_documents_ProfessionalProfileId_DocumentRefere~",
                schema: "professional",
                table: "professional_documents",
                columns: new[] { "ProfessionalProfileId", "DocumentReferenceId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "professional_compliance_requirements",
                schema: "professional");

            migrationBuilder.DropTable(
                name: "professional_credentials",
                schema: "professional");

            migrationBuilder.DropTable(
                name: "professional_documents",
                schema: "professional");

            migrationBuilder.DropColumn(
                name: "ComplianceEvaluatedAtUtc",
                schema: "professional",
                table: "professional_profiles");

            migrationBuilder.DropColumn(
                name: "MarketplaceVisibility",
                schema: "professional",
                table: "professional_profiles");

            migrationBuilder.DropColumn(
                name: "TeachingCapabilities",
                schema: "professional",
                table: "professional_profiles");

            migrationBuilder.DropColumn(
                name: "VerificationBadge",
                schema: "professional",
                table: "professional_profiles");
        }
    }
}
