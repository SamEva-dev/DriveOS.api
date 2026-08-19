using DriveOS.Modules.CurriculumPedagogy.Domain.Curricula;
using DriveOS.Modules.CurriculumPedagogy.Domain.Curricula.Events;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.CurriculumPedagogy.Curricula;

public sealed class CurriculumTests
{
    private static readonly OrganizationId OrganizationId = new(Guid.NewGuid());
    private static readonly UserId ActorUserId = new(Guid.NewGuid());

    [Fact]
    public void Create_WithValidData_CreatesDraftCurriculumAndNormalizesCodes()
    {
        var result = Curriculum.Create(
            CurriculumId.New(),
            OrganizationId,
            " fr-b-permis ",
            " Référentiel permis B ",
            " Référentiel national de formation. ",
            " fr ",
            " b ");

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(CurriculumStatus.Draft);
        result.Value.Code.Should().Be("FR-B-PERMIS");
        result.Value.Name.Should().Be("Référentiel permis B");
        result.Value.Description.Should().Be("Référentiel national de formation.");
        result.Value.CountryCode.Should().Be("FR");
        result.Value.LicenseCategoryCode.Should().Be("B");
        result.Value.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<CurriculumCreatedDomainEvent>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("F")]
    [InlineData("FRA")]
    [InlineData("F1")]
    public void Create_WithInvalidCountryCode_Fails(string countryCode)
    {
        var result = Curriculum.Create(
            CurriculumId.New(),
            OrganizationId,
            "FR-B",
            "Référentiel permis B",
            null,
            countryCode,
            "B");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CurriculumErrors.InvalidCountryCode);
    }

    [Fact]
    public void UpdateMetadata_WhileDraft_UpdatesNameAndDescription()
    {
        Curriculum curriculum = CreateCurriculum();

        var result = curriculum.UpdateMetadata(
            " Référentiel B modernisé ",
            " Nouvelle description. ");

        result.IsSuccess.Should().BeTrue();
        curriculum.Name.Should().Be("Référentiel B modernisé");
        curriculum.Description.Should().Be("Nouvelle description.");
        curriculum.DomainEvents.Should().Contain(e => e is CurriculumMetadataUpdatedDomainEvent);
    }

    [Fact]
    public void Archive_MakesCurriculumImmutableForMetadataChanges()
    {
        Curriculum curriculum = CreateCurriculum();
        DateTimeOffset now = DateTimeOffset.UtcNow;

        curriculum.Archive(ActorUserId, now).IsSuccess.Should().BeTrue();

        curriculum.Status.Should().Be(CurriculumStatus.Archived);
        curriculum.ArchivedAtUtc.Should().Be(now);
        curriculum.ArchivedByUserId.Should().Be(ActorUserId);
        curriculum.DomainEvents.Should().Contain(e => e is CurriculumArchivedDomainEvent);

        var updateResult = curriculum.UpdateMetadata("Autre nom", null);
        updateResult.IsFailure.Should().BeTrue();
        updateResult.Error.Should().Be(CurriculumErrors.ModificationNotAllowed);
    }

    [Fact]
    public void Create_WithEmptyOrganization_Fails()
    {
        var result = Curriculum.Create(
            CurriculumId.New(),
            OrganizationId.Empty,
            "FR-B",
            "Référentiel permis B",
            null,
            "FR",
            "B");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CurriculumErrors.InvalidOrganization);
    }

    [Fact]
    public void CreateVersion_FirstVersion_CreatesImmutableSnapshotWithNumberOne()
    {
        Curriculum curriculum = CreateCurriculum();
        DateTimeOffset now = DateTimeOffset.UtcNow;

        var result = curriculum.CreateVersion(
            CurriculumVersionId.New(),
            new DateOnly(2026, 9, 1),
            null,
            "Version initiale",
            ActorUserId,
            now);

        result.IsSuccess.Should().BeTrue();
        result.Value.VersionNumber.Should().Be(1);
        result.Value.SourceVersionId.Should().BeNull();
        result.Value.NameSnapshot.Should().Be("Référentiel permis B");
        result.Value.CountryCodeSnapshot.Should().Be("FR");
        result.Value.LicenseCategoryCodeSnapshot.Should().Be("B");
        result.Value.Status.Should().Be(CurriculumVersionStatus.Draft);
        curriculum.LatestVersionNumber.Should().Be(1);
        curriculum.DomainEvents.Should().Contain(e => e is CurriculumVersionCreatedDomainEvent);
    }

    [Fact]
    public void CreateVersion_SecondVersion_PreservesLineageAndSequentialNumber()
    {
        Curriculum curriculum = CreateCurriculum();
        DateTimeOffset now = DateTimeOffset.UtcNow;

        CurriculumVersion first = curriculum.CreateVersion(
            CurriculumVersionId.New(),
            new DateOnly(2026, 9, 1),
            new DateOnly(2027, 8, 31),
            "V1",
            ActorUserId,
            now).Value;

        curriculum.UpdateMetadata("Référentiel permis B 2027", "Révision réglementaire");

        CurriculumVersion second = curriculum.CreateVersion(
            CurriculumVersionId.New(),
            new DateOnly(2027, 9, 1),
            null,
            "V2",
            ActorUserId,
            now.AddDays(1)).Value;

        second.VersionNumber.Should().Be(2);
        second.SourceVersionId.Should().Be(first.Id);
        second.NameSnapshot.Should().Be("Référentiel permis B 2027");

        // The historical snapshot is never rewritten by later curriculum metadata changes.
        first.NameSnapshot.Should().Be("Référentiel permis B");
        curriculum.Versions.Should().HaveCount(2);
    }

    [Fact]
    public void CreateVersion_WithInvalidEffectivePeriod_FailsWithoutAddingVersion()
    {
        Curriculum curriculum = CreateCurriculum();

        var result = curriculum.CreateVersion(
            CurriculumVersionId.New(),
            new DateOnly(2027, 9, 1),
            new DateOnly(2027, 8, 31),
            null,
            ActorUserId,
            DateTimeOffset.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CurriculumErrors.VersionEffectivePeriodInvalid);
        curriculum.Versions.Should().BeEmpty();
    }

    [Fact]
    public void CreateVersion_WhenCurriculumArchived_IsRejected()
    {
        Curriculum curriculum = CreateCurriculum();
        curriculum.Archive(ActorUserId, DateTimeOffset.UtcNow);

        var result = curriculum.CreateVersion(
            CurriculumVersionId.New(),
            DateOnly.FromDateTime(DateTime.UtcNow),
            null,
            null,
            ActorUserId,
            DateTimeOffset.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CurriculumErrors.VersionCreationNotAllowed);
    }


    [Fact]
    public void AddModule_ToDraftVersion_AddsOrderedModuleAndRaisesEvent()
    {
        Curriculum curriculum = CreateCurriculum();
        CurriculumVersion version = CreateVersion(curriculum);
        CurriculumModuleId moduleId = CurriculumModuleId.New();

        var result = curriculum.AddModule(
            version.Id,
            moduleId,
            " module-1 ",
            "Maîtriser le véhicule",
            "Installation et commandes",
            1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Code.Should().Be("MODULE-1");
        result.Value.Order.Should().Be(1);
        version.Modules.Should().ContainSingle();
        curriculum.DomainEvents.Should().Contain(e => e is CurriculumModuleAddedDomainEvent);
    }

    [Fact]
    public void AddModule_WithDuplicateCodeOrOrder_IsRejected()
    {
        Curriculum curriculum = CreateCurriculum();
        CurriculumVersion version = CreateVersion(curriculum);

        curriculum.AddModule(versionId: version.Id, moduleId: CurriculumModuleId.New(), code: "M1", name: "Module 1", description: null, order: 1);

        var duplicateCode = curriculum.AddModule(version.Id, CurriculumModuleId.New(), "m1", "Module 2", null, 2);
        var duplicateOrder = curriculum.AddModule(version.Id, CurriculumModuleId.New(), "M2", "Module 2", null, 1);

        duplicateCode.IsFailure.Should().BeTrue();
        duplicateCode.Error.Should().Be(CurriculumErrors.ModuleCodeAlreadyExists);
        duplicateOrder.IsFailure.Should().BeTrue();
        duplicateOrder.Error.Should().Be(CurriculumErrors.ModuleOrderAlreadyExists);
    }

    [Fact]
    public void AddCompetency_ToModule_AddsObjectiveAndRaisesEvent()
    {
        Curriculum curriculum = CreateCurriculum();
        CurriculumVersion version = CreateVersion(curriculum);
        CurriculumModule module = curriculum.AddModule(version.Id, CurriculumModuleId.New(), "M1", "Module 1", null, 1).Value;

        var result = curriculum.AddCompetency(
            version.Id,
            module.Id,
            CompetencyId.New(),
            "c1",
            "S'installer au poste de conduite",
            null,
            "Être capable de s'installer de façon autonome et sécurisée.",
            1,
            true);

        result.IsSuccess.Should().BeTrue();
        result.Value.Code.Should().Be("C1");
        result.Value.IsRequired.Should().BeTrue();
        module.Competencies.Should().ContainSingle();
        curriculum.DomainEvents.Should().Contain(e => e is CompetencyAddedDomainEvent);
    }

    [Fact]
    public void AddCompetency_WithDuplicateCodeOrOrder_IsRejectedInsideSameModule()
    {
        Curriculum curriculum = CreateCurriculum();
        CurriculumVersion version = CreateVersion(curriculum);
        CurriculumModule module = curriculum.AddModule(version.Id, CurriculumModuleId.New(), "M1", "Module 1", null, 1).Value;

        curriculum.AddCompetency(version.Id, module.Id, CompetencyId.New(), "C1", "Compétence 1", null, "Objectif pédagogique 1", 1);

        var duplicateCode = curriculum.AddCompetency(version.Id, module.Id, CompetencyId.New(), "c1", "Compétence 2", null, "Objectif pédagogique 2", 2);
        var duplicateOrder = curriculum.AddCompetency(version.Id, module.Id, CompetencyId.New(), "C2", "Compétence 2", null, "Objectif pédagogique 2", 1);

        duplicateCode.IsFailure.Should().BeTrue();
        duplicateCode.Error.Should().Be(CurriculumErrors.CompetencyCodeAlreadyExists);
        duplicateOrder.IsFailure.Should().BeTrue();
        duplicateOrder.Error.Should().Be(CurriculumErrors.CompetencyOrderAlreadyExists);
    }

    [Fact]
    public void RemoveModule_WhenItContainsCompetencies_IsRejected()
    {
        Curriculum curriculum = CreateCurriculum();
        CurriculumVersion version = CreateVersion(curriculum);
        CurriculumModule module = curriculum.AddModule(version.Id, CurriculumModuleId.New(), "M1", "Module 1", null, 1).Value;
        curriculum.AddCompetency(version.Id, module.Id, CompetencyId.New(), "C1", "Compétence 1", null, "Objectif pédagogique", 1);

        var result = curriculum.RemoveModule(version.Id, module.Id);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CurriculumErrors.ModuleHasCompetencies);
        version.Modules.Should().ContainSingle();
    }

    private static CurriculumVersion CreateVersion(Curriculum curriculum) => curriculum.CreateVersion(
        CurriculumVersionId.New(),
        new DateOnly(2026, 9, 1),
        null,
        "Version initiale",
        ActorUserId,
        DateTimeOffset.UtcNow).Value;

    private static Curriculum CreateCurriculum() => Curriculum.Create(
        CurriculumId.New(),
        OrganizationId,
        "FR-B",
        "Référentiel permis B",
        null,
        "FR",
        "B").Value;
}
