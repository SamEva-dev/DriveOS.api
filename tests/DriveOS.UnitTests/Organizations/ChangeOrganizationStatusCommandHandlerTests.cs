using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Organizations.Application.Abstractions;
using DriveOS.Modules.Organizations.Application.Organizations.Lifecycle;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.UnitTests.Organizations;

public sealed class ChangeOrganizationStatusCommandHandlerTests
{
    private static readonly OrganizationId OrganizationId =
        new(
            Guid.Parse(
                "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));

    private static readonly UserId CurrentUserId =
        new(
            Guid.Parse(
                "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));

    private static readonly DateTimeOffset UtcNow =
        new(
            2026,
            7,
            29,
            8,
            30,
            0,
            TimeSpan.Zero);

    [Fact]
    public async Task Handle_WhenDraftIsSubmittedForActivation_ShouldSucceed()
    {
        // Arrange
        Organization organization =
            OrganizationTestData.CreateDraft(
                OrganizationId);

        var repository =
            new FakeOrganizationRepository
            {
                Organization = organization
            };

        var unitOfWork =
            new FakeUnitOfWork();

        var currentUser =
            FakeCurrentUser.Authenticated(
                CurrentUserId);

        var clock =
            new FakeClock(UtcNow);

        var handler =
            new ChangeOrganizationStatusCommandHandler(
                repository,
                unitOfWork,
                currentUser,
                clock);

        var command =
            new ChangeOrganizationStatusCommand(
                OrganizationId.Value,
                OrganizationStatus.PendingActivation,
                "Dossier soumis pour vérification.");

        // Act
        var result =
            await handler.Handle(
                command,
                CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(
            OrganizationStatus.PendingActivation,
            organization.Status);

        Assert.Equal(
            1,
            unitOfWork.SaveChangesCallCount);

        Assert.Equal(
            QueryTracking.Tracking,
            repository.LastTrackingMode);

        Assert.Equal(
            OrganizationId,
            repository.LastRequestedOrganizationId);

        OrganizationStatusHistoryEntry historyEntry =
            Assert.Single(
                organization.StatusHistory);

        Assert.Equal(
            OrganizationStatus.Draft,
            historyEntry.PreviousStatus);

        Assert.Equal(
            OrganizationStatus.PendingActivation,
            historyEntry.NewStatus);

        Assert.Equal(
            CurrentUserId.Value,
            historyEntry.ChangedByUserId);

        Assert.Equal(
            UtcNow,
            historyEntry.ChangedAtUtc);

        Assert.Equal(
            "Dossier soumis pour vérification.",
            historyEntry.Reason.Value);
    }

    [Fact]
    public async Task Handle_WhenPendingOrganizationIsActivated_ShouldSucceed()
    {
        // Arrange
        Organization organization =
            OrganizationTestData.CreatePendingActivation(
                OrganizationId);

        int initialHistoryCount =
            organization.StatusHistory.Count;

        var repository =
            new FakeOrganizationRepository
            {
                Organization = organization
            };

        var unitOfWork =
            new FakeUnitOfWork();

        var currentUser =
            FakeCurrentUser.Authenticated(
                CurrentUserId);

        var clock =
            new FakeClock(UtcNow);

        var handler =
            new ChangeOrganizationStatusCommandHandler(
                repository,
                unitOfWork,
                currentUser,
                clock);

        var command =
            new ChangeOrganizationStatusCommand(
                OrganizationId.Value,
                OrganizationStatus.Active,
                "Contrôles administratifs validés.");

        // Act
        var result =
            await handler.Handle(
                command,
                CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(
            OrganizationStatus.Active,
            organization.Status);

        Assert.Equal(
            initialHistoryCount + 1,
            organization.StatusHistory.Count);

        OrganizationStatusHistoryEntry lastEntry =
            organization.StatusHistory.Last();

        Assert.Equal(
            OrganizationStatus.PendingActivation,
            lastEntry.PreviousStatus);

        Assert.Equal(
            OrganizationStatus.Active,
            lastEntry.NewStatus);

        Assert.Equal(
            CurrentUserId.Value,
            lastEntry.ChangedByUserId);

        Assert.Equal(
            UtcNow,
            lastEntry.ChangedAtUtc);

        Assert.Equal(
            1,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenActiveOrganizationIsRestricted_ShouldSucceed()
    {
        // Arrange
        Organization organization =
            OrganizationTestData.CreateActive(
                OrganizationId);

        var repository =
            new FakeOrganizationRepository
            {
                Organization = organization
            };

        var unitOfWork =
            new FakeUnitOfWork();

        var handler =
            CreateHandler(
                repository,
                unitOfWork);

        var command =
            new ChangeOrganizationStatusCommand(
                OrganizationId.Value,
                OrganizationStatus.Restricted,
                "Document réglementaire bientôt expiré.");

        // Act
        var result =
            await handler.Handle(
                command,
                CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(
            OrganizationStatus.Restricted,
            organization.Status);

        OrganizationStatusHistoryEntry lastEntry =
            organization.StatusHistory.Last();

        Assert.Equal(
            OrganizationStatus.Active,
            lastEntry.PreviousStatus);

        Assert.Equal(
            OrganizationStatus.Restricted,
            lastEntry.NewStatus);

        Assert.Equal(
            1,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenRestrictedOrganizationIsSuspended_ShouldSucceed()
    {
        // Arrange
        Organization organization =
            OrganizationTestData.CreateRestricted(
                OrganizationId);

        var repository =
            new FakeOrganizationRepository
            {
                Organization = organization
            };

        var unitOfWork =
            new FakeUnitOfWork();

        var handler =
            CreateHandler(
                repository,
                unitOfWork);

        var command =
            new ChangeOrganizationStatusCommand(
                OrganizationId.Value,
                OrganizationStatus.Suspended,
                "Non-conformité majeure non corrigée.");

        // Act
        var result =
            await handler.Handle(
                command,
                CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(
            OrganizationStatus.Suspended,
            organization.Status);

        OrganizationStatusHistoryEntry lastEntry =
            organization.StatusHistory.Last();

        Assert.Equal(
            OrganizationStatus.Restricted,
            lastEntry.PreviousStatus);

        Assert.Equal(
            OrganizationStatus.Suspended,
            lastEntry.NewStatus);

        Assert.Equal(
            1,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenSuspendedOrganizationIsReactivated_ShouldSucceed()
    {
        // Arrange
        Organization organization =
            OrganizationTestData.CreateSuspended(
                OrganizationId);

        var repository =
            new FakeOrganizationRepository
            {
                Organization = organization
            };

        var unitOfWork =
            new FakeUnitOfWork();

        var handler =
            CreateHandler(
                repository,
                unitOfWork);

        var command =
            new ChangeOrganizationStatusCommand(
                OrganizationId.Value,
                OrganizationStatus.Active,
                "Les conditions de réactivation sont remplies.");

        // Act
        var result =
            await handler.Handle(
                command,
                CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(
            OrganizationStatus.Active,
            organization.Status);

        OrganizationStatusHistoryEntry lastEntry =
            organization.StatusHistory.Last();

        Assert.Equal(
            OrganizationStatus.Suspended,
            lastEntry.PreviousStatus);

        Assert.Equal(
            OrganizationStatus.Active,
            lastEntry.NewStatus);

        Assert.Equal(
            1,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenActiveOrganizationIsClosed_ShouldSucceed()
    {
        // Arrange
        Organization organization =
            OrganizationTestData.CreateActive(
                OrganizationId);

        var repository =
            new FakeOrganizationRepository
            {
                Organization = organization
            };

        var unitOfWork =
            new FakeUnitOfWork();

        var handler =
            CreateHandler(
                repository,
                unitOfWork);

        var command =
            new ChangeOrganizationStatusCommand(
                OrganizationId.Value,
                OrganizationStatus.Closed,
                "Cessation définitive de l'activité.");

        // Act
        var result =
            await handler.Handle(
                command,
                CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(
            OrganizationStatus.Closed,
            organization.Status);

        OrganizationStatusHistoryEntry lastEntry =
            organization.StatusHistory.Last();

        Assert.Equal(
            OrganizationStatus.Active,
            lastEntry.PreviousStatus);

        Assert.Equal(
            OrganizationStatus.Closed,
            lastEntry.NewStatus);

        Assert.Equal(
            1,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenOrganizationDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var repository =
            new FakeOrganizationRepository
            {
                Organization = null
            };

        var unitOfWork =
            new FakeUnitOfWork();

        var handler =
            CreateHandler(
                repository,
                unitOfWork);

        var command =
            new ChangeOrganizationStatusCommand(
                OrganizationId.Value,
                OrganizationStatus.Active,
                "Activation demandée.");

        // Act
        var result =
            await handler.Handle(
                command,
                CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(
            "Organizations.NotFound",
            result.Error.Code);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCallCount);

        Assert.Equal(
            QueryTracking.Tracking,
            repository.LastTrackingMode);
    }

    [Fact]
    public async Task Handle_WhenTransitionIsInvalid_ShouldReturnConflict()
    {
        // Arrange
        Organization organization =
            OrganizationTestData.CreateDraft(
                OrganizationId);

        var repository =
            new FakeOrganizationRepository
            {
                Organization = organization
            };

        var unitOfWork =
            new FakeUnitOfWork();

        var handler =
            CreateHandler(
                repository,
                unitOfWork);

        var command =
            new ChangeOrganizationStatusCommand(
                OrganizationId.Value,
                OrganizationStatus.Suspended,
                "Suspension demandée.");

        // Act
        var result =
            await handler.Handle(
                command,
                CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(
            "Organizations.InvalidStatusTransition",
            result.Error.Code);

        Assert.Equal(
            OrganizationStatus.Draft,
            organization.Status);

        Assert.Empty(
            organization.StatusHistory);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenOrganizationIsClosed_ShouldRejectReactivation()
    {
        // Arrange
        Organization organization =
            OrganizationTestData.CreateClosed(
                OrganizationId);

        int initialHistoryCount =
            organization.StatusHistory.Count;

        var repository =
            new FakeOrganizationRepository
            {
                Organization = organization
            };

        var unitOfWork =
            new FakeUnitOfWork();

        var handler =
            CreateHandler(
                repository,
                unitOfWork);

        var command =
            new ChangeOrganizationStatusCommand(
                OrganizationId.Value,
                OrganizationStatus.Active,
                "Réactivation demandée.");

        // Act
        var result =
            await handler.Handle(
                command,
                CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(
            "Organizations.InvalidStatusTransition",
            result.Error.Code);

        Assert.Equal(
            OrganizationStatus.Closed,
            organization.Status);

        Assert.Equal(
            initialHistoryCount,
            organization.StatusHistory.Count);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_ShouldUseCurrentAuthenticatedUser()
    {
        // Arrange
        Organization organization =
            OrganizationTestData.CreateDraft(
                OrganizationId);

        UserId expectedUserId =
            new(
                Guid.Parse(
                    "cccccccc-cccc-cccc-cccc-cccccccccccc"));

        var repository =
            new FakeOrganizationRepository
            {
                Organization = organization
            };

        var unitOfWork =
            new FakeUnitOfWork();

        var currentUser =
            FakeCurrentUser.Authenticated(
                expectedUserId);

        var clock =
            new FakeClock(UtcNow);

        var handler =
            new ChangeOrganizationStatusCommandHandler(
                repository,
                unitOfWork,
                currentUser,
                clock);

        var command =
            new ChangeOrganizationStatusCommand(
                OrganizationId.Value,
                OrganizationStatus.PendingActivation,
                "Soumission pour activation.");

        // Act
        var result =
            await handler.Handle(
                command,
                CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        OrganizationStatusHistoryEntry entry =
            Assert.Single(
                organization.StatusHistory);

        Assert.Equal(
            expectedUserId.Value,
            entry.ChangedByUserId);
    }

    [Fact]
    public async Task Handle_ShouldUseInjectedClock()
    {
        // Arrange
        Organization organization =
            OrganizationTestData.CreateDraft(
                OrganizationId);

        DateTimeOffset expectedDate =
            new(
                2030,
                1,
                15,
                14,
                45,
                0,
                TimeSpan.Zero);

        var repository =
            new FakeOrganizationRepository
            {
                Organization = organization
            };

        var unitOfWork =
            new FakeUnitOfWork();

        var currentUser =
            FakeCurrentUser.Authenticated(
                CurrentUserId);

        var clock =
            new FakeClock(expectedDate);

        var handler =
            new ChangeOrganizationStatusCommandHandler(
                repository,
                unitOfWork,
                currentUser,
                clock);

        var command =
            new ChangeOrganizationStatusCommand(
                OrganizationId.Value,
                OrganizationStatus.PendingActivation,
                "Soumission pour activation.");

        // Act
        var result =
            await handler.Handle(
                command,
                CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        OrganizationStatusHistoryEntry entry =
            Assert.Single(
                organization.StatusHistory);

        Assert.Equal(
            expectedDate,
            entry.ChangedAtUtc);
    }

    [Fact]
    public async Task Handle_WhenCurrentUserIsNotAuthenticated_ShouldFail()
    {
        // Arrange
        Organization organization =
            OrganizationTestData.CreateDraft(
                OrganizationId);

        var repository =
            new FakeOrganizationRepository
            {
                Organization = organization
            };

        var unitOfWork =
            new FakeUnitOfWork();

        var currentUser =
            FakeCurrentUser.Anonymous();

        var clock =
            new FakeClock(UtcNow);

        var handler =
            new ChangeOrganizationStatusCommandHandler(
                repository,
                unitOfWork,
                currentUser,
                clock);

        var command =
            new ChangeOrganizationStatusCommand(
                OrganizationId.Value,
                OrganizationStatus.PendingActivation,
                "Soumission pour activation.");

        // Act
        var result =
            await handler.Handle(
                command,
                CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(
            "Organizations.CurrentUser.Required",
            result.Error.Code);

        Assert.Equal(
            OrganizationStatus.Draft,
            organization.Status);

        Assert.Empty(
            organization.StatusHistory);

        Assert.Equal(
            0,
            unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_ShouldForwardCancellationTokenToRepositoryAndUnitOfWork()
    {
        // Arrange
        Organization organization =
            OrganizationTestData.CreateDraft(
                OrganizationId);

        var repository =
            new FakeOrganizationRepository
            {
                Organization = organization
            };

        var unitOfWork =
            new FakeUnitOfWork();

        var handler =
            CreateHandler(
                repository,
                unitOfWork);

        using var cancellationTokenSource =
            new CancellationTokenSource();

        CancellationToken cancellationToken =
            cancellationTokenSource.Token;

        var command =
            new ChangeOrganizationStatusCommand(
                OrganizationId.Value,
                OrganizationStatus.PendingActivation,
                "Soumission pour activation.");

        // Act
        var result =
            await handler.Handle(
                command,
                cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(
            cancellationToken,
            repository.LastCancellationToken);

        Assert.Equal(
            cancellationToken,
            unitOfWork.LastCancellationToken);
    }

    private static ChangeOrganizationStatusCommandHandler
        CreateHandler(
            FakeOrganizationRepository repository,
            FakeUnitOfWork unitOfWork)
    {
        return new ChangeOrganizationStatusCommandHandler(
            repository,
            unitOfWork,
            FakeCurrentUser.Authenticated(
                CurrentUserId),
            new FakeClock(UtcNow));
    }

    private sealed class FakeOrganizationRepository
        : IOrganizationRepository
    {
        public Organization? Organization { get; init; }

        public OrganizationId? LastRequestedOrganizationId
        {
            get;
            private set;
        }

        public QueryTracking? LastTrackingMode
        {
            get;
            private set;
        }

        public CancellationToken LastCancellationToken
        {
            get;
            private set;
        }

        public Task<bool> ExistsByLegalNameAsync(
            string legalName,
            string countryCode,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        public Task<Organization?> GetByIdAsync(
            OrganizationId organizationId,
            QueryTracking tracking = QueryTracking.NoTracking,
            CancellationToken cancellationToken = default)
        {
            LastRequestedOrganizationId =
                organizationId;

            LastTrackingMode =
                tracking;

            LastCancellationToken =
                cancellationToken;

            if (
                Organization is null ||
                Organization.Id != organizationId)
            {
                return Task.FromResult<Organization?>(null);
            }

            return Task.FromResult<Organization?>(
                Organization);
        }

        public void Add(
            Organization organization)
        {
            throw new NotSupportedException(
                "Add is not used by lifecycle tests.");
        }
    }

    private sealed class FakeUnitOfWork
        : IUnitOfWork
    {
        public int SaveChangesCallCount
        {
            get;
            private set;
        }

        public CancellationToken LastCancellationToken
        {
            get;
            private set;
        }

        public Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;

            LastCancellationToken =
                cancellationToken;

            return Task.FromResult(1);
        }
    }

    private sealed class FakeClock
        : IClock
    {
        public FakeClock(
            DateTimeOffset utcNow)
        {
            UtcNow = utcNow;
        }

        public DateTimeOffset UtcNow
        {
            get;
        }
    }

    private sealed class FakeCurrentUser
        : ICurrentUser
    {
        private static readonly IReadOnlySet<string>
            EmptyPermissions =
                new HashSet<string>(
                    StringComparer.Ordinal);

        private FakeCurrentUser(
            bool isAuthenticated,
            UserId? userId,
            string? email)
        {
            IsAuthenticated =
                isAuthenticated;

            UserId =
                userId;

            Email =
                email;
        }

        public bool IsAuthenticated
        {
            get;
        }

        public UserId? UserId
        {
            get;
        }

        public string? Email
        {
            get;
        }

        public IReadOnlySet<string> Permissions =>
            EmptyPermissions;

        public bool HasPermission(
            string permission)
        {
            return Permissions.Contains(
                permission);
        }

        public static FakeCurrentUser Authenticated(
            UserId userId)
        {
            return new FakeCurrentUser(
                true,
                userId,
                "admin@driveos.test");
        }

        public static FakeCurrentUser Anonymous()
        {
            return new FakeCurrentUser(
                false,
                null,
                null);
        }
    }
}