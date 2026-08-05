using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Application.OrganizationSequences.Models;
using DriveOS.Modules.Organizations.Domain.OrganizationSequences;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationSequences.GetById;

public sealed record GetOrganizationSequenceByIdQuery(
    OrganizationId OrganizationId,
    OrganizationSequenceId SequenceId)
    : IQuery<OrganizationSequenceResponse>;
