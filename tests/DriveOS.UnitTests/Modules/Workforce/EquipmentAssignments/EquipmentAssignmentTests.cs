using DriveOS.Modules.Workforce.Domain.EquipmentAssignments; using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.UnitTests.Modules.Workforce.EquipmentAssignments;
public sealed class EquipmentAssignmentTests
{
 [Fact] public void Returned_assignment_cannot_be_returned_twice(){var now=DateTimeOffset.UtcNow;var x=EquipmentAssignment.Create(EquipmentAssignmentId.New(),OrganizationId.New(),EmployeeId.New(),EquipmentResourceType.Tablet,Guid.NewGuid(),DateOnly.FromDateTime(DateTime.UtcNow),null,now,UserId.New()).Value;x.HandOver(EquipmentCondition.Good,null,now,UserId.New());x.Return(DateOnly.FromDateTime(DateTime.UtcNow),EquipmentCondition.Good,null,now,UserId.New());Assert.True(x.Return(DateOnly.FromDateTime(DateTime.UtcNow),EquipmentCondition.Good,null,now,UserId.New()).IsFailure);}
 [Fact] public void Planned_assignment_can_be_cancelled_but_not_handed_over_afterwards(){var now=DateTimeOffset.UtcNow;var x=EquipmentAssignment.Create(EquipmentAssignmentId.New(),OrganizationId.New(),EmployeeId.New(),EquipmentResourceType.Badge,Guid.NewGuid(),DateOnly.FromDateTime(DateTime.UtcNow),null,now,UserId.New()).Value;Assert.True(x.Cancel("Not required",now,UserId.New()).IsSuccess);Assert.True(x.HandOver(EquipmentCondition.Good,null,now,UserId.New()).IsFailure);}
}
