using Xunit;

namespace DriveOS.Security.Contracts.Tests;

public sealed class ExamAnalyticsPermissionCatalogTests
{
    [Fact]
    public void Analytics_permission_is_declared_once_and_part_of_exam_catalog()
    {
        Assert.Single(
            DriveOsPermissionCatalog.All,
            item => item.Code == DriveOsPermissionCodes.Exams.AnalyticsRead);

        Assert.Contains(DriveOsPermissionCodes.Exams.AnalyticsRead, DriveOsPermissionCodes.Exams.All);
        Assert.Contains(DriveOsPermissionCodes.Exams.AnalyticsRead, DriveOsPermissionCodes.Exams.ReadOnly);
    }

    [Fact]
    public void ExamCoordinator_receives_analytics_permission()
    {
        Assert.Contains(
            DriveOsPermissionCodes.Exams.AnalyticsRead,
            DriveOsRolePermissionDefaults.GetPermissions(DriveOsRoleCodes.ExamCoordinator));
    }
}
