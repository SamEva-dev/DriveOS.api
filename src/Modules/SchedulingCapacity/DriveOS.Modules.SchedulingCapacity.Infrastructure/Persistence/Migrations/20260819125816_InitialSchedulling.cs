using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchedulling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "scheduling_capacity");

            migrationBuilder.CreateTable(
                name: "availability_plans",
                schema: "scheduling_capacity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CalendarResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_availability_plans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "bookings",
                schema: "scheduling_capacity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    BookingType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    StartAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreationIdempotencyKey = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    CreationRequestFingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TrainingPathId = table.Column<Guid>(type: "uuid", nullable: true),
                    Objectives = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    MeetingPoint = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PricingReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TrainingCreditAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreditQuantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CreditReservationStatus = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    CreditReservationReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    NotificationPolicy = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    HoldExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CancellationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bookings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "calendar_resources",
                schema: "scheduling_capacity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResourceType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ExternalResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    TimeZoneId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RestrictionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UnavailabilityReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calendar_resources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "recurrence_series",
                schema: "scheduling_capacity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Frequency = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Interval = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    OccurrenceCount = table.Column<int>(type: "integer", nullable: true),
                    LocalTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    TimeZoneId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ResourceSelectionPolicy = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DaysOfWeek = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Revision = table.Column<int>(type: "integer", nullable: false),
                    IsCancelled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recurrence_series", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "scheduling_conflicts",
                schema: "scheduling_capacity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    CalendarResourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConflictingBookingId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Priority = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CauseKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Details = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SuggestedActions = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DetectedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ResolvedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Resolution = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ResolutionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ResolvedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    OverrideReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    OverrideRisk = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    OverrideApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    OverrideExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scheduling_conflicts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "waiting_list_entries",
                schema: "scheduling_capacity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedSessionType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PreferredFromUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PreferredToUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    PreferredBranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    PreferredInstructorId = table.Column<Guid>(type: "uuid", nullable: true),
                    PriorityScore = table.Column<int>(type: "integer", nullable: false),
                    PriorityExplanation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_waiting_list_entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "availability_exceptions",
                schema: "scheduling_capacity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AvailabilityPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Type = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_availability_exceptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_availability_exceptions_availability_plans_AvailabilityPlan~",
                        column: x => x.AvailabilityPlanId,
                        principalSchema: "scheduling_capacity",
                        principalTable: "availability_plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "availability_rules",
                schema: "scheduling_capacity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AvailabilityPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_availability_rules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_availability_rules_availability_plans_AvailabilityPlanId",
                        column: x => x.AvailabilityPlanId,
                        principalSchema: "scheduling_capacity",
                        principalTable: "availability_plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "booking_attendance_history",
                schema: "scheduling_capacity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupersedesAttendanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    RecordedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RecordedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    ArrivalTimeUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DepartureTimeUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DelayMinutes = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EvidenceDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ChargeDecision = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreditDecision = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    FollowUpAction = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    OverrideApplied = table.Column<bool>(type: "boolean", nullable: false),
                    OverrideReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_booking_attendance_history", x => x.Id);
                    table.ForeignKey(
                        name: "FK_booking_attendance_history_bookings_BookingId",
                        column: x => x.BookingId,
                        principalSchema: "scheduling_capacity",
                        principalTable: "bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "booking_cancellations",
                schema: "scheduling_capacity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Initiator = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    InitiatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReasonCode = table.Column<string>(type: "character varying(48)", maxLength: 48, nullable: false),
                    ReasonDetails = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CancelledAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    NoticeDurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    PolicyCode = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    PolicyVersion = table.Column<int>(type: "integer", nullable: false),
                    PolicyExplanationKey = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    CreditDecision = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    FeeDecision = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    NotificationDecision = table.Column<string>(type: "character varying(48)", maxLength: 48, nullable: false),
                    ReplacementRequired = table.Column<bool>(type: "boolean", nullable: false),
                    OverrideApplied = table.Column<bool>(type: "boolean", nullable: false),
                    OverrideReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_booking_cancellations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_booking_cancellations_bookings_BookingId",
                        column: x => x.BookingId,
                        principalSchema: "scheduling_capacity",
                        principalTable: "bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "booking_instructor_replacements",
                schema: "scheduling_capacity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousInstructorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReplacementInstructorId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReplacementResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Mode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AccessExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_booking_instructor_replacements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_booking_instructor_replacements_bookings_BookingId",
                        column: x => x.BookingId,
                        principalSchema: "scheduling_capacity",
                        principalTable: "bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "booking_participants",
                schema: "scheduling_capacity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParticipantType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ExternalParticipantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_booking_participants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_booking_participants_bookings_BookingId",
                        column: x => x.BookingId,
                        principalSchema: "scheduling_capacity",
                        principalTable: "bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "booking_reschedule_history",
                schema: "scheduling_capacity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousStartAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PreviousEndAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    NewStartAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    NewEndAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PreviousBranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    NewBranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    PreviousStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ResourcesChanged = table.Column<bool>(type: "boolean", nullable: false),
                    PreviousResourceFingerprint = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    NewResourceFingerprint = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_booking_reschedule_history", x => x.Id);
                    table.ForeignKey(
                        name: "FK_booking_reschedule_history_bookings_BookingId",
                        column: x => x.BookingId,
                        principalSchema: "scheduling_capacity",
                        principalTable: "bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "booking_resources",
                schema: "scheduling_capacity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    CalendarResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_booking_resources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_booking_resources_bookings_BookingId",
                        column: x => x.BookingId,
                        principalSchema: "scheduling_capacity",
                        principalTable: "bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "booking_vehicle_replacements",
                schema: "scheduling_capacity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousVehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReplacementVehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReplacementResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Mode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_booking_vehicle_replacements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_booking_vehicle_replacements_bookings_BookingId",
                        column: x => x.BookingId,
                        principalSchema: "scheduling_capacity",
                        principalTable: "bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "recurrence_occurrences",
                schema: "scheduling_capacity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SeriesId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduledDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Revision = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    ExceptionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recurrence_occurrences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_recurrence_occurrences_recurrence_series_SeriesId",
                        column: x => x.SeriesId,
                        principalSchema: "scheduling_capacity",
                        principalTable: "recurrence_series",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "recurrence_resources",
                schema: "scheduling_capacity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SeriesId = table.Column<Guid>(type: "uuid", nullable: false),
                    CalendarResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recurrence_resources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_recurrence_resources_recurrence_series_SeriesId",
                        column: x => x.SeriesId,
                        principalSchema: "scheduling_capacity",
                        principalTable: "recurrence_series",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "waiting_list_proposals",
                schema: "scheduling_capacity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WaitingListEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    InstructorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProposedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SlotKey = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    ActiveHoldKey = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: true),
                    HeldUntilUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DecidedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DecisionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FulfilledBookingId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_waiting_list_proposals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_waiting_list_proposals_waiting_list_entries_WaitingListEntr~",
                        column: x => x.WaitingListEntryId,
                        principalSchema: "scheduling_capacity",
                        principalTable: "waiting_list_entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_availability_exceptions_AvailabilityPlanId_Date_StartTime_E~",
                schema: "scheduling_capacity",
                table: "availability_exceptions",
                columns: new[] { "AvailabilityPlanId", "Date", "StartTime", "EndTime" });

            migrationBuilder.CreateIndex(
                name: "IX_availability_plans_OrganizationId_CalendarResourceId_Status",
                schema: "scheduling_capacity",
                table: "availability_plans",
                columns: new[] { "OrganizationId", "CalendarResourceId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_availability_rules_AvailabilityPlanId_DayOfWeek_StartTime_E~",
                schema: "scheduling_capacity",
                table: "availability_rules",
                columns: new[] { "AvailabilityPlanId", "DayOfWeek", "StartTime", "EndTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_booking_attendance_history_BookingId_OperationId",
                schema: "scheduling_capacity",
                table: "booking_attendance_history",
                columns: new[] { "BookingId", "OperationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_booking_attendance_history_BookingId_RecordedAtUtc",
                schema: "scheduling_capacity",
                table: "booking_attendance_history",
                columns: new[] { "BookingId", "RecordedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_booking_cancellations_BookingId",
                schema: "scheduling_capacity",
                table: "booking_cancellations",
                column: "BookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_booking_cancellations_BookingId_OperationId",
                schema: "scheduling_capacity",
                table: "booking_cancellations",
                columns: new[] { "BookingId", "OperationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_booking_instructor_replacements_BookingId_OperationId",
                schema: "scheduling_capacity",
                table: "booking_instructor_replacements",
                columns: new[] { "BookingId", "OperationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_booking_instructor_replacements_PreviousInstructorId_Occurr~",
                schema: "scheduling_capacity",
                table: "booking_instructor_replacements",
                columns: new[] { "PreviousInstructorId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_booking_instructor_replacements_ReplacementInstructorId_Occ~",
                schema: "scheduling_capacity",
                table: "booking_instructor_replacements",
                columns: new[] { "ReplacementInstructorId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_booking_participants_BookingId_ParticipantType_ExternalPart~",
                schema: "scheduling_capacity",
                table: "booking_participants",
                columns: new[] { "BookingId", "ParticipantType", "ExternalParticipantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_booking_reschedule_history_BookingId_OccurredAtUtc",
                schema: "scheduling_capacity",
                table: "booking_reschedule_history",
                columns: new[] { "BookingId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_booking_reschedule_history_BookingId_OperationId",
                schema: "scheduling_capacity",
                table: "booking_reschedule_history",
                columns: new[] { "BookingId", "OperationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_booking_resources_BookingId_CalendarResourceId",
                schema: "scheduling_capacity",
                table: "booking_resources",
                columns: new[] { "BookingId", "CalendarResourceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_booking_resources_CalendarResourceId",
                schema: "scheduling_capacity",
                table: "booking_resources",
                column: "CalendarResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_booking_vehicle_replacements_BookingId_OperationId",
                schema: "scheduling_capacity",
                table: "booking_vehicle_replacements",
                columns: new[] { "BookingId", "OperationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_booking_vehicle_replacements_PreviousVehicleId_OccurredAtUtc",
                schema: "scheduling_capacity",
                table: "booking_vehicle_replacements",
                columns: new[] { "PreviousVehicleId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_booking_vehicle_replacements_ReplacementVehicleId_OccurredA~",
                schema: "scheduling_capacity",
                table: "booking_vehicle_replacements",
                columns: new[] { "ReplacementVehicleId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_bookings_OrganizationId_BranchId_Status_StartAtUtc",
                schema: "scheduling_capacity",
                table: "bookings",
                columns: new[] { "OrganizationId", "BranchId", "Status", "StartAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_bookings_OrganizationId_CreationIdempotencyKey",
                schema: "scheduling_capacity",
                table: "bookings",
                columns: new[] { "OrganizationId", "CreationIdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_bookings_OrganizationId_StartAtUtc_EndAtUtc",
                schema: "scheduling_capacity",
                table: "bookings",
                columns: new[] { "OrganizationId", "StartAtUtc", "EndAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_calendar_resources_OrganizationId_BranchId_Status",
                schema: "scheduling_capacity",
                table: "calendar_resources",
                columns: new[] { "OrganizationId", "BranchId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_calendar_resources_OrganizationId_ResourceType_ExternalReso~",
                schema: "scheduling_capacity",
                table: "calendar_resources",
                columns: new[] { "OrganizationId", "ResourceType", "ExternalResourceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recurrence_occurrences_SeriesId_ScheduledDate_Revision",
                schema: "scheduling_capacity",
                table: "recurrence_occurrences",
                columns: new[] { "SeriesId", "ScheduledDate", "Revision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recurrence_resources_SeriesId_CalendarResourceId",
                schema: "scheduling_capacity",
                table: "recurrence_resources",
                columns: new[] { "SeriesId", "CalendarResourceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recurrence_series_OrganizationId_StartDate_EndDate",
                schema: "scheduling_capacity",
                table: "recurrence_series",
                columns: new[] { "OrganizationId", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_scheduling_conflicts_OrganizationId_BookingId_Status",
                schema: "scheduling_capacity",
                table: "scheduling_conflicts",
                columns: new[] { "OrganizationId", "BookingId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_scheduling_conflicts_OrganizationId_Status_Priority_Detecte~",
                schema: "scheduling_capacity",
                table: "scheduling_conflicts",
                columns: new[] { "OrganizationId", "Status", "Priority", "DetectedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_waiting_list_entries_OrganizationId_PreferredFromUtc_Prefer~",
                schema: "scheduling_capacity",
                table: "waiting_list_entries",
                columns: new[] { "OrganizationId", "PreferredFromUtc", "PreferredToUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_waiting_list_entries_OrganizationId_Status_PriorityScore_Cr~",
                schema: "scheduling_capacity",
                table: "waiting_list_entries",
                columns: new[] { "OrganizationId", "Status", "PriorityScore", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_waiting_list_entries_OrganizationId_StudentId_Status",
                schema: "scheduling_capacity",
                table: "waiting_list_entries",
                columns: new[] { "OrganizationId", "StudentId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_waiting_list_proposals_ActiveHoldKey",
                schema: "scheduling_capacity",
                table: "waiting_list_proposals",
                column: "ActiveHoldKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_waiting_list_proposals_WaitingListEntryId_Status_ExpiresAtU~",
                schema: "scheduling_capacity",
                table: "waiting_list_proposals",
                columns: new[] { "WaitingListEntryId", "Status", "ExpiresAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "availability_exceptions",
                schema: "scheduling_capacity");

            migrationBuilder.DropTable(
                name: "availability_rules",
                schema: "scheduling_capacity");

            migrationBuilder.DropTable(
                name: "booking_attendance_history",
                schema: "scheduling_capacity");

            migrationBuilder.DropTable(
                name: "booking_cancellations",
                schema: "scheduling_capacity");

            migrationBuilder.DropTable(
                name: "booking_instructor_replacements",
                schema: "scheduling_capacity");

            migrationBuilder.DropTable(
                name: "booking_participants",
                schema: "scheduling_capacity");

            migrationBuilder.DropTable(
                name: "booking_reschedule_history",
                schema: "scheduling_capacity");

            migrationBuilder.DropTable(
                name: "booking_resources",
                schema: "scheduling_capacity");

            migrationBuilder.DropTable(
                name: "booking_vehicle_replacements",
                schema: "scheduling_capacity");

            migrationBuilder.DropTable(
                name: "calendar_resources",
                schema: "scheduling_capacity");

            migrationBuilder.DropTable(
                name: "recurrence_occurrences",
                schema: "scheduling_capacity");

            migrationBuilder.DropTable(
                name: "recurrence_resources",
                schema: "scheduling_capacity");

            migrationBuilder.DropTable(
                name: "scheduling_conflicts",
                schema: "scheduling_capacity");

            migrationBuilder.DropTable(
                name: "waiting_list_proposals",
                schema: "scheduling_capacity");

            migrationBuilder.DropTable(
                name: "availability_plans",
                schema: "scheduling_capacity");

            migrationBuilder.DropTable(
                name: "bookings",
                schema: "scheduling_capacity");

            migrationBuilder.DropTable(
                name: "recurrence_series",
                schema: "scheduling_capacity");

            migrationBuilder.DropTable(
                name: "waiting_list_entries",
                schema: "scheduling_capacity");
        }
    }
}
