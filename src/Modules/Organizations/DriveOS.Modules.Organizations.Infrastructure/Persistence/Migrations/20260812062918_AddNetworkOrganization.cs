using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.Organizations.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNetworkOrganization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "network_organization_memberships",
                schema: "organization",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    network_organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    member_organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    joined_at_utc = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    ended_at_utc = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_network_organization_memberships", x => x.id);
                    table.ForeignKey(
                        name: "FK_network_organization_memberships_organizations_member_organ~",
                        column: x => x.member_organization_id,
                        principalSchema: "organization",
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_network_organization_memberships_organizations_network_orga~",
                        column: x => x.network_organization_id,
                        principalSchema: "organization",
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "ux_network_memberships_active_member",
                schema: "organization",
                table: "network_organization_memberships",
                column: "member_organization_id",
                unique: true,
                filter: "ended_at_utc IS NULL"
            );

            migrationBuilder.CreateIndex(
                name: "ux_network_memberships_network_member",
                schema: "organization",
                table: "network_organization_memberships",
                columns: new[] { "network_organization_id", "member_organization_id" },
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "network_organization_memberships",
                schema: "organization"
            );
        }
    }
}
