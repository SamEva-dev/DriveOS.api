using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.Contracts.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BC13_CLOSE_ProfessionalServiceContracts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "professional_service_contracts",
                schema: "contracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    EngagementId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessionalProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderOrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractNumber = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ContractType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    SignatureOrder = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    TermsSnapshotJson = table.Column<string>(type: "jsonb", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Signatories = table.Column<string>(type: "jsonb", nullable: false),
                    PreviousVersions = table.Column<string>(type: "jsonb", nullable: false),
                    DocumentReference = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DocumentSha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    GeneratedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    GeneratedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SentForSignatureAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SentForSignatureByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SignedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TerminatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TerminatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    TerminationReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_professional_service_contracts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_professional_service_contracts_EngagementId",
                schema: "contracts",
                table: "professional_service_contracts",
                column: "EngagementId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_professional_service_contracts_OrganizationId_Status",
                schema: "contracts",
                table: "professional_service_contracts",
                columns: new[] { "OrganizationId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "professional_service_contracts",
                schema: "contracts");
        }
    }
}
