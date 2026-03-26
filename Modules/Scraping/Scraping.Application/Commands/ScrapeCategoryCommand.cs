using MediatR;
using PcBuilder.SharedKernel.Enums;

namespace Scraping.Application.Commands
{
    public record ScrapeCategoryCommand(string CategoryUrl, ComponentType ComponentType, Type EntityType) : IRequest;
}
