namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Read;

internal sealed class SchedulingTransitionOptions
{
    public const string SectionName = "Scheduling:Transitions";

    public int InstructorSameBranchBufferMinutes { get; init; } = 15;
    public int InstructorCrossBranchTravelMinutes { get; init; } = 45;
    public int VehicleSameBranchBufferMinutes { get; init; } = 10;
    public int VehicleCrossBranchTravelMinutes { get; init; } = 30;
}
