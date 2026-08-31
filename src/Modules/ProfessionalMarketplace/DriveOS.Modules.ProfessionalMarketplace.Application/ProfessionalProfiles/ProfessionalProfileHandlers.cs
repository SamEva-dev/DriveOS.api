using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.ProfessionalMarketplace.Application.Persistence;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.ProfessionalMarketplace.Application.ProfessionalProfiles;
public sealed class CreateProfessionalProfileCommandHandler(IProfessionalProfileRepository repo,IProfessionalMarketplaceUnitOfWork uow,IClock clock):ICommandHandler<CreateProfessionalProfileCommand,DriveOS.SharedKernel.Identifiers.ProfessionalProfileId>{public async Task<Result<DriveOS.SharedKernel.Identifiers.ProfessionalProfileId>> Handle(CreateProfessionalProfileCommand c,CancellationToken ct){if(await repo.FindByPersonAsync(c.PersonId,ct) is not null)return Result.Failure<DriveOS.SharedKernel.Identifiers.ProfessionalProfileId>(Error.Conflict("ProfessionalMarketplace.Profile.PersonAlreadyLinked","errors.professionalMarketplace.profile.personAlreadyLinked"));var r=ProfessionalProfile.Create(c.Id,c.PersonId,c.ProviderOrganizationId,c.UserId,clock.UtcNow);if(r.IsFailure)return Result.Failure<DriveOS.SharedKernel.Identifiers.ProfessionalProfileId>(r.Error);r.Value.SetCreatedAudit(clock.UtcNow,c.ActorUserId);repo.Add(r.Value);await uow.CommitAsync(ct);return Result.Success(r.Value.Id);}}
public sealed class GetCurrentProfessionalProfileQueryHandler(IProfessionalProfileRepository repo):IQueryHandler<GetCurrentProfessionalProfileQuery,ProfessionalProfileResponse>{public async Task<Result<ProfessionalProfileResponse>> Handle(GetCurrentProfessionalProfileQuery q,CancellationToken ct){var x=await repo.FindByUserAsync(q.UserId,ct);return x is null?Result.Failure<ProfessionalProfileResponse>(ProfessionalProfileErrors.NotFound):Result.Success(GetProfessionalProfileQueryHandler.Map(x));}}
public sealed class GetProfessionalProfileQueryHandler(IProfessionalProfileRepository repo):IQueryHandler<GetProfessionalProfileQuery,ProfessionalProfileResponse>{public async Task<Result<ProfessionalProfileResponse>> Handle(GetProfessionalProfileQuery q,CancellationToken ct){var x=await repo.GetByIdAsync(q.Id,ct);return x is null?Result.Failure<ProfessionalProfileResponse>(ProfessionalProfileErrors.NotFound):Result.Success(Map(x));}internal static ProfessionalProfileResponse Map(ProfessionalProfile x)=>new(x.Id.Value,x.PersonId.Value,x.ProviderOrganizationId.Value,x.UserId?.Value,x.Status.ToString(),x.ComplianceStatus.ToString(),x.ProfessionalType.ToString(),x.LegalName,x.TradeName,x.LegalStatusCode,x.RegistrationNumber,x.TaxNumber,x.ProfessionalEmail,x.ProfessionalPhone,x.BillingAddressLine1,x.BillingAddressLine2,x.BillingPostalCode,x.BillingCity,x.BillingCountryCode,x.Headline,x.Biography,x.ExperienceYears,x.Languages,x.TeachingCategoryCodes,x.SpecializationCodes,x.TeachingCapabilities.Select(c=>new TeachingCapabilityResponse(c.CategoryCode,c.DeliveryModeCodes,c.AudienceCodes,c.LanguageCodes,c.SpecializationCodes)).ToArray(),x.PreferredEngagementTypes,x.PrimaryServiceArea,x.MobilityRadiusKm,x.ServiceAreas.Select(a=>new ProfessionalServiceAreaInput(a.AreaCode,a.CountryCode,a.DisplayName,a.Latitude,a.Longitude,a.RadiusKm,a.Primary,a.MobilityMode)).ToArray(),
x.AvailabilityPolicy.RecurringRules.Select(a=>new MarketplaceAvailabilityRuleInput(a.DayOfWeek,a.StartTime,a.EndTime,a.TimeZoneId)).ToArray(),
x.AvailabilityPolicy.Exceptions.Select(a=>new MarketplaceAvailabilityExceptionInput(a.Date,a.StartTime,a.EndTime,a.Type,a.Reason)).ToArray(),
x.AvailabilityPolicy.MinimumBookingNoticeHours,
x.AvailabilityPolicy.MaximumDailyWorkMinutes,
x.AvailabilityPolicy.MaximumConsecutiveWorkMinutes,
x.Rates.Select(r=>new ProfessionalRateInput(r.RateCode,r.Unit,r.Amount,r.Currency,r.TeachingCategoryCode,r.VehicleProvisionMode,r.MileageRate,r.MinimumBillableQuantity,r.Negotiable,r.EffectiveFrom,r.EffectiveTo)).ToArray(),
x.HasPersonalTrainingVehicle,x.PersonalVehicleNotes,x.IsProfileComplete);}
internal static class ProfileUpdate{public static async Task<Result> Run(DriveOS.SharedKernel.Identifiers.ProfessionalProfileId id,Func<ProfessionalProfile,Result> action,IProfessionalProfileRepository repo,IProfessionalMarketplaceUnitOfWork uow,CancellationToken ct){var x=await repo.GetByIdForUpdateAsync(id,ct);if(x is null)return Result.Failure(ProfessionalProfileErrors.NotFound);var r=action(x);if(r.IsFailure)return r;await uow.CommitAsync(ct);return Result.Success();}}
public sealed class UpdateProfessionalBusinessIdentityCommandHandler(IProfessionalProfileRepository r,IProfessionalMarketplaceUnitOfWork u,IClock c):ICommandHandler<UpdateProfessionalBusinessIdentityCommand>{public Task<Result> Handle(UpdateProfessionalBusinessIdentityCommand x,CancellationToken ct)=>ProfileUpdate.Run(x.Id,p=>p.UpdateBusinessIdentity(x.ProfessionalType,x.LegalName,x.TradeName,x.LegalStatusCode,x.RegistrationNumber,x.TaxNumber,x.ProfessionalEmail,x.ProfessionalPhone,x.AddressLine1,x.AddressLine2,x.PostalCode,x.City,x.CountryCode,c.UtcNow,x.ActorUserId),r,u,ct);}
public sealed class UpdateProfessionalPresentationCommandHandler(IProfessionalProfileRepository r,IProfessionalMarketplaceUnitOfWork u,IClock c):ICommandHandler<UpdateProfessionalPresentationCommand>{public Task<Result> Handle(UpdateProfessionalPresentationCommand x,CancellationToken ct)=>ProfileUpdate.Run(x.Id,p=>p.UpdatePresentation(x.Headline,x.Biography,x.ExperienceYears,x.Languages,x.TeachingCategoryCodes,x.SpecializationCodes,c.UtcNow,x.ActorUserId),r,u,ct);}
public sealed class ReplaceProfessionalServiceAreasCommandHandler(
    IProfessionalProfileRepository repo,
    IProfessionalMarketplaceUnitOfWork uow,
    IClock clock):ICommandHandler<ReplaceProfessionalServiceAreasCommand>
{
    public async Task<Result> Handle(ReplaceProfessionalServiceAreasCommand c,CancellationToken ct)
    {
        var p=await repo.GetByIdForUpdateAsync(c.Id,ct);
        if(p is null)return Result.Failure(ProfessionalProfileErrors.NotFound);
        var r=p.ReplaceServiceAreas(c.Areas.Select(x=>new ProfessionalServiceArea(
            x.AreaCode,x.CountryCode,x.DisplayName,x.Latitude,x.Longitude,x.RadiusKm,x.Primary,x.MobilityMode)),clock.UtcNow,c.ActorUserId);
        if(r.IsFailure)return r;
        await uow.CommitAsync(ct);
        return Result.Success();
    }
}
public sealed class ReplaceMarketplaceAvailabilityCommandHandler(
    IProfessionalProfileRepository repo,
    IProfessionalMarketplaceUnitOfWork uow,
    IClock clock):ICommandHandler<ReplaceMarketplaceAvailabilityCommand>
{
    public async Task<Result> Handle(ReplaceMarketplaceAvailabilityCommand c,CancellationToken ct)
    {
        var p=await repo.GetByIdForUpdateAsync(c.Id,ct);
        if(p is null)return Result.Failure(ProfessionalProfileErrors.NotFound);
        var policy=new MarketplaceAvailabilityPolicy(
            c.RecurringRules.Select(x=>new MarketplaceAvailabilityRule(x.DayOfWeek,x.StartTime,x.EndTime,x.TimeZoneId)).ToArray(),
            c.Exceptions.Select(x=>new MarketplaceAvailabilityException(x.Date,x.StartTime,x.EndTime,x.Type,x.Reason)).ToArray(),
            c.MinimumBookingNoticeHours,
            c.MaximumDailyWorkMinutes,
            c.MaximumConsecutiveWorkMinutes);
        var r=p.ReplaceMarketplaceAvailability(policy,clock.UtcNow,c.ActorUserId);
        if(r.IsFailure)return r;
        await uow.CommitAsync(ct);
        return Result.Success();
    }
}
public sealed class ReplaceProfessionalRatesCommandHandler(
    IProfessionalProfileRepository repo,
    IProfessionalMarketplaceUnitOfWork uow,
    IClock clock):ICommandHandler<ReplaceProfessionalRatesCommand>
{
    public async Task<Result> Handle(ReplaceProfessionalRatesCommand c,CancellationToken ct)
    {
        var p=await repo.GetByIdForUpdateAsync(c.Id,ct);
        if(p is null)return Result.Failure(ProfessionalProfileErrors.NotFound);
        var r=p.ReplaceProfessionalRates(c.Rates.Select(x=>new ProfessionalRate(
            x.RateCode,x.Unit,x.Amount,x.Currency,x.TeachingCategoryCode,x.VehicleProvisionMode,
            x.MileageRate,x.MinimumBillableQuantity,x.Negotiable,x.EffectiveFrom,x.EffectiveTo)),clock.UtcNow,c.ActorUserId);
        if(r.IsFailure)return r;
        await uow.CommitAsync(ct);
        return Result.Success();
    }
}
public sealed class UpdateProfessionalVehicleCommandHandler(IProfessionalProfileRepository r,IProfessionalMarketplaceUnitOfWork u,IClock c):ICommandHandler<UpdateProfessionalVehicleCommand>{public Task<Result> Handle(UpdateProfessionalVehicleCommand x,CancellationToken ct)=>ProfileUpdate.Run(x.Id,p=>p.UpdatePersonalVehicle(x.HasPersonalTrainingVehicle,x.Notes,c.UtcNow,x.ActorUserId),r,u,ct);}
public sealed class UpdateProfessionalEngagementPreferencesCommandHandler(IProfessionalProfileRepository r,IProfessionalMarketplaceUnitOfWork u,IClock c):ICommandHandler<UpdateProfessionalEngagementPreferencesCommand>{public Task<Result> Handle(UpdateProfessionalEngagementPreferencesCommand x,CancellationToken ct)=>ProfileUpdate.Run(x.Id,p=>p.ReplaceEngagementPreferences(x.EngagementTypes,c.UtcNow,x.ActorUserId),r,u,ct);}
public sealed class CompleteProfessionalProfileCommandHandler(IProfessionalProfileRepository r,IProfessionalMarketplaceUnitOfWork u,IClock c):ICommandHandler<CompleteProfessionalProfileCommand>{public Task<Result> Handle(CompleteProfessionalProfileCommand x,CancellationToken ct)=>ProfileUpdate.Run(x.Id,p=>p.CompleteProfile(c.UtcNow,x.ActorUserId),r,u,ct);}

public sealed class ReplaceTeachingCapabilitiesCommandHandler(
    IProfessionalProfileRepository repository,
    IProfessionalMarketplaceUnitOfWork unitOfWork,
    IClock clock)
    : ICommandHandler<ReplaceTeachingCapabilitiesCommand>
{
    public Task<Result> Handle(ReplaceTeachingCapabilitiesCommand command, CancellationToken cancellationToken) =>
        ProfileUpdate.Run(
            command.ProfileId,
            profile => profile.ReplaceTeachingCapabilities(
                command.Capabilities.Select(x => new TeachingCapability(
                    x.CategoryCode,
                    x.DeliveryModeCodes,
                    x.AudienceCodes,
                    x.LanguageCodes,
                    x.SpecializationCodes)),
                clock.UtcNow,
                command.ActorUserId),
            repository,
            unitOfWork,
            cancellationToken);
}
