using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MKT_018_ServiceAreasAndMobility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "professional_applications",
                schema: "professional",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OpportunityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessionalProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ProposedRate = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    RateUnit = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: true),
                    Negotiable = table.Column<bool>(type: "boolean", nullable: false),
                    AvailableFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    AvailableUntil = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    DecisionReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    SubmittedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DecidedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_professional_applications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "professional_commercial_offers",
                schema: "professional",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessionalProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProposalId = table.Column<Guid>(type: "uuid", nullable: true),
                    OpportunityId = table.Column<Guid>(type: "uuid", nullable: true),
                    Terms = table.Column<string>(type: "jsonb", nullable: false),
                    Revision = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SentAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    OrganizationAcceptedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ProfessionalAcceptedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FinalizedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    OrganizationAcceptedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProfessionalAcceptedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_professional_commercial_offers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "professional_engagements",
                schema: "professional",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProfessionalProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommercialOfferId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommercialOfferRevision = table.Column<int>(type: "integer", nullable: false),
                    TermsSnapshot = table.Column<string>(type: "jsonb", nullable: false),
                    StartsOn = table.Column<DateOnly>(type: "date", nullable: false),
                    EndsOn = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CompliancePrepared = table.Column<bool>(type: "boolean", nullable: false),
                    ContractPrepared = table.Column<bool>(type: "boolean", nullable: false),
                    AccessPrepared = table.Column<bool>(type: "boolean", nullable: false),
                    SchedulingPrepared = table.Column<bool>(type: "boolean", nullable: false),
                    ActivatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SuspendedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EndedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    StatusReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_professional_engagements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "professional_opportunities",
                schema: "professional",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ProfessionalType = table.Column<string>(type: "character varying(48)", maxLength: 48, nullable: false),
                    TeachingCategoryCodes = table.Column<string[]>(type: "text[]", nullable: false),
                    RequiredLanguageCodes = table.Column<string[]>(type: "text[]", nullable: false),
                    RequiredSpecializationCodes = table.Column<string[]>(type: "text[]", nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    AreaCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    AreaDisplayName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    Latitude = table.Column<decimal>(type: "numeric(8,3)", precision: 8, scale: 3, nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric(9,3)", precision: 9, scale: 3, nullable: true),
                    RadiusKm = table.Column<int>(type: "integer", nullable: true),
                    StartsOn = table.Column<DateOnly>(type: "date", nullable: false),
                    EndsOn = table.Column<DateOnly>(type: "date", nullable: false),
                    TimeWindows = table.Column<string>(type: "jsonb", nullable: false),
                    EstimatedMinutes = table.Column<int>(type: "integer", nullable: true),
                    EngagementType = table.Column<string>(type: "character varying(48)", maxLength: 48, nullable: false),
                    VehicleProvisionMode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    BudgetMin = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    BudgetMax = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    BudgetUnit = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: true),
                    BudgetNegotiable = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    PublishedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosureReason = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_professional_opportunities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "professional_proposals",
                schema: "professional",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProfessionalProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    OpportunityId = table.Column<Guid>(type: "uuid", nullable: true),
                    Subject = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    Message = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    StartsOn = table.Column<DateOnly>(type: "date", nullable: false),
                    EndsOn = table.Column<DateOnly>(type: "date", nullable: false),
                    TeachingCategoryCodes = table.Column<string[]>(type: "text[]", nullable: false),
                    EngagementType = table.Column<string>(type: "character varying(48)", maxLength: 48, nullable: false),
                    VehicleProvisionMode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ProposedRate = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    RateUnit = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: true),
                    Negotiable = table.Column<bool>(type: "boolean", nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    Revision = table.Column<int>(type: "integer", nullable: false),
                    DecisionReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    SentAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RespondedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_professional_proposals", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_professional_applications_OpportunityId_ProfessionalProfile~",
                schema: "professional",
                table: "professional_applications",
                columns: new[] { "OpportunityId", "ProfessionalProfileId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_professional_applications_OrganizationId_Status",
                schema: "professional",
                table: "professional_applications",
                columns: new[] { "OrganizationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_professional_commercial_offers_OrganizationId_Status",
                schema: "professional",
                table: "professional_commercial_offers",
                columns: new[] { "OrganizationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_professional_commercial_offers_ProfessionalProfileId_Status",
                schema: "professional",
                table: "professional_commercial_offers",
                columns: new[] { "ProfessionalProfileId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_professional_engagements_CommercialOfferId",
                schema: "professional",
                table: "professional_engagements",
                column: "CommercialOfferId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_professional_engagements_OrganizationId_Status",
                schema: "professional",
                table: "professional_engagements",
                columns: new[] { "OrganizationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_professional_engagements_ProfessionalProfileId_Status",
                schema: "professional",
                table: "professional_engagements",
                columns: new[] { "ProfessionalProfileId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_professional_opportunities_CountryCode_Status",
                schema: "professional",
                table: "professional_opportunities",
                columns: new[] { "CountryCode", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_professional_opportunities_OrganizationId_Status",
                schema: "professional",
                table: "professional_opportunities",
                columns: new[] { "OrganizationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_professional_proposals_OrganizationId_ProfessionalProfileId~",
                schema: "professional",
                table: "professional_proposals",
                columns: new[] { "OrganizationId", "ProfessionalProfileId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_professional_proposals_ProfessionalProfileId_Status",
                schema: "professional",
                table: "professional_proposals",
                columns: new[] { "ProfessionalProfileId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "professional_applications",
                schema: "professional");

            migrationBuilder.DropTable(
                name: "professional_commercial_offers",
                schema: "professional");

            migrationBuilder.DropTable(
                name: "professional_engagements",
                schema: "professional");

            migrationBuilder.DropTable(
                name: "professional_opportunities",
                schema: "professional");

            migrationBuilder.DropTable(
                name: "professional_proposals",
                schema: "professional");
        }
    }
}
