namespace PcBuilder.SharedKernel
{
    public record Error(string Code, string Message, int StatusCode = 400);
}
