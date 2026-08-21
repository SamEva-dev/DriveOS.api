namespace DriveOS.Modules.TrainingDelivery.Domain.Sessions;

public enum TrainingSessionAttendanceStatus
{
    Present = 1,
    LateArrival = 2,
    StudentAbsent = 3,
    InstructorAbsent = 4,
    PartialAttendance = 5,
    ExcusedAbsence = 6,
    UnexcusedAbsence = 7,
    UnableToDeliver = 8
}
