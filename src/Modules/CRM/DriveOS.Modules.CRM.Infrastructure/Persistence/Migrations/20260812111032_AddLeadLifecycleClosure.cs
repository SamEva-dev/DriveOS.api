using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.CRM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLeadLifecycleClosure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "automatic_follow_ups_enabled",
                schema: "crm",
                table: "leads",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "closed_at_utc",
                schema: "crm",
                table: "leads",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "closure_comment",
                schema: "crm",
                table: "leads",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "closure_reason",
                schema: "crm",
                table: "leads",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "dormancy_campaign_code",
                schema: "crm",
                table: "leads",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "dormancy_responsible_user_id",
                schema: "crm",
                table: "leads",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "referral_consent_collected_at_utc",
                schema: "crm",
                table: "leads",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "referred_partner_name",
                schema: "crm",
                table: "leads",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "reopened_at_utc",
                schema: "crm",
                table: "leads",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "resume_at_utc",
                schema: "crm",
                table: "leads",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "shared_data_description",
                schema: "crm",
                table: "leads",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_leads_organization_resume_at_utc",
                schema: "crm",
                table: "leads",
                columns: new[] { "organization_id", "resume_at_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_leads_organization_resume_at_utc",
                schema: "crm",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "automatic_follow_ups_enabled",
                schema: "crm",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "closed_at_utc",
                schema: "crm",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "closure_comment",
                schema: "crm",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "closure_reason",
                schema: "crm",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "dormancy_campaign_code",
                schema: "crm",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "dormancy_responsible_user_id",
                schema: "crm",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "referral_consent_collected_at_utc",
                schema: "crm",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "referred_partner_name",
                schema: "crm",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "reopened_at_utc",
                schema: "crm",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "resume_at_utc",
                schema: "crm",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "shared_data_description",
                schema: "crm",
                table: "leads");
        }
    }
}
