using DriveOS.SharedKernel.Results;

namespace DriveOS.UnitTests.SharedKernel;

public sealed class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void Failure_ShouldCreateFailedResult()
    {
        Error error = Error.Validation(
            code: "Test.Invalid",
            messageKey: "errors.test.invalid");

        Result result = Result.Failure(error);

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
        Assert.Equal("Test.Invalid", result.Error.Code);
        Assert.Equal(
            "errors.test.invalid",
            result.Error.MessageKey);
    }

    [Fact]
    public void FailedGenericResult_Value_ShouldThrow()
    {
        var result = Result.Failure<string>(
            Error.NotFound(
                "Test.NotFound",
                "Value not found."));

        var action = () => result.Value;

        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void Error_ShouldExposeTranslationParameters()
    {
        Error error = Error.Validation(
            code: "Test.TooLong",
            messageKey: "errors.test.tooLong",
            parameters: new Dictionary<string, object?>
            {
                ["maxLength"] = 200
            });

        Assert.NotNull(error.Parameters);
        Assert.Equal(
            200,
            error.Parameters["maxLength"]);
    }
}