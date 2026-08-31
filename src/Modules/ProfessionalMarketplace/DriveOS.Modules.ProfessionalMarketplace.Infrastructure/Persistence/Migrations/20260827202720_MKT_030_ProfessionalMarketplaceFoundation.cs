using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MKT_030_ProfessionalMarketplaceFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "external_access_grants",
                schema: "professional",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EngagementId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessionalProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResourceType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Permission = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    GrantedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    RevokedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RevokedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RevocationReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_external_access_grants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "professional_invoices",
                schema: "professional",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EngagementId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessionalProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceStatementId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderOrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientOrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Mode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    InvoiceNumber = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    IssueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BankReference = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    PaymentStatus = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    FinanceSupplierInvoiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    FinanceSupplierInvoiceStatus = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    FinanceStatusSyncedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ValidatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ValidatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_professional_invoices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "professional_missions",
                schema: "professional",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EngagementId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessionalProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    Description = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    StartsOn = table.Column<DateOnly>(type: "date", nullable: false),
                    EndsOn = table.Column<DateOnly>(type: "date", nullable: false),
                    TeachingCategoryCodes = table.Column<string[]>(type: "text[]", nullable: false),
                    EstimatedMinutes = table.Column<int>(type: "integer", nullable: true),
                    VehicleProvisionMode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TimeWindows = table.Column<string>(type: "jsonb", nullable: false),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    ProposedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RespondedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ActivatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    StatusReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_professional_missions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "professional_review_reports",
                schema: "professional",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Details = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    Resolution = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ResolvedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ResolvedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_professional_review_reports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "professional_reviews",
                schema: "professional",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessionalProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    EngagementId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Ratings = table.Column<string>(type: "jsonb", nullable: false),
                    Comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    ProfessionalResponse = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RespondedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RespondedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    HiddenAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    HiddenByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ModerationReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_professional_reviews", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "service_entries",
                schema: "professional",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EngagementId = table.Column<Guid>(type: "uuid", nullable: false),
                    MissionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProfessionalProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ServiceCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    QuantityMinutes = table.Column<int>(type: "integer", nullable: false),
                    UnitRate = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    SubmittedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReviewedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "service_statements",
                schema: "professional",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EngagementId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessionalProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientOrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderOrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodStart = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodEnd = table.Column<DateOnly>(type: "date", nullable: false),
                    Lines = table.Column<string>(type: "jsonb", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SubmittedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReviewedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_statements", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_external_access_grants_EngagementId_ResourceType_ResourceId~",
                schema: "professional",
                table: "external_access_grants",
                columns: new[] { "EngagementId", "ResourceType", "ResourceId", "Permission", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_external_access_grants_EngagementId_Status",
                schema: "professional",
                table: "external_access_grants",
                columns: new[] { "EngagementId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_external_access_grants_ProfessionalProfileId_OrganizationId~",
                schema: "professional",
                table: "external_access_grants",
                columns: new[] { "ProfessionalProfileId", "OrganizationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_external_access_grants_ResourceType_ResourceId_Permission_S~",
                schema: "professional",
                table: "external_access_grants",
                columns: new[] { "ResourceType", "ResourceId", "Permission", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_professional_invoices_ClientOrganizationId_Status_DueDate",
                schema: "professional",
                table: "professional_invoices",
                columns: new[] { "ClientOrganizationId", "Status", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_professional_invoices_FinanceSupplierInvoiceId",
                schema: "professional",
                table: "professional_invoices",
                column: "FinanceSupplierInvoiceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_professional_invoices_InvoiceNumber_ProviderOrganizationId",
                schema: "professional",
                table: "professional_invoices",
                columns: new[] { "InvoiceNumber", "ProviderOrganizationId" });

            migrationBuilder.CreateIndex(
                name: "IX_professional_invoices_ProviderOrganizationId_Status_DueDate",
                schema: "professional",
                table: "professional_invoices",
                columns: new[] { "ProviderOrganizationId", "Status", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_professional_invoices_ServiceStatementId",
                schema: "professional",
                table: "professional_invoices",
                column: "ServiceStatementId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_professional_missions_BranchId_StartsOn_EndsOn",
                schema: "professional",
                table: "professional_missions",
                columns: new[] { "BranchId", "StartsOn", "EndsOn" });

            migrationBuilder.CreateIndex(
                name: "IX_professional_missions_EngagementId_Status",
                schema: "professional",
                table: "professional_missions",
                columns: new[] { "EngagementId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_professional_missions_OrganizationId_Status",
                schema: "professional",
                table: "professional_missions",
                columns: new[] { "OrganizationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_professional_missions_ProfessionalProfileId_Status",
                schema: "professional",
                table: "professional_missions",
                columns: new[] { "ProfessionalProfileId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_professional_review_reports_ReviewId_Status",
                schema: "professional",
                table: "professional_review_reports",
                columns: new[] { "ReviewId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_professional_reviews_EngagementId",
                schema: "professional",
                table: "professional_reviews",
                column: "EngagementId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_professional_reviews_ProfessionalProfileId_Status",
                schema: "professional",
                table: "professional_reviews",
                columns: new[] { "ProfessionalProfileId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_service_entries_EngagementId_SourceType_SourceId",
                schema: "professional",
                table: "service_entries",
                columns: new[] { "EngagementId", "SourceType", "SourceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_service_entries_OrganizationId_Status_ServiceDate",
                schema: "professional",
                table: "service_entries",
                columns: new[] { "OrganizationId", "Status", "ServiceDate" });

            migrationBuilder.CreateIndex(
                name: "IX_service_entries_ProfessionalProfileId_Status_ServiceDate",
                schema: "professional",
                table: "service_entries",
                columns: new[] { "ProfessionalProfileId", "Status", "ServiceDate" });

            migrationBuilder.CreateIndex(
                name: "IX_service_statements_ClientOrganizationId_Status_PeriodEnd",
                schema: "professional",
                table: "service_statements",
                columns: new[] { "ClientOrganizationId", "Status", "PeriodEnd" });

            migrationBuilder.CreateIndex(
                name: "IX_service_statements_EngagementId_PeriodStart_PeriodEnd",
                schema: "professional",
                table: "service_statements",
                columns: new[] { "EngagementId", "PeriodStart", "PeriodEnd" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_service_statements_ProfessionalProfileId_Status_PeriodEnd",
                schema: "professional",
                table: "service_statements",
                columns: new[] { "ProfessionalProfileId", "Status", "PeriodEnd" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "external_access_grants",
                schema: "professional");

            migrationBuilder.DropTable(
                name: "professional_invoices",
                schema: "professional");

            migrationBuilder.DropTable(
                name: "professional_missions",
                schema: "professional");

            migrationBuilder.DropTable(
                name: "professional_review_reports",
                schema: "professional");

            migrationBuilder.DropTable(
                name: "professional_reviews",
                schema: "professional");

            migrationBuilder.DropTable(
                name: "service_entries",
                schema: "professional");

            migrationBuilder.DropTable(
                name: "service_statements",
                schema: "professional");
        }
    }
}
