using System.Reflection;

namespace DriveOS.ArchitectureTests;

public sealed class ProfessionalMarketplaceFinanceBoundaryTests
{
    [Fact]
    public void ProfessionalMarketplace_Application_ShouldNotReferenceFundingBilling()
    {
        Assembly assembly=typeof(
            DriveOS.Modules.ProfessionalMarketplace.Application.Invoices.IProfessionalInvoiceFinanceGateway).Assembly;

        string[] references=assembly.GetReferencedAssemblies()
            .Select(x=>x.Name??string.Empty)
            .ToArray();

        Assert.DoesNotContain(
            references,
            x=>x.StartsWith("DriveOS.Modules.FundingBilling",StringComparison.Ordinal));
    }
}
