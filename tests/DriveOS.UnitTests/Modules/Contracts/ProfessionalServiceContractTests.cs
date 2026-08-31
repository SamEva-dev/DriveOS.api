using DriveOS.Modules.Contracts.Domain.ProfessionalServiceContracts;
using DriveOS.SharedKernel.Identifiers;
using Xunit;

namespace DriveOS.UnitTests.Modules.Contracts;

public sealed class ProfessionalServiceContractTests
{
    private static readonly UserId Actor=new(Guid.NewGuid());
    private static readonly PersonId School=new(Guid.NewGuid());
    private static readonly PersonId Professional=new(Guid.NewGuid());
    private static readonly DateTimeOffset Now=DateTimeOffset.UtcNow;
    private const string Hash="AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";

    private static ProfessionalServiceContract Create(ProfessionalServiceContractSignatureOrder order)
    {
        return ProfessionalServiceContract.Create(
            new(Guid.NewGuid()),new(Guid.NewGuid()),new(Guid.NewGuid()),new(Guid.NewGuid()),Guid.NewGuid(),
            "PSC-2026-001","MISSION",order,"{}",
            [
                new(School,"SCHOOL",1,true),
                new(Professional,"PROFESSIONAL",2,true)
            ],
            Now,Actor).Value;
    }

    [Fact]
    public void Sequential_signature_order_is_enforced()
    {
        var contract=Create(ProfessionalServiceContractSignatureOrder.Sequential);
        contract.Generate("doc://contract/1",Hash,Now,Actor);
        contract.SendForSignature(Now,Actor);

        var result=contract.RecordSignature(
            Professional,Hash,"ELECTRONIC","MFA","TEST","REF-2",null,"127.0.0.1",Now,Now,Actor);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Parallel_signature_order_allows_independent_cosignatures()
    {
        var contract=Create(ProfessionalServiceContractSignatureOrder.Parallel);
        contract.Generate("doc://contract/1",Hash,Now,Actor);
        contract.SendForSignature(Now,Actor);

        Assert.True(contract.RecordSignature(
            Professional,Hash,"ELECTRONIC","MFA","TEST","REF-2",null,"127.0.0.1",Now,Now,Actor).IsSuccess);
    }

    [Fact]
    public void Revision_preserves_previous_signed_version_and_resets_signatures()
    {
        var contract=Create(ProfessionalServiceContractSignatureOrder.Parallel);
        contract.Generate("doc://contract/1",Hash,Now,Actor);
        contract.SendForSignature(Now,Actor);
        contract.RecordSignature(School,Hash,"ELECTRONIC","MFA","TEST","REF-1",null,null,Now,Now,Actor);
        contract.RecordSignature(Professional,Hash,"ELECTRONIC","MFA","TEST","REF-2",null,null,Now,Now,Actor);

        Assert.Equal(ProfessionalServiceContractStatus.Signed,contract.Status);

        const string hash2="BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB";
        Assert.True(contract.CreateRevision("doc://contract/2",hash2,"Avenant tarifaire",Now.AddDays(1),Actor).IsSuccess);

        Assert.Equal(2,contract.Version);
        Assert.Single(contract.PreviousVersions);
        Assert.Equal(ProfessionalServiceContractStatus.Generated,contract.Status);
        Assert.All(contract.Signatories,x=>Assert.Null(x.SignedAtUtc));
    }
}
