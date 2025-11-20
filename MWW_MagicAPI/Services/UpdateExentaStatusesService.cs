using Microsoft.EntityFrameworkCore;
using MWW_Api.Config;

namespace MWW_MagicAPI.Services;

public class UpdateExentaStatusesService : IUpdateExentaStatusesService
{
    private ShopfloorDbContextFactory _contextFactory;
    private ILogger<UpdateExentaStatusesService> _logger;
    private List<string> _shopfloors  = new List<string>() { "HV", "PD", "TJ", "GM" };
    public record UpdateData
    {
        public string AlphaNumId { get; set; }
        public string MilestoneName { get; set; }
        public long OperationId { get; set; }
        public long ProductId { get; set; }
        public string SerialNumber { get; set; }
        public DateTime Created { get; set; }
    }

    public UpdateExentaStatusesService(ShopfloorDbContextFactory contextFactory,
        ILogger<UpdateExentaStatusesService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public void UpdateExentaStatuses(int minutes)
    {
        foreach (string sf in _shopfloors)
        {
            _logger.LogInformation($"Exenta update status starting for {sf}");
            List<UpdateData> data = GetUpdateData(minutes, _contextFactory.GetContext(sf));
            _logger.LogInformation($"Exenta update status ending for {sf}");
        }
    }

    public List<UpdateData> GetUpdateData(int minutes, ShopfloorDbContext context)
    {
        // Calculate cutoff time (equivalent to DATEADD(minute, -args.time, GETUTCDATE()))
        DateTime cutoff = DateTime.UtcNow.AddMinutes(-minutes);

        var query =
            from u in context.Units.AsNoTracking()
            join t in context.Transactions
                on u.Id equals t.UnitId into ut // LEFT JOIN
            from t in ut.DefaultIfEmpty()
            join wo in context.WorkOrders.AsNoTracking()
                on t.WorkorderId equals wo.Id
            join po in context.ProductOperations
                on new { t.OperationId, wo.ProductId }
                equals new { po.OperationId, po.ProductId }
            join m in context.MileStones
                on po.MileStoneId equals m.Id
            where m.Name != "READY"
                  && t.DateTime >= cutoff
            orderby t.Created descending
            select new UpdateData
            {
                AlphaNumId = u.AlphaNumId,
                MilestoneName = m.Name,
                OperationId = po.OperationId,
                ProductId = po.ProductId,
                SerialNumber = wo.Serialnumber,
                Created = t.Created
            };

        List<UpdateData> results = query.Distinct().ToList();
        return results;
    }
}
