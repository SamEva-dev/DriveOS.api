using DriveOS.Modules.Contracts.Domain.TrainingContracts;
using DriveOS.Modules.Contracts.Domain.TrainingContracts.Events;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.Contracts.TrainingContracts;

public sealed class TrainingContractTests
{
    [Fact]
    public void CreateDraft_WithValidSnapshot_CreatesDraftAndRaisesDomainEvent()
    {
        OrganizationId organizationId = OrganizationId.New();
        PersonId studentId = PersonId.New();

        var result = TrainingContract.CreateDraft(
            TrainingContractId.New(),
            organizationId,
            BranchId.New(),
            studentId,
            CommercialOfferId.New(),
            2,
            "ctr-2026-0001",
            new DateOnly(2026, 9, 1),
            new DateOnly(2027, 9, 1),
            1890m,
            "eur",
            CreateTerms(),
            CreateParties(organizationId, studentId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(TrainingContractStatus.Draft);
        result.Value.ContractNumber.Should().Be("CTR-2026-0001");
        result.Value.Currency.Should().Be("EUR");
        result.Value.TotalAmount.Should().Be(1890m);
        result.Value.Parties.Should().HaveCount(2);
        result.Value.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TrainingContractDraftCreatedDomainEvent>();
    }

    [Fact]
    public void CreateDraft_WithEmptyTypedIdentifier_FailsWithStableErrorKey()
    {
        OrganizationId organizationId = OrganizationId.New();
        PersonId studentId = PersonId.New();

        var result = TrainingContract.CreateDraft(
            TrainingContractId.Empty,
            organizationId,
            BranchId.New(),
            studentId,
            CommercialOfferId.New(),
            1,
            "CTR-1",
            new DateOnly(2026, 9, 1),
            null,
            1000m,
            "EUR",
            CreateTerms(),
            CreateParties(organizationId, studentId));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Contracts.TrainingContract.Id.Invalid");
    }

    [Fact]
    public void CreateDraft_WithEndDateBeforeStartDate_Fails()
    {
        OrganizationId organizationId = OrganizationId.New();
        PersonId studentId = PersonId.New();

        var result = TrainingContract.CreateDraft(
            TrainingContractId.New(),
            organizationId,
            BranchId.New(),
            studentId,
            CommercialOfferId.New(),
            1,
            "CTR-2",
            new DateOnly(2026, 9, 2),
            new DateOnly(2026, 9, 1),
            1000m,
            "EUR",
            CreateTerms(),
            CreateParties(organizationId, studentId));

        result.Error.Should().Be(TrainingContractErrors.InvalidEffectivePeriod);
    }

    [Fact]
    public void CreateDraft_WithoutProviderParty_Fails()
    {
        OrganizationId organizationId = OrganizationId.New();
        PersonId studentId = PersonId.New();
        TrainingContractParty studentParty = TrainingContractParty
            .ForPerson(TrainingContractPartyKind.Student, studentId, "Jean Dupont")
            .Value;

        var result = TrainingContract.CreateDraft(
            TrainingContractId.New(),
            organizationId,
            BranchId.New(),
            studentId,
            CommercialOfferId.New(),
            1,
            "CTR-3",
            new DateOnly(2026, 9, 1),
            null,
            1000m,
            "EUR",
            CreateTerms(),
            [studentParty]);

        result.Error.Should().Be(TrainingContractErrors.ProviderPartyRequired);
    }

    [Fact]
    public void CreateDraft_WithoutStudentParty_Fails()
    {
        OrganizationId organizationId = OrganizationId.New();
        PersonId studentId = PersonId.New();
        TrainingContractParty providerParty = TrainingContractParty
            .ForOrganization(TrainingContractPartyKind.TrainingProvider, organizationId, "Auto-école Horizon")
            .Value;

        var result = TrainingContract.CreateDraft(
            TrainingContractId.New(),
            organizationId,
            BranchId.New(),
            studentId,
            CommercialOfferId.New(),
            1,
            "CTR-4",
            new DateOnly(2026, 9, 1),
            null,
            1000m,
            "EUR",
            CreateTerms(),
            [providerParty]);

        result.Error.Should().Be(TrainingContractErrors.StudentPartyRequired);
    }

    [Fact]
    public void CreateDraft_WithDuplicateParty_Fails()
    {
        OrganizationId organizationId = OrganizationId.New();
        PersonId studentId = PersonId.New();
        TrainingContractParty provider = TrainingContractParty
            .ForOrganization(TrainingContractPartyKind.TrainingProvider, organizationId, "Auto-école Horizon")
            .Value;
        TrainingContractParty student = TrainingContractParty
            .ForPerson(TrainingContractPartyKind.Student, studentId, "Jean Dupont")
            .Value;

        var result = TrainingContract.CreateDraft(
            TrainingContractId.New(),
            organizationId,
            BranchId.New(),
            studentId,
            CommercialOfferId.New(),
            1,
            "CTR-5",
            new DateOnly(2026, 9, 1),
            null,
            1000m,
            "EUR",
            CreateTerms(),
            [provider, student, student]);

        result.Error.Should().Be(TrainingContractErrors.DuplicateParty);
    }

    [Fact]
    public void TermsSnapshot_IsImmutableAndContainsContractualSnapshot()
    {
        TrainingContractTermsSnapshot terms = CreateTerms();

        terms.TrainingCode.Should().Be("B-MANUAL");
        terms.PracticalHours.Should().Be(20m);
        terms.ServicesSnapshot.Should().Contain("driving");
        terms.PaymentScheduleSnapshot.Should().Contain("installments");
    }

    [Fact]
    public void Contract_IsNotSignedAtDraftCreation()
    {
        OrganizationId organizationId = OrganizationId.New();
        PersonId studentId = PersonId.New();

        TrainingContract contract = TrainingContract.CreateDraft(
            TrainingContractId.New(),
            organizationId,
            BranchId.New(),
            studentId,
            CommercialOfferId.New(),
            1,
            "CTR-6",
            new DateOnly(2026, 9, 1),
            null,
            1000m,
            "EUR",
            CreateTerms(),
            CreateParties(organizationId, studentId)).Value;

        contract.IsSignedOrBeyond.Should().BeFalse();
    }


    [Fact]
    public void CreateDraft_CapturesVersionOneSnapshot()
    {
        OrganizationId organizationId = OrganizationId.New();
        PersonId studentId = PersonId.New();

        TrainingContract contract = TrainingContract.CreateDraft(
            TrainingContractId.New(),
            organizationId,
            BranchId.New(),
            studentId,
            CommercialOfferId.New(),
            3,
            "CTR-7",
            new DateOnly(2026, 9, 1),
            null,
            1200m,
            "EUR",
            CreateTerms(),
            CreateParties(organizationId, studentId)).Value;

        contract.CurrentVersionNumber.Should().Be(1);
        contract.Versions.Should().ContainSingle();
        contract.CurrentVersion.SourceOfferVersion.Should().Be(3);
        contract.CurrentVersion.TotalAmount.Should().Be(1200m);
        contract.CurrentVersion.TermsSnapshot.Should().BeSameAs(contract.TermsSnapshot);
    }

    [Fact]
    public void CreateRevision_ProducesImmutableHistoricalVersionAndUpdatesCurrentSnapshot()
    {
        OrganizationId organizationId = OrganizationId.New();
        PersonId studentId = PersonId.New();
        CommercialOfferId offerId = CommercialOfferId.New();

        TrainingContract contract = TrainingContract.CreateDraft(
            TrainingContractId.New(),
            organizationId,
            BranchId.New(),
            studentId,
            offerId,
            1,
            "CTR-8",
            new DateOnly(2026, 9, 1),
            null,
            1200m,
            "EUR",
            CreateTerms(),
            CreateParties(organizationId, studentId)).Value;

        TrainingContractVersion versionOne = contract.CurrentVersion;
        TrainingContractTermsSnapshot revisedTerms = TrainingContractTermsSnapshot.Create(
            "B-MANUAL",
            25m,
            "{\"services\":[\"driving\",\"theory\",\"exam\"]}",
            "{\"installments\":4}",
            "New cancellation terms",
            "New booking rules",
            "Student obligations",
            "Provider obligations",
            "Exam presentation terms",
            "Data processing terms").Value;

        var result = contract.CreateRevision(
            offerId,
            2,
            new DateOnly(2026, 9, 15),
            new DateOnly(2027, 9, 15),
            1500m,
            "eur",
            revisedTerms,
            CreateParties(organizationId, studentId),
            "Accepted offer was revised after changing the practical package.",
            UserId.New(),
            new DateTimeOffset(2026, 8, 17, 12, 0, 0, TimeSpan.Zero));

        result.IsSuccess.Should().BeTrue();
        contract.CurrentVersionNumber.Should().Be(2);
        contract.Versions.Should().HaveCount(2);
        contract.TotalAmount.Should().Be(1500m);
        contract.Currency.Should().Be("EUR");
        contract.SourceOfferVersion.Should().Be(2);
        contract.TermsSnapshot.PracticalHours.Should().Be(25m);

        versionOne.VersionNumber.Should().Be(1);
        versionOne.TotalAmount.Should().Be(1200m);
        versionOne.SourceOfferVersion.Should().Be(1);
        versionOne.TermsSnapshot.PracticalHours.Should().Be(20m);

        contract.DomainEvents.Should().ContainSingle(e => e is TrainingContractVersionCreatedDomainEvent);
    }

    [Fact]
    public void CreateRevision_WithoutReason_FailsAndKeepsCurrentVersionUntouched()
    {
        OrganizationId organizationId = OrganizationId.New();
        PersonId studentId = PersonId.New();
        CommercialOfferId offerId = CommercialOfferId.New();

        TrainingContract contract = TrainingContract.CreateDraft(
            TrainingContractId.New(),
            organizationId,
            BranchId.New(),
            studentId,
            offerId,
            1,
            "CTR-9",
            new DateOnly(2026, 9, 1),
            null,
            1200m,
            "EUR",
            CreateTerms(),
            CreateParties(organizationId, studentId)).Value;

        var result = contract.CreateRevision(
            offerId,
            2,
            new DateOnly(2026, 9, 1),
            null,
            1300m,
            "EUR",
            CreateTerms(),
            CreateParties(organizationId, studentId),
            " ",
            null,
            new DateTimeOffset(2026, 8, 17, 12, 0, 0, TimeSpan.Zero));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TrainingContractErrors.InvalidRevisionReason);
        contract.CurrentVersionNumber.Should().Be(1);
        contract.Versions.Should().ContainSingle();
        contract.TotalAmount.Should().Be(1200m);
    }

    [Fact]
    public void SetCreatedAudit_StampsInitialVersionWithSameAuditMetadata()
    {
        OrganizationId organizationId = OrganizationId.New();
        PersonId studentId = PersonId.New();
        UserId userId = UserId.New();
        DateTimeOffset createdAt = new(2026, 8, 17, 11, 30, 0, TimeSpan.Zero);

        TrainingContract contract = TrainingContract.CreateDraft(
            TrainingContractId.New(),
            organizationId,
            BranchId.New(),
            studentId,
            CommercialOfferId.New(),
            1,
            "CTR-10",
            new DateOnly(2026, 9, 1),
            null,
            1000m,
            "EUR",
            CreateTerms(),
            CreateParties(organizationId, studentId)).Value;

        contract.SetCreatedAudit(createdAt, userId);

        contract.CreatedAtUtc.Should().Be(createdAt);
        contract.CurrentVersion.CreatedAtUtc.Should().Be(createdAt);
        contract.CurrentVersion.CreatedByUserId.Should().Be(userId);
    }

    private static TrainingContractTermsSnapshot CreateTerms() =>
        TrainingContractTermsSnapshot.Create(
            "B-MANUAL",
            20m,
            "{\"services\":[\"driving\",\"theory\"]}",
            "{\"installments\":3}",
            "Cancellation terms",
            "Booking rules",
            "Student obligations",
            "Provider obligations",
            "Exam presentation terms",
            "Data processing terms").Value;

    private static IReadOnlyCollection<TrainingContractParty> CreateParties(
        OrganizationId organizationId,
        PersonId studentId) =>
    [
        TrainingContractParty.ForOrganization(
            TrainingContractPartyKind.TrainingProvider,
            organizationId,
            "Auto-école Horizon").Value,
        TrainingContractParty.ForPerson(
            TrainingContractPartyKind.Student,
            studentId,
            "Jean Dupont").Value
    ];
}
