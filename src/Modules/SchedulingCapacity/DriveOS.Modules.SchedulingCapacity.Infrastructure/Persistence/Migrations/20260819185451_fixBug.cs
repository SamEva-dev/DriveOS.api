using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class fixBug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_availability_rules_AvailabilityPlanId_DayOfWeek_StartTime_E~",
                schema: "scheduling_capacity",
                table: "availability_rules");

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                schema: "scheduling_capacity",
                table: "availability_rules",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                schema: "scheduling_capacity",
                table: "availability_rules",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ServiceArea",
                schema: "scheduling_capacity",
                table: "availability_rules",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                schema: "scheduling_capacity",
                table: "availability_rules",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TrainingCategory",
                schema: "scheduling_capacity",
                table: "availability_rules",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                schema: "scheduling_capacity",
                table: "availability_rules",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IntensiveRhythm",
                schema: "scheduling_capacity",
                table: "availability_plans",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MaximumTravelDistanceKm",
                schema: "scheduling_capacity",
                table: "availability_plans",
                type: "numeric(8,2)",
                precision: 8,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinimumNoticeMinutes",
                schema: "scheduling_capacity",
                table: "availability_plans",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OneTimeGeolocationAllowed",
                schema: "scheduling_capacity",
                table: "availability_plans",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "PreferredInstructorId",
                schema: "scheduling_capacity",
                table: "availability_plans",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredMeetingPoint",
                schema: "scheduling_capacity",
                table: "availability_plans",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TrainingFrequencyPerWeek",
                schema: "scheduling_capacity",
                table: "availability_plans",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                schema: "scheduling_capacity",
                table: "availability_exceptions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                schema: "scheduling_capacity",
                table: "availability_exceptions",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_availability_rules_AvailabilityPlanId_DayOfWeek_StartTime_E~",
                schema: "scheduling_capacity",
                table: "availability_rules",
                columns: new[] { "AvailabilityPlanId", "DayOfWeek", "StartTime", "EndTime", "Type" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_availability_rules_AvailabilityPlanId_DayOfWeek_StartTime_E~",
                schema: "scheduling_capacity",
                table: "availability_rules");

            migrationBuilder.DropColumn(
                name: "BranchId",
                schema: "scheduling_capacity",
                table: "availability_rules");

            migrationBuilder.DropColumn(
                name: "Priority",
                schema: "scheduling_capacity",
                table: "availability_rules");

            migrationBuilder.DropColumn(
                name: "ServiceArea",
                schema: "scheduling_capacity",
                table: "availability_rules");

            migrationBuilder.DropColumn(
                name: "Source",
                schema: "scheduling_capacity",
                table: "availability_rules");

            migrationBuilder.DropColumn(
                name: "TrainingCategory",
                schema: "scheduling_capacity",
                table: "availability_rules");

            migrationBuilder.DropColumn(
                name: "Type",
                schema: "scheduling_capacity",
                table: "availability_rules");

            migrationBuilder.DropColumn(
                name: "IntensiveRhythm",
                schema: "scheduling_capacity",
                table: "availability_plans");

            migrationBuilder.DropColumn(
                name: "MaximumTravelDistanceKm",
                schema: "scheduling_capacity",
                table: "availability_plans");

            migrationBuilder.DropColumn(
                name: "MinimumNoticeMinutes",
                schema: "scheduling_capacity",
                table: "availability_plans");

            migrationBuilder.DropColumn(
                name: "OneTimeGeolocationAllowed",
                schema: "scheduling_capacity",
                table: "availability_plans");

            migrationBuilder.DropColumn(
                name: "PreferredInstructorId",
                schema: "scheduling_capacity",
                table: "availability_plans");

            migrationBuilder.DropColumn(
                name: "PreferredMeetingPoint",
                schema: "scheduling_capacity",
                table: "availability_plans");

            migrationBuilder.DropColumn(
                name: "TrainingFrequencyPerWeek",
                schema: "scheduling_capacity",
                table: "availability_plans");

            migrationBuilder.DropColumn(
                name: "Priority",
                schema: "scheduling_capacity",
                table: "availability_exceptions");

            migrationBuilder.DropColumn(
                name: "Source",
                schema: "scheduling_capacity",
                table: "availability_exceptions");

            migrationBuilder.CreateIndex(
                name: "IX_availability_rules_AvailabilityPlanId_DayOfWeek_StartTime_E~",
                schema: "scheduling_capacity",
                table: "availability_rules",
                columns: new[] { "AvailabilityPlanId", "DayOfWeek", "StartTime", "EndTime" },
                unique: true);
        }
    }
}
