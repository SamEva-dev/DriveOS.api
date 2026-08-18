using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Contracts.Application.Persistence;
using DriveOS.Modules.Contracts.Domain.SignatureProcesses;
using DriveOS.Modules.Contracts.Domain.TrainingContracts;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.Contracts;

internal static class ContractsApplicationTestFixture
{
    internal static TrainingContract CreateDraft(
        OrganizationId organizationId,
        DateOnly startDate,
        DateOnly? endDate = null)
    {
        PersonId studentId = PersonId.New();
        TrainingContractTermsSnapshot terms = TrainingContractTermsSnapshot.Create(
            "B-MANUAL",
            20m,
            "Driving lessons",
            "3 installments",
            "Cancellation terms",
            "Booking rules",
            "Student obligations",
            "Provider obligations",
            "Exam presentation terms",
            "Data processing terms").Value;

        IReadOnlyCollection<TrainingContractParty> parties =
        [
            TrainingContractParty.ForOrganization(
                TrainingContractPartyKind.TrainingProvider,
                organizationId,
                "Auto-école Horizon").Value,
            TrainingContractParty.ForPerson(
                TrainingContractPartyKind.Student,
                studentId,
                "Student").Value,
        ];

        return TrainingContract.CreateDraft(
            TrainingContractId.New(),
            organizationId,
            BranchId.New(),
            studentId,
            CommercialOfferId.New(),
            1,
            $"CTR-{Guid.NewGuid():N}"[..20],
            startDate,
            endDate,
            1500m,
            "EUR",
            terms,
            parties).Value;
    }

    internal static TrainingContract CreateSigned(
        OrganizationId organizationId,
        DateOnly startDate,
        DateOnly? endDate,
        DateTimeOffset now)
    {
        TrainingContract contract = CreateDraft(organizationId, startDate, endDate);
        UserId actor = UserId.New();
        TrainingContractSignatory signatory = contract.AddSignatory(
            TrainingContractSignatoryKind.Student,
            contract.StudentId,
            null,
            "Student",
            1,
            true,
            null).Value;

        contract.MarkGenerated(
            "contracts/test/document",
            "contract.html",
            "text/html",
            new string('A', 64),
            actor,
            now).IsSuccess.Should().BeTrue();

        contract.MarkSentForSignature(SignatureProcessId.New(), actor, now).IsSuccess.Should().BeTrue();
        contract.RecordSignatorySignature(
            signatory.Id,
            SignatureEvidenceId.New(),
            actor,
            now).IsSuccess.Should().BeTrue();

        return contract;
    }

    internal static TrainingContract CreateActive(
        OrganizationId organizationId,
        DateOnly startDate,
        DateOnly? endDate,
        DateTimeOffset now)
    {
        TrainingContract contract = CreateSigned(organizationId, startDate, endDate, now);
        contract.Activate(UserId.New(), now).IsSuccess.Should().BeTrue();
        return contract;
    }

    internal sealed class FakeClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow => utcNow;
    }

    internal sealed class FakeContractsUnitOfWork : IContractsUnitOfWork
    {
        public bool HasActiveTransaction { get; private set; }
        public int CommitCount { get; private set; }

        public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            HasActiveTransaction = true;
            return Task.CompletedTask;
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(0);

        public Task<int> CommitAsync(CancellationToken cancellationToken = default)
        {
            CommitCount++;
            return Task.FromResult(1);
        }

        public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (!HasActiveTransaction)
                throw new InvalidOperationException("No active transaction.");

            CommitCount++;
            HasActiveTransaction = false;
            return Task.CompletedTask;
        }

        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (!HasActiveTransaction)
                throw new InvalidOperationException("No active transaction.");

            HasActiveTransaction = false;
            return Task.CompletedTask;
        }
    }

    internal sealed class FakeTrainingContractRepository(TrainingContract? contract)
        : ITrainingContractRepository
    {
        public TrainingContract? Contract { get; set; } = contract;
        public TrainingContract? Added { get; private set; }

        public Task<TrainingContract?> GetByIdAsync(
            TrainingContractId contractId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Contract is not null && Contract.Id == contractId ? Contract : null);

        public Task<TrainingContract?> GetByContractNumberAsync(
            OrganizationId organizationId,
            string contractNumber,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(
                Contract is not null &&
                Contract.OrganizationId == organizationId &&
                Contract.ContractNumber == contractNumber
                    ? Contract
                    : null);

        public Task AddAsync(
            TrainingContract contract,
            CancellationToken cancellationToken = default)
        {
            Added = contract;
            Contract = contract;
            return Task.CompletedTask;
        }
    }

    internal sealed class FakeSignatureProcessRepository : ISignatureProcessRepository
    {
        private readonly Dictionary<SignatureProcessId, SignatureProcess> processes = [];

        public bool ExistingForVersion { get; set; }
        public SignatureProcess? Added { get; private set; }

        public Task AddAsync(
            SignatureProcess process,
            CancellationToken cancellationToken = default)
        {
            Added = process;
            processes[process.Id] = process;
            return Task.CompletedTask;
        }

        public Task<SignatureProcess?> GetByIdAsync(
            SignatureProcessId id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(processes.GetValueOrDefault(id));

        public Task<bool> ExistsForContractVersionAsync(
            TrainingContractId contractId,
            int contractVersionNumber,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(ExistingForVersion || processes.Values.Any(x =>
                x.ContractId == contractId && x.ContractVersionNumber == contractVersionNumber));

        public void Seed(SignatureProcess process) => processes[process.Id] = process;
    }
}
