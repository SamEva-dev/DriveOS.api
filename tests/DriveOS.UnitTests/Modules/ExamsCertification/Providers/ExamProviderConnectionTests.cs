using DriveOS.Modules.ExamsCertification.Domain.Providers;
using DriveOS.Modules.ExamsCertification.Domain.Providers.Connections;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.ExamsCertification.Providers;

public sealed class ExamProviderConnectionTests
{
    [Fact]
    public void Create_ShouldNeverPersistRawCredentials_AndOAuthStartsPending()
    {
        var result = ExamProviderConnection.Create(
            ExamProviderConnectionId.New(), OrganizationId.New(), "rdvpermis", "RdvPermis", "FR",
            ExamPlaceProviderKind.OfficialApi, ExamProviderAuthenticationMode.OAuth2AuthorizationCode,
            "https://example.invalid", "vault://driveos/exams/rdvpermis/tenant", 60, DateTimeOffset.UtcNow);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(ExamProviderConnectionStatus.PendingAuthorization);
        result.Value.CredentialReference.Should().Be("vault://driveos/exams/rdvpermis/tenant");
    }

    [Fact]
    public void Revoke_ShouldRemoveCredentialReference()
    {
        var now = DateTimeOffset.UtcNow;
        var result = ExamProviderConnection.Create(
            ExamProviderConnectionId.New(), OrganizationId.New(), "partner-api", "Partner", "FR",
            ExamPlaceProviderKind.AuthorizedPartnerApi, ExamProviderAuthenticationMode.ApiKey,
            "https://example.invalid", "vault://secret", 30, now);

        result.Value.Revoke(UserId.New(), now.AddMinutes(1));

        result.Value.Status.Should().Be(ExamProviderConnectionStatus.Revoked);
        result.Value.CredentialReference.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldRejectNonHttpsRemoteEndpoint()
    {
        var result = ExamProviderConnection.Create(
            ExamProviderConnectionId.New(), OrganizationId.New(), "provider", "Provider", "FR",
            ExamPlaceProviderKind.OfficialApi, ExamProviderAuthenticationMode.OAuth2AuthorizationCode,
            "http://remote.example.com", null, 60, DateTimeOffset.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Exams.ProviderConnection.InvalidEndpoint");
    }
}
