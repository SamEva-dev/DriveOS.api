using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Domain.Assessments;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Application.Assessments.ScheduleAssessment;

public sealed record ScheduleAssessmentCommand(
    OrganizationId OrganizationId,
    LeadId LeadId,
    BranchId? BranchId,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    AssessmentType Type,
    AssessmentDeliveryMode DeliveryMode,
    AssessmentLocationKind LocationKind,
    string? LocationDetails,
    UserId? EvaluatorUserId,
    Guid? VehicleId,
    Guid? RoomId,
    Guid? SimulatorId,
    decimal? PriceAmount,
    string? PriceCurrency,
    string? Notes) : ICommand<Guid>;
