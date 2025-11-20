using Microsoft.EntityFrameworkCore;
using MWW_Api.Config;
using System;

namespace MWW_MagicAPI.Services;

public class UpdateExentaStatusesService : IUpdateExentaStatusesService
{
    private readonly IShopfloorDbContextFactory _contextFactory;
    private ILogger<UpdateExentaStatusesService> _logger;
    private List<string> _shopfloors  = new List<string>() { "HV", "PD", "TJ", "GM" };
    private List<string> excludedStatuses = new List<string>() { "shipped", "cancelled", "cancel", "ready", "pods", "toqc", "stship" };
    string[] progression = new string[]
    {
        "printed",
        "Printed",
        "PRINTED",
        "ToTenter",
        "AtTenter",
        "InTenter",
        "waitingLoom",
        "ToStretch",
        "stretch",
        "inLoom",
        "ToCut",
        "toCut",
        "ToPack",
        "ToPB",
        "ToProc",
        "ToFinishing",
        "InPB",
        "ToSew",
        "InSew",
        "ToCircleTack",
        "ToShip",
        "cancel"
    };

    public record UpdateData
    {
        public string AlphaNumId { get; set; }
        public string MilestoneName { get; set; }
        public long OperationId { get; set; }
        public long ProductId { get; set; }
        public string SerialNumber { get; set; }
        public DateTime Created { get; set; }
    }

    public record LegacyData
    {
        public string Po { get; set; }
        public string Co { get; set; }
        public string CcApproved { get; set; }
        public string LnNo { get; set; }
        public string Status { get; set; }
        public string BatchSeq { get; set; }
    }

    public UpdateExentaStatusesService(IShopfloorDbContextFactory contextFactory,
        ILogger<UpdateExentaStatusesService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public bool UpdateExentaStatuses(int minutes)
    {
        List<UpdateData> data = CollectData(minutes);
        return UpdateMagic(data);
    }

    /// <summary>
    /// update the magic DB with the new status
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private bool UpdateMagic(List<UpdateData> data)
    {
        return true;
    }

    /// <summary>
    /// get all the data to update for all DBs
    /// </summary>
    /// <param name="minutes"></param>
    /// <returns>distinct PO / serial number data to update</returns>
    public List<UpdateData> CollectData(int minutes)
    {
        List<UpdateData> data = new();
        foreach (string sf in _shopfloors)
        {
            _logger.LogInformation($"Exenta update status starting for {sf}");
            data.AddRange(GetUpdateData(minutes, _contextFactory.GetContext(sf)));
            _logger.LogInformation($"Exenta update status ending for {sf}");
        }
        return data.DistinctBy(x => x.SerialNumber).ToList();
    }

    /// <summary>
    /// get the update data for a specific DB instance
    /// </summary>
    /// <param name="minutes"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    private List<UpdateData> GetUpdateData(int minutes, ShopfloorDbContext context)
    {
        // Calculate cutoff time (equivalent to DATEADD(minute, -args.time, GETUTCDATE()))
        DateTime cutoff = DateTime.UtcNow.AddMinutes(-minutes);

        var query =
            from u in context.Units.AsNoTracking()
            join t in context.Transactions.AsNoTracking()
                on u.Id equals t.UnitId into ut // LEFT JOIN
            from t in ut.DefaultIfEmpty()
            join wo in context.WorkOrders.AsNoTracking()
                on t.WorkorderId equals wo.Id
            join po in context.ProductOperations.AsNoTracking()
                on new { t.OperationId, wo.ProductId }
                equals new { po.OperationId, po.ProductId }
            join m in context.MileStones.AsNoTracking()
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

    private List<LegacyData> GetLegacyData()
    {
        // Entity Framework LINQ query
        var query = from dpd in context.DyePrintDetails
                    join dp in context.DapPartners
                        on dpd.Po equals dp.Po
                    where !excludedStatuses.Contains(dpd.Status)
                    select new LegacyData
                    {
                        Po = dpd.Po,
                        Co = dpd.CoNumber,
                        CcApproved = dpd.CcApproved,
                        LnNo = dpd.LnNo,
                        Status = dpd.Status,
                        BatchSeq = dpd.BatchId + "_" + dpd.PrintOrder
                    };

        // Execute query (e.g., ToList)
        var results = query.ToList();
        return results;

    }
}