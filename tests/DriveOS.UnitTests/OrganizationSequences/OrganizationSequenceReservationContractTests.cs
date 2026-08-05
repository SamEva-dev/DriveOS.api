using DriveOS.Modules.Organizations.Application.OrganizationSequences;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using FluentAssertions;

namespace DriveOS.UnitTests.OrganizationSequences;

public sealed class OrganizationSequenceReservationContractTests
{
    [Fact]
    public async Task Contract_should_preserve_typed_tenant_and_branch_scope()
    {
        var organizationId = OrganizationId.New();
        var branchId = BranchId.New();
        var generator = new RecordingGenerator("INV-2026-000001");

        Result<string> result = await generator.ReserveNextAsync(
            organizationId,
            branchId,
            "INVOICE",
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("INV-2026-000001");
        generator.OrganizationId.Should().Be(organizationId);
        generator.BranchId.Should().Be(branchId);
        generator.Code.Should().Be("INVOICE");
    }

    private sealed class RecordingGenerator(string value)
        : IOrganizationSequenceNumberGenerator
    {
        public OrganizationId OrganizationId { get; private set; }
        public BranchId? BranchId { get; private set; }
        public string? Code { get; private set; }

        public Task<Result<string>> ReserveNextAsync(
            OrganizationId organizationId,
            BranchId? branchId,
            string code,
            CancellationToken cancellationToken = default)
        {
            OrganizationId = organizationId;
            BranchId = branchId;
            Code = code;
            return Task.FromResult(Result.Success(value));
        }
    }
}
