using DriveOS.Modules.FundingBilling.Application.CreditNotes.Issue;
using DriveOS.Modules.Organizations.Application.OrganizationSequences;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
namespace DriveOS.Api.Integrations.FundingBilling;
internal sealed class CreditNoteNumberGenerator(IOrganizationSequenceNumberGenerator sequences):ICreditNoteNumberGenerator
{
 private const string SequenceCode="CREDIT_NOTE";
 public async Task<Result<string>> ReserveNextAsync(OrganizationId organizationId,CancellationToken cancellationToken=default){var r=await sequences.ReserveNextAsync(organizationId,null,SequenceCode,cancellationToken);return r.IsFailure&&r.Error.Code=="OrganizationSequences.NotFound"?Result.Failure<string>(IssueCreditNoteErrors.NumberSequenceNotConfigured):r;}
}
