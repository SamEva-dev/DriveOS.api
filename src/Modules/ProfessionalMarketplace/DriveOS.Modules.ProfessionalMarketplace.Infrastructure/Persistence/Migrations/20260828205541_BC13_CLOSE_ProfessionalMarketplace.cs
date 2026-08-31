using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BC13_CLOSE_ProfessionalMarketplace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                schema: "professional",
                table: "service_entries",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExpensesAmount",
                schema: "professional",
                table: "service_entries",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "IndemnitiesAmount",
                schema: "professional",
                table: "service_entries",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ComplianceEnforcementAction",
                schema: "professional",
                table: "professional_profiles",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComplianceEnforcementReason",
                schema: "professional",
                table: "professional_profiles",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ComplianceEnforcementUpdatedAtUtc",
                schema: "professional",
                table: "professional_profiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ComplianceGraceUntil",
                schema: "professional",
                table: "professional_profiles",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NewSessionsBlocked",
                schema: "professional",
                table: "professional_profiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SuspendedByCompliancePolicy",
                schema: "professional",
                table: "professional_profiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ConfirmedPaymentMethod",
                schema: "professional",
                table: "professional_engagements",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FirstPaidFinanceSupplierInvoiceId",
                schema: "professional",
                table: "professional_engagements",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FirstPaidProfessionalInvoiceId",
                schema: "professional",
                table: "professional_engagements",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FirstPaymentAttemptId",
                schema: "professional",
                table: "professional_engagements",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "InitialIntegrationCompletedAtUtc",
                schema: "professional",
                table: "professional_engagements",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "InternalApprovalPrepared",
                schema: "professional",
                table: "professional_engagements",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReliableRelationshipEstablishedAtUtc",
                schema: "professional",
                table: "professional_engagements",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SatisfactionRequestedAtUtc",
                schema: "professional",
                table: "professional_engagements",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "freelance_invitations",
                schema: "professional",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientOrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    MissionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProfessionalProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    InvitedUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    Phone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ExpirationDate = table.Column<DateOnly>(type: "date", nullable: false),
                    InvitedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SentAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeliveredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    OpenedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RespondedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AcceptedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeclineReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_freelance_invitations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "professional_compliance_criticality_policies",
                schema: "professional",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    RequirementCode = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Criticality = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    Action = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    GracePeriodDays = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_professional_compliance_criticality_policies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "professional_compliance_waivers",
                schema: "professional",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessionalProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequirementCode = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ValidUntil = table.Column<DateOnly>(type: "date", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_professional_compliance_waivers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "professional_student_assignments",
                schema: "professional",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    EngagementId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessionalProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartsOn = table.Column<DateOnly>(type: "date", nullable: false),
                    EndsOn = table.Column<DateOnly>(type: "date", nullable: false),
                    ScopeCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    ResponsibleUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignmentReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
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
                    table.PrimaryKey("PK_professional_student_assignments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "service_disputes",
                schema: "professional",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    EngagementId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessionalProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientOrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RaisedByOrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Description = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    Evidence = table.Column<string>(type: "jsonb", nullable: false),
                    Discussion = table.Column<string>(type: "jsonb", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ResolutionOutcome = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Resolution = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    ResolvedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ResolvedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    EscalatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EscalatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    EscalationReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_disputes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_freelance_invitations_ClientOrganizationId_Status_Expiratio~",
                schema: "professional",
                table: "freelance_invitations",
                columns: new[] { "ClientOrganizationId", "Status", "ExpirationDate" });

            migrationBuilder.CreateIndex(
                name: "IX_freelance_invitations_Email_Status",
                schema: "professional",
                table: "freelance_invitations",
                columns: new[] { "Email", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_freelance_invitations_InvitedUserId_Status",
                schema: "professional",
                table: "freelance_invitations",
                columns: new[] { "InvitedUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_freelance_invitations_TokenHash",
                schema: "professional",
                table: "freelance_invitations",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_professional_compliance_criticality_policies_CountryCode_Re~",
                schema: "professional",
                table: "professional_compliance_criticality_policies",
                columns: new[] { "CountryCode", "RequirementCode", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_professional_compliance_criticality_policies_CountryCode_St~",
                schema: "professional",
                table: "professional_compliance_criticality_policies",
                columns: new[] { "CountryCode", "Status", "EffectiveFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_professional_compliance_waivers_ProfessionalProfileId_Requi~",
                schema: "professional",
                table: "professional_compliance_waivers",
                columns: new[] { "ProfessionalProfileId", "RequirementCode", "Status", "ValidUntil" });

            migrationBuilder.CreateIndex(
                name: "IX_professional_student_assignments_MissionId_StudentId_Status",
                schema: "professional",
                table: "professional_student_assignments",
                columns: new[] { "MissionId", "StudentId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_professional_student_assignments_ProfessionalProfileId_Stat~",
                schema: "professional",
                table: "professional_student_assignments",
                columns: new[] { "ProfessionalProfileId", "Status", "EndsOn" });

            migrationBuilder.CreateIndex(
                name: "IX_service_disputes_ClientOrganizationId_Status_CreatedAtUtc",
                schema: "professional",
                table: "service_disputes",
                columns: new[] { "ClientOrganizationId", "Status", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_service_disputes_ProfessionalProfileId_Status_CreatedAtUtc",
                schema: "professional",
                table: "service_disputes",
                columns: new[] { "ProfessionalProfileId", "Status", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_service_disputes_ServiceEntryId_Status",
                schema: "professional",
                table: "service_disputes",
                columns: new[] { "ServiceEntryId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "freelance_invitations",
                schema: "professional");

            migrationBuilder.DropTable(
                name: "professional_compliance_criticality_policies",
                schema: "professional");

            migrationBuilder.DropTable(
                name: "professional_compliance_waivers",
                schema: "professional");

            migrationBuilder.DropTable(
                name: "professional_student_assignments",
                schema: "professional");

            migrationBuilder.DropTable(
                name: "service_disputes",
                schema: "professional");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                schema: "professional",
                table: "service_entries");

            migrationBuilder.DropColumn(
                name: "ExpensesAmount",
                schema: "professional",
                table: "service_entries");

            migrationBuilder.DropColumn(
                name: "IndemnitiesAmount",
                schema: "professional",
                table: "service_entries");

            migrationBuilder.DropColumn(
                name: "ComplianceEnforcementAction",
                schema: "professional",
                table: "professional_profiles");

            migrationBuilder.DropColumn(
                name: "ComplianceEnforcementReason",
                schema: "professional",
                table: "professional_profiles");

            migrationBuilder.DropColumn(
                name: "ComplianceEnforcementUpdatedAtUtc",
                schema: "professional",
                table: "professional_profiles");

            migrationBuilder.DropColumn(
                name: "ComplianceGraceUntil",
                schema: "professional",
                table: "professional_profiles");

            migrationBuilder.DropColumn(
                name: "NewSessionsBlocked",
                schema: "professional",
                table: "professional_profiles");

            migrationBuilder.DropColumn(
                name: "SuspendedByCompliancePolicy",
                schema: "professional",
                table: "professional_profiles");

            migrationBuilder.DropColumn(
                name: "ConfirmedPaymentMethod",
                schema: "professional",
                table: "professional_engagements");

            migrationBuilder.DropColumn(
                name: "FirstPaidFinanceSupplierInvoiceId",
                schema: "professional",
                table: "professional_engagements");

            migrationBuilder.DropColumn(
                name: "FirstPaidProfessionalInvoiceId",
                schema: "professional",
                table: "professional_engagements");

            migrationBuilder.DropColumn(
                name: "FirstPaymentAttemptId",
                schema: "professional",
                table: "professional_engagements");

            migrationBuilder.DropColumn(
                name: "InitialIntegrationCompletedAtUtc",
                schema: "professional",
                table: "professional_engagements");

            migrationBuilder.DropColumn(
                name: "InternalApprovalPrepared",
                schema: "professional",
                table: "professional_engagements");

            migrationBuilder.DropColumn(
                name: "ReliableRelationshipEstablishedAtUtc",
                schema: "professional",
                table: "professional_engagements");

            migrationBuilder.DropColumn(
                name: "SatisfactionRequestedAtUtc",
                schema: "professional",
                table: "professional_engagements");
        }
    }
}
