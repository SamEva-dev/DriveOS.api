namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct StudentStatusBoardId(Guid Value)
{
    public static StudentStatusBoardId New() => new(Guid.NewGuid());
    public static StudentStatusBoardId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
    public static implicit operator Guid(StudentStatusBoardId id) => id.Value;
    public static explicit operator StudentStatusBoardId(Guid value) => new(value);
}
