using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeSchedull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TrainingCategory",
                schema: "scheduling_capacity",
                table: "bookings",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TrainingCategory",
                schema: "scheduling_capacity",
                table: "bookings");
        }
    }
}
