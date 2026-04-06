using MediatR;

namespace Scraping.Application.Commands
{
    public record TranslateCpuCoolerFieldsCommand() : IRequest;
}
