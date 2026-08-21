namespace DriveOS.SharedKernel.Identifiers;
public readonly record struct GroupTrainingSessionOperationId(Guid Value){public static GroupTrainingSessionOperationId New()=>new(Guid.NewGuid());}
