using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.Students.Infrastructure.Persistence.Migrastions
{
    /// <inheritdoc />
    public partial class initialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "students");

            migrationBuilder.CreateTable(
                name: "enrollment_checklist_rules",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    training_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    label_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    category = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    is_blocking = table.Column<bool>(type: "boolean", nullable: false),
                    target_route = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    due_in_days = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enrollment_checklist_rules", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "students",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    phone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    preferred_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: true),
                    birth_place = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    nationality = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    address_line1 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    address_line2 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    country_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    preferred_language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    time_zone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    allow_email = table.Column<bool>(type: "boolean", nullable: false),
                    allow_sms = table.Column<bool>(type: "boolean", nullable: false),
                    allow_phone = table.Column<bool>(type: "boolean", nullable: false),
                    identity_verification_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    identity_verified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    identity_verified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_students", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "administrative_cases",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_administrative_cases", x => x.id);
                    table.ForeignKey(
                        name: "FK_administrative_cases_students_student_id",
                        column: x => x.student_id,
                        principalSchema: "students",
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "enrollments",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_lead_id = table.Column<Guid>(type: "uuid", nullable: true),
                    training_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    source = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    regulatory_country_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    preferred_language_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    required_consents_accepted = table.Column<bool>(type: "boolean", nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enrollments", x => x.id);
                    table.ForeignKey(
                        name: "FK_enrollments_students_student_id",
                        column: x => x.student_id,
                        principalSchema: "students",
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "external_transfer_cases",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transfer_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    data_scope = table.Column<long>(type: "bigint", nullable: false),
                    effective_on = table.Column<DateOnly>(type: "date", nullable: false),
                    temporary_until = table.Column<DateOnly>(type: "date", nullable: true),
                    country_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    responsibilities = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    consent_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    consent_evidence_reference = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    consent_verified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    financial_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    financial_resolution = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    relationship_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    completed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_external_transfer_cases", x => x.id);
                    table.ForeignKey(
                        name: "FK_external_transfer_cases_students_student_id",
                        column: x => x.student_id,
                        principalSchema: "students",
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "guardian_relationships",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    guardian_person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    guardian_first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    guardian_last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    guardian_email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    guardian_phone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    relationship_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    legal_basis = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    parental_authority_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    permissions = table.Column<long>(type: "bigint", nullable: false),
                    effective_from = table.Column<DateOnly>(type: "date", nullable: false),
                    effective_to = table.Column<DateOnly>(type: "date", nullable: true),
                    financial_rights = table.Column<bool>(type: "boolean", nullable: false),
                    signature_rights = table.Column<bool>(type: "boolean", nullable: false),
                    notification_preferences = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    invited_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    invited_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    revocation_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guardian_relationships", x => x.id);
                    table.ForeignKey(
                        name: "FK_guardian_relationships_students_student_id",
                        column: x => x.student_id,
                        principalSchema: "students",
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "internal_transfer_cases",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_branch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_branch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    mode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    elements = table.Column<int>(type: "integer", nullable: false),
                    effective_on = table.Column<DateOnly>(type: "date", nullable: false),
                    temporary_until = table.Column<DateOnly>(type: "date", nullable: true),
                    reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    analyzed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    analysis_expires_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    analyzed_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    validated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    validated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_internal_transfer_cases", x => x.id);
                    table.ForeignKey(
                        name: "FK_internal_transfer_cases_students_student_id",
                        column: x => x.student_id,
                        principalSchema: "students",
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "student_branch_portfolios",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_branch_portfolios", x => x.id);
                    table.ForeignKey(
                        name: "FK_student_branch_portfolios_students_student_id",
                        column: x => x.student_id,
                        principalSchema: "students",
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "student_identity_audit",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    justification = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    actor_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    occurred_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_identity_audit", x => x.id);
                    table.ForeignKey(
                        name: "FK_student_identity_audit_students_student_id",
                        column: x => x.student_id,
                        principalSchema: "students",
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "student_instructor_portfolios",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_instructor_portfolios", x => x.id);
                    table.ForeignKey(
                        name: "FK_student_instructor_portfolios_students_student_id",
                        column: x => x.student_id,
                        principalSchema: "students",
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "student_relationships",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    party_id = table.Column<Guid>(type: "uuid", nullable: false),
                    party_kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    phone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    relationship_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    permissions = table.Column<int>(type: "integer", nullable: false),
                    financial_scope = table.Column<int>(type: "integer", nullable: false),
                    communication_scope = table.Column<int>(type: "integer", nullable: false),
                    effective_from = table.Column<DateOnly>(type: "date", nullable: false),
                    effective_to = table.Column<DateOnly>(type: "date", nullable: true),
                    is_primary_payer = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    invited_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    invited_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_relationships", x => x.id);
                    table.ForeignKey(
                        name: "FK_student_relationships_students_student_id",
                        column: x => x.student_id,
                        principalSchema: "students",
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "student_status_boards",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    financial_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    pedagogical_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    scheduling_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    exam_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    portal_access_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_status_boards", x => x.id);
                    table.ForeignKey(
                        name: "FK_student_status_boards_students_student_id",
                        column: x => x.student_id,
                        principalSchema: "students",
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "administrative_blocks",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    administrative_case_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    applied_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    applied_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    release_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    released_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    released_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_administrative_blocks", x => x.id);
                    table.ForeignKey(
                        name: "FK_administrative_blocks_administrative_cases_administrative_c~",
                        column: x => x.administrative_case_id,
                        principalSchema: "students",
                        principalTable: "administrative_cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "administrative_history",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    administrative_case_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    subject_id = table.Column<Guid>(type: "uuid", nullable: false),
                    actor_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    occurred_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    detail = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_administrative_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_administrative_history_administrative_cases_administrative_~",
                        column: x => x.administrative_case_id,
                        principalSchema: "students",
                        principalTable: "administrative_cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "administrative_requirements",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    administrative_case_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    label_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_blocking = table.Column<bool>(type: "boolean", nullable: false),
                    due_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    policy_source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    decision_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    decided_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    decided_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_administrative_requirements", x => x.id);
                    table.ForeignKey(
                        name: "FK_administrative_requirements_administrative_cases_administra~",
                        column: x => x.administrative_case_id,
                        principalSchema: "students",
                        principalTable: "administrative_cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compliance_exceptions",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    administrative_case_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requirement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    request_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    requested_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requested_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    decision_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    decided_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    decided_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliance_exceptions", x => x.id);
                    table.ForeignKey(
                        name: "FK_compliance_exceptions_administrative_cases_administrative_c~",
                        column: x => x.administrative_case_id,
                        principalSchema: "students",
                        principalTable: "administrative_cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "enrollment_checklists",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    enrollment_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enrollment_checklists", x => x.id);
                    table.ForeignKey(
                        name: "FK_enrollment_checklists_enrollments_enrollment_id",
                        column: x => x.enrollment_id,
                        principalSchema: "students",
                        principalTable: "enrollments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_enrollment_checklists_students_student_id",
                        column: x => x.student_id,
                        principalSchema: "students",
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "enrollment_closures",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    enrollment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    previous_enrollment_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    reason = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    closure_date = table.Column<DateOnly>(type: "date", nullable: false),
                    reason_detail = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    operational_block_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    closed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    closed_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    archived_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    archived_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    retain_until = table.Column<DateOnly>(type: "date", nullable: true),
                    retention_legal_basis = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    retention_scope = table.Column<int>(type: "integer", nullable: false),
                    reopened_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    reopened_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reopen_justification = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enrollment_closures", x => x.id);
                    table.ForeignKey(
                        name: "FK_enrollment_closures_enrollments_enrollment_id",
                        column: x => x.enrollment_id,
                        principalSchema: "students",
                        principalTable: "enrollments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_enrollment_closures_students_student_id",
                        column: x => x.student_id,
                        principalSchema: "students",
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "enrollment_suspensions",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    enrollment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reason = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    scope = table.Column<int>(type: "integer", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    expected_end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    immediate_actions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    bookings_decision = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    future_bookings_count = table.Column<int>(type: "integer", nullable: false),
                    credit_decision = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    notification_plan = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    review_date = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    notification_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    operational_block_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    activated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enrollment_suspensions", x => x.id);
                    table.ForeignKey(
                        name: "FK_enrollment_suspensions_enrollments_enrollment_id",
                        column: x => x.enrollment_id,
                        principalSchema: "students",
                        principalTable: "enrollments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_enrollment_suspensions_students_student_id",
                        column: x => x.student_id,
                        principalSchema: "students",
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "student_documents",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    enrollment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    document_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    category = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    visibility = table.Column<int>(type: "integer", nullable: false),
                    expires_on = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    current_version = table.Column<int>(type: "integer", nullable: false),
                    requested_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    requested_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    decision_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_documents", x => x.id);
                    table.ForeignKey(
                        name: "FK_student_documents_enrollments_enrollment_id",
                        column: x => x.enrollment_id,
                        principalSchema: "students",
                        principalTable: "enrollments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_student_documents_students_student_id",
                        column: x => x.student_id,
                        principalSchema: "students",
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "external_transfer_audit_entries",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_transfer_case_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    detail = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    actor_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    occurred_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_external_transfer_audit_entries", x => x.id);
                    table.ForeignKey(
                        name: "FK_external_transfer_audit_entries_external_transfer_cases_ext~",
                        column: x => x.external_transfer_case_id,
                        principalSchema: "students",
                        principalTable: "external_transfer_cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "student_data_grants",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_transfer_case_id = table.Column<Guid>(type: "uuid", nullable: false),
                    grantee_organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_scope = table.Column<long>(type: "bigint", nullable: false),
                    granted_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_on = table.Column<DateOnly>(type: "date", nullable: true),
                    revoked_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_data_grants", x => x.id);
                    table.ForeignKey(
                        name: "FK_student_data_grants_external_transfer_cases_external_transf~",
                        column: x => x.external_transfer_case_id,
                        principalSchema: "students",
                        principalTable: "external_transfer_cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "internal_transfer_impacts",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    internal_transfer_case_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    affected_count = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    message_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    requires_action = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_internal_transfer_impacts", x => x.id);
                    table.ForeignKey(
                        name: "FK_internal_transfer_impacts_internal_transfer_cases_internal_~",
                        column: x => x.internal_transfer_case_id,
                        principalSchema: "students",
                        principalTable: "internal_transfer_cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "primary_branch_change_analyses",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_branch_portfolio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_branch_id = table.Column<Guid>(type: "uuid", nullable: true),
                    target_branch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    analyzed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    analyzed_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    applied_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    applied_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_primary_branch_change_analyses", x => x.id);
                    table.ForeignKey(
                        name: "FK_primary_branch_change_analyses_student_branch_portfolios_st~",
                        column: x => x.student_branch_portfolio_id,
                        principalSchema: "students",
                        principalTable: "student_branch_portfolios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "student_branch_assignments",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_branch_portfolio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    services_allowed = table.Column<int>(type: "integer", nullable: false),
                    effective_from = table.Column<DateOnly>(type: "date", nullable: false),
                    effective_to = table.Column<DateOnly>(type: "date", nullable: true),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ended_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ended_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_branch_assignments", x => x.id);
                    table.ForeignKey(
                        name: "FK_student_branch_assignments_student_branch_portfolios_studen~",
                        column: x => x.student_branch_portfolio_id,
                        principalSchema: "students",
                        principalTable: "student_branch_portfolios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "student_instructor_assignments",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_instructor_portfolio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    instructor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    effective_from = table.Column<DateOnly>(type: "date", nullable: false),
                    effective_to = table.Column<DateOnly>(type: "date", nullable: true),
                    training_category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    maximum_scope = table.Column<int>(type: "integer", nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ended_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ended_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_instructor_assignments", x => x.id);
                    table.ForeignKey(
                        name: "FK_student_instructor_assignments_student_instructor_portfolio~",
                        column: x => x.student_instructor_portfolio_id,
                        principalSchema: "students",
                        principalTable: "student_instructor_portfolios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "student_instructor_history",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_instructor_portfolio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assignment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    actor_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    occurred_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_instructor_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_student_instructor_history_student_instructor_portfolios_st~",
                        column: x => x.student_instructor_portfolio_id,
                        principalSchema: "students",
                        principalTable: "student_instructor_portfolios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "student_block_history",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_status_board_id = table.Column<Guid>(type: "uuid", nullable: false),
                    block_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    detail = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    actor_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    occurred_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_block_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_student_block_history_student_status_boards_student_status_~",
                        column: x => x.student_status_board_id,
                        principalSchema: "students",
                        principalTable: "student_status_boards",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "student_operational_blocks",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_status_board_id = table.Column<Guid>(type: "uuid", nullable: false),
                    block_type = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    source_domain = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    blocking_actions = table.Column<int>(type: "integer", nullable: false),
                    severity = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    applied_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    applied_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expected_resolution = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    resolution_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    resolution_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    resolved_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    resolved_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    override_until_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    override_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    override_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_operational_blocks", x => x.id);
                    table.ForeignKey(
                        name: "FK_student_operational_blocks_student_status_boards_student_st~",
                        column: x => x.student_status_board_id,
                        principalSchema: "students",
                        principalTable: "student_status_boards",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "enrollment_checklist_items",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    checklist_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    label_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    category = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    is_blocking = table.Column<bool>(type: "boolean", nullable: false),
                    target_route = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    responsible_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    due_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    decision_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    reminder_count = table.Column<int>(type: "integer", nullable: false),
                    last_reminder_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enrollment_checklist_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_enrollment_checklist_items_enrollment_checklists_checklist_~",
                        column: x => x.checklist_id,
                        principalSchema: "students",
                        principalTable: "enrollment_checklists",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "enrollment_closure_checks",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    enrollment_closure_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    detail = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enrollment_closure_checks", x => x.id);
                    table.ForeignKey(
                        name: "FK_enrollment_closure_checks_enrollment_closures_enrollment_cl~",
                        column: x => x.enrollment_closure_id,
                        principalSchema: "students",
                        principalTable: "enrollment_closures",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "enrollment_reactivations",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    enrollment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    suspension_id = table.Column<Guid>(type: "uuid", nullable: false),
                    mode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    resume_date = table.Column<DateOnly>(type: "date", nullable: false),
                    conditions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    pedagogy_review_requested = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    applied_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enrollment_reactivations", x => x.id);
                    table.ForeignKey(
                        name: "FK_enrollment_reactivations_enrollment_suspensions_suspension_~",
                        column: x => x.suspension_id,
                        principalSchema: "students",
                        principalTable: "enrollment_suspensions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_enrollment_reactivations_enrollments_enrollment_id",
                        column: x => x.enrollment_id,
                        principalSchema: "students",
                        principalTable: "enrollments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_enrollment_reactivations_students_student_id",
                        column: x => x.student_id,
                        principalSchema: "students",
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "enrollment_suspension_history",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    enrollment_suspension_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    detail = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    actor_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    occurred_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enrollment_suspension_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_enrollment_suspension_history_enrollment_suspensions_enroll~",
                        column: x => x.enrollment_suspension_id,
                        principalSchema: "students",
                        principalTable: "enrollment_suspensions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "student_document_access_logs",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version_id = table.Column<Guid>(type: "uuid", nullable: true),
                    action = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    actor_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    occurred_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_document_access_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_student_document_access_logs_student_documents_document_id",
                        column: x => x.document_id,
                        principalSchema: "students",
                        principalTable: "student_documents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "student_document_versions",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version_number = table.Column<int>(type: "integer", nullable: false),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    content_type = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    checksum = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    storage_reference = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_current = table.Column<bool>(type: "boolean", nullable: false),
                    uploaded_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    uploaded_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reviewed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    reviewed_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    replaced_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_document_versions", x => x.id);
                    table.ForeignKey(
                        name: "FK_student_document_versions_student_documents_document_id",
                        column: x => x.document_id,
                        principalSchema: "students",
                        principalTable: "student_documents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "branch_change_impacts",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    primary_branch_change_analysis_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    affected_count = table.Column<int>(type: "integer", nullable: false),
                    message_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    requires_action = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_branch_change_impacts", x => x.id);
                    table.ForeignKey(
                        name: "FK_branch_change_impacts_primary_branch_change_analyses_primar~",
                        column: x => x.primary_branch_change_analysis_id,
                        principalSchema: "students",
                        principalTable: "primary_branch_change_analyses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "student_instructor_access_grants",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_instructor_portfolio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assignment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    instructor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    scope = table.Column<int>(type: "integer", nullable: false),
                    effective_from = table.Column<DateOnly>(type: "date", nullable: false),
                    effective_to = table.Column<DateOnly>(type: "date", nullable: true),
                    granted_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revoked_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    revoked_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_instructor_access_grants", x => x.id);
                    table.ForeignKey(
                        name: "FK_student_instructor_access_grants_student_instructor_assignm~",
                        column: x => x.assignment_id,
                        principalSchema: "students",
                        principalTable: "student_instructor_assignments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_student_instructor_access_grants_student_instructor_portfol~",
                        column: x => x.student_instructor_portfolio_id,
                        principalSchema: "students",
                        principalTable: "student_instructor_portfolios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "enrollment_reactivation_checks",
                schema: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    enrollment_reactivation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    detail = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enrollment_reactivation_checks", x => x.id);
                    table.ForeignKey(
                        name: "FK_enrollment_reactivation_checks_enrollment_reactivations_enr~",
                        column: x => x.enrollment_reactivation_id,
                        principalSchema: "students",
                        principalTable: "enrollment_reactivations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_administrative_blocks_administrative_case_id",
                schema: "students",
                table: "administrative_blocks",
                column: "administrative_case_id");

            migrationBuilder.CreateIndex(
                name: "IX_administrative_cases_student_id",
                schema: "students",
                table: "administrative_cases",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "ux_administrative_cases_owner_student",
                schema: "students",
                table: "administrative_cases",
                columns: new[] { "organization_id", "student_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_administrative_history_administrative_case_id_occurred_at_u~",
                schema: "students",
                table: "administrative_history",
                columns: new[] { "administrative_case_id", "occurred_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_administrative_requirements_administrative_case_id_code",
                schema: "students",
                table: "administrative_requirements",
                columns: new[] { "administrative_case_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_branch_change_impacts_primary_branch_change_analysis_id",
                schema: "students",
                table: "branch_change_impacts",
                column: "primary_branch_change_analysis_id");

            migrationBuilder.CreateIndex(
                name: "IX_compliance_exceptions_administrative_case_id",
                schema: "students",
                table: "compliance_exceptions",
                column: "administrative_case_id");

            migrationBuilder.CreateIndex(
                name: "IX_enrollment_checklist_items_checklist_id_rule_id",
                schema: "students",
                table: "enrollment_checklist_items",
                columns: new[] { "checklist_id", "rule_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_enrollment_checklist_rules_scope_code",
                schema: "students",
                table: "enrollment_checklist_rules",
                columns: new[] { "organization_id", "training_code", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_enrollment_checklists_enrollment_id",
                schema: "students",
                table: "enrollment_checklists",
                column: "enrollment_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_enrollment_checklists_student_id",
                schema: "students",
                table: "enrollment_checklists",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "ux_enrollment_checklists_owner_enrollment",
                schema: "students",
                table: "enrollment_checklists",
                columns: new[] { "organization_id", "enrollment_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_enrollment_closure_checks_enrollment_closure_id_type",
                schema: "students",
                table: "enrollment_closure_checks",
                columns: new[] { "enrollment_closure_id", "type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_enrollment_closures_enrollment_id_status",
                schema: "students",
                table: "enrollment_closures",
                columns: new[] { "enrollment_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_enrollment_closures_organization_id_student_id_status",
                schema: "students",
                table: "enrollment_closures",
                columns: new[] { "organization_id", "student_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_enrollment_closures_student_id",
                schema: "students",
                table: "enrollment_closures",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_enrollment_reactivation_checks_enrollment_reactivation_id_t~",
                schema: "students",
                table: "enrollment_reactivation_checks",
                columns: new[] { "enrollment_reactivation_id", "type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_enrollment_reactivations_enrollment_id",
                schema: "students",
                table: "enrollment_reactivations",
                column: "enrollment_id");

            migrationBuilder.CreateIndex(
                name: "IX_enrollment_reactivations_organization_id_student_id_status",
                schema: "students",
                table: "enrollment_reactivations",
                columns: new[] { "organization_id", "student_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_enrollment_reactivations_status_resume_date",
                schema: "students",
                table: "enrollment_reactivations",
                columns: new[] { "status", "resume_date" });

            migrationBuilder.CreateIndex(
                name: "IX_enrollment_reactivations_student_id",
                schema: "students",
                table: "enrollment_reactivations",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_enrollment_reactivations_suspension_id",
                schema: "students",
                table: "enrollment_reactivations",
                column: "suspension_id");

            migrationBuilder.CreateIndex(
                name: "IX_enrollment_suspension_history_enrollment_suspension_id_occu~",
                schema: "students",
                table: "enrollment_suspension_history",
                columns: new[] { "enrollment_suspension_id", "occurred_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_enrollment_suspensions_enrollment_id",
                schema: "students",
                table: "enrollment_suspensions",
                column: "enrollment_id");

            migrationBuilder.CreateIndex(
                name: "IX_enrollment_suspensions_organization_id_student_id_status",
                schema: "students",
                table: "enrollment_suspensions",
                columns: new[] { "organization_id", "student_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_enrollment_suspensions_status_start_date",
                schema: "students",
                table: "enrollment_suspensions",
                columns: new[] { "status", "start_date" });

            migrationBuilder.CreateIndex(
                name: "IX_enrollment_suspensions_student_id",
                schema: "students",
                table: "enrollment_suspensions",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "ix_enrollments_organization_branch_status",
                schema: "students",
                table: "enrollments",
                columns: new[] { "organization_id", "branch_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_enrollments_organization_student",
                schema: "students",
                table: "enrollments",
                columns: new[] { "organization_id", "student_id" });

            migrationBuilder.CreateIndex(
                name: "IX_enrollments_student_id",
                schema: "students",
                table: "enrollments",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "ux_enrollments_organization_idempotency_key",
                schema: "students",
                table: "enrollments",
                columns: new[] { "organization_id", "idempotency_key" },
                unique: true,
                filter: "idempotency_key IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_enrollments_organization_source_lead",
                schema: "students",
                table: "enrollments",
                columns: new[] { "organization_id", "source_lead_id" },
                unique: true,
                filter: "source_lead_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_external_transfer_audit_entries_external_transfer_case_id_o~",
                schema: "students",
                table: "external_transfer_audit_entries",
                columns: new[] { "external_transfer_case_id", "occurred_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_external_transfer_cases_source_organization_id_student_id_s~",
                schema: "students",
                table: "external_transfer_cases",
                columns: new[] { "source_organization_id", "student_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_external_transfer_cases_student_id",
                schema: "students",
                table: "external_transfer_cases",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_guardian_relationships_guardian_person_id",
                schema: "students",
                table: "guardian_relationships",
                column: "guardian_person_id");

            migrationBuilder.CreateIndex(
                name: "ix_guardian_relationships_owner_student_guardian",
                schema: "students",
                table: "guardian_relationships",
                columns: new[] { "organization_id", "student_id", "guardian_person_id" });

            migrationBuilder.CreateIndex(
                name: "ix_guardian_relationships_owner_student_status",
                schema: "students",
                table: "guardian_relationships",
                columns: new[] { "organization_id", "student_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_guardian_relationships_student_id",
                schema: "students",
                table: "guardian_relationships",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_internal_transfer_cases_organization_id_student_id_status",
                schema: "students",
                table: "internal_transfer_cases",
                columns: new[] { "organization_id", "student_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_internal_transfer_cases_student_id",
                schema: "students",
                table: "internal_transfer_cases",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_internal_transfer_impacts_internal_transfer_case_id_type",
                schema: "students",
                table: "internal_transfer_impacts",
                columns: new[] { "internal_transfer_case_id", "type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_primary_branch_change_analyses_student_branch_portfolio_id_~",
                schema: "students",
                table: "primary_branch_change_analyses",
                columns: new[] { "student_branch_portfolio_id", "expires_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_student_block_history_student_status_board_id_occurred_at_u~",
                schema: "students",
                table: "student_block_history",
                columns: new[] { "student_status_board_id", "occurred_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_student_branch_assignments_branch_id_status",
                schema: "students",
                table: "student_branch_assignments",
                columns: new[] { "branch_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_student_branch_assignments_student_branch_portfolio_id_type~",
                schema: "students",
                table: "student_branch_assignments",
                columns: new[] { "student_branch_portfolio_id", "type", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_student_branch_portfolios_organization_id_student_id",
                schema: "students",
                table: "student_branch_portfolios",
                columns: new[] { "organization_id", "student_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_student_branch_portfolios_student_id",
                schema: "students",
                table: "student_branch_portfolios",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_data_grants_external_transfer_case_id",
                schema: "students",
                table: "student_data_grants",
                column: "external_transfer_case_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_data_grants_grantee_organization_id_expires_on",
                schema: "students",
                table: "student_data_grants",
                columns: new[] { "grantee_organization_id", "expires_on" });

            migrationBuilder.CreateIndex(
                name: "IX_student_document_access_logs_document_id_occurred_at_utc",
                schema: "students",
                table: "student_document_access_logs",
                columns: new[] { "document_id", "occurred_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_student_document_versions_document_id_is_current",
                schema: "students",
                table: "student_document_versions",
                columns: new[] { "document_id", "is_current" },
                unique: true,
                filter: "is_current = TRUE");

            migrationBuilder.CreateIndex(
                name: "IX_student_document_versions_document_id_version_number",
                schema: "students",
                table: "student_document_versions",
                columns: new[] { "document_id", "version_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_student_documents_enrollment_id",
                schema: "students",
                table: "student_documents",
                column: "enrollment_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_documents_expires_on",
                schema: "students",
                table: "student_documents",
                column: "expires_on");

            migrationBuilder.CreateIndex(
                name: "ix_student_documents_owner_student_status",
                schema: "students",
                table: "student_documents",
                columns: new[] { "organization_id", "student_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_student_documents_student_id",
                schema: "students",
                table: "student_documents",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "ix_student_identity_audit_owner_date",
                schema: "students",
                table: "student_identity_audit",
                columns: new[] { "organization_id", "student_id", "occurred_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_student_identity_audit_student_id",
                schema: "students",
                table: "student_identity_audit",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_instructor_access_grants_assignment_id",
                schema: "students",
                table: "student_instructor_access_grants",
                column: "assignment_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_student_instructor_access_grants_instructor_id_effective_fr~",
                schema: "students",
                table: "student_instructor_access_grants",
                columns: new[] { "instructor_id", "effective_from", "effective_to" });

            migrationBuilder.CreateIndex(
                name: "IX_student_instructor_access_grants_student_instructor_portfol~",
                schema: "students",
                table: "student_instructor_access_grants",
                column: "student_instructor_portfolio_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_instructor_assignments_instructor_id_status",
                schema: "students",
                table: "student_instructor_assignments",
                columns: new[] { "instructor_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_student_instructor_assignments_student_instructor_portfolio~",
                schema: "students",
                table: "student_instructor_assignments",
                columns: new[] { "student_instructor_portfolio_id", "type", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_student_instructor_history_student_instructor_portfolio_id_~",
                schema: "students",
                table: "student_instructor_history",
                columns: new[] { "student_instructor_portfolio_id", "occurred_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_student_instructor_portfolios_organization_id_student_id",
                schema: "students",
                table: "student_instructor_portfolios",
                columns: new[] { "organization_id", "student_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_student_instructor_portfolios_student_id",
                schema: "students",
                table: "student_instructor_portfolios",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_operational_blocks_student_status_board_id_status",
                schema: "students",
                table: "student_operational_blocks",
                columns: new[] { "student_status_board_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_student_relationships_owner_party_type",
                schema: "students",
                table: "student_relationships",
                columns: new[] { "organization_id", "student_id", "party_id", "relationship_type" });

            migrationBuilder.CreateIndex(
                name: "ix_student_relationships_owner_student_status",
                schema: "students",
                table: "student_relationships",
                columns: new[] { "organization_id", "student_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_student_relationships_student_id",
                schema: "students",
                table: "student_relationships",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "ux_student_relationships_primary_payer",
                schema: "students",
                table: "student_relationships",
                columns: new[] { "organization_id", "student_id", "is_primary_payer" },
                unique: true,
                filter: "is_primary_payer = TRUE AND status = 'Active'");

            migrationBuilder.CreateIndex(
                name: "IX_student_status_boards_organization_id_student_id",
                schema: "students",
                table: "student_status_boards",
                columns: new[] { "organization_id", "student_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_student_status_boards_student_id",
                schema: "students",
                table: "student_status_boards",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "ix_students_organization_status",
                schema: "students",
                table: "students",
                columns: new[] { "organization_id", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "administrative_blocks",
                schema: "students");

            migrationBuilder.DropTable(
                name: "administrative_history",
                schema: "students");

            migrationBuilder.DropTable(
                name: "administrative_requirements",
                schema: "students");

            migrationBuilder.DropTable(
                name: "branch_change_impacts",
                schema: "students");

            migrationBuilder.DropTable(
                name: "compliance_exceptions",
                schema: "students");

            migrationBuilder.DropTable(
                name: "enrollment_checklist_items",
                schema: "students");

            migrationBuilder.DropTable(
                name: "enrollment_checklist_rules",
                schema: "students");

            migrationBuilder.DropTable(
                name: "enrollment_closure_checks",
                schema: "students");

            migrationBuilder.DropTable(
                name: "enrollment_reactivation_checks",
                schema: "students");

            migrationBuilder.DropTable(
                name: "enrollment_suspension_history",
                schema: "students");

            migrationBuilder.DropTable(
                name: "external_transfer_audit_entries",
                schema: "students");

            migrationBuilder.DropTable(
                name: "guardian_relationships",
                schema: "students");

            migrationBuilder.DropTable(
                name: "internal_transfer_impacts",
                schema: "students");

            migrationBuilder.DropTable(
                name: "student_block_history",
                schema: "students");

            migrationBuilder.DropTable(
                name: "student_branch_assignments",
                schema: "students");

            migrationBuilder.DropTable(
                name: "student_data_grants",
                schema: "students");

            migrationBuilder.DropTable(
                name: "student_document_access_logs",
                schema: "students");

            migrationBuilder.DropTable(
                name: "student_document_versions",
                schema: "students");

            migrationBuilder.DropTable(
                name: "student_identity_audit",
                schema: "students");

            migrationBuilder.DropTable(
                name: "student_instructor_access_grants",
                schema: "students");

            migrationBuilder.DropTable(
                name: "student_instructor_history",
                schema: "students");

            migrationBuilder.DropTable(
                name: "student_operational_blocks",
                schema: "students");

            migrationBuilder.DropTable(
                name: "student_relationships",
                schema: "students");

            migrationBuilder.DropTable(
                name: "primary_branch_change_analyses",
                schema: "students");

            migrationBuilder.DropTable(
                name: "administrative_cases",
                schema: "students");

            migrationBuilder.DropTable(
                name: "enrollment_checklists",
                schema: "students");

            migrationBuilder.DropTable(
                name: "enrollment_closures",
                schema: "students");

            migrationBuilder.DropTable(
                name: "enrollment_reactivations",
                schema: "students");

            migrationBuilder.DropTable(
                name: "internal_transfer_cases",
                schema: "students");

            migrationBuilder.DropTable(
                name: "external_transfer_cases",
                schema: "students");

            migrationBuilder.DropTable(
                name: "student_documents",
                schema: "students");

            migrationBuilder.DropTable(
                name: "student_instructor_assignments",
                schema: "students");

            migrationBuilder.DropTable(
                name: "student_status_boards",
                schema: "students");

            migrationBuilder.DropTable(
                name: "student_branch_portfolios",
                schema: "students");

            migrationBuilder.DropTable(
                name: "enrollment_suspensions",
                schema: "students");

            migrationBuilder.DropTable(
                name: "student_instructor_portfolios",
                schema: "students");

            migrationBuilder.DropTable(
                name: "enrollments",
                schema: "students");

            migrationBuilder.DropTable(
                name: "students",
                schema: "students");
        }
    }
}
