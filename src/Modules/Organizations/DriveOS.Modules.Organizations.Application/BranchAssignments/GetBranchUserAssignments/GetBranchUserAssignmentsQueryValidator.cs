using FluentValidation;

namespace DriveOS.Modules.Organizations.Application
    .BranchAssignments.GetUserBranchAssignments;

internal sealed class
    GetUserBranchAssignmentsQueryValidator
    : AbstractValidator<
        GetUserBranchAssignmentsQuery>
{
    public GetUserBranchAssignmentsQueryValidator()
    {
        RuleFor(query =>
                query.OrganizationId.Value)
            .NotEmpty()
            .WithMessage(
                "errors.branchAssignments.organizationId.empty");

        RuleFor(query =>
                query.UserId.Value)
            .NotEmpty()
            .WithMessage(
                "errors.branchAssignments.userId.empty");

        RuleFor(query =>
                query.PageNumber)
            .GreaterThanOrEqualTo(1);

        RuleFor(query =>
                query.PageSize)
            .InclusiveBetween(
                1,
                100);

        RuleFor(query =>
                query.SortBy)
            .IsInEnum();

        RuleFor(query =>
                query.SortDirection)
            .IsInEnum();

        When(
            query =>
                query.Status.HasValue,
            () =>
            {
                RuleFor(query =>
                        query.Status!.Value)
                    .IsInEnum();
            });

        When(
            query =>
                query.Role.HasValue,
            () =>
            {
                RuleFor(query =>
                        query.Role!.Value)
                    .IsInEnum();
            });

        When(
            query =>
                query.AssignmentType.HasValue,
            () =>
            {
                RuleFor(query =>
                        query.AssignmentType!.Value)
                    .IsInEnum();
            });
    }
}