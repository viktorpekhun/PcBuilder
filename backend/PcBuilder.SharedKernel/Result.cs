namespace PcBuilder.SharedKernel
{
    public class Result<T>
    {
        public T? Value { get; }
        public Error? Error { get; }
        public bool IsSuccess => Error is null;
        public bool IsFailure => !IsSuccess;

        private Result(T value)
        {
            Value = value;
        }

        private Result(Error error)
        {
            Error = error;
        }

        public static Result<T> Success(T value) => new(value);
        public static Result<T> Failure(Error error) => new(error);
    }

    public static class Result
    {
        public static Result<T> Success<T>(T value) => Result<T>.Success(value);
        public static Result<T> Failure<T>(Error error) => Result<T>.Failure(error);
    }
}
