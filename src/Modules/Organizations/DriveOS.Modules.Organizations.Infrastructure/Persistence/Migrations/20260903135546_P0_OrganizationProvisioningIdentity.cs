using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.Organizations.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class P0_OrganizationProvisioningIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "provisioning_external_user_id",
                schema: "organization",
                table: "organizations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "provisioning_key",
                schema: "organization",
                table: "organizations",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ux_organizations_provisioning_key",
                schema: "organization",
                table: "organizations",
                column: "provisioning_key",
                unique: true,
                filter: "provisioning_key IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_organizations_provisioning_key",
                schema: "organization",
                table: "organizations");

            migrationBuilder.DropColumn(
                name: "provisioning_external_user_id",
                schema: "organization",
                table: "organizations");

            migrationBuilder.DropColumn(
                name: "provisioning_key",
                schema: "organization",
                table: "organizations");
        }
    }
}
