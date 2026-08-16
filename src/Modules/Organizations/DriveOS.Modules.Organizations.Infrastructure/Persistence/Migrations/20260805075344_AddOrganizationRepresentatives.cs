using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.Organizations.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationRepresentatives : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "organization_representatives",
                schema: "organization",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    representative_type = table.Column<int>(type: "integer", nullable: false),
                    authority_scope = table.Column<string>(
                        type: "character varying(2000)",
                        maxLength: 2000,
                        nullable: false
                    ),
                    is_primary_owner = table.Column<bool>(type: "boolean", nullable: false),
                    effective_from = table.Column<DateOnly>(type: "date", nullable: false),
                    effective_to = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    revision = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modified_at_utc = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    last_modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_representatives", x => x.id);
                    table.ForeignKey(
                        name: "FK_organization_representatives_organizations_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "organization",
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_organization_representatives_org_status",
                schema: "organization",
                table: "organization_representatives",
                columns: new[] { "organization_id", "status" }
            );

            migrationBuilder.CreateIndex(
                name: "ux_organization_representatives_active_identity",
                schema: "organization",
                table: "organization_representatives",
                columns: new[] { "organization_id", "person_id", "representative_type" },
                unique: true,
                filter: "status IN (1, 2, 3)"
            );

            migrationBuilder.CreateIndex(
                name: "ux_organization_representatives_primary_owner",
                schema: "organization",
                table: "organization_representatives",
                column: "organization_id",
                unique: true,
                filter: "is_primary_owner = TRUE AND status = 2"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "organization_representatives",
                schema: "organization"
            );
        }
    }
}
