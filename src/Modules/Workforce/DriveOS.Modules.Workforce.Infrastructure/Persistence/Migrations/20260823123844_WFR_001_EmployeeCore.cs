using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.Workforce.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WFR_001_EmployeeCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "workforce");

            migrationBuilder.CreateTable(
                name: "employee_documents",
                schema: "workforce",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    DocumentTypeCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Confidentiality = table.Column<int>(type: "integer", nullable: false),
                    IssuedOn = table.Column<DateOnly>(type: "date", nullable: true),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    ExpiresOn = table.Column<DateOnly>(type: "date", nullable: true),
                    Issuer = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ReferenceNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    VerifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RevocationReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    SupersededByEmployeeDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee_documents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "employees",
                schema: "workforce",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployerOrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EmploymentStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EmploymentEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    rehired_from_employee_id = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "equipment_assignments",
                schema: "workforce",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceType = table.Column<int>(type: "integer", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PlannedEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ReturnedOn = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    HandoverCondition = table.Column<int>(type: "integer", nullable: false),
                    HandoverNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    HandedOverAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    HandedOverByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReturnCondition = table.Column<int>(type: "integer", nullable: false),
                    ReturnNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ReturnedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReturnedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_equipment_assignments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "job_positions",
                schema: "workforce",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ProfessionalFunction = table.Column<string>(type: "character varying(48)", maxLength: 48, nullable: false),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_positions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "leave_policies",
                schema: "workforce",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Category = table.Column<string>(type: "character varying(48)", maxLength: 48, nullable: false),
                    IsPaid = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresApproval = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresEvidence = table.Column<bool>(type: "boolean", nullable: false),
                    AllowHalfDay = table.Column<bool>(type: "boolean", nullable: false),
                    MinimumNoticeDays = table.Column<int>(type: "integer", nullable: true),
                    MaximumConsecutiveDays = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leave_policies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "leave_requests",
                schema: "workforce",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeavePolicyId = table.Column<Guid>(type: "uuid", nullable: false),
                    PolicyCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartPortion = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    EndPortion = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    EvidenceDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequiresApproval = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresEvidence = table.Column<bool>(type: "boolean", nullable: false),
                    AllowHalfDay = table.Column<bool>(type: "boolean", nullable: false),
                    MinimumNoticeDays = table.Column<int>(type: "integer", nullable: true),
                    MaximumConsecutiveDays = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    SubmittedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DecidedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DecidedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecisionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CancelledAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leave_requests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "offboarding_processes",
                schema: "workforce",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlannedEndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelledAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_offboarding_processes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "performance_reviews",
                schema: "workforce",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvaluatorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodTo = table.Column<DateOnly>(type: "date", nullable: false),
                    Title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OverallAssessment = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Objectives = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    SubmittedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    AcknowledgedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AcknowledgedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeComment = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_performance_reviews", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "professional_restrictions",
                schema: "workforce",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Activity = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    LicenseCategoryCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    SupportingDocumentReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ActivatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ActivatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LiftedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LiftedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LiftReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CancelledAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_professional_restrictions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "timesheets",
                schema: "workforce",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodTo = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SubmittedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewStartedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReviewerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DecidedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecisionReason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    LockedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_timesheets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "working_time_policies",
                schema: "workforce",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    ContractualWeeklyHours = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    ContractualDailyHours = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: true),
                    MaxWorkingDaysPerWeek = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_working_time_policies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "employee_branch_assignments",
                schema: "workforce",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee_branch_assignments", x => x.id);
                    table.ForeignKey(
                        name: "FK_employee_branch_assignments_employees_employee_id",
                        column: x => x.employee_id,
                        principalSchema: "workforce",
                        principalTable: "employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "employee_employment_contracts",
                schema: "workforce",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    contract_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    contractual_weekly_hours = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    primary_job_position_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    contract_document_id = table.Column<Guid>(type: "uuid", nullable: true),
                    signature_process_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee_employment_contracts", x => x.id);
                    table.ForeignKey(
                        name: "FK_employee_employment_contracts_employees_employee_id",
                        column: x => x.employee_id,
                        principalSchema: "workforce",
                        principalTable: "employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "employee_instructor_authorizations",
                schema: "workforce",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    country_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    authorization_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    identifier = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    issuing_authority = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    jurisdiction_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    license_category_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IssuedOn = table.Column<DateOnly>(type: "date", nullable: true),
                    ExpiresOn = table.Column<DateOnly>(type: "date", nullable: true),
                    source = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DeclaredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    declared_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    VerifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    verified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    VerificationMethod = table.Column<string>(type: "text", nullable: true),
                    DecisionReason = table.Column<string>(type: "text", nullable: true),
                    superseded_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee_instructor_authorizations", x => x.id);
                    table.ForeignKey(
                        name: "FK_employee_instructor_authorizations_employees_employee_id",
                        column: x => x.employee_id,
                        principalSchema: "workforce",
                        principalTable: "employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "employee_job_position_assignments",
                schema: "workforce",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_position_id = table.Column<Guid>(type: "uuid", nullable: false),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: true),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee_job_position_assignments", x => x.id);
                    table.ForeignKey(
                        name: "FK_employee_job_position_assignments_employees_employee_id",
                        column: x => x.employee_id,
                        principalSchema: "workforce",
                        principalTable: "employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "employee_qualifications",
                schema: "workforce",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    country_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    qualification_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    identifier = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    issuing_authority = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    IssuedOn = table.Column<DateOnly>(type: "date", nullable: true),
                    ExpiresOn = table.Column<DateOnly>(type: "date", nullable: true),
                    source = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DeclaredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    declared_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    VerifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    verified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    VerificationMethod = table.Column<string>(type: "text", nullable: true),
                    DecisionReason = table.Column<string>(type: "text", nullable: true),
                    superseded_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee_qualifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_employee_qualifications_employees_employee_id",
                        column: x => x.employee_id,
                        principalSchema: "workforce",
                        principalTable: "employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "offboarding_checklist_items",
                schema: "workforce",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IsAutomatic = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    BlockerCount = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ResolvedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ResolvedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    WaiverReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LastEvaluatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    offboarding_process_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_offboarding_checklist_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_offboarding_checklist_items_offboarding_processes_offboardi~",
                        column: x => x.offboarding_process_id,
                        principalSchema: "workforce",
                        principalTable: "offboarding_processes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "performance_review_criteria",
                schema: "workforce",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Weight = table.Column<int>(type: "integer", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: true),
                    Comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    performance_review_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_performance_review_criteria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_performance_review_criteria_performance_reviews_performance~",
                        column: x => x.performance_review_id,
                        principalSchema: "workforce",
                        principalTable: "performance_reviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "timesheet_entries",
                schema: "workforce",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    ActivityType = table.Column<int>(type: "integer", nullable: false),
                    Hours = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    SourceReference = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    timesheet_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_timesheet_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_timesheet_entries_timesheets_timesheet_id",
                        column: x => x.timesheet_id,
                        principalSchema: "workforce",
                        principalTable: "timesheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_employee_branch_assignments_employee_id_branch_id_start_date",
                schema: "workforce",
                table: "employee_branch_assignments",
                columns: new[] { "employee_id", "branch_id", "start_date" });

            migrationBuilder.CreateIndex(
                name: "IX_employee_branch_assignments_employee_id_is_primary_start_da~",
                schema: "workforce",
                table: "employee_branch_assignments",
                columns: new[] { "employee_id", "is_primary", "start_date", "end_date" });

            migrationBuilder.CreateIndex(
                name: "IX_employee_documents_OrganizationId_DocumentReferenceId",
                schema: "workforce",
                table: "employee_documents",
                columns: new[] { "OrganizationId", "DocumentReferenceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_employee_documents_OrganizationId_EmployeeId_Category",
                schema: "workforce",
                table: "employee_documents",
                columns: new[] { "OrganizationId", "EmployeeId", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_employee_documents_OrganizationId_EmployeeId_Status",
                schema: "workforce",
                table: "employee_documents",
                columns: new[] { "OrganizationId", "EmployeeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_employee_documents_OrganizationId_ExpiresOn",
                schema: "workforce",
                table: "employee_documents",
                columns: new[] { "OrganizationId", "ExpiresOn" });

            migrationBuilder.CreateIndex(
                name: "IX_employee_employment_contracts_contract_document_id",
                schema: "workforce",
                table: "employee_employment_contracts",
                column: "contract_document_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_employment_contracts_employee_id_start_date_end_da~",
                schema: "workforce",
                table: "employee_employment_contracts",
                columns: new[] { "employee_id", "start_date", "end_date" });

            migrationBuilder.CreateIndex(
                name: "IX_employee_employment_contracts_employee_id_status",
                schema: "workforce",
                table: "employee_employment_contracts",
                columns: new[] { "employee_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_employee_instructor_authorizations_employee_id_country_code~",
                schema: "workforce",
                table: "employee_instructor_authorizations",
                columns: new[] { "employee_id", "country_code", "authorization_type", "license_category_code", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_employee_job_position_assignments_employee_id_branch_id_sta~",
                schema: "workforce",
                table: "employee_job_position_assignments",
                columns: new[] { "employee_id", "branch_id", "start_date", "end_date" });

            migrationBuilder.CreateIndex(
                name: "IX_employee_job_position_assignments_employee_id_is_primary_st~",
                schema: "workforce",
                table: "employee_job_position_assignments",
                columns: new[] { "employee_id", "is_primary", "start_date", "end_date" });

            migrationBuilder.CreateIndex(
                name: "IX_employee_job_position_assignments_employee_id_job_position_~",
                schema: "workforce",
                table: "employee_job_position_assignments",
                columns: new[] { "employee_id", "job_position_id", "start_date" });

            migrationBuilder.CreateIndex(
                name: "IX_employee_qualifications_employee_id_country_code_qualificat~",
                schema: "workforce",
                table: "employee_qualifications",
                columns: new[] { "employee_id", "country_code", "qualification_type", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_employees_EmployerOrganizationId_EmployeeNumber",
                schema: "workforce",
                table: "employees",
                columns: new[] { "EmployerOrganizationId", "EmployeeNumber" },
                unique: true,
                filter: "\"Status\" <> 'Ended'");

            migrationBuilder.CreateIndex(
                name: "IX_employees_EmployerOrganizationId_PersonId_Status",
                schema: "workforce",
                table: "employees",
                columns: new[] { "EmployerOrganizationId", "PersonId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_employees_EmployerOrganizationId_rehired_from_employee_id",
                schema: "workforce",
                table: "employees",
                columns: new[] { "EmployerOrganizationId", "rehired_from_employee_id" });

            migrationBuilder.CreateIndex(
                name: "IX_employees_EmployerOrganizationId_Status",
                schema: "workforce",
                table: "employees",
                columns: new[] { "EmployerOrganizationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_employees_EmployerOrganizationId_UserId_Status",
                schema: "workforce",
                table: "employees",
                columns: new[] { "EmployerOrganizationId", "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_equipment_assignments_OrganizationId_EmployeeId_Status",
                schema: "workforce",
                table: "equipment_assignments",
                columns: new[] { "OrganizationId", "EmployeeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_equipment_assignments_OrganizationId_ResourceType_ResourceI~",
                schema: "workforce",
                table: "equipment_assignments",
                columns: new[] { "OrganizationId", "ResourceType", "ResourceId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_equipment_assignments_OrganizationId_StartDate_PlannedEndDa~",
                schema: "workforce",
                table: "equipment_assignments",
                columns: new[] { "OrganizationId", "StartDate", "PlannedEndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_job_positions_OrganizationId_Code",
                schema: "workforce",
                table: "job_positions",
                columns: new[] { "OrganizationId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_job_positions_OrganizationId_ProfessionalFunction_Status",
                schema: "workforce",
                table: "job_positions",
                columns: new[] { "OrganizationId", "ProfessionalFunction", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_leave_policies_OrganizationId_CountryCode_Code",
                schema: "workforce",
                table: "leave_policies",
                columns: new[] { "OrganizationId", "CountryCode", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_leave_policies_OrganizationId_CountryCode_Status",
                schema: "workforce",
                table: "leave_policies",
                columns: new[] { "OrganizationId", "CountryCode", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_leave_requests_OrganizationId_EmployeeId_Status",
                schema: "workforce",
                table: "leave_requests",
                columns: new[] { "OrganizationId", "EmployeeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_leave_requests_OrganizationId_LeavePolicyId_Status",
                schema: "workforce",
                table: "leave_requests",
                columns: new[] { "OrganizationId", "LeavePolicyId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_leave_requests_OrganizationId_StartDate_EndDate",
                schema: "workforce",
                table: "leave_requests",
                columns: new[] { "OrganizationId", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_offboarding_checklist_items_offboarding_process_id_Kind",
                schema: "workforce",
                table: "offboarding_checklist_items",
                columns: new[] { "offboarding_process_id", "Kind" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_offboarding_processes_OrganizationId_EmployeeId_Status",
                schema: "workforce",
                table: "offboarding_processes",
                columns: new[] { "OrganizationId", "EmployeeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_performance_review_criteria_performance_review_id_Code",
                schema: "workforce",
                table: "performance_review_criteria",
                columns: new[] { "performance_review_id", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_performance_reviews_OrganizationId_EmployeeId_Status",
                schema: "workforce",
                table: "performance_reviews",
                columns: new[] { "OrganizationId", "EmployeeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_performance_reviews_OrganizationId_PeriodFrom_PeriodTo",
                schema: "workforce",
                table: "performance_reviews",
                columns: new[] { "OrganizationId", "PeriodFrom", "PeriodTo" });

            migrationBuilder.CreateIndex(
                name: "IX_professional_restrictions_OrganizationId_Activity_Status_St~",
                schema: "workforce",
                table: "professional_restrictions",
                columns: new[] { "OrganizationId", "Activity", "Status", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_professional_restrictions_OrganizationId_EmployeeId_Country~",
                schema: "workforce",
                table: "professional_restrictions",
                columns: new[] { "OrganizationId", "EmployeeId", "CountryCode", "LicenseCategoryCode" });

            migrationBuilder.CreateIndex(
                name: "IX_professional_restrictions_OrganizationId_EmployeeId_Status",
                schema: "workforce",
                table: "professional_restrictions",
                columns: new[] { "OrganizationId", "EmployeeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_timesheet_entries_timesheet_id_Date",
                schema: "workforce",
                table: "timesheet_entries",
                columns: new[] { "timesheet_id", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_timesheets_OrganizationId_EmployeeId_PeriodFrom_PeriodTo",
                schema: "workforce",
                table: "timesheets",
                columns: new[] { "OrganizationId", "EmployeeId", "PeriodFrom", "PeriodTo" });

            migrationBuilder.CreateIndex(
                name: "IX_timesheets_OrganizationId_Status",
                schema: "workforce",
                table: "timesheets",
                columns: new[] { "OrganizationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_working_time_policies_OrganizationId_EmployeeId_EffectiveFr~",
                schema: "workforce",
                table: "working_time_policies",
                columns: new[] { "OrganizationId", "EmployeeId", "EffectiveFrom" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "employee_branch_assignments",
                schema: "workforce");

            migrationBuilder.DropTable(
                name: "employee_documents",
                schema: "workforce");

            migrationBuilder.DropTable(
                name: "employee_employment_contracts",
                schema: "workforce");

            migrationBuilder.DropTable(
                name: "employee_instructor_authorizations",
                schema: "workforce");

            migrationBuilder.DropTable(
                name: "employee_job_position_assignments",
                schema: "workforce");

            migrationBuilder.DropTable(
                name: "employee_qualifications",
                schema: "workforce");

            migrationBuilder.DropTable(
                name: "equipment_assignments",
                schema: "workforce");

            migrationBuilder.DropTable(
                name: "job_positions",
                schema: "workforce");

            migrationBuilder.DropTable(
                name: "leave_policies",
                schema: "workforce");

            migrationBuilder.DropTable(
                name: "leave_requests",
                schema: "workforce");

            migrationBuilder.DropTable(
                name: "offboarding_checklist_items",
                schema: "workforce");

            migrationBuilder.DropTable(
                name: "performance_review_criteria",
                schema: "workforce");

            migrationBuilder.DropTable(
                name: "professional_restrictions",
                schema: "workforce");

            migrationBuilder.DropTable(
                name: "timesheet_entries",
                schema: "workforce");

            migrationBuilder.DropTable(
                name: "working_time_policies",
                schema: "workforce");

            migrationBuilder.DropTable(
                name: "employees",
                schema: "workforce");

            migrationBuilder.DropTable(
                name: "offboarding_processes",
                schema: "workforce");

            migrationBuilder.DropTable(
                name: "performance_reviews",
                schema: "workforce");

            migrationBuilder.DropTable(
                name: "timesheets",
                schema: "workforce");
        }
    }
}
