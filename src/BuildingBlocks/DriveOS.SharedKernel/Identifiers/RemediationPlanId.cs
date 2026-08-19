namespace DriveOS.SharedKernel.Identifiers;
public readonly record struct RemediationPlanId(Guid Value){public static RemediationPlanId New()=>new(Guid.NewGuid());public static RemediationPlanId Empty=>new(Guid.Empty);public bool IsEmpty=>Value==Guid.Empty;public override string ToString()=>Value.ToString();}
