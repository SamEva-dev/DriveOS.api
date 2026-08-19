using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.CurriculumPedagogy.Application.Competencies;
using DriveOS.Modules.CurriculumPedagogy.Application.Persistence;
using DriveOS.Modules.CurriculumPedagogy.Domain.Competencies;
using DriveOS.Modules.CurriculumPedagogy.Domain.TrainingPaths;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.CurriculumPedagogy.Competencies;

public sealed class RecordCompetencyAssessmentCommandHandlerTests
{
    [Fact]
    public async Task Handle_CreatesRecordAndAssessment_WhenPathIsActiveAndCompetencyBelongsToVersion()
    {
        OrganizationId organizationId = new(Guid.NewGuid());
        TrainingPathId pathId = TrainingPathId.New();
        CurriculumVersionId versionId = CurriculumVersionId.New();
        CompetencyId competencyId = CompetencyId.New();
        UserId assessor = new(Guid.NewGuid());
        DateTimeOffset now = new(2026, 8, 18, 17, 50, 0, TimeSpan.Zero);

        TrainingPath path = TrainingPath.Create(pathId, organizationId, new PersonId(Guid.NewGuid()), versionId,
            TrainingMode.Standard, new DateOnly(2026, 8, 1), null, 20).Value;
        path.MarkReadyForActivation();
        path.Activate(assessor, now);

        var paths = new FakeTrainingPathRepository(path);
        var records = new FakeCompetencyRecordRepository();
        var eligibility = new FakeEligibilityService(new(versionId, competencyId, "B-01", "Installation", true));
        var unitOfWork = new FakeUnitOfWork();
        var handler = new RecordCompetencyAssessmentCommandHandler(paths, records, eligibility, unitOfWork, new FakeClock(now));

        var result = await handler.Handle(new RecordCompetencyAssessmentCommand(
            organizationId, pathId, competencyId, "IN_PROGRESS", assessor, null, "Bonne progression", true, null), default);

        result.IsSuccess.Should().BeTrue();
        records.Record.Should().NotBeNull();
        records.Record!.CurrentLevelCode.Should().Be("IN_PROGRESS");
        records.Record.Assessments.Should().ContainSingle();
        unitOfWork.CommitCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_RejectsAssessment_WhenTrainingPathIsNotActive()
    {
        OrganizationId organizationId = new(Guid.NewGuid());
        TrainingPathId pathId = TrainingPathId.New();
        CurriculumVersionId versionId = CurriculumVersionId.New();
        CompetencyId competencyId = CompetencyId.New();
        UserId assessor = new(Guid.NewGuid());
        DateTimeOffset now = new(2026, 8, 18, 17, 50, 0, TimeSpan.Zero);

        TrainingPath path = TrainingPath.Create(pathId, organizationId, new PersonId(Guid.NewGuid()), versionId,
            TrainingMode.Standard, new DateOnly(2026, 8, 1), null, 20).Value;
        var handler = new RecordCompetencyAssessmentCommandHandler(
            new FakeTrainingPathRepository(path), new FakeCompetencyRecordRepository(),
            new FakeEligibilityService(new(versionId, competencyId, "B-01", "Installation", true)),
            new FakeUnitOfWork(), new FakeClock(now));

        var result = await handler.Handle(new RecordCompetencyAssessmentCommand(
            organizationId, pathId, competencyId, "IN_PROGRESS", assessor, null, null, false, null), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CurriculumPedagogy.CompetencyAssessment.TrainingPath.NotActive");
    }

    private sealed class FakeClock(DateTimeOffset now) : IClock { public DateTimeOffset UtcNow => now; }

    private sealed class FakeTrainingPathRepository(TrainingPath path) : ITrainingPathRepository
    {
        public Task<TrainingPath?> GetByIdAsync(TrainingPathId id, OrganizationId organizationId, CancellationToken cancellationToken = default) => Task.FromResult<TrainingPath?>(path.Id == id && path.OrganizationId == organizationId ? path : null);
        public Task<TrainingPath?> GetByIdForUpdateAsync(TrainingPathId id, OrganizationId organizationId, CancellationToken cancellationToken = default) => GetByIdAsync(id, organizationId, cancellationToken);
        public Task<bool> ExistsOpenForStudentAndVersionAsync(OrganizationId organizationId, PersonId studentId, CurriculumVersionId curriculumVersionId, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task AddAsync(TrainingPath trainingPath, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeCompetencyRecordRepository : ICompetencyRecordRepository
    {
        public CompetencyRecord? Record { get; private set; }
        public Task<CompetencyRecord?> GetByIdAsync(OrganizationId organizationId, CompetencyRecordId competencyRecordId, CancellationToken cancellationToken = default) => Task.FromResult(Record);
        public Task<CompetencyRecord?> GetByIdForUpdateAsync(OrganizationId organizationId, CompetencyRecordId competencyRecordId, CancellationToken cancellationToken = default) => Task.FromResult(Record);
        public Task<CompetencyRecord?> GetByTrainingPathAndCompetencyAsync(OrganizationId organizationId, TrainingPathId trainingPathId, CompetencyId competencyId, CancellationToken cancellationToken = default) => Task.FromResult(Record);
        public Task<CompetencyRecord?> GetByTrainingPathAndCompetencyForUpdateAsync(OrganizationId organizationId, TrainingPathId trainingPathId, CompetencyId competencyId, CancellationToken cancellationToken = default) => Task.FromResult(Record);
        public Task AddAsync(CompetencyRecord competencyRecord, CancellationToken cancellationToken = default) { Record = competencyRecord; return Task.CompletedTask; }
    }

    private sealed class FakeEligibilityService(CurriculumCompetencyEligibility eligibility) : ICurriculumCompetencyEligibilityService
    {
        public Task<CurriculumCompetencyEligibility?> GetAsync(OrganizationId organizationId, CurriculumVersionId curriculumVersionId, CompetencyId competencyId, CancellationToken cancellationToken = default) =>
            Task.FromResult<CurriculumCompetencyEligibility?>(curriculumVersionId == eligibility.CurriculumVersionId && competencyId == eligibility.CompetencyId ? eligibility : null);
    }

    private sealed class FakeUnitOfWork : ICurriculumPedagogyUnitOfWork
    {
        public int CommitCount { get; private set; }
        public bool HasActiveTransaction => false;
        public Task BeginTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
        public Task<int> CommitAsync(CancellationToken cancellationToken = default) { CommitCount++; return Task.FromResult(1); }
        public Task CommitTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
