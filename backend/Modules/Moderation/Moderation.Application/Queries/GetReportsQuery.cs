using MediatR;
using Moderation.Application.Dtos;
using Moderation.Domain.Enums;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Filtering;

namespace Moderation.Application.Queries
{
    public record GetReportsQuery(
        ReportStatus? Status,
        int PageNumber = 1,
        int PageSize = 20) : IRequest<Result<PagedResponse<ReportDto>>>;
}