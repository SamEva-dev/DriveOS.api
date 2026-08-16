using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.CRM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class HardenActivityImports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "lead_id",
                schema: "crm",
                table: "activities",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid"
            );

            migrationBuilder.AddColumn<Guid>(
                name: "advisor_user_id",
                schema: "crm",
                table: "activities",
                type: "uuid",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "attachment_name",
                schema: "crm",
                table: "activities",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "attachment_reference",
                schema: "crm",
                table: "activities",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true
            );

            migrationBuilder.AddColumn<int>(
                name: "duration_minutes",
                schema: "crm",
                table: "activities",
                type: "integer",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "external_id",
                schema: "crm",
                table: "activities",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "idempotency_key",
                schema: "crm",
                table: "activities",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true
            );

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "invalidated_at_utc",
                schema: "crm",
                table: "activities",
                type: "timestamp with time zone",
                nullable: true
            );

            migrationBuilder.AddColumn<Guid>(
                name: "invalidated_by_user_id",
                schema: "crm",
                table: "activities",
                type: "uuid",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "invalidation_reason",
                schema: "crm",
                table: "activities",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true
            );

            migrationBuilder.AddColumn<bool>(
                name: "is_internal",
                schema: "crm",
                table: "activities",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<bool>(
                name: "is_unfollowed",
                schema: "crm",
                table: "activities",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "last_sync_attempt_at_utc",
                schema: "crm",
                table: "activities",
                type: "timestamp with time zone",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "origin",
                schema: "crm",
                table: "activities",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<bool>(
                name: "requires_regularization",
                schema: "crm",
                table: "activities",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<string>(
                name: "result",
                schema: "crm",
                table: "activities",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true
            );

            migrationBuilder.AddColumn<int>(
                name: "sync_attempt_count",
                schema: "crm",
                table: "activities",
                type: "integer",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<string>(
                name: "sync_error_key",
                schema: "crm",
                table: "activities",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "sync_status",
                schema: "crm",
                table: "activities",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.CreateIndex(
                name: "ix_activities_organization_advisor_occurred",
                schema: "crm",
                table: "activities",
                columns: new[] { "organization_id", "advisor_user_id", "occurred_at_utc" }
            );

            migrationBuilder.AddCheckConstraint(
                name: "ck_activities_duration_minutes",
                schema: "crm",
                table: "activities",
                sql: "duration_minutes IS NULL OR (duration_minutes >= 0 AND duration_minutes <= 1440)"
            );

            migrationBuilder.AddCheckConstraint(
                name: "ck_activities_failed_sync_error",
                schema: "crm",
                table: "activities",
                sql: "sync_status <> 'Failed' OR sync_error_key IS NOT NULL"
            );

            migrationBuilder.AddCheckConstraint(
                name: "ck_activities_import_metadata",
                schema: "crm",
                table: "activities",
                sql: "origin <> 'Imported' OR (external_id IS NOT NULL AND idempotency_key IS NOT NULL)"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_activities_organization_advisor_occurred",
                schema: "crm",
                table: "activities"
            );

            migrationBuilder.DropCheckConstraint(
                name: "ck_activities_duration_minutes",
                schema: "crm",
                table: "activities"
            );

            migrationBuilder.DropCheckConstraint(
                name: "ck_activities_failed_sync_error",
                schema: "crm",
                table: "activities"
            );

            migrationBuilder.DropCheckConstraint(
                name: "ck_activities_import_metadata",
                schema: "crm",
                table: "activities"
            );

            migrationBuilder.DropColumn(
                name: "advisor_user_id",
                schema: "crm",
                table: "activities"
            );

            migrationBuilder.DropColumn(
                name: "attachment_name",
                schema: "crm",
                table: "activities"
            );

            migrationBuilder.DropColumn(
                name: "attachment_reference",
                schema: "crm",
                table: "activities"
            );

            migrationBuilder.DropColumn(
                name: "duration_minutes",
                schema: "crm",
                table: "activities"
            );

            migrationBuilder.DropColumn(name: "external_id", schema: "crm", table: "activities");

            migrationBuilder.DropColumn(
                name: "idempotency_key",
                schema: "crm",
                table: "activities"
            );

            migrationBuilder.DropColumn(
                name: "invalidated_at_utc",
                schema: "crm",
                table: "activities"
            );

            migrationBuilder.DropColumn(
                name: "invalidated_by_user_id",
                schema: "crm",
                table: "activities"
            );

            migrationBuilder.DropColumn(
                name: "invalidation_reason",
                schema: "crm",
                table: "activities"
            );

            migrationBuilder.DropColumn(name: "is_internal", schema: "crm", table: "activities");

            migrationBuilder.DropColumn(name: "is_unfollowed", schema: "crm", table: "activities");

            migrationBuilder.DropColumn(
                name: "last_sync_attempt_at_utc",
                schema: "crm",
                table: "activities"
            );

            migrationBuilder.DropColumn(name: "origin", schema: "crm", table: "activities");

            migrationBuilder.DropColumn(
                name: "requires_regularization",
                schema: "crm",
                table: "activities"
            );

            migrationBuilder.DropColumn(name: "result", schema: "crm", table: "activities");

            migrationBuilder.DropColumn(
                name: "sync_attempt_count",
                schema: "crm",
                table: "activities"
            );

            migrationBuilder.DropColumn(name: "sync_error_key", schema: "crm", table: "activities");

            migrationBuilder.DropColumn(name: "sync_status", schema: "crm", table: "activities");

            migrationBuilder.AlterColumn<Guid>(
                name: "lead_id",
                schema: "crm",
                table: "activities",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true
            );
        }
    }
}
