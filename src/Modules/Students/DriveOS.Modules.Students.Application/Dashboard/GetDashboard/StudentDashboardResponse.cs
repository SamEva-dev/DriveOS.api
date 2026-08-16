namespace DriveOS.Modules.Students.Application.Dashboard.GetDashboard;

public sealed record StudentDashboardResponse(
    int ActiveStudents,
    int DraftEnrollments,
    int PendingDocuments,
    int ReadyForValidation,
    IReadOnlyList<StudentDashboardActionItem> PriorityActions,
    IReadOnlyList<RecentStudentItem> RecentStudents
);

public sealed record StudentDashboardActionItem(
    Guid EnrollmentId,
    Guid StudentId,
    string StudentName,
    string TrainingCode,
    string Status,
    DateTimeOffset CreatedAtUtc
);

public sealed record RecentStudentItem(
    Guid StudentId,
    string StudentName,
    string? Email,
    string? Phone,
    DateTimeOffset CreatedAtUtc
);
