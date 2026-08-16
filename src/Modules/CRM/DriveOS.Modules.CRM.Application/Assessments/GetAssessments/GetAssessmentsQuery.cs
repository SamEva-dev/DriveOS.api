using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Domain.Assessments;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Application.Assessments.GetAssessments;

public sealed record GetLeadAssessmentsQuery(OrganizationId OrganizationId, LeadId LeadId)
    : IQuery<IReadOnlyList<AssessmentAppointmentResponse>>;

public sealed record GetAssessmentQuery(
    OrganizationId OrganizationId,
    AssessmentAppointmentId AppointmentId
) : IQuery<AssessmentAppointmentResponse>;

public sealed record AssessmentAppointmentResponse(
    Guid Id,
    Guid LeadId,
    Guid? BranchId,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    string Type,
    string DeliveryMode,
    string LocationKind,
    string? LocationDetails,
    Guid? EvaluatorUserId,
    Guid? VehicleId,
    Guid? RoomId,
    Guid? SimulatorId,
    decimal? PriceAmount,
    string? PriceCurrency,
    string? Notes,
    string Status,
    DateTimeOffset? ClosedAtUtc,
    DateTimeOffset CreatedAtUtc
);
