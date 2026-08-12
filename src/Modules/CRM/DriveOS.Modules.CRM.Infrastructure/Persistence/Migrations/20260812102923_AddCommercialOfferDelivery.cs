using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.CRM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCommercialOfferDelivery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "attachment_snapshot_json",
                schema: "crm",
                table: "commercial_offers",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "delivery_attempt_count",
                schema: "crm",
                table: "commercial_offers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "delivery_channel",
                schema: "crm",
                table: "commercial_offers",
                type: "character varying(24)",
                maxLength: 24,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "delivery_language",
                schema: "crm",
                table: "commercial_offers",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "delivery_message",
                schema: "crm",
                table: "commercial_offers",
                type: "character varying(8000)",
                maxLength: 8000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "delivery_status",
                schema: "crm",
                table: "commercial_offers",
                type: "character varying(24)",
                maxLength: 24,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "delivery_subject",
                schema: "crm",
                table: "commercial_offers",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "document_reference",
                schema: "crm",
                table: "commercial_offers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "recipient_snapshot_json",
                schema: "crm",
                table: "commercial_offers",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "secure_link_expires_at_utc",
                schema: "crm",
                table: "commercial_offers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "secure_link_revoked_at_utc",
                schema: "crm",
                table: "commercial_offers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "secure_link_token_hash",
                schema: "crm",
                table: "commercial_offers",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_commercial_offers_secure_link_token_hash",
                schema: "crm",
                table: "commercial_offers",
                column: "secure_link_token_hash",
                unique: true,
                filter: "secure_link_token_hash IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_commercial_offers_secure_link_token_hash",
                schema: "crm",
                table: "commercial_offers");

            migrationBuilder.DropColumn(
                name: "attachment_snapshot_json",
                schema: "crm",
                table: "commercial_offers");

            migrationBuilder.DropColumn(
                name: "delivery_attempt_count",
                schema: "crm",
                table: "commercial_offers");

            migrationBuilder.DropColumn(
                name: "delivery_channel",
                schema: "crm",
                table: "commercial_offers");

            migrationBuilder.DropColumn(
                name: "delivery_language",
                schema: "crm",
                table: "commercial_offers");

            migrationBuilder.DropColumn(
                name: "delivery_message",
                schema: "crm",
                table: "commercial_offers");

            migrationBuilder.DropColumn(
                name: "delivery_status",
                schema: "crm",
                table: "commercial_offers");

            migrationBuilder.DropColumn(
                name: "delivery_subject",
                schema: "crm",
                table: "commercial_offers");

            migrationBuilder.DropColumn(
                name: "document_reference",
                schema: "crm",
                table: "commercial_offers");

            migrationBuilder.DropColumn(
                name: "recipient_snapshot_json",
                schema: "crm",
                table: "commercial_offers");

            migrationBuilder.DropColumn(
                name: "secure_link_expires_at_utc",
                schema: "crm",
                table: "commercial_offers");

            migrationBuilder.DropColumn(
                name: "secure_link_revoked_at_utc",
                schema: "crm",
                table: "commercial_offers");

            migrationBuilder.DropColumn(
                name: "secure_link_token_hash",
                schema: "crm",
                table: "commercial_offers");
        }
    }
}
