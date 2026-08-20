using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.Modules.SchedulingCapacity.Domain.CalendarResources;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.SchedulingCapacity.Bookings;

public sealed class BookingTests
{
    [Fact]
    public void Reserve_ShouldSucceed_WhenConflictAssessmentIsClear()
    {
        Booking booking = CreateBooking();
        CalendarResourceId resourceId = CalendarResourceId.New();
        booking.AddResource(BookingResourceId.New(), resourceId).IsSuccess.Should().BeTrue();

        var assessment = BookingConflictDetector.Assess(
            booking,
            [new CalendarResourceSchedulingSnapshot(resourceId, 1, 1, CalendarResourceStatus.Active, null, null, CalendarResourceType.Other, null)],
            []);

        booking.Reserve(assessment).IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(BookingStatus.Reserved);
    }

    [Fact]
    public void ConflictDetector_ShouldRejectDoubleBooking_ForUnitCapacityResource()
    {
        Booking booking = CreateBooking();
        CalendarResourceId resourceId = CalendarResourceId.New();
        booking.AddResource(BookingResourceId.New(), resourceId).IsSuccess.Should().BeTrue();

        var existing = new ExistingBookingResourceReservation(
            BookingId.New(),
            resourceId,
            booking.StartAtUtc.AddMinutes(-15),
            booking.EndAtUtc.AddMinutes(-15),
            1,
            BookingStatus.Confirmed,
            booking.BranchId);

        BookingConflictAssessment assessment = BookingConflictDetector.Assess(
            booking,
            [new CalendarResourceSchedulingSnapshot(resourceId, 1, 1, CalendarResourceStatus.Active, null, null, CalendarResourceType.Other, null)],
            [existing]);

        assessment.IsConflictFree.Should().BeFalse();
        assessment.Conflicts.Should().ContainSingle(x => x.Type == BookingConflictType.OverlappingBooking);
        booking.Reserve(assessment).IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ConflictDetector_ShouldUseCapacity_ForSharedResource()
    {
        Booking booking = CreateBooking();
        CalendarResourceId roomId = CalendarResourceId.New();
        booking.AddResource(BookingResourceId.New(), roomId, 3).IsSuccess.Should().BeTrue();

        var existing = new ExistingBookingResourceReservation(
            BookingId.New(), roomId, booking.StartAtUtc, booking.EndAtUtc, 8, BookingStatus.Reserved, booking.BranchId);

        BookingConflictAssessment assessment = BookingConflictDetector.Assess(
            booking,
            [new CalendarResourceSchedulingSnapshot(roomId, 10, 10, CalendarResourceStatus.Active, null, null, CalendarResourceType.Room, null)],
            [existing]);

        assessment.IsConflictFree.Should().BeFalse();
        assessment.Conflicts.Should().ContainSingle(x =>
            x.Type == BookingConflictType.CapacityExceeded && x.AvailableCapacity == 2);
    }

    [Fact]
    public void ConflictDetector_ShouldAggregateMultipleOverlappingReservations_ForCollectiveResource()
    {
        Booking booking = CreateBooking();
        CalendarResourceId branchCapacityId = CalendarResourceId.New();
        booking.AddResource(BookingResourceId.New(), branchCapacityId, 4).IsSuccess.Should().BeTrue();

        ExistingBookingResourceReservation[] existing =
        [
            new(BookingId.New(), branchCapacityId, booking.StartAtUtc, booking.EndAtUtc, 3, BookingStatus.Confirmed, booking.BranchId),
            new(BookingId.New(), branchCapacityId, booking.StartAtUtc.AddMinutes(10), booking.EndAtUtc.AddMinutes(-10), 4, BookingStatus.Reserved, booking.BranchId)
        ];

        BookingConflictAssessment assessment = BookingConflictDetector.Assess(
            booking,
            [new CalendarResourceSchedulingSnapshot(branchCapacityId, 10, 10, CalendarResourceStatus.Active, null, null, CalendarResourceType.Branch, booking.BranchId)],
            existing);

        assessment.IsConflictFree.Should().BeFalse();
        assessment.Conflicts.Should().ContainSingle(x =>
            x.Type == BookingConflictType.CapacityExceeded &&
            x.RequestedQuantity == 4 &&
            x.AvailableCapacity == 3);
    }

    [Fact]
    public void ConflictDetector_ShouldAllowReservation_AtExactRemainingCapacity()
    {
        Booking booking = CreateBooking();
        CalendarResourceId roomId = CalendarResourceId.New();
        booking.AddResource(BookingResourceId.New(), roomId, 4).IsSuccess.Should().BeTrue();

        var existing = new ExistingBookingResourceReservation(
            BookingId.New(), roomId, booking.StartAtUtc, booking.EndAtUtc, 6, BookingStatus.Confirmed, booking.BranchId);

        BookingConflictAssessment assessment = BookingConflictDetector.Assess(
            booking,
            [new CalendarResourceSchedulingSnapshot(roomId, 10, 10, CalendarResourceStatus.Active, null, null, CalendarResourceType.Room, booking.BranchId)],
            [existing]);

        assessment.IsConflictFree.Should().BeTrue();
    }

    [Fact]
    public void RestrictedResource_ShouldProduceConflict()
    {
        Booking booking = CreateBooking();
        CalendarResourceId resourceId = CalendarResourceId.New();
        booking.AddResource(BookingResourceId.New(), resourceId).IsSuccess.Should().BeTrue();

        BookingConflictAssessment assessment = BookingConflictDetector.Assess(
            booking,
            [new CalendarResourceSchedulingSnapshot(resourceId, 1, 1, CalendarResourceStatus.Restricted, "Validation manuelle", null, CalendarResourceType.Other, null)],
            []);

        assessment.Conflicts.Should().ContainSingle(x => x.Type == BookingConflictType.ResourceRestricted);
    }

    [Fact]
    public void OutsideAvailability_ShouldProduceConflict()
    {
        Booking booking = CreateBooking();
        CalendarResourceId resourceId = CalendarResourceId.New();
        booking.AddResource(BookingResourceId.New(), resourceId).IsSuccess.Should().BeTrue();

        BookingConflictAssessment assessment = BookingConflictDetector.Assess(
            booking,
            [new CalendarResourceSchedulingSnapshot(resourceId, 1, 0, CalendarResourceStatus.Active, null, null, CalendarResourceType.Other, null)],
            []);

        assessment.Conflicts.Should().ContainSingle(x => x.Type == BookingConflictType.OutsideAvailability);
    }

    [Fact]
    public void AdjacentBookings_ShouldNotOverlap()
    {
        DateTimeOffset start = new(2026, 8, 19, 8, 0, 0, TimeSpan.Zero);
        DateTimeOffset end = start.AddHours(1);

        BookingConflictDetector.Overlaps(start, end, end, end.AddHours(1)).Should().BeFalse();
    }

    [Fact]
    public void Confirm_ShouldRequireReservedStatus()
    {
        Booking booking = CreateBooking();
        booking.Confirm().IsFailure.Should().BeTrue();
    }


    [Fact]
    public void Reschedule_ShouldReturnBookingToDraft_AndRaiseTraceableEvent()
    {
        Booking booking = CreateBooking();
        CalendarResourceId resourceId = CalendarResourceId.New();
        booking.AddResource(BookingResourceId.New(), resourceId).IsSuccess.Should().BeTrue();

        var assessment = BookingConflictDetector.Assess(
            booking,
            [new CalendarResourceSchedulingSnapshot(resourceId, 1, 1, CalendarResourceStatus.Active, null, null, CalendarResourceType.Other, null)],
            []);
        booking.Reserve(assessment).IsSuccess.Should().BeTrue();

        DateTimeOffset newStart = booking.StartAtUtc.AddHours(2);
        DateTimeOffset newEnd = booking.EndAtUtc.AddHours(2);
        booking.Reschedule(newStart, newEnd).IsSuccess.Should().BeTrue();

        booking.Status.Should().Be(BookingStatus.Draft);
        booking.StartAtUtc.Should().Be(newStart);
        booking.EndAtUtc.Should().Be(newEnd);
        booking.DomainEvents.Should().Contain(x => x is DriveOS.Modules.SchedulingCapacity.Domain.Bookings.Events.BookingRescheduledDomainEvent);
    }

    [Fact]
    public void Instructor_ShouldRequireTravelTime_WhenBranchChanges()
    {
        Booking booking = CreateBooking();
        CalendarResourceId instructorId = CalendarResourceId.New();
        booking.AddResource(BookingResourceId.New(), instructorId).IsSuccess.Should().BeTrue();

        var previous = new ExistingBookingResourceReservation(
            BookingId.New(),
            instructorId,
            booking.StartAtUtc.AddHours(-1).AddMinutes(-20),
            booking.StartAtUtc.AddMinutes(-20),
            1,
            BookingStatus.Confirmed,
            BranchId.New());

        BookingConflictAssessment assessment = BookingConflictDetector.Assess(
            booking,
            [new CalendarResourceSchedulingSnapshot(instructorId, 1, 1, CalendarResourceStatus.Active, null, null, CalendarResourceType.Instructor, booking.BranchId)],
            [previous],
            new BookingTransitionPolicy(15, 45, 10, 30));

        assessment.Conflicts.Should().ContainSingle(x => x.Type == BookingConflictType.TravelTimeViolation);
    }

    [Fact]
    public void Instructor_ShouldAllowAdjacentBooking_WhenConfiguredBufferIsZero()
    {
        Booking booking = CreateBooking();
        CalendarResourceId instructorId = CalendarResourceId.New();
        booking.AddResource(BookingResourceId.New(), instructorId).IsSuccess.Should().BeTrue();

        var previous = new ExistingBookingResourceReservation(
            BookingId.New(),
            instructorId,
            booking.StartAtUtc.AddHours(-1),
            booking.StartAtUtc,
            1,
            BookingStatus.Confirmed,
            booking.BranchId);

        BookingConflictAssessment assessment = BookingConflictDetector.Assess(
            booking,
            [new CalendarResourceSchedulingSnapshot(instructorId, 1, 1, CalendarResourceStatus.Active, null, null, CalendarResourceType.Instructor, booking.BranchId)],
            [previous],
            new BookingTransitionPolicy(0, 45, 10, 30));

        assessment.IsConflictFree.Should().BeTrue();
    }

    [Fact]
    public void Vehicle_ShouldRequireSameBranchBuffer()
    {
        Booking booking = CreateBooking();
        CalendarResourceId vehicleId = CalendarResourceId.New();
        booking.AddResource(BookingResourceId.New(), vehicleId).IsSuccess.Should().BeTrue();

        var previous = new ExistingBookingResourceReservation(
            BookingId.New(),
            vehicleId,
            booking.StartAtUtc.AddHours(-1).AddMinutes(-5),
            booking.StartAtUtc.AddMinutes(-5),
            1,
            BookingStatus.Reserved,
            booking.BranchId);

        BookingConflictAssessment assessment = BookingConflictDetector.Assess(
            booking,
            [new CalendarResourceSchedulingSnapshot(vehicleId, 1, 1, CalendarResourceStatus.Active, null, null, CalendarResourceType.Vehicle, booking.BranchId)],
            [previous],
            new BookingTransitionPolicy(15, 45, 10, 30));

        assessment.Conflicts.Should().ContainSingle(x => x.Type == BookingConflictType.TransitionBufferViolation);
    }


    [Fact]
    public void ConfirmedBooking_ShouldBeReschedulable_AndKeepPersistentHistory()
    {
        Booking booking = CreateBooking();
        CalendarResourceId resourceId = CalendarResourceId.New();
        booking.AddResource(BookingResourceId.New(), resourceId).IsSuccess.Should().BeTrue();
        var assessment = BookingConflictDetector.Assess(
            booking,
            [new CalendarResourceSchedulingSnapshot(resourceId, 1, 1, CalendarResourceStatus.Active, null, null, CalendarResourceType.Vehicle, booking.BranchId)],
            []);
        booking.Reserve(assessment).IsSuccess.Should().BeTrue();
        booking.Confirm().IsSuccess.Should().BeTrue();

        Guid operationId = Guid.NewGuid();
        DateTimeOffset oldStart = booking.StartAtUtc;
        DateTimeOffset oldEnd = booking.EndAtUtc;
        DateTimeOffset newStart = oldStart.AddDays(1);
        DateTimeOffset newEnd = oldEnd.AddDays(1);
        string fingerprint = Booking.ResourceFingerprint(booking.Resources);

        booking.Reschedule(operationId, newStart, newEnd, booking.BranchId, "student requested another day", false, fingerprint, newStart.AddHours(-2)).IsSuccess.Should().BeTrue();

        booking.Status.Should().Be(BookingStatus.Draft);
        booking.RescheduleHistory.Should().ContainSingle();
        BookingRescheduleHistory history = booking.RescheduleHistory.Single();
        history.OperationId.Should().Be(operationId);
        history.PreviousStartAtUtc.Should().Be(oldStart);
        history.PreviousEndAtUtc.Should().Be(oldEnd);
        history.NewStartAtUtc.Should().Be(newStart);
        history.PreviousStatus.Should().Be(BookingStatus.Confirmed);
        history.Reason.Should().Be("student requested another day");
    }

    [Fact]
    public void Reschedule_ShouldBeIdempotent_ForSameOperationId()
    {
        Booking booking = CreateBooking();
        CalendarResourceId resourceId = CalendarResourceId.New();
        booking.AddResource(BookingResourceId.New(), resourceId).IsSuccess.Should().BeTrue();
        Guid operationId = Guid.NewGuid();
        DateTimeOffset newStart = booking.StartAtUtc.AddHours(2);
        DateTimeOffset newEnd = booking.EndAtUtc.AddHours(2);
        string fingerprint = Booking.ResourceFingerprint(booking.Resources);

        booking.Reschedule(operationId, newStart, newEnd, booking.BranchId, "administrative reschedule", false, fingerprint, DateTimeOffset.UtcNow).IsSuccess.Should().BeTrue();
        booking.Reschedule(operationId, newStart, newEnd, booking.BranchId, "administrative reschedule", false, fingerprint, DateTimeOffset.UtcNow).IsSuccess.Should().BeTrue();

        booking.RescheduleHistory.Should().ContainSingle();
    }

    [Fact]
    public void Reschedule_ShouldReject_ReusedOperationIdWithDifferentPayload()
    {
        Booking booking = CreateBooking();
        CalendarResourceId resourceId = CalendarResourceId.New();
        booking.AddResource(BookingResourceId.New(), resourceId).IsSuccess.Should().BeTrue();
        Guid operationId = Guid.NewGuid();
        string fingerprint = Booking.ResourceFingerprint(booking.Resources);

        booking.Reschedule(operationId, booking.StartAtUtc.AddHours(2), booking.EndAtUtc.AddHours(2), booking.BranchId, "administrative reschedule", false, fingerprint, DateTimeOffset.UtcNow).IsSuccess.Should().BeTrue();
        var second = booking.Reschedule(operationId, booking.StartAtUtc.AddHours(1), booking.EndAtUtc.AddHours(1), booking.BranchId, "different payload", false, fingerprint, DateTimeOffset.UtcNow);

        second.IsFailure.Should().BeTrue();
        booking.RescheduleHistory.Should().ContainSingle();
    }

    [Fact]
    public void Cancel_ShouldPersistStructuredPolicy_AndBeIdempotent()
    {
        Booking booking = CreateBooking();
        Guid operationId = Guid.NewGuid();
        DateTimeOffset cancelledAt = booking.StartAtUtc.AddHours(-72);
        var policy = new BookingCancellationPolicyResolutionSnapshot(
            "scheduling.default-cancellation",
            3,
            "scheduling.cancellation.policy.noCharge.beforeDeadline",
            BookingCreditDecision.Released,
            BookingFeeDecision.NoCharge,
            false);

        booking.Cancel(
            operationId,
            CancellationInitiator.Student,
            Guid.NewGuid(),
            CancellationReasonCode.Illness,
            "medical appointment",
            cancelledAt,
            policy,
            BookingNotificationDecision.NotifyAffectedParticipants,
            false,
            null).IsSuccess.Should().BeTrue();

        BookingCancellation cancellation = booking.Cancellations.Single();
        cancellation.PolicyCode.Should().Be("scheduling.default-cancellation");
        cancellation.PolicyVersion.Should().Be(3);
        cancellation.CreditDecision.Should().Be(BookingCreditDecision.Released);
        cancellation.FeeDecision.Should().Be(BookingFeeDecision.NoCharge);
        cancellation.NoticeDurationMinutes.Should().Be(72 * 60);
        booking.Status.Should().Be(BookingStatus.Cancelled);
    }

    [Fact]
    public void Cancel_ShouldReject_AfterScheduledStart()
    {
        Booking booking = CreateBooking();
        var policy = new BookingCancellationPolicyResolutionSnapshot(
            "scheduling.default-cancellation", 1, "policy", BookingCreditDecision.Released, BookingFeeDecision.NoCharge, false);

        Result result = booking.Cancel(
            Guid.NewGuid(),
            CancellationInitiator.Student,
            Guid.NewGuid(),
            CancellationReasonCode.StudentRequest,
            null,
            booking.StartAtUtc,
            policy,
            BookingNotificationDecision.NotifyAffectedParticipants,
            false,
            null);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void CancelOverride_ShouldRequireReason()
    {
        Booking booking = CreateBooking();
        var policy = new BookingCancellationPolicyResolutionSnapshot(
            "scheduling.default-cancellation", 1, "policy", BookingCreditDecision.PendingExternalReview, BookingFeeDecision.PendingExternalReview, false);

        Result result = booking.Cancel(
            Guid.NewGuid(),
            CancellationInitiator.Organization,
            Guid.NewGuid(),
            CancellationReasonCode.Administrative,
            null,
            booking.StartAtUtc.AddHours(-1),
            policy,
            BookingNotificationDecision.NotifyAffectedParticipants,
            true,
            null);

        result.IsFailure.Should().BeTrue();
    }


    [Fact]
    public void Attendance_ShouldBeRecordedOnce_AndReplayIdempotently()
    {
        Booking booking = CreateReservedBooking();
        Guid operationId = Guid.NewGuid();
        DateTimeOffset recordedAt = booking.StartAtUtc.AddMinutes(5);
        UserId userId = UserId.New();

        Result first = booking.RecordAttendance(operationId, AttendanceStatus.Present, recordedAt, userId,
            booking.StartAtUtc, booking.EndAtUtc, 0, null, null, AttendanceChargeDecision.None,
            AttendanceCreditDecision.None, AttendanceFollowUpAction.None, false, null);
        Result replay = booking.RecordAttendance(operationId, AttendanceStatus.Present, recordedAt.AddMinutes(1), userId,
            booking.StartAtUtc, booking.EndAtUtc, 0, null, null, AttendanceChargeDecision.None,
            AttendanceCreditDecision.None, AttendanceFollowUpAction.None, false, null);

        first.IsSuccess.Should().BeTrue();
        replay.IsSuccess.Should().BeTrue();
        booking.AttendanceHistory.Should().ContainSingle();
    }

    [Fact]
    public void Attendance_ShouldRejectEmptyRecordedByUserId()
    {
        Booking booking = CreateReservedBooking();

        Result result = booking.RecordAttendance(
            Guid.NewGuid(),
            AttendanceStatus.Present,
            booking.StartAtUtc.AddMinutes(5),
            UserId.Empty,
            booking.StartAtUtc,
            booking.EndAtUtc,
            0,
            null,
            null,
            AttendanceChargeDecision.None,
            AttendanceCreditDecision.None,
            AttendanceFollowUpAction.None,
            false,
            null);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void AttendanceCorrection_ShouldAppendHistory_AndSupersedePreviousRecord()
    {
        Booking booking = CreateReservedBooking();
        UserId userId = UserId.New();
        DateTimeOffset firstRecordedAt = booking.StartAtUtc.AddMinutes(10);

        booking.RecordAttendance(Guid.NewGuid(), AttendanceStatus.LateArrival, firstRecordedAt, userId,
            booking.StartAtUtc.AddMinutes(10), booking.EndAtUtc, 10, "traffic", null,
            AttendanceChargeDecision.None, AttendanceCreditDecision.None, AttendanceFollowUpAction.None, false, null).IsSuccess.Should().BeTrue();

        BookingAttendance previous = booking.CurrentAttendance!;
        booking.RecordAttendance(Guid.NewGuid(), AttendanceStatus.Present, firstRecordedAt.AddMinutes(20), userId,
            booking.StartAtUtc, booking.EndAtUtc, 0, "corrected", null,
            AttendanceChargeDecision.None, AttendanceCreditDecision.None, AttendanceFollowUpAction.None, false, null).IsSuccess.Should().BeTrue();

        booking.AttendanceHistory.Should().HaveCount(2);
        booking.CurrentAttendance!.SupersedesAttendanceId.Should().Be(previous.Id);
    }

    [Fact]
    public void AttendanceCorrection_ShouldRequireOverride_AfterCorrectionWindow()
    {
        Booking booking = CreateReservedBooking();
        UserId userId = UserId.New();
        DateTimeOffset firstRecordedAt = booking.StartAtUtc.AddMinutes(5);
        booking.RecordAttendance(Guid.NewGuid(), AttendanceStatus.StudentAbsent, firstRecordedAt, userId, null, null, 0,
            "no show", null, AttendanceChargeDecision.PendingExternalReview, AttendanceCreditDecision.PendingExternalReview,
            AttendanceFollowUpAction.ContactStudent, false, null).IsSuccess.Should().BeTrue();

        Result correction = booking.RecordAttendance(Guid.NewGuid(), AttendanceStatus.ExcusedAbsence, firstRecordedAt.AddHours(25),
            userId, null, null, 0, "medical evidence", Guid.NewGuid(), AttendanceChargeDecision.PendingExternalReview,
            AttendanceCreditDecision.PendingExternalReview, AttendanceFollowUpAction.None, false, null);

        correction.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ReplaceInstructor_ShouldKeepHistory_AndUpdateResourceAndParticipant()
    {
        Booking booking = CreateBooking();
        UserId previousInstructor = UserId.New();
        UserId replacementInstructor = UserId.New();
        CalendarResourceId previousResource = CalendarResourceId.New();
        CalendarResourceId replacementResource = CalendarResourceId.New();
        booking.AddResource(BookingResourceId.New(), previousResource).IsSuccess.Should().BeTrue();
        booking.AddParticipant(BookingParticipantId.New(), BookingParticipantType.Instructor, previousInstructor.Value).IsSuccess.Should().BeTrue();

        Guid operationId = Guid.NewGuid();
        Result result = booking.ReplaceInstructor(operationId, previousResource, replacementResource, previousInstructor, replacementInstructor,
            InstructorReplacementMode.SingleSession, "instructor absent", booking.StartAtUtc.AddHours(-2), booking.EndAtUtc);

        result.IsSuccess.Should().BeTrue();
        booking.Resources.Should().ContainSingle(x => x.CalendarResourceId == replacementResource);
        booking.Participants.Should().ContainSingle(x => x.ParticipantType == BookingParticipantType.Instructor && x.ExternalParticipantId == replacementInstructor.Value);
        booking.InstructorReplacementHistory.Should().ContainSingle(x =>
            x.OperationId == operationId && x.PreviousInstructorId == previousInstructor && x.ReplacementInstructorId == replacementInstructor);
    }

    [Fact]
    public void ReplaceInstructor_ShouldBeIdempotent_ForSameOperation()
    {
        Booking booking = CreateBooking();
        UserId previousInstructor = UserId.New();
        UserId replacementInstructor = UserId.New();
        CalendarResourceId previousResource = CalendarResourceId.New();
        CalendarResourceId replacementResource = CalendarResourceId.New();
        booking.AddResource(BookingResourceId.New(), previousResource).IsSuccess.Should().BeTrue();
        Guid operationId = Guid.NewGuid();
        DateTimeOffset occurredAt = booking.StartAtUtc.AddHours(-2);

        booking.ReplaceInstructor(operationId, previousResource, replacementResource, previousInstructor, replacementInstructor,
            InstructorReplacementMode.SingleSession, "absence", occurredAt, null).IsSuccess.Should().BeTrue();
        booking.ReplaceInstructor(operationId, previousResource, replacementResource, previousInstructor, replacementInstructor,
            InstructorReplacementMode.SingleSession, "absence", occurredAt.AddMinutes(1), null).IsSuccess.Should().BeTrue();

        booking.InstructorReplacementHistory.Should().ContainSingle();
    }

    [Fact]
    public void ReplaceInstructor_ShouldReject_ReusedOperationWithDifferentPayload()
    {
        Booking booking = CreateBooking();
        UserId previousInstructor = UserId.New();
        UserId replacementInstructor = UserId.New();
        CalendarResourceId previousResource = CalendarResourceId.New();
        CalendarResourceId replacementResource = CalendarResourceId.New();
        booking.AddResource(BookingResourceId.New(), previousResource).IsSuccess.Should().BeTrue();
        Guid operationId = Guid.NewGuid();

        booking.ReplaceInstructor(operationId, previousResource, replacementResource, previousInstructor, replacementInstructor,
            InstructorReplacementMode.SingleSession, "absence", booking.StartAtUtc.AddHours(-2), null).IsSuccess.Should().BeTrue();
        Result replay = booking.ReplaceInstructor(operationId, previousResource, replacementResource, previousInstructor, replacementInstructor,
            InstructorReplacementMode.SingleSession, "different reason", booking.StartAtUtc.AddHours(-1), null);

        replay.IsFailure.Should().BeTrue();
        booking.InstructorReplacementHistory.Should().ContainSingle();
    }


    [Fact]
    public void ReplaceVehicle_ShouldKeepHistory_AndUpdateResource()
    {
        Booking booking = CreateBooking();
        Guid previousVehicleId = Guid.NewGuid();
        Guid replacementVehicleId = Guid.NewGuid();
        CalendarResourceId previousResource = CalendarResourceId.New();
        CalendarResourceId replacementResource = CalendarResourceId.New();
        booking.AddResource(BookingResourceId.New(), previousResource).IsSuccess.Should().BeTrue();

        Guid operationId = Guid.NewGuid();
        Result result = booking.ReplaceVehicle(operationId, previousResource, replacementResource, previousVehicleId, replacementVehicleId,
            VehicleReplacementMode.SingleSession, "vehicle immobilized", booking.StartAtUtc.AddHours(-2));

        result.IsSuccess.Should().BeTrue();
        booking.Resources.Should().ContainSingle(x => x.CalendarResourceId == replacementResource);
        booking.VehicleReplacementHistory.Should().ContainSingle(x => x.OperationId == operationId &&
            x.PreviousVehicleId == previousVehicleId && x.ReplacementVehicleId == replacementVehicleId);
    }

    [Fact]
    public void ReplaceVehicle_ShouldBeIdempotent_ForSameOperation()
    {
        Booking booking = CreateBooking();
        Guid previousVehicleId = Guid.NewGuid();
        Guid replacementVehicleId = Guid.NewGuid();
        CalendarResourceId previousResource = CalendarResourceId.New();
        CalendarResourceId replacementResource = CalendarResourceId.New();
        booking.AddResource(BookingResourceId.New(), previousResource).IsSuccess.Should().BeTrue();
        Guid operationId = Guid.NewGuid();

        booking.ReplaceVehicle(operationId, previousResource, replacementResource, previousVehicleId, replacementVehicleId,
            VehicleReplacementMode.SelectedSessions, "maintenance", booking.StartAtUtc.AddHours(-2)).IsSuccess.Should().BeTrue();
        booking.ReplaceVehicle(operationId, previousResource, replacementResource, previousVehicleId, replacementVehicleId,
            VehicleReplacementMode.SelectedSessions, "maintenance", booking.StartAtUtc.AddHours(-1)).IsSuccess.Should().BeTrue();

        booking.VehicleReplacementHistory.Should().ContainSingle();
    }

    [Fact]
    public void ReplaceVehicle_ShouldReject_ReusedOperationWithDifferentPayload()
    {
        Booking booking = CreateBooking();
        Guid previousVehicleId = Guid.NewGuid();
        Guid replacementVehicleId = Guid.NewGuid();
        CalendarResourceId previousResource = CalendarResourceId.New();
        CalendarResourceId replacementResource = CalendarResourceId.New();
        booking.AddResource(BookingResourceId.New(), previousResource).IsSuccess.Should().BeTrue();
        Guid operationId = Guid.NewGuid();

        booking.ReplaceVehicle(operationId, previousResource, replacementResource, previousVehicleId, replacementVehicleId,
            VehicleReplacementMode.SingleSession, "breakdown", booking.StartAtUtc.AddHours(-2)).IsSuccess.Should().BeTrue();
        Result replay = booking.ReplaceVehicle(operationId, previousResource, replacementResource, previousVehicleId, replacementVehicleId,
            VehicleReplacementMode.SingleSession, "different reason", booking.StartAtUtc.AddHours(-1));

        replay.IsFailure.Should().BeTrue();
        booking.VehicleReplacementHistory.Should().ContainSingle();
    }

    [Fact]
    public void Create_ShouldPersistBookingCreationDetails()
    {
        Guid trainingPathId = Guid.NewGuid();
        Guid creditAccountId = Guid.NewGuid();
        BookingId bookingId = BookingId.New();

        Result<Booking> result = Booking.Create(
            bookingId,
            OrganizationId.New(),
            BranchId.New(),
            BookingType.TrainingSession,
            new DateTimeOffset(2026, 8, 19, 10, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 8, 19, 11, 0, 0, TimeSpan.Zero),
            "Leçon",
            new BookingCreationDetails(
                "booking-create-0001",
                new string('a', 64),
                trainingPathId,
                "B",
                "Intersections",
                "Gare centrale",
                "PRICE-2026-A",
                creditAccountId,
                1m,
                "Note interne",
                BookingNotificationPolicy.Standard));

        result.IsSuccess.Should().BeTrue();
        result.Value.TrainingPathId.Should().Be(trainingPathId);
        result.Value.TrainingCategory.Should().Be("B");
        result.Value.Objectives.Should().Be("Intersections");
        result.Value.MeetingPoint.Should().Be("Gare centrale");
        result.Value.TrainingCreditAccountId.Should().Be(creditAccountId);
        result.Value.CreditReservationStatus.Should().Be(BookingCreditReservationStatus.Pending);
    }

    [Fact]
    public void Confirm_ShouldRequireConfiguredCreditReservation()
    {
        Booking booking = Booking.Create(
            BookingId.New(),
            OrganizationId.New(),
            BranchId.New(),
            BookingType.TrainingSession,
            new DateTimeOffset(2026, 8, 19, 10, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 8, 19, 11, 0, 0, TimeSpan.Zero),
            "Leçon",
            new BookingCreationDetails(
                "booking-create-0002",
                new string('b', 64),
                null, null, null, null, null, Guid.NewGuid(), 1m, null, BookingNotificationPolicy.None)).Value;

        CalendarResourceId resourceId = CalendarResourceId.New();
        booking.AddResource(BookingResourceId.New(), resourceId).IsSuccess.Should().BeTrue();
        BookingConflictAssessment assessment = BookingConflictDetector.Assess(booking,
            [new CalendarResourceSchedulingSnapshot(resourceId, 1, 1, CalendarResourceStatus.Active, null, null, CalendarResourceType.Other, null)], []);
        booking.Reserve(assessment).IsSuccess.Should().BeTrue();

        booking.Confirm().IsFailure.Should().BeTrue();
        booking.MarkCreditReserved("scheduling-booking:credit-reservation").IsSuccess.Should().BeTrue();
        booking.Confirm().IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Hold_ShouldCreateShortTentativeReservation()
    {
        Booking booking = CreateBooking();
        CalendarResourceId resourceId = CalendarResourceId.New();
        booking.AddResource(BookingResourceId.New(), resourceId).IsSuccess.Should().BeTrue();
        BookingConflictAssessment assessment = BookingConflictDetector.Assess(booking,
            [new CalendarResourceSchedulingSnapshot(resourceId, 1, 1, CalendarResourceStatus.Active, null, null, CalendarResourceType.Other, null)], []);
        DateTimeOffset now = new(2026, 8, 19, 9, 0, 0, TimeSpan.Zero);

        booking.Hold(assessment, now.AddMinutes(5), now).IsSuccess.Should().BeTrue();

        booking.Status.Should().Be(BookingStatus.Tentative);
        booking.HoldExpiresAtUtc.Should().Be(now.AddMinutes(5));
    }

    private static Booking CreateReservedBooking()
    {
        Booking booking = CreateBooking();
        CalendarResourceId resourceId = CalendarResourceId.New();
        booking.AddResource(BookingResourceId.New(), resourceId).IsSuccess.Should().BeTrue();
        BookingConflictAssessment assessment = BookingConflictDetector.Assess(booking,
            [new CalendarResourceSchedulingSnapshot(resourceId, 1, 1, CalendarResourceStatus.Active, null, null, CalendarResourceType.Other, null)], []);
        booking.Reserve(assessment).IsSuccess.Should().BeTrue();
        return booking;
    }

    private static Booking CreateBooking() => Booking.Create(
        BookingId.New(),
        OrganizationId.New(),
        BranchId.New(),
        BookingType.TrainingSession,
        new DateTimeOffset(2026, 8, 19, 8, 0, 0, TimeSpan.Zero),
        new DateTimeOffset(2026, 8, 19, 9, 0, 0, TimeSpan.Zero),
        "Leçon de conduite").Value;
}
