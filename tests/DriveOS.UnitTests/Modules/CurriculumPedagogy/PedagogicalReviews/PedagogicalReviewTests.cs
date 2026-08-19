using DriveOS.Modules.CurriculumPedagogy.Domain.PedagogicalReviews;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.CurriculumPedagogy.PedagogicalReviews;

public sealed class PedagogicalReviewTests
{
    private static PedagogicalReview Create() => PedagogicalReview.Request(
        PedagogicalReviewId.New(),
        new OrganizationId(Guid.NewGuid()),
        new PersonId(Guid.NewGuid()),
        TrainingPathId.New(),
        new UserId(Guid.NewGuid()),
        "Bilan intermédiaire après plusieurs séances.",
        DateTimeOffset.UtcNow).Value;

    [Fact]
    public void Request_CreatesOpenReview()
    {
        PedagogicalReview review = Create();
        review.Status.Should().Be(PedagogicalReviewStatus.Requested);
        review.Findings.Should().BeNull();
        review.Recommendations.Should().BeNull();
    }

    [Fact]
    public void Complete_PreservesFindingsRecommendationsAndRemainingNeeds()
    {
        PedagogicalReview review = Create();
        review.Start(DateTimeOffset.UtcNow);
        review.Complete("Bonne maîtrise générale.", "Renforcer les intersections complexes.", 5m, DateTimeOffset.UtcNow).IsSuccess.Should().BeTrue();
        review.Status.Should().Be(PedagogicalReviewStatus.Completed);
        review.Findings.Should().Be("Bonne maîtrise générale.");
        review.Recommendations.Should().Be("Renforcer les intersections complexes.");
        review.EstimatedRemainingPracticalHours.Should().Be(5m);
    }

    [Fact]
    public void CompletedReview_CannotBeCancelled()
    {
        PedagogicalReview review = Create();
        review.Complete("Constat.", "Recommandation.", null, DateTimeOffset.UtcNow);
        review.Cancel("Annulation", DateTimeOffset.UtcNow).Error.Should().Be(PedagogicalReviewErrors.CancellationNotAllowed);
    }
}
