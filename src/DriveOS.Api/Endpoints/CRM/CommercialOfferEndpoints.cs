using DomainRelay.Abstractions;
using DriveOS.Api.Errors;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.CRM.Application.Offers.GenerateOffer;
using DriveOS.Modules.CRM.Application.Offers.CreateVariant;
using DriveOS.Modules.CRM.Application.Offers.GetOffers;
using DriveOS.Modules.CRM.Application.Offers.ChangeStatus;
using DriveOS.Modules.CRM.Application.Offers.SendOffer;
using DriveOS.Modules.CRM.Application.Offers.TrackOffer;
using DriveOS.Modules.CRM.Domain.Assessments;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.Modules.CRM.Domain.Offers;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using System.Security.Cryptography;
using System.Text;

namespace DriveOS.Api.Endpoints.Crm;

public static class CommercialOfferEndpoints
{
    public static IEndpointRouteBuilder MapCommercialOfferEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/crm").WithTags("CRM - Offers");
        group.MapPost("/leads/{leadId:guid}/offers", GenerateAsync).RequireAuthorization("Crm.Offers.Create");
        group.MapGet("/leads/{leadId:guid}/offers", GetByLeadAsync).RequireAuthorization("Crm.Offers.Read");
        group.MapGet("/offers/{offerId:guid}", GetByIdAsync).RequireAuthorization("Crm.Offers.Read");
        group.MapPost("/offers/{offerId:guid}/variants", CreateVariantAsync).RequireAuthorization("Crm.Offers.Create");
        group.MapPost("/offers/{offerId:guid}/submit-for-review", SubmitForReviewAsync).RequireAuthorization("Crm.Offers.SubmitForApproval");
        group.MapPost("/offers/{offerId:guid}/approve", ApproveAsync).RequireAuthorization("Crm.Offers.Approve");
        group.MapPost("/offers/{offerId:guid}/send", SendAsync).RequireAuthorization("Crm.Offers.Send");
        group.MapPost("/offers/access/{token}/view", RecordViewAsync).AllowAnonymous();
        group.MapPost("/offers/{offerId:guid}/exchanges", RecordExchangeAsync).RequireAuthorization("Crm.Activities.Create");
        group.MapPost("/offers/{offerId:guid}/follow-up", ScheduleFollowUpAsync).RequireAuthorization("Crm.Offers.Revise");
        group.MapPost("/offers/{offerId:guid}/withdraw", WithdrawAsync).RequireAuthorization("Crm.Offers.Withdraw");
        group.MapPost("/offers/{offerId:guid}/mark-accepted", MarkAcceptedAsync).RequireAuthorization("Crm.Offers.MarkAccepted");
        group.MapPost("/offers/{offerId:guid}/mark-rejected", MarkRejectedAsync).RequireAuthorization("Crm.Offers.MarkRejected");
        return endpoints;
    }

    private static async Task<IResult> GenerateAsync(Guid leadId, GenerateCommercialOfferRequest request,
        IMediator mediator, ICurrentTenant tenant, HttpContext context, CancellationToken cancellationToken)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(context);

        CommercialOfferLineDraft[] lines = request.Lines.Select(x => new CommercialOfferLineDraft(
            x.Type, x.ServiceId.HasValue ? new ServiceId(x.ServiceId.Value) : null,
            x.Description, x.Quantity, x.Unit, x.UnitPrice,
            x.DiscountAmount, x.TaxRate, x.Mandatory, x.PriceSource,
            x.ManualOverrideReason)).ToArray();

        Result<Guid> result = await mediator.Send(new GenerateCommercialOfferCommand(
            tenant.OrganizationId.Value, new LeadId(leadId),
            new AssessmentSessionId(request.AssessmentSessionId),
            request.BranchId.HasValue ? new BranchId(request.BranchId.Value) : null,
            request.TrainingCode, request.Currency, request.ValidUntilUtc,
            request.EstimatedFundingAmount, request.FinancingNotes,
            request.Conditions, request.InternalNotes, lines), cancellationToken);

        return result.IsFailure ? result.Error.ToHttpResult(context) :
            Results.Created($"/api/crm/offers/{result.Value}", new { offerId = result.Value });
    }

    private static async Task<IResult> GetByLeadAsync(Guid leadId, IMediator mediator,
        ICurrentTenant tenant, HttpContext context, CancellationToken cancellationToken)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(context);
        Result<IReadOnlyList<CommercialOfferResponse>> result = await mediator.Send(
            new GetLeadCommercialOffersQuery(tenant.OrganizationId.Value, new LeadId(leadId)), cancellationToken);
        return result.IsFailure ? result.Error.ToHttpResult(context) : Results.Ok(result.Value);
    }

    private static async Task<IResult> GetByIdAsync(Guid offerId, IMediator mediator,
        ICurrentTenant tenant, HttpContext context, CancellationToken cancellationToken)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(context);
        Result<CommercialOfferResponse> result = await mediator.Send(
            new GetCommercialOfferQuery(tenant.OrganizationId.Value, new CommercialOfferId(offerId)), cancellationToken);
        return result.IsFailure ? result.Error.ToHttpResult(context) : Results.Ok(result.Value);
    }

    private static async Task<IResult> CreateVariantAsync(Guid offerId, CreateCommercialOfferVariantRequest request,
        IMediator mediator, ICurrentTenant tenant, HttpContext context, CancellationToken cancellationToken)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(context);
        CommercialOfferLineDraft[] lines = request.Lines.Select(x => new CommercialOfferLineDraft(
            x.Type, x.ServiceId.HasValue ? new ServiceId(x.ServiceId.Value) : null,
            x.Description, x.Quantity, x.Unit, x.UnitPrice,
            x.DiscountAmount, x.TaxRate, x.Mandatory, x.PriceSource, x.ManualOverrideReason)).ToArray();
        Result<Guid> result = await mediator.Send(new CreateCommercialOfferVariantCommand(
            tenant.OrganizationId.Value, new CommercialOfferId(offerId), request.TrainingCode,
            request.ValidUntilUtc, request.EstimatedFundingAmount, request.FinancingNotes,
            request.Conditions, request.InternalNotes, lines), cancellationToken);
        return result.IsFailure ? result.Error.ToHttpResult(context) :
            Results.Created($"/api/crm/offers/{result.Value}", new { offerId = result.Value });
    }

    private static async Task<IResult> SubmitForReviewAsync(Guid offerId, IMediator mediator,
        ICurrentTenant tenant, HttpContext context, CancellationToken cancellationToken)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(context);
        Result result = await mediator.Send(new SubmitCommercialOfferForReviewCommand(
            tenant.OrganizationId.Value, new CommercialOfferId(offerId)), cancellationToken);
        return result.IsFailure ? result.Error.ToHttpResult(context) : Results.NoContent();
    }

    private static async Task<IResult> ApproveAsync(Guid offerId, IMediator mediator,
        ICurrentTenant tenant, HttpContext context, CancellationToken cancellationToken)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(context);
        Result result = await mediator.Send(new ApproveCommercialOfferCommand(
            tenant.OrganizationId.Value, new CommercialOfferId(offerId)), cancellationToken);
        return result.IsFailure ? result.Error.ToHttpResult(context) : Results.NoContent();
    }

    private static async Task<IResult> SendAsync(Guid offerId, SendCommercialOfferRequest request,
        IMediator mediator, ICurrentTenant tenant, HttpContext context, CancellationToken cancellationToken)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(context);
        OfferRecipientDraft[] recipients = request.Recipients.Select(x =>
            new OfferRecipientDraft(x.Type, x.DisplayName, x.Address)).ToArray();
        Result<SendCommercialOfferResponse> result = await mediator.Send(new SendCommercialOfferCommand(
            tenant.OrganizationId.Value, new CommercialOfferId(offerId), request.Channel,
            recipients, request.Subject, request.Message, request.Language,
            request.DocumentReference, request.AttachmentReferences,
            request.SecureLinkLifetimeHours), cancellationToken);
        return result.IsFailure ? result.Error.ToHttpResult(context) : Results.Ok(result.Value);
    }

    private static async Task<IResult> RecordViewAsync(string token, IMediator mediator,
        HttpContext context, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token) || token.Length > 512)
            return CommercialOfferErrors.SecureLinkExpired.ToHttpResult(context);
        string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();
        Result result = await mediator.Send(new RecordCommercialOfferViewCommand(hash), cancellationToken);
        return result.IsFailure ? result.Error.ToHttpResult(context) : Results.NoContent();
    }

    private static async Task<IResult> RecordExchangeAsync(Guid offerId, RecordOfferExchangeRequest request,
        IMediator mediator, ICurrentTenant tenant, HttpContext context, CancellationToken cancellationToken)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(context);
        Result result = await mediator.Send(new RecordCommercialOfferExchangeCommand(tenant.OrganizationId.Value,
            new CommercialOfferId(offerId), request.Type, request.Summary, request.MetadataJson), cancellationToken);
        return result.IsFailure ? result.Error.ToHttpResult(context) : Results.NoContent();
    }

    private static async Task<IResult> ScheduleFollowUpAsync(Guid offerId, ScheduleOfferFollowUpRequest request,
        IMediator mediator, ICurrentTenant tenant, HttpContext context, CancellationToken cancellationToken)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(context);
        Result result = await mediator.Send(new ScheduleCommercialOfferFollowUpCommand(tenant.OrganizationId.Value,
            new CommercialOfferId(offerId), request.NextFollowUpAtUtc, request.Note), cancellationToken);
        return result.IsFailure ? result.Error.ToHttpResult(context) : Results.NoContent();
    }

    private static async Task<IResult> WithdrawAsync(Guid offerId, OfferDecisionReasonRequest request,
        IMediator mediator, ICurrentTenant tenant, HttpContext context, CancellationToken cancellationToken)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(context);
        Result result = await mediator.Send(new WithdrawCommercialOfferCommand(tenant.OrganizationId.Value,
            new CommercialOfferId(offerId), request.Reason), cancellationToken);
        return result.IsFailure ? result.Error.ToHttpResult(context) : Results.NoContent();
    }

    private static async Task<IResult> MarkAcceptedAsync(Guid offerId, IMediator mediator,
        ICurrentTenant tenant, HttpContext context, CancellationToken cancellationToken)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(context);
        Result result = await mediator.Send(new MarkCommercialOfferAcceptedCommand(tenant.OrganizationId.Value,
            new CommercialOfferId(offerId)), cancellationToken);
        return result.IsFailure ? result.Error.ToHttpResult(context) : Results.NoContent();
    }

    private static async Task<IResult> MarkRejectedAsync(Guid offerId, OfferDecisionReasonRequest request,
        IMediator mediator, ICurrentTenant tenant, HttpContext context, CancellationToken cancellationToken)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(context);
        Result result = await mediator.Send(new MarkCommercialOfferRejectedCommand(tenant.OrganizationId.Value,
            new CommercialOfferId(offerId), request.Reason), cancellationToken);
        return result.IsFailure ? result.Error.ToHttpResult(context) : Results.NoContent();
    }
}

public sealed record RecordOfferExchangeRequest(OfferInteractionType Type, string Summary, string? MetadataJson);
public sealed record ScheduleOfferFollowUpRequest(DateTimeOffset NextFollowUpAtUtc, string? Note);
public sealed record OfferDecisionReasonRequest(string Reason);

public sealed record SendCommercialOfferRequest(OfferDeliveryChannel Channel,
    IReadOnlyCollection<OfferRecipientRequest> Recipients, string Subject, string Message,
    string Language, string DocumentReference, IReadOnlyCollection<string> AttachmentReferences,
    int SecureLinkLifetimeHours);

public sealed record OfferRecipientRequest(OfferRecipientType Type, string DisplayName, string Address);

public sealed record CreateCommercialOfferVariantRequest(string TrainingCode,
    DateTimeOffset ValidUntilUtc, decimal EstimatedFundingAmount, string? FinancingNotes,
    string? Conditions, string? InternalNotes, IReadOnlyCollection<CommercialOfferLineRequest> Lines);

public sealed record GenerateCommercialOfferRequest(Guid AssessmentSessionId, Guid? BranchId,
    string TrainingCode, string Currency, DateTimeOffset ValidUntilUtc,
    decimal EstimatedFundingAmount, string? FinancingNotes, string? Conditions,
    string? InternalNotes, IReadOnlyCollection<CommercialOfferLineRequest> Lines);

public sealed record CommercialOfferLineRequest(OfferLineType Type, Guid? ServiceId,
    string Description, decimal Quantity, string Unit, decimal UnitPrice,
    decimal DiscountAmount, decimal TaxRate, bool Mandatory,
    OfferPriceSource PriceSource, string? ManualOverrideReason);
