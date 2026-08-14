using DriveOS.Modules.CRM.Application.Activities.ImportActivity;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace DriveOS.Modules.CRM.Infrastructure.Persistence.Queries;

internal sealed class CrmActivityImportLock(CrmDbContext db) : ICrmActivityImportLock
{
    public async Task AcquireAsync(OrganizationId organizationId, string idempotencyKey,
        CancellationToken ct)
    {
        if (db.Database.CurrentTransaction is null)
            throw new InvalidOperationException("An active transaction is required for an import lock.");

        var connection = (NpgsqlConnection)db.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync(ct);

        await using NpgsqlCommand command = connection.CreateCommand();
        command.Transaction = (NpgsqlTransaction)db.Database.CurrentTransaction.GetDbTransaction();
        command.CommandText = "SELECT pg_advisory_xact_lock(hashtextextended(@scope, 0));";
        command.Parameters.AddWithValue("scope", $"{organizationId.Value:N}:{idempotencyKey}");
        await command.ExecuteNonQueryAsync(ct);
    }
}
