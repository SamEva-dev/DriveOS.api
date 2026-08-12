using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.CRM.Application.Abstractions.Persistence;
using DriveOS.Modules.CRM.Domain.Offers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Application.Offers.SendOffer;

internal sealed class SendCommercialOfferCommandHandler(
    ICommercialOfferRepository offers,
    ICrmUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<SendCommercialOfferCommand, SendCommercialOfferResponse>
{
    public async Task<Result<SendCommercialOfferResponse>> Handle(
        SendCommercialOfferCommand command, CancellationToken cancellationToken)
    {
        CommercialOffer? offer = await offers.GetForUpdateAsync(
            command.OrganizationId, command.OfferId, cancellationToken);
        if (offer is null)
            return Result.Failure<SendCommercialOfferResponse>(CommercialOfferErrors.NotFound);

        if (command.Recipients.Count == 0)
            return Result.Failure<SendCommercialOfferResponse>(CommercialOfferErrors.RecipientRequired);
        if (command.Recipients.Any(x => string.IsNullOrWhiteSpace(x.DisplayName) ||
            string.IsNullOrWhiteSpace(x.Address) || x.DisplayName.Length > 200 || x.Address.Length > 320))
            return Result.Failure<SendCommercialOfferResponse>(CommercialOfferErrors.InvalidRecipient);

        DateTimeOffset nowUtc = clock.UtcNow;
        string? rawToken = null;
        string? tokenHash = null;
        DateTimeOffset? expiresAtUtc = null;
        if (command.Channel.RequiresSecureLink())
        {
            if (command.SecureLinkLifetimeHours is < 1 or > 720)
                return Result.Failure<SendCommercialOfferResponse>(CommercialOfferErrors.SecureLinkExpired);
            rawToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
            tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken))).ToLowerInvariant();
            expiresAtUtc = nowUtc.AddHours(command.SecureLinkLifetimeHours);
        }

        string recipientsJson = JsonSerializer.Serialize(command.Recipients);
        string attachmentsJson = JsonSerializer.Serialize(command.AttachmentReferences);
        Result prepared = offer.PrepareDelivery(command.Channel, recipientsJson,
            command.Subject, command.Message, command.Language, command.DocumentReference,
            attachmentsJson, tokenHash, expiresAtUtc, nowUtc);
        if (prepared.IsFailure)
            return Result.Failure<SendCommercialOfferResponse>(prepared.Error);

        if (command.Channel == OfferDeliveryChannel.Printed)
        {
            Result sent = offer.MarkSent(nowUtc);
            if (sent.IsFailure) return Result.Failure<SendCommercialOfferResponse>(sent.Error);
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(new SendCommercialOfferResponse(
            offer.Id.Value, offer.Status.ToString(), offer.DeliveryStatus!.Value.ToString(),
            rawToken, expiresAtUtc));
    }
}
