using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.Organizations.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RestartMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "organization");

            migrationBuilder.EnsureSchema(
                name: "organizations");

            migrationBuilder.CreateTable(
                name: "organizations",
                schema: "organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    legal_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    country_code = table.Column<string>(type: "character(2)", fixedLength: true, maxLength: 2, nullable: false),
                    organization_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "branches",
                schema: "organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    normalized_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    branch_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    address_line1 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    address_line2 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    city = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    country_code = table.Column<string>(type: "character(2)", fixedLength: true, maxLength: 2, nullable: false),
                    time_zone_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_branches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_branches_organizations_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "organization",
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "organization_settings",
                schema: "organization",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    trade_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    registration_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    tax_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    contact_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    contact_phone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    website = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    address_line1 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    address_line2 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    city = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    region = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    address_country_code = table.Column<string>(type: "character(2)", fixedLength: true, maxLength: 2, nullable: false),
                    default_language = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    supported_languages = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    time_zone_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    currency_code = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    date_format = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    time_format = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    first_day_of_week = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    measurement_system = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    default_session_duration_minutes = table.Column<int>(type: "integer", nullable: false),
                    default_booking_lead_time_minutes = table.Column<int>(type: "integer", nullable: false),
                    default_cancellation_delay_hours = table.Column<int>(type: "integer", nullable: false),
                    allow_student_self_booking = table.Column<bool>(type: "boolean", nullable: false),
                    require_branch_for_operations = table.Column<bool>(type: "boolean", nullable: false),
                    default_branch_id = table.Column<Guid>(type: "uuid", nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_settings", x => x.id);
                    table.ForeignKey(
                        name: "FK_organization_settings_organizations_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "organization",
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "organization_status_history",
                schema: "organization",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    previous_status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    new_status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    changed_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    changed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_status_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_organization_status_history_organizations_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "organization",
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "organization_subscriptions",
                schema: "organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    billing_cycle = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    current_period_starts_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    current_period_ends_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    trial_starts_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    trial_ends_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    cancellation_requested_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    cancellation_effective_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    cancellation_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    cancellation_requested_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    external_provider = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    external_subscription_id = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_organization_subscriptions_organizations_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "organization",
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "branch_manager_assignments",
                schema: "organization",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    manager_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    effective_from_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    effective_to_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    assigned_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ended_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ended_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_branch_manager_assignments", x => x.id);
                    table.ForeignKey(
                        name: "FK_branch_manager_assignments_branches_branch_id",
                        column: x => x.branch_id,
                        principalSchema: "organization",
                        principalTable: "branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "branch_status_history",
                schema: "organizations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    previous_status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    new_status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    changed_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    changed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_branch_status_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_branch_status_history_branches_branch_id",
                        column: x => x.branch_id,
                        principalSchema: "organization",
                        principalTable: "branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "branch_user_assignments",
                schema: "organization",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    assignment_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    starts_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    planned_end_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    effective_end_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    suspension_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    suspended_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    suspended_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    end_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ended_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ended_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_branch_user_assignments", x => x.id);
                    table.ForeignKey(
                        name: "FK_branch_user_assignments_branches_branch_id",
                        column: x => x.branch_id,
                        principalSchema: "organization",
                        principalTable: "branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "organization_subscription_entitlements",
                schema: "organization",
                columns: table => new
                {
                    entitlement_code = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    organization_subscription_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_subscription_entitlements", x => new { x.organization_subscription_id, x.entitlement_code });
                    table.ForeignKey(
                        name: "FK_organization_subscription_entitlements_organization_subscri~",
                        column: x => x.organization_subscription_id,
                        principalSchema: "organization",
                        principalTable: "organization_subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "organization_subscription_limits",
                schema: "organization",
                columns: table => new
                {
                    limit_code = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    organization_subscription_id = table.Column<Guid>(type: "uuid", nullable: false),
                    limit_value = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_subscription_limits", x => new { x.organization_subscription_id, x.limit_code });
                    table.ForeignKey(
                        name: "FK_organization_subscription_limits_organization_subscriptions~",
                        column: x => x.organization_subscription_id,
                        principalSchema: "organization",
                        principalTable: "organization_subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_branch_manager_assignments_branch_date",
                schema: "organization",
                table: "branch_manager_assignments",
                columns: new[] { "branch_id", "effective_from_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_branch_manager_assignments_branch_status",
                schema: "organization",
                table: "branch_manager_assignments",
                columns: new[] { "branch_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ux_branch_manager_assignments_active_branch",
                schema: "organization",
                table: "branch_manager_assignments",
                column: "branch_id",
                unique: true,
                filter: "status = 'Active' AND effective_to_utc IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_branch_status_history_branch_date",
                schema: "organizations",
                table: "branch_status_history",
                columns: new[] { "branch_id", "changed_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_branch_user_assignments_branch_id",
                schema: "organization",
                table: "branch_user_assignments",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_branch_user_assignments_branch_status",
                schema: "organization",
                table: "branch_user_assignments",
                columns: new[] { "organization_id", "branch_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_branch_user_assignments_user_status",
                schema: "organization",
                table: "branch_user_assignments",
                columns: new[] { "organization_id", "user_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ux_branch_user_assignments_open_role",
                schema: "organization",
                table: "branch_user_assignments",
                columns: new[] { "organization_id", "branch_id", "user_id", "role" },
                unique: true,
                filter: "status <> 'Ended'");

            migrationBuilder.CreateIndex(
                name: "ux_branch_user_assignments_primary_user",
                schema: "organization",
                table: "branch_user_assignments",
                columns: new[] { "organization_id", "user_id", "assignment_type" },
                unique: true,
                filter: "assignment_type = 'Primary' AND status <> 'Ended'");

            migrationBuilder.CreateIndex(
                name: "ix_branches_organization_status",
                schema: "organization",
                table: "branches",
                columns: new[] { "organization_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ux_branches_organization_code",
                schema: "organization",
                table: "branches",
                columns: new[] { "organization_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_branches_organization_normalized_name",
                schema: "organization",
                table: "branches",
                columns: new[] { "organization_id", "normalized_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_branches_primary_per_organization",
                schema: "organization",
                table: "branches",
                columns: new[] { "organization_id", "is_primary" },
                unique: true,
                filter: "is_primary = true AND status <> 'Closed'");

            migrationBuilder.CreateIndex(
                name: "ux_organization_settings_organization_id",
                schema: "organization",
                table: "organization_settings",
                column: "organization_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_organization_status_history_org_date",
                schema: "organization",
                table: "organization_status_history",
                columns: new[] { "organization_id", "changed_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ux_organization_subscriptions_external_reference",
                schema: "organization",
                table: "organization_subscriptions",
                columns: new[] { "external_provider", "external_subscription_id" },
                unique: true,
                filter: "external_provider IS NOT NULL AND external_subscription_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_organization_subscriptions_organization_id",
                schema: "organization",
                table: "organization_subscriptions",
                column: "organization_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_organizations_country_legal_name",
                schema: "organization",
                table: "organizations",
                columns: new[] { "country_code", "legal_name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "branch_manager_assignments",
                schema: "organization");

            migrationBuilder.DropTable(
                name: "branch_status_history",
                schema: "organizations");

            migrationBuilder.DropTable(
                name: "branch_user_assignments",
                schema: "organization");

            migrationBuilder.DropTable(
                name: "organization_settings",
                schema: "organization");

            migrationBuilder.DropTable(
                name: "organization_status_history",
                schema: "organization");

            migrationBuilder.DropTable(
                name: "organization_subscription_entitlements",
                schema: "organization");

            migrationBuilder.DropTable(
                name: "organization_subscription_limits",
                schema: "organization");

            migrationBuilder.DropTable(
                name: "branches",
                schema: "organization");

            migrationBuilder.DropTable(
                name: "organization_subscriptions",
                schema: "organization");

            migrationBuilder.DropTable(
                name: "organizations",
                schema: "organization");
        }
    }
}
