using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.CRM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLeadLifecycleClosureAddSavedLeadViews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "saved_lead_views",
                schema: "crm",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    filters_json = table.Column<string>(type: "jsonb", nullable: false),
                    sort_json = table.Column<string>(type: "jsonb", nullable: false),
                    columns_json = table.Column<string>(type: "jsonb", nullable: false),
                    scope = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_saved_lead_views", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_saved_lead_views_scope",
                schema: "crm",
                table: "saved_lead_views",
                columns: new[] { "organization_id", "scope", "branch_id" });

            migrationBuilder.CreateIndex(
                name: "ux_saved_lead_views_owner_name",
                schema: "crm",
                table: "saved_lead_views",
                columns: new[] { "organization_id", "owner_user_id", "name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "saved_lead_views",
                schema: "crm");
        }
    }
}
