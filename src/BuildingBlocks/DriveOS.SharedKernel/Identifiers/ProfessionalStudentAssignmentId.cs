namespace DriveOS.SharedKernel.Identifiers;
public readonly record struct ProfessionalStudentAssignmentId(Guid Value)
{
    public static ProfessionalStudentAssignmentId Empty=>new(Guid.Empty);
    public bool IsEmpty=>Value==Guid.Empty;
    public override string ToString()=>Value.ToString();
}
