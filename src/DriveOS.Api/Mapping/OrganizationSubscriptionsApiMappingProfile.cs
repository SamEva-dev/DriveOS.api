using DomainRelay.Mapping.Abstractions.Configuration;
using DomainRelay.Mapping.Abstractions.Profiles;
using DriveOS.Api.Endpoints.Organization.OrganizationSubscriptions;
using DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.CancelOrganizationSubscription;
using DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.ChangeOrganizationSubscriptionPlan;
using DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.ChangeStatus;
using DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.CreateOrganizationSubscription;
using DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.Models;

namespace DriveOS.Api.Mapping;

public sealed class OrganizationSubscriptionsApiMappingProfile : MappingProfile
{
    public override void Configure(IMappingConfiguration configuration)
    {
        configuration.CreateMap<
            OrganizationSubscriptionResponse,
            OrganizationSubscriptionResponseContract
        >();
        configuration.CreateMap<SubscriptionPeriodResponse, SubscriptionPeriodResponseContract>();
        configuration.CreateMap<
            SubscriptionCancellationResponse,
            SubscriptionCancellationResponseContract
        >();
        configuration.CreateMap<
            SubscriptionEntitlementResponse,
            SubscriptionEntitlementResponseContract
        >();
        configuration.CreateMap<SubscriptionLimitResponse, SubscriptionLimitResponseContract>();

        configuration.CreateMap<
            CreateOrganizationSubscriptionApiModel,
            CreateOrganizationSubscriptionCommand
        >();
        configuration.CreateMap<
            ChangeOrganizationSubscriptionPlanApiModel,
            ChangeOrganizationSubscriptionPlanCommand
        >();
        configuration.CreateMap<
            ChangeOrganizationSubscriptionStatusApiModel,
            ChangeOrganizationSubscriptionStatusCommand
        >();
        configuration.CreateMap<
            CancelOrganizationSubscriptionApiModel,
            CancelOrganizationSubscriptionCommand
        >();
    }
}
