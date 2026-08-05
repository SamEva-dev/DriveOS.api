namespace DriveOS.Modules.Organizations.Infrastructure.OrganizationSequences;

public sealed class OrganizationSequenceReservationOptions
{
    public const string SectionName = "OrganizationSequences:Reservation";
    public const int DefaultMaxConcurrencyRetries = 3;
    public const int MaximumAllowedRetries = 10;

    public int MaxConcurrencyRetries { get; set; } = DefaultMaxConcurrencyRetries;

    internal int GetValidatedMaxConcurrencyRetries() =>
        Math.Clamp(MaxConcurrencyRetries, 0, MaximumAllowedRetries);
}
