using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Organizations.Application.OrganizationSequences;
using DriveOS.Modules.Organizations.Domain.OrganizationSequences;
using DriveOS.Modules.Organizations.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;

namespace DriveOS.Modules.Organizations.Infrastructure.OrganizationSequences;

internal sealed class OrganizationSequenceNumberGenerator(
    OrganizationsDbContext dbContext,
    IClock clock,
    IOptions<OrganizationSequenceReservationOptions> options
) : IOrganizationSequenceNumberGenerator
{
    public async Task<Result<string>> ReserveNextAsync(
        OrganizationId organizationId,
        BranchId? branchId,
        string code,
        CancellationToken cancellationToken = default
    )
    {
        if (organizationId.IsEmpty)
        {
            return Result.Failure<string>(OrganizationSequenceErrors.EmptyOrganizationId);
        }

        string normalizedCode = code?.Trim().ToUpperInvariant() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedCode))
        {
            return Result.Failure<string>(OrganizationSequenceErrors.EmptyCode);
        }

        int maximumRetries = options.Value.GetValidatedMaxConcurrencyRetries();

        IExecutionStrategy executionStrategy = dbContext.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(async () =>
        {
            for (int attempt = 0; ; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await using IDbContextTransaction transaction =
                    await dbContext.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    OrganizationSequence? sequence =
                        await dbContext.OrganizationSequences.SingleOrDefaultAsync(
                            candidate =>
                                candidate.OrganizationId == organizationId
                                && candidate.BranchId == branchId
                                && candidate.Code == normalizedCode,
                            cancellationToken
                        );

                    if (sequence is null)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        dbContext.ChangeTracker.Clear();

                        return Result.Failure<string>(OrganizationSequenceErrors.NotFound);
                    }

                    Result<string> reservation = sequence.ReserveNext(clock.UtcNow);

                    if (reservation.IsFailure)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        dbContext.ChangeTracker.Clear();
                        return reservation;
                    }

                    await dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    dbContext.ChangeTracker.Clear();

                    return reservation;
                }
                catch (DbUpdateConcurrencyException)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    dbContext.ChangeTracker.Clear();

                    if (attempt >= maximumRetries)
                    {
                        return Result.Failure<string>(
                            OrganizationSequenceErrors.ConcurrencyRetryExhausted(attempt + 1)
                        );
                    }
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    dbContext.ChangeTracker.Clear();
                    throw;
                }
            }
        });
    }
}
