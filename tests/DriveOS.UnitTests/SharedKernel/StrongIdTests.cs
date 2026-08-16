using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.UnitTests.SharedKernel;

public sealed class StrongIdTests
{
    [Fact]
    public void OrganizationIds_WithSameValue_ShouldBeEqual()
    {
        Guid value = Guid.NewGuid();

        var first = new OrganizationId(value);
        var second = new OrganizationId(value);

        Assert.Equal(first, second);
    }

    [Fact]
    public void OrganizationIds_WithDifferentValues_ShouldNotBeEqual()
    {
        OrganizationId first = OrganizationId.New();
        OrganizationId second = OrganizationId.New();

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void NewOrganizationId_ShouldNotBeEmpty()
    {
        OrganizationId id = OrganizationId.New();

        Assert.False(id.IsEmpty);
        Assert.NotEqual(Guid.Empty, id.Value);
    }
}
