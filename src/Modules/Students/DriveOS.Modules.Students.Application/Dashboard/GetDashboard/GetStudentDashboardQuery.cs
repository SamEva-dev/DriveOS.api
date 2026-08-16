using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Students.Application.Dashboard.GetDashboard;

public sealed record GetStudentDashboardQuery(OrganizationId OrganizationId, BranchId? BranchId)
    : IQuery<StudentDashboardResponse>;
