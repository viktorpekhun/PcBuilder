using MediatR;
using PcBuilder.SharedKernel.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scraping.Application.Commands
{
    public record ScrapeSingleComponentCommand(string ComponentUrl, ComponentType ComponentType, Type EntityType, Type[]? NestedTypes = null) : IRequest;

}
