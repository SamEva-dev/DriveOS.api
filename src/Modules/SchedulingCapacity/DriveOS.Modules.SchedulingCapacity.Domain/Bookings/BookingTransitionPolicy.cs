using DriveOS.Modules.SchedulingCapacity.Domain.CalendarResources;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Bookings;

public sealed record BookingTransitionPolicy(
    int InstructorSameBranchBufferMinutes,
    int InstructorCrossBranchTravelMinutes,
    int VehicleSameBranchBufferMinutes,
    int VehicleCrossBranchTravelMinutes)
{
    public static BookingTransitionPolicy Default { get; } = new(15, 45, 10, 30);

    public int MaximumTransitionMinutes => new[]
    {
        InstructorSameBranchBufferMinutes,
        InstructorCrossBranchTravelMinutes,
        VehicleSameBranchBufferMinutes,
        VehicleCrossBranchTravelMinutes
    }.Max();

    public int RequiredMinutes(CalendarResourceType resourceType, bool branchChanged) => resourceType switch
    {
        CalendarResourceType.Instructor => branchChanged
            ? InstructorCrossBranchTravelMinutes
            : InstructorSameBranchBufferMinutes,
        CalendarResourceType.Vehicle => branchChanged
            ? VehicleCrossBranchTravelMinutes
            : VehicleSameBranchBufferMinutes,
        _ => 0
    };

    public BookingTransitionPolicy Validate()
    {
        if (InstructorSameBranchBufferMinutes < 0 || InstructorCrossBranchTravelMinutes < 0 ||
            VehicleSameBranchBufferMinutes < 0 || VehicleCrossBranchTravelMinutes < 0)
            throw new ArgumentOutOfRangeException(nameof(BookingTransitionPolicy));

        return this;
    }
}
