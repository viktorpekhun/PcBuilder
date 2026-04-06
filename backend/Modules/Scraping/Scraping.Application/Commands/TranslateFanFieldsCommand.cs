using MediatR;

namespace Scraping.Application.Commands
{
    public record TranslateFanFieldsCommand() : IRequest;
}
