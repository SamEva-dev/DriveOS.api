using DriveOS.Modules.ProfessionalMarketplace.Application.Engagements;
using DriveOS.Modules.SchedulingCapacity.Application.Persistence;
using DriveOS.Modules.SchedulingCapacity.Domain.CalendarResources;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Integrations.ProfessionalMarketplace;

internal sealed class ProfessionalSchedulingPreparationGateway(
    ICalendarResourceRepository resources,
    ISchedulingCapacityUnitOfWork unitOfWork)
    : IProfessionalSchedulingPreparationGateway
{
    public async Task<ProfessionalSchedulingPreparationResult> PrepareAsync(
        ProfessionalSchedulingPreparationRequest request,
        CancellationToken cancellationToken = default)
    {
        if(request.StartsOn>request.EndsOn)
            return new(false,null,"professional-marketplace.scheduling.invalid-period");

        if(request.TeachingCategoryCodes.Length==0)
            return new(false,null,"professional-marketplace.scheduling.category-required");

        try
        {
            _=TimeZoneInfo.FindSystemTimeZoneById(request.TimeZoneId);
        }
        catch(TimeZoneNotFoundException)
        {
            return new(false,null,"professional-marketplace.scheduling.invalid-time-zone");
        }
        catch(InvalidTimeZoneException)
        {
            return new(false,null,"professional-marketplace.scheduling.invalid-time-zone");
        }

        CalendarResource? existing=await resources.GetByExternalReferenceAsync(
            request.OrganizationId,
            CalendarResourceType.Instructor,
            request.ProfessionalUserId.Value,
            cancellationToken);

        if(existing is not null)
        {
            if(existing.Status!=CalendarResourceStatus.Active)
                return new(false,existing.Id.Value,$"professional-marketplace.scheduling.resource-status.{existing.Status.ToString().ToLowerInvariant()}");

            if(request.BranchId.HasValue &&
               existing.BranchId.HasValue &&
               existing.BranchId.Value!=request.BranchId.Value)
                return new(false,existing.Id.Value,"professional-marketplace.scheduling.resource-branch-mismatch");

            return new(true,existing.Id.Value,null);
        }

        CalendarResourceId id=CalendarResourceId.New();
        Result<CalendarResource> created=CalendarResource.Create(
            id,
            request.OrganizationId,
            request.BranchId,
            CalendarResourceType.Instructor,
            request.ProfessionalUserId.Value,
            request.DisplayName,
            1,
            request.TimeZoneId);

        if(created.IsFailure)
            return new(false,null,$"professional-marketplace.scheduling.resource-create:{created.Error.Code}");

        resources.Add(created.Value);
        await unitOfWork.CommitAsync(cancellationToken);

        return new(true,id.Value,null);
    }
}
