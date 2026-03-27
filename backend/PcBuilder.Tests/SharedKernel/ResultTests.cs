using PcBuilder.SharedKernel;

namespace PcBuilder.Tests.SharedKernel
{
    public class ResultTests
    {
        [Fact]
        public void Success_ShouldSetValueAndIsSuccess()
        {
            var result = Result<string>.Success("test");

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.Equal("test", result.Value);
            Assert.Null(result.Error);
        }

        [Fact]
        public void Failure_ShouldSetErrorAndIsFailure()
        {
            var error = new Error("NotFound", "Item not found", 404);
            var result = Result<string>.Failure(error);

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
            Assert.Equal(error, result.Error);
        }

        [Fact]
        public void Success_WithNullValue_ShouldStillBeSuccess()
        {
            var result = Result<string?>.Success(null);

            Assert.True(result.IsSuccess);
            Assert.Null(result.Value);
            Assert.Null(result.Error);
        }

        [Fact]
        public void StaticHelper_Success_CreatesSuccessResult()
        {
            var result = Result.Success(42);

            Assert.True(result.IsSuccess);
            Assert.Equal(42, result.Value);
        }

        [Fact]
        public void StaticHelper_Failure_CreatesFailureResult()
        {
            var error = new Error("BadRequest", "Invalid input");
            var result = Result.Failure<int>(error);

            Assert.True(result.IsFailure);
            Assert.Equal("BadRequest", result.Error!.Code);
            Assert.Equal("Invalid input", result.Error.Message);
            Assert.Equal(400, result.Error.StatusCode);
        }
    }

    public class ErrorTests
    {
        [Fact]
        public void Error_DefaultStatusCode_Is400()
        {
            var error = new Error("Code", "Message");
            Assert.Equal(400, error.StatusCode);
        }

        [Fact]
        public void Error_CustomStatusCode_IsPreserved()
        {
            var error = new Error("NotFound", "Not found", 404);
            Assert.Equal(404, error.StatusCode);
        }

        [Fact]
        public void Error_RecordEquality_WorksCorrectly()
        {
            var error1 = new Error("Code", "Message", 400);
            var error2 = new Error("Code", "Message", 400);

            Assert.Equal(error1, error2);
        }
    }
}
