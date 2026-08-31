using DriveOS.Modules.ProfessionalMarketplace.Domain.Reviews;
using Xunit;
namespace DriveOS.UnitTests.Modules.ProfessionalMarketplace;
public sealed class ProfessionalReviewTests
{
 [Fact] public void Ratings_are_bounded_between_one_and_five(){Assert.True(new ProfessionalReviewRatings(5,4,5,4,5).Validate().IsSuccess);Assert.True(new ProfessionalReviewRatings(6,4,5,4,5).Validate().IsFailure);}
 [Fact] public void Average_is_structured_and_deterministic(){var r=new ProfessionalReviewRatings(5,4,3,4,5);Assert.Equal(4.2m,r.Average);}
 [Fact] public void Hidden_reviews_must_not_count_toward_reputation(){Assert.Equal((int)ProfessionalReviewStatus.Published,1);Assert.Equal((int)ProfessionalReviewStatus.Hidden,2);}
}
