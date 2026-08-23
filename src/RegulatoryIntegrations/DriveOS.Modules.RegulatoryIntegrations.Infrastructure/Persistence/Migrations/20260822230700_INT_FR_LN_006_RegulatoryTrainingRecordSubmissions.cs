using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.RegulatoryIntegrations.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class INT_FR_LN_006_RegulatoryTrainingRecordSubmissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "regulatory_integrations");

            migrationBuilder.CreateTable(
                name: "training_record_submissions",
                schema: "regulatory_integrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectionSchemaVersion = table.Column<int>(type: "integer", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingPathId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    ProviderCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false),
                    PayloadHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IssuesJson = table.Column<string>(type: "jsonb", nullable: false),
                    Revision = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    SupersedesSubmissionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastAttemptAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    NextAttemptAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SubmittedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AcknowledgedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExternalReference = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    LastErrorCode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LastErrorDetail = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_record_submissions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_training_record_submissions_OrganizationId_SessionId",
                schema: "regulatory_integrations",
                table: "training_record_submissions",
                columns: new[] { "OrganizationId", "SessionId" });

            migrationBuilder.CreateIndex(
                name: "IX_training_record_submissions_OrganizationId_SessionId_Provid~",
                schema: "regulatory_integrations",
                table: "training_record_submissions",
                columns: new[] { "OrganizationId", "SessionId", "ProviderCode", "Revision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_training_record_submissions_OrganizationId_StudentId_Status",
                schema: "regulatory_integrations",
                table: "training_record_submissions",
                columns: new[] { "OrganizationId", "StudentId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_training_record_submissions_OrganizationId_StudentId_Traini~",
                schema: "regulatory_integrations",
                table: "training_record_submissions",
                columns: new[] { "OrganizationId", "StudentId", "TrainingPathId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_training_record_submissions_ProjectionId_ProviderCode_Revis~",
                schema: "regulatory_integrations",
                table: "training_record_submissions",
                columns: new[] { "ProjectionId", "ProviderCode", "Revision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_training_record_submissions_Status_NextAttemptAtUtc",
                schema: "regulatory_integrations",
                table: "training_record_submissions",
                columns: new[] { "Status", "NextAttemptAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "training_record_submissions",
                schema: "regulatory_integrations");
        }
    }
}
