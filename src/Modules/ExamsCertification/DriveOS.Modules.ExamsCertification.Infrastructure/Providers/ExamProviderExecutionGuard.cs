using System.Collections.Concurrent;
using DriveOS.Modules.ExamsCertification.Application.Providers.Connections;
using DriveOS.Modules.ExamsCertification.Domain.Providers.Connections;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.Modules.ExamsCertification.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Providers;

/// <summary>
/// Tenant/provider execution guard. It applies a conservative client-side rate limit and opens a short circuit after
/// repeated transport failures. Provider-side limits remain authoritative and may be stricter.
/// </summary>
internal sealed class ExamProviderExecutionGuard(
    IExamProviderConnectionRepository connections,
    IOptions<ExamProviderExecutionOptions> options) : IExamProviderExecutionGuard
{
    private readonly ExamProviderExecutionOptions _options = options.Value;
    private sealed class State
    {
        public readonly SemaphoreSlim Gate = new(1, 1);
        public DateTimeOffset LastRequestAtUtc;
        public int ConsecutiveFailures;
        public DateTimeOffset? CircuitOpenUntilUtc;
    }

    private static readonly ConcurrentDictionary<string, State> States = new(StringComparer.OrdinalIgnoreCase);

    public async Task<T> ExecuteAsync<T>(OrganizationId organizationId, string providerCode,
        Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default)
    {
        ExamProviderConnection? connection = await connections.FindByProviderCodeAsync(organizationId, providerCode, cancellationToken);
        int requestsPerMinute = connection?.RequestsPerMinute ?? _options.DefaultRequestsPerMinute;
        if (connection?.Status is ExamProviderConnectionStatus.Suspended or ExamProviderConnectionStatus.Revoked)
            throw new InvalidOperationException("Exam provider connection is not active.");

        string key = $"{organizationId.Value:N}:{providerCode}";
        State state = States.GetOrAdd(key, _ => new State());
        await state.Gate.WaitAsync(cancellationToken);
        try
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            if (state.CircuitOpenUntilUtc is { } openUntil && openUntil > now)
                throw new InvalidOperationException("Exam provider circuit is temporarily open.");

            TimeSpan minimumSpacing = TimeSpan.FromMinutes(1d / Math.Clamp(requestsPerMinute, 1, _options.MaxRequestsPerMinute));
            TimeSpan wait = minimumSpacing - (now - state.LastRequestAtUtc);
            if (wait > TimeSpan.Zero)
                await Task.Delay(wait, cancellationToken);

            state.LastRequestAtUtc = DateTimeOffset.UtcNow;
            try
            {
                T result = await operation(cancellationToken);
                state.ConsecutiveFailures = 0;
                state.CircuitOpenUntilUtc = null;
                return result;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                state.ConsecutiveFailures++;
                if (state.ConsecutiveFailures >= _options.CircuitFailureThreshold)
                    state.CircuitOpenUntilUtc = DateTimeOffset.UtcNow.AddMinutes(_options.CircuitOpenMinutes);
                throw;
            }
        }
        finally
        {
            state.Gate.Release();
        }
    }
}
