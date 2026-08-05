namespace DriveOS.Modules.Organizations.Application.OrganizationSequences.Models;

public sealed record OrganizationSequenceListItem(
    Guid Id,
    Guid? BranchId,
    string Scope,
    string Code,
    string Pattern,
    int Padding,
    long NextValue,
    string ResetPolicy,
    string Status,
    int Revision);
