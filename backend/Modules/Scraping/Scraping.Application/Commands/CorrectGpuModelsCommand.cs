using MediatR;

namespace Scraping.Application.Commands
{
    public record CorrectGpuModelsCommand() : IRequest;
}
