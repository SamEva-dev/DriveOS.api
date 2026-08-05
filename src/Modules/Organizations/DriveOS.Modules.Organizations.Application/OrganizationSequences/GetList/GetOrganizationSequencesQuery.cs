using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Application.OrganizationSequences.Models;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationSequences.GetList;

public sealed record GetOrganizationSequencesQuery(
    OrganizationId OrganizationId,
    BranchId? BranchId)
    : IQuery<IReadOnlyList<OrganizationSequenceListItem>>;
