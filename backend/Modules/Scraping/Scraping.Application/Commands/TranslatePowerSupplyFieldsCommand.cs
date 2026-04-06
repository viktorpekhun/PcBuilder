using MediatR;

namespace Scraping.Application.Commands
{
    public record TranslatePowerSupplyFieldsCommand() : IRequest;
}
