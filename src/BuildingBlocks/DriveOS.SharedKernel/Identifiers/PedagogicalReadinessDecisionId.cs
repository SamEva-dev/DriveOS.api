namespace DriveOS.SharedKernel.Identifiers;
public readonly record struct PedagogicalReadinessDecisionId(Guid Value){public static PedagogicalReadinessDecisionId New()=>new(Guid.NewGuid());public static PedagogicalReadinessDecisionId Empty=>new(Guid.Empty);public bool IsEmpty=>Value==Guid.Empty;public override string ToString()=>Value.ToString();}
