using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MKT_003_TeachingCapabilities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BillingAddressLine1",
                schema: "professional",
                table: "professional_profiles",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingAddressLine2",
                schema: "professional",
                table: "professional_profiles",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingCity",
                schema: "professional",
                table: "professional_profiles",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingCountryCode",
                schema: "professional",
                table: "professional_profiles",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingPostalCode",
                schema: "professional",
                table: "professional_profiles",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Biography",
                schema: "professional",
                table: "professional_profiles",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExperienceYears",
                schema: "professional",
                table: "professional_profiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "HasPersonalTrainingVehicle",
                schema: "professional",
                table: "professional_profiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Headline",
                schema: "professional",
                table: "professional_profiles",
                type: "character varying(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "Languages",
                schema: "professional",
                table: "professional_profiles",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string>(
                name: "LegalName",
                schema: "professional",
                table: "professional_profiles",
                type: "character varying(180)",
                maxLength: 180,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegalStatusCode",
                schema: "professional",
                table: "professional_profiles",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MobilityRadiusKm",
                schema: "professional",
                table: "professional_profiles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PersonalVehicleNotes",
                schema: "professional",
                table: "professional_profiles",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "PreferredEngagementTypes",
                schema: "professional",
                table: "professional_profiles",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryServiceArea",
                schema: "professional",
                table: "professional_profiles",
                type: "character varying(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfessionalEmail",
                schema: "professional",
                table: "professional_profiles",
                type: "character varying(254)",
                maxLength: 254,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfessionalPhone",
                schema: "professional",
                table: "professional_profiles",
                type: "character varying(48)",
                maxLength: 48,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfessionalType",
                schema: "professional",
                table: "professional_profiles",
                type: "character varying(48)",
                maxLength: 48,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RegistrationNumber",
                schema: "professional",
                table: "professional_profiles",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "SpecializationCodes",
                schema: "professional",
                table: "professional_profiles",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string>(
                name: "TaxNumber",
                schema: "professional",
                table: "professional_profiles",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "TeachingCategoryCodes",
                schema: "professional",
                table: "professional_profiles",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string>(
                name: "TradeName",
                schema: "professional",
                table: "professional_profiles",
                type: "character varying(180)",
                maxLength: 180,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_professional_profiles_BillingCountryCode_ProfessionalType_S~",
                schema: "professional",
                table: "professional_profiles",
                columns: new[] { "BillingCountryCode", "ProfessionalType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_professional_profiles_RegistrationNumber",
                schema: "professional",
                table: "professional_profiles",
                column: "RegistrationNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_professional_profiles_BillingCountryCode_ProfessionalType_S~",
                schema: "professional",
                table: "professional_profiles");

            migrationBuilder.DropIndex(
                name: "IX_professional_profiles_RegistrationNumber",
                schema: "professional",
                table: "professional_profiles");

            migrationBuilder.DropColumn(
                name: "BillingAddressLine1",
                schema: "professional",
                table: "professional_profiles");

            migrationBuilder.DropColumn(
                name: "BillingAddressLine2",
                schema: "professional",
                table: "professional_profiles");

            migrationBuilder.DropColumn(
                name: "BillingCity",
                schema: "professional",
                table: "professional_profiles");

            migrationBuilder.DropColumn(
                name: "BillingCountryCode",
                schema: "professional",
                table: "professional_profiles");

            migrationBuilder.DropColumn(
                name: "BillingPostalCode",
                schema: "professional",
                table: "professional_profiles");

            migrationBuilder.DropColumn(
                name: "Biography",
                schema: "professional",
                table: "professional_profiles");

            migrationBuilder.DropColumn(
                name: "ExperienceYears",
                schema: "professional",
                table: "professional_profiles");

            migrationBuilder.DropColumn(
                name: "HasPersonalTrainingVehicle",
                schema: "professional",
                table: "professional_profiles");

            migrationBuilder.DropColumn(
                name: "Headline",
                schema: "professional",
                table: "professional_profiles");

            migrationBuilder.DropColumn(
                name: "Languages",
                schema: "professional",
                table: "professional_profiles");

            migrationBuilder.DropColumn(
                name: "LegalName",
                schema: "professional",
                table: "professional_profiles");

            migrationBuilder.DropColumn(
                name: "LegalStatusCode",
                schema: "professional",
                table: "professional_profiles");

            migrationBuilder.DropColumn(
                name: "MobilityRadiusKm",
                schema: "professional",
                table: "professional_profiles");

            migrationBuilder.DropColumn(
                name: "PersonalVehicleNotes",
                schema: "professional",
                table: "professional_profiles");

            migrationBuilder.DropColumn(
                name: "PreferredEngagementTypes",
                schema: "professional",
                table: "professional_profiles");

            migrationBuilder.DropColumn(
                name: "PrimaryServiceArea",
                schema: "professional",
                table: "professional_profiles");

            migrationBuilder.DropColumn(
                name: "ProfessionalEmail",
                schema: "professional",
                table: "professional_profiles");

            migrationBuilder.DropColumn(
                name: "ProfessionalPhone",
                schema: "professional",
                table: "professional_profiles");

            migrationBuilder.DropColumn(
                name: "ProfessionalType",
                schema: "professional",
                table: "professional_profiles");

            migrationBuilder.DropColumn(
                name: "RegistrationNumber",
                schema: "professional",
                table: "professional_profiles");

            migrationBuilder.DropColumn(
                name: "SpecializationCodes",
                schema: "professional",
                table: "professional_profiles");

            migrationBuilder.DropColumn(
                name: "TaxNumber",
                schema: "professional",
                table: "professional_profiles");

            migrationBuilder.DropColumn(
                name: "TeachingCategoryCodes",
                schema: "professional",
                table: "professional_profiles");

            migrationBuilder.DropColumn(
                name: "TradeName",
                schema: "professional",
                table: "professional_profiles");
        }
    }
}
