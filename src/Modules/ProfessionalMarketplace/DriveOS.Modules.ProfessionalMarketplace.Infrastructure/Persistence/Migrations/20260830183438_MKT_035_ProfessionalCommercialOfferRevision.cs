using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MKT_035_ProfessionalCommercialOfferRevision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RevisionHistory",
                schema: "professional",
                table: "professional_proposals",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RevisionHistory",
                schema: "professional",
                table: "professional_commercial_offers",
                type: "jsonb",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RevisionHistory",
                schema: "professional",
                table: "professional_proposals");

            migrationBuilder.DropColumn(
                name: "RevisionHistory",
                schema: "professional",
                table: "professional_commercial_offers");
        }
    }
}
