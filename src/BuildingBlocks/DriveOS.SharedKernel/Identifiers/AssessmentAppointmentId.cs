namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct AssessmentAppointmentId(Guid Value)
{
    public static AssessmentAppointmentId New() => new(Guid.NewGuid());
    public static AssessmentAppointmentId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
