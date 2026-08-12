using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.CRM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCommercialOfferTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "last_contact_at_utc",
                schema: "crm",
                table: "commercial_offers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "last_viewed_at_utc",
                schema: "crm",
                table: "commercial_offers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "next_follow_up_at_utc",
                schema: "crm",
                table: "commercial_offers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "view_count",
                schema: "crm",
                table: "commercial_offers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "viewed_at_utc",
                schema: "crm",
                table: "commercial_offers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "commercial_offer_interactions",
                schema: "crm",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    interaction_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    occurred_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    actor_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    metadata_json = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_commercial_offer_interactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_commercial_offer_interactions_commercial_offers_offer_id",
                        column: x => x.offer_id,
                        principalSchema: "crm",
                        principalTable: "commercial_offers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_commercial_offer_interactions_offer_id_occurred_at_utc",
                schema: "crm",
                table: "commercial_offer_interactions",
                columns: new[] { "offer_id", "occurred_at_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "commercial_offer_interactions",
                schema: "crm");

            migrationBuilder.DropColumn(
                name: "last_contact_at_utc",
                schema: "crm",
                table: "commercial_offers");

            migrationBuilder.DropColumn(
                name: "last_viewed_at_utc",
                schema: "crm",
                table: "commercial_offers");

            migrationBuilder.DropColumn(
                name: "next_follow_up_at_utc",
                schema: "crm",
                table: "commercial_offers");

            migrationBuilder.DropColumn(
                name: "view_count",
                schema: "crm",
                table: "commercial_offers");

            migrationBuilder.DropColumn(
                name: "viewed_at_utc",
                schema: "crm",
                table: "commercial_offers");
        }
    }
}
