namespace DriveOS.SharedKernel.Identifiers;
public readonly record struct EquipmentAssignmentId(Guid Value)
{
    public static EquipmentAssignmentId New() => new(Guid.NewGuid());
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
