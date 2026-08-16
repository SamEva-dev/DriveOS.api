using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.CRM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLeadQualification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "qualification_availability",
                schema: "crm",
                table: "leads",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "qualification_financing",
                schema: "crm",
                table: "leads",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "qualification_license_category",
                schema: "crm",
                table: "leads",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "qualification_need",
                schema: "crm",
                table: "leads",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "qualification_notes",
                schema: "crm",
                table: "leads",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true
            );

            migrationBuilder.AddColumn<DateOnly>(
                name: "qualification_target_date",
                schema: "crm",
                table: "leads",
                type: "date",
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "qualification_availability",
                schema: "crm",
                table: "leads"
            );

            migrationBuilder.DropColumn(
                name: "qualification_financing",
                schema: "crm",
                table: "leads"
            );

            migrationBuilder.DropColumn(
                name: "qualification_license_category",
                schema: "crm",
                table: "leads"
            );

            migrationBuilder.DropColumn(name: "qualification_need", schema: "crm", table: "leads");

            migrationBuilder.DropColumn(name: "qualification_notes", schema: "crm", table: "leads");

            migrationBuilder.DropColumn(
                name: "qualification_target_date",
                schema: "crm",
                table: "leads"
            );
        }
    }
}
