namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct StudentBranchPortfolioId(Guid Value)
{
    public static StudentBranchPortfolioId New() => new(Guid.NewGuid());
    public static StudentBranchPortfolioId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
    public static implicit operator Guid(StudentBranchPortfolioId id) => id.Value;
    public static explicit operator StudentBranchPortfolioId(Guid value) => new(value);
}
