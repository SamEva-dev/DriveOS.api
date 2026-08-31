using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MKT_001_ProfessionalMarketplaceFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "professional");

            migrationBuilder.CreateTable(
                name: "professional_profiles",
                schema: "professional",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderOrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ComplianceStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_professional_profiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_professional_profiles_PersonId",
                schema: "professional",
                table: "professional_profiles",
                column: "PersonId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_professional_profiles_ProviderOrganizationId",
                schema: "professional",
                table: "professional_profiles",
                column: "ProviderOrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_professional_profiles_Status_ComplianceStatus",
                schema: "professional",
                table: "professional_profiles",
                columns: new[] { "Status", "ComplianceStatus" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "professional_profiles",
                schema: "professional");
        }
    }
}
