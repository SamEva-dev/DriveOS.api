using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.FleetResources.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialFleetResources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "fleet_resources");

            migrationBuilder.CreateTable(
                name: "vehicles",
                schema: "fleet_resources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerOrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderOrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    RegistrationNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Vin = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Make = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Model = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    TransmissionType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EnergyType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DualControl = table.Column<bool>(type: "boolean", nullable: false),
                    LicenseCategoriesCsv = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    AdaptationsCsv = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    OperationalStatus = table.Column<int>(type: "integer", nullable: false),
                    TechnicalComplianceVerified = table.Column<bool>(type: "boolean", nullable: false),
                    DocumentsCompliant = table.Column<bool>(type: "boolean", nullable: false),
                    InsuranceValidUntilUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    MaintenanceBlocking = table.Column<bool>(type: "boolean", nullable: false),
                    NextMaintenanceDueAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastComplianceVerifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ComplianceNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_OrganizationId_BranchId_OperationalStatus",
                schema: "fleet_resources",
                table: "vehicles",
                columns: new[] { "OrganizationId", "BranchId", "OperationalStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_OrganizationId_RegistrationNumber",
                schema: "fleet_resources",
                table: "vehicles",
                columns: new[] { "OrganizationId", "RegistrationNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vehicles",
                schema: "fleet_resources");
        }
    }
}
