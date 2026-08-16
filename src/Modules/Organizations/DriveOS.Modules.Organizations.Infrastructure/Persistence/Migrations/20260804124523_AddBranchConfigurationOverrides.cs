using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.Organizations.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchConfigurationOverrides : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "branch_configuration_overrides",
                schema: "organization",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    base_configuration_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version_number = table.Column<int>(type: "integer", nullable: false),
                    country_code = table.Column<string>(
                        type: "character(2)",
                        fixedLength: true,
                        maxLength: 2,
                        nullable: false
                    ),
                    override_json = table.Column<string>(type: "jsonb", nullable: false),
                    status = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false
                    ),
                    effective_from_utc = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    effective_to_utc = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    published_at_utc = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    published_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_branch_configuration_overrides", x => x.id);
                    table.ForeignKey(
                        name: "FK_branch_configuration_overrides_branches_branch_id",
                        column: x => x.branch_id,
                        principalSchema: "organization",
                        principalTable: "branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_branch_configuration_overrides_organization_configurations_~",
                        column: x => x.base_configuration_id,
                        principalSchema: "organization",
                        principalTable: "organization_configurations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_branch_configuration_overrides_organizations_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "organization",
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_branch_configuration_overrides_base_configuration",
                schema: "organization",
                table: "branch_configuration_overrides",
                column: "base_configuration_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_branch_configuration_overrides_branch_id",
                schema: "organization",
                table: "branch_configuration_overrides",
                column: "branch_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_branch_configuration_overrides_effective_resolution",
                schema: "organization",
                table: "branch_configuration_overrides",
                columns: new[] { "organization_id", "branch_id", "status", "effective_from_utc" }
            );

            migrationBuilder.CreateIndex(
                name: "ux_branch_configuration_overrides_org_branch_version",
                schema: "organization",
                table: "branch_configuration_overrides",
                columns: new[] { "organization_id", "branch_id", "version_number" },
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "branch_configuration_overrides",
                schema: "organization"
            );
        }
    }
}
