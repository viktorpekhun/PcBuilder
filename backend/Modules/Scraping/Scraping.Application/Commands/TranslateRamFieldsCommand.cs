using MediatR;

namespace Scraping.Application.Commands
{
    public record TranslateRamFieldsCommand() : IRequest;
}
