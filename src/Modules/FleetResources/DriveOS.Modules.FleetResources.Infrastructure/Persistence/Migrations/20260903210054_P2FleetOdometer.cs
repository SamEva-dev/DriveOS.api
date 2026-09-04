using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.FleetResources.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class P2FleetOdometer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CurrentOdometerKilometers",
                schema: "fleet_resources",
                table: "vehicles",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastOdometerRecordedAtUtc",
                schema: "fleet_resources",
                table: "vehicles",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentOdometerKilometers",
                schema: "fleet_resources",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "LastOdometerRecordedAtUtc",
                schema: "fleet_resources",
                table: "vehicles");
        }
    }
}
