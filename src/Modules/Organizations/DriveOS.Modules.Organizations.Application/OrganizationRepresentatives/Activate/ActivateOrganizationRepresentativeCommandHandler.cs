using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Domain.OrganizationRepresentatives;
using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.Activate;
internal sealed class ActivateOrganizationRepresentativeCommandHandler(IOrganizationRepresentativeRepository repository,IUnitOfWork unitOfWork):ICommandHandler<ActivateOrganizationRepresentativeCommand>
{ public async Task<Result> Handle(ActivateOrganizationRepresentativeCommand c,CancellationToken ct){ var e=await repository.GetForUpdateAsync(c.RepresentativeId,c.OrganizationId,ct); if(e is null)return Result.Failure(OrganizationRepresentativeErrors.NotFound); if(e.Revision!=c.ExpectedRevision)return Result.Failure(OrganizationRepresentativeErrors.ConcurrentUpdate); var r=e.Activate(); if(r.IsFailure)return r; await unitOfWork.CommitAsync(ct); return Result.Success(); } }
