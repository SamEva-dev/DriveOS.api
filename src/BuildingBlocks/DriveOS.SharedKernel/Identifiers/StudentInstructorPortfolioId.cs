namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct StudentInstructorPortfolioId(Guid Value)
{
    public static StudentInstructorPortfolioId New() => new(Guid.NewGuid());
    public static StudentInstructorPortfolioId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
    public static implicit operator Guid(StudentInstructorPortfolioId id) => id.Value;
    public static explicit operator StudentInstructorPortfolioId(Guid value) => new(value);
}
