namespace DriveOS.Modules.Students.Domain.Instructors;

public enum StudentInstructorAssignmentType
{
    PrimaryInstructor = 1,
    SecondaryInstructor = 2,
    TemporaryReplacement = 3,
    SpecialistInstructor = 4,
    ExamAccompanist = 5,
    PartnerInstructor = 6,
}

[Flags]
public enum StudentInstructorScope
{
    None = 0,
    StudentRead = 1,
    SessionsRead = 2,
    PedagogyRead = 4,
    Theory = 8,
    Practical = 16,
    Simulator = 32,
    Exam = 64,
    All = 127,
}

public enum StudentInstructorAssignmentStatus
{
    Planned = 1,
    Active = 2,
    Expired = 3,
    Ended = 4,
    Replaced = 5,
}

public enum InstructorMetricStatus
{
    Available = 1,
    Unavailable = 2,
    NotEvaluated = 3,
    Warning = 4,
}
