using DriveOS.Modules.ProfessionalMarketplace.Application.Compliance;
using DriveOS.Modules.SchedulingCapacity.Application.Persistence;
using DriveOS.Modules.SchedulingCapacity.Domain.CalendarResources;

namespace DriveOS.Api.Integrations.ProfessionalMarketplace;

internal sealed class ProfessionalComplianceOperationalGateway(
    ICalendarResourceRepository resources,
    ISchedulingCapacityUnitOfWork uow)
    :IProfessionalComplianceOperationalGateway
{
    private const string Prefix="COMPLIANCE:";

    public async Task ApplyAsync(
        ProfessionalComplianceOperationalRequest request,
        CancellationToken cancellationToken=default)
    {
        foreach(OrganizationId organizationId in request.OrganizationIds.Distinct())
        {
            CalendarResource? resource=await resources.GetByExternalReferenceForUpdateAsync(
                organizationId,
                CalendarResourceType.Instructor,
                request.ProfessionalUserId.Value,
                cancellationToken);

            if(resource is null)continue;

            if(request.BlockNewSessions)
            {
                string reason=Prefix+request.Reason;
                if(resource.Status!=CalendarResourceStatus.Restricted||
                   !string.Equals(resource.RestrictionReason,reason,StringComparison.Ordinal))
                {
                    var result=resource.Restrict(reason);
                    if(result.IsFailure)
                        throw new InvalidOperationException($"{result.Error.Code}:{result.Error.Message}");
                }
            }
            else if(resource.Status==CalendarResourceStatus.Restricted&&
                    resource.RestrictionReason?.StartsWith(Prefix,StringComparison.Ordinal)==true)
            {
                var result=resource.Activate();
                if(result.IsFailure)
                    throw new InvalidOperationException($"{result.Error.Code}:{result.Error.Message}");
            }
        }

        await uow.CommitAsync(cancellationToken);
    }
}
