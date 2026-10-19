using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Dashboard.Dtos;
using MechanicShop.Domain.Common.Results;


using MediatR;

using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Application.Features.Dashboard.Queries.GetWorkOrderStats;

public class GetWorkOrderStatsQueryHandler(IAppDbContext context) :
    IRequestHandler<GetWorkOrderStatsQuery, Result<TodayWorkOrderStatsDto>>
{
    public async Task<Result<TodayWorkOrderStatsDto>> Handle(GetWorkOrderStatsQuery request, CancellationToken cancellationToken)
    {
        var start = request.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end = request.Date.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var query = context.WorkOrders
            .Include(w => w.Invoice)
            .Where(w => w.StartAtUtc >= start && w.StartAtUtc < end);

        var total = await query.CountAsync(cancellationToken);

        if (total == 0)
        {
            return new TodayWorkOrderStatsDto
            {
                Date = request.Date,
                Total = 0,
                Scheduled = 0,
                InProgress = 0,
                Completed = 0,
                Cancelled = 0,
                TotalRevenue = 0,
                TotalPartsCost = 0,
                TotalLaborCost = 0,
                UniqueVehicles = 0,
                UniqueCustomers = 0
            };
        }

        var stats = await query.ToListAsync(cancellationToken);

        var totalRevenue = stats.Sum(x => x.Invoice?.Total ?? 0);
     
       



        return new TodayWorkOrderStatsDto
        {
            Date = request.Date,
            Total = total,
           
            TotalRevenue = totalRevenue,

        
            AverageRevenuePerOrder = total > 0 ? totalRevenue / total : 0,

        };
    }
}