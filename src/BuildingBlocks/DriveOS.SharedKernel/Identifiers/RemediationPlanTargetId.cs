namespace DriveOS.SharedKernel.Identifiers;
public readonly record struct RemediationPlanTargetId(Guid Value){public static RemediationPlanTargetId New()=>new(Guid.NewGuid());public static RemediationPlanTargetId Empty=>new(Guid.Empty);public bool IsEmpty=>Value==Guid.Empty;public override string ToString()=>Value.ToString();}
