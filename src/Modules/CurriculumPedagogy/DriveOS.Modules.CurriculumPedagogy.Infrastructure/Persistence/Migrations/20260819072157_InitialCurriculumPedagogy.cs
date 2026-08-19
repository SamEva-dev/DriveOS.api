using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.CurriculumPedagogy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCurriculumPedagogy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "curriculum_pedagogy");

            migrationBuilder.CreateTable(
                name: "competency_records",
                schema: "curriculum_pedagogy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingPathId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurriculumVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompetencyId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_competency_records", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "curricula",
                schema: "curriculum_pedagogy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CountryCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    LicenseCategoryCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ArchivedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ArchivedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_curricula", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "license_category_definitions",
                schema: "curriculum_pedagogy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ActivatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ActivatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ArchivedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ArchivedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_license_category_definitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "pedagogical_readiness_decisions",
                schema: "curriculum_pedagogy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingPathId = table.Column<Guid>(type: "uuid", nullable: false),
                    Decision = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Rationale = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Conditions = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ReviewerId = table.Column<Guid>(type: "uuid", nullable: false),
                    DecidedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pedagogical_readiness_decisions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "pedagogical_reviews",
                schema: "curriculum_pedagogy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingPathId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Findings = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    Recommendations = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    EstimatedRemainingPracticalHours = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: true),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    RequestedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pedagogical_reviews", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "remediation_plans",
                schema: "curriculum_pedagogy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingPathId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResponsibleUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourcePedagogicalReviewId = table.Column<Guid>(type: "uuid", nullable: true),
                    Recommendation = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    RecommendedPracticalHours = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: true),
                    RecommendedSessions = table.Column<int>(type: "integer", nullable: true),
                    ReviewDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    PlanCreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ActivatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_remediation_plans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "training_paths",
                schema: "curriculum_pedagogy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurriculumVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingMode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TargetCompletionDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EstimatedPracticalHours = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: true),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ActivatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ActivatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SuspendedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SuspensionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CancelledAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_paths", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "competency_assessments",
                schema: "curriculum_pedagogy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompetencyRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    LevelCode = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    AssessorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceSessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Comment = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    IsVisibleToStudent = table.Column<bool>(type: "boolean", nullable: false),
                    AssessedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RecordedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_competency_assessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_competency_assessments_competency_records_CompetencyRecordId",
                        column: x => x.CompetencyRecordId,
                        principalSchema: "curriculum_pedagogy",
                        principalTable: "competency_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "curriculum_versions",
                schema: "curriculum_pedagogy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CurriculumId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    SourceVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    NameSnapshot = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    DescriptionSnapshot = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CountryCodeSnapshot = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    LicenseCategoryCodeSnapshot = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    ChangeSummary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PublishedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    PublishedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_curriculum_versions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_curriculum_versions_curricula_CurriculumId",
                        column: x => x.CurriculumId,
                        principalSchema: "curriculum_pedagogy",
                        principalTable: "curricula",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "remediation_plan_targets",
                schema: "curriculum_pedagogy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompetencyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Objective = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    RemediationPlanId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_remediation_plan_targets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_remediation_plan_targets_remediation_plans_RemediationPlanId",
                        column: x => x.RemediationPlanId,
                        principalSchema: "curriculum_pedagogy",
                        principalTable: "remediation_plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "training_path_milestones",
                schema: "curriculum_pedagogy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingPathId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    TargetDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_path_milestones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_training_path_milestones_training_paths_TrainingPathId",
                        column: x => x.TrainingPathId,
                        principalSchema: "curriculum_pedagogy",
                        principalTable: "training_paths",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "curriculum_modules",
                schema: "curriculum_pedagogy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CurriculumVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_curriculum_modules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_curriculum_modules_curriculum_versions_CurriculumVersionId",
                        column: x => x.CurriculumVersionId,
                        principalSchema: "curriculum_pedagogy",
                        principalTable: "curriculum_versions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "competencies",
                schema: "curriculum_pedagogy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CurriculumModuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    LearningObjective = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_competencies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_competencies_curriculum_modules_CurriculumModuleId",
                        column: x => x.CurriculumModuleId,
                        principalSchema: "curriculum_pedagogy",
                        principalTable: "curriculum_modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_competencies_CurriculumModuleId_Code",
                schema: "curriculum_pedagogy",
                table: "competencies",
                columns: new[] { "CurriculumModuleId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_competencies_CurriculumModuleId_Order",
                schema: "curriculum_pedagogy",
                table: "competencies",
                columns: new[] { "CurriculumModuleId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_competency_assessments_CompetencyRecordId_AssessedAtUtc",
                schema: "curriculum_pedagogy",
                table: "competency_assessments",
                columns: new[] { "CompetencyRecordId", "AssessedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_competency_assessments_CompetencyRecordId_SourceSessionId",
                schema: "curriculum_pedagogy",
                table: "competency_assessments",
                columns: new[] { "CompetencyRecordId", "SourceSessionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_competency_records_OrganizationId_CurriculumVersionId",
                schema: "curriculum_pedagogy",
                table: "competency_records",
                columns: new[] { "OrganizationId", "CurriculumVersionId" });

            migrationBuilder.CreateIndex(
                name: "IX_competency_records_OrganizationId_TrainingPathId_Competency~",
                schema: "curriculum_pedagogy",
                table: "competency_records",
                columns: new[] { "OrganizationId", "TrainingPathId", "CompetencyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_curricula_OrganizationId_Code",
                schema: "curriculum_pedagogy",
                table: "curricula",
                columns: new[] { "OrganizationId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_curriculum_modules_CurriculumVersionId_Code",
                schema: "curriculum_pedagogy",
                table: "curriculum_modules",
                columns: new[] { "CurriculumVersionId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_curriculum_modules_CurriculumVersionId_Order",
                schema: "curriculum_pedagogy",
                table: "curriculum_modules",
                columns: new[] { "CurriculumVersionId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_curriculum_versions_CurriculumId_VersionNumber",
                schema: "curriculum_pedagogy",
                table: "curriculum_versions",
                columns: new[] { "CurriculumId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_license_category_definitions_OrganizationId_CountryCode_Code",
                schema: "curriculum_pedagogy",
                table: "license_category_definitions",
                columns: new[] { "OrganizationId", "CountryCode", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pedagogical_readiness_decisions_OrganizationId_TrainingPath~",
                schema: "curriculum_pedagogy",
                table: "pedagogical_readiness_decisions",
                columns: new[] { "OrganizationId", "TrainingPathId", "DecidedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_pedagogical_reviews_OrganizationId_StudentId_Status",
                schema: "curriculum_pedagogy",
                table: "pedagogical_reviews",
                columns: new[] { "OrganizationId", "StudentId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_pedagogical_reviews_OrganizationId_TrainingPathId_Requested~",
                schema: "curriculum_pedagogy",
                table: "pedagogical_reviews",
                columns: new[] { "OrganizationId", "TrainingPathId", "RequestedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_remediation_plan_targets_RemediationPlanId_CompetencyId",
                schema: "curriculum_pedagogy",
                table: "remediation_plan_targets",
                columns: new[] { "RemediationPlanId", "CompetencyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_remediation_plans_OrganizationId_TrainingPathId_Status",
                schema: "curriculum_pedagogy",
                table: "remediation_plans",
                columns: new[] { "OrganizationId", "TrainingPathId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_training_path_milestones_TrainingPathId_Code",
                schema: "curriculum_pedagogy",
                table: "training_path_milestones",
                columns: new[] { "TrainingPathId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_training_path_milestones_TrainingPathId_Order",
                schema: "curriculum_pedagogy",
                table: "training_path_milestones",
                columns: new[] { "TrainingPathId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_training_paths_OrganizationId_StudentId",
                schema: "curriculum_pedagogy",
                table: "training_paths",
                columns: new[] { "OrganizationId", "StudentId" });

            migrationBuilder.CreateIndex(
                name: "IX_training_paths_OrganizationId_StudentId_CurriculumVersionId",
                schema: "curriculum_pedagogy",
                table: "training_paths",
                columns: new[] { "OrganizationId", "StudentId", "CurriculumVersionId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "competencies",
                schema: "curriculum_pedagogy");

            migrationBuilder.DropTable(
                name: "competency_assessments",
                schema: "curriculum_pedagogy");

            migrationBuilder.DropTable(
                name: "license_category_definitions",
                schema: "curriculum_pedagogy");

            migrationBuilder.DropTable(
                name: "pedagogical_readiness_decisions",
                schema: "curriculum_pedagogy");

            migrationBuilder.DropTable(
                name: "pedagogical_reviews",
                schema: "curriculum_pedagogy");

            migrationBuilder.DropTable(
                name: "remediation_plan_targets",
                schema: "curriculum_pedagogy");

            migrationBuilder.DropTable(
                name: "training_path_milestones",
                schema: "curriculum_pedagogy");

            migrationBuilder.DropTable(
                name: "curriculum_modules",
                schema: "curriculum_pedagogy");

            migrationBuilder.DropTable(
                name: "competency_records",
                schema: "curriculum_pedagogy");

            migrationBuilder.DropTable(
                name: "remediation_plans",
                schema: "curriculum_pedagogy");

            migrationBuilder.DropTable(
                name: "training_paths",
                schema: "curriculum_pedagogy");

            migrationBuilder.DropTable(
                name: "curriculum_versions",
                schema: "curriculum_pedagogy");

            migrationBuilder.DropTable(
                name: "curricula",
                schema: "curriculum_pedagogy");
        }
    }
}
