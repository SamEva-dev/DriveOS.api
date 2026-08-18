using DriveOS.Modules.FundingBilling.Application.Invoices.Issue;
using DriveOS.Modules.Organizations.Application.OrganizationSequences;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Integrations.FundingBilling;

internal sealed class InvoiceNumberGenerator(IOrganizationSequenceNumberGenerator sequences) : IInvoiceNumberGenerator
{
    private const string SequenceCode = "INVOICE";

    public async Task<Result<string>> ReserveNextAsync(OrganizationId organizationId, CancellationToken cancellationToken = default)
    {
        Result<string> result = await sequences.ReserveNextAsync(organizationId, null, SequenceCode, cancellationToken);
        if (result.IsFailure && result.Error.Code == "OrganizationSequences.NotFound")
            return Result.Failure<string>(IssueInvoiceErrors.NumberSequenceNotConfigured);

        return result;
    }
}
