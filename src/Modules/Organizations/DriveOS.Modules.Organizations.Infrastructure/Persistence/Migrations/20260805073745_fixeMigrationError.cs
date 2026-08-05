using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.Organizations.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class fixeMigrationError : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "organization_sequences",
                schema: "organization",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: true),
                    scope = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    pattern = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    padding = table.Column<int>(type: "integer", nullable: false),
                    next_value = table.Column<long>(type: "bigint", nullable: false),
                    reset_policy = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    last_reset_year = table.Column<int>(type: "integer", nullable: true),
                    last_reset_month = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    revision = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_sequences", x => x.id);
                    table.ForeignKey(
                        name: "FK_organization_sequences_branches_branch_id",
                        column: x => x.branch_id,
                        principalSchema: "organization",
                        principalTable: "branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_organization_sequences_organizations_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "organization",
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_organization_sequences_branch_id",
                schema: "organization",
                table: "organization_sequences",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_organization_sequences_scope_status",
                schema: "organization",
                table: "organization_sequences",
                columns: new[] { "organization_id", "branch_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ux_organization_sequences_branch_code",
                schema: "organization",
                table: "organization_sequences",
                columns: new[] { "organization_id", "branch_id", "code" },
                unique: true,
                filter: "branch_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_organization_sequences_organization_code",
                schema: "organization",
                table: "organization_sequences",
                columns: new[] { "organization_id", "code" },
                unique: true,
                filter: "branch_id IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "organization_sequences",
                schema: "organization");
        }
    }
}
