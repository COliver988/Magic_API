using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using MWW_Api.Config;
using MWW_Api.Models.Shopfloor;
using MWW_Api.Repositories.Exenta;
using MWW_MagicAPI.Data.Models.DTO;
using System.Globalization;
using System.Text;

namespace MWW_MagicAPI.Services;

public class FixBatchService : IFixBatchService
{
    private readonly IShopfloorDbContextFactory _contextFactory;
    private IConfiguration _configuration;
    private readonly MagicDbContext _magicDbContext;
    private IGetBatchUnitValues _getBatchUnitValues;
    private ShopfloorDbContext _context;
    private string _timeStamp;
    private static readonly HashSet<string> ExcludedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
       "queue", "ready", "pods"
    };


    public FixBatchService(IShopfloorDbContextFactory contextFactory,
        MagicDbContext magicDbContext,
        IGetBatchUnitValues getBatchUnitValues,
        IConfiguration configuration)
    {
        _contextFactory = contextFactory;
        _magicDbContext = magicDbContext;
        _getBatchUnitValues = getBatchUnitValues;
        _configuration = configuration;
        _timeStamp = DateTime.Now.ToString("MMddyyyyHHmmss");
    }

    public async Task<List<WorkOrderDataDTO>?> GetMissingBatches(string batchId)
    {
        // get the correct Shopfloor context
        _context = _contextFactory.GetContext(batchId);
        List<WorkOrderDataDTO> workorderData = await gatherWorkorderData(batchId);
        write_to_workorder_file(workorderData, batchId);
        await write_to_workorder_units_file(batchId);

        return workorderData;
    }

    private async Task<List<WorkOrderDataDTO>> gatherWorkorderData(string batchId)
    {
        List<MagicUnit> missingUnits = await getMissingBatches(batchId);
        if (!missingUnits.Any()) return null;
        return await getWorkorderData(missingUnits);
    }

    private async Task<List<WorkOrderDataDTO>> getWorkorderData(List<MagicUnit> missingUnits)
    {
        var semaphore = new SemaphoreSlim(10); // Limit to 10 concurrent tasks
        var tasks = new List<Task<WorkOrderDataDTO?>>();

        foreach (var unit in missingUnits)
        {
            tasks.Add(Task.Run(async () =>
            {
                await semaphore.WaitAsync();
                try
                {
                    return await _getBatchUnitValues.GetBatchUnitValue(unit.ProdNoCompany, unit.OpenSeq.Value);
                }
                finally
                {
                    semaphore.Release();
                }
            }));
        }
        WorkOrderDataDTO?[] results = await Task.WhenAll(tasks);

        return results
            .Where(dto => dto != null)
            .Select(dto => dto!)
            .ToList();
    }

    /// <summary>
    /// write to file; 1st to temp file, then move to final location to prevent hot folder grabbing incomplete file
    /// </summary>
    /// <param name="workorderFileData"></param>
    private void write_to_workorder_file(List<WorkOrderDataDTO> workorderData, string batchId)
    {
        if (workorderData == null || workorderData.Count() == 0) return;
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ",", // Specify the delimiter
            Mode = CsvMode.NoEscape, // Disable escaping and quoting
        };
        string tempFilePath = Path.Combine(Path.GetTempFileName());
        using (StreamWriter writer = new StreamWriter(tempFilePath, false, Encoding.UTF8))
        using (var csv = new CsvWriter(writer, config))
        {
            csv.WriteRecords(workorderData);
        }

        // move to final location and cleanup
        if (File.Exists(tempFilePath))
        {
            string finalFilePath = $"{getPath(batchId)}\\MWW-{_timeStamp}-Workorder.Exenta";
            Directory.CreateDirectory(Path.GetDirectoryName(finalFilePath)!);
            if (File.Exists(finalFilePath))
                File.Delete(finalFilePath);
            File.Move(tempFilePath, finalFilePath);
            File.Delete(tempFilePath);
        }
    }

    private async Task write_to_workorder_units_file(string batchId)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ",", // Specify the delimiter
            Mode = CsvMode.NoEscape, // Disable escaping and quoting
        };
        string tempFilePath = Path.Combine(Path.GetTempFileName());
        Directory.CreateDirectory(Path.GetDirectoryName(tempFilePath)!);
        List<WorkOrderUnitData> unitData = await GetFormattedPrintDetails(batchId);

        using (StreamWriter writer = new StreamWriter(tempFilePath, false, Encoding.UTF8))
        using (var csv = new CsvWriter(writer, config))
        {
            csv.WriteRecords(unitData);
        }
        if (File.Exists(tempFilePath))
        {
            string finalFilePath = $"{getPath(batchId)}\\{batchId}-WorkorderUnits.MWW";
            Directory.CreateDirectory(Path.GetDirectoryName(finalFilePath)!);
            if (File.Exists(finalFilePath))
                File.Delete(finalFilePath);
            File.Move(tempFilePath, finalFilePath);
            File.Delete(tempFilePath);
        }
    }

    private async Task<List<WorkOrderUnitData>> GetFormattedPrintDetails(string batchId)
    {
        return await (from dpd in _magicDbContext.DyePrintDetails.AsNoTracking()
                      join ack in _magicDbContext.ExentaPOLinesWithAckNos.AsNoTracking()
                          on new { dpd.PO, LineNo = (int)dpd.Ln_No } equals new { ack.PO, LineNo = ack.LN_NO }
                      join doh in _magicDbContext.DyePrintHeaders.AsNoTracking()
                          on dpd.PO equals doh.PO
                      from dih in _magicDbContext.DyeItemAttributes
                      where dpd.BatchID == batchId
                            && !ExcludedStatuses.Contains(dpd.Status)
                            && (doh.ItemCode == dih.MWWItemCode || doh.ItemCode == (dih.MWWItemCode + "-1"))
                      orderby dpd.printedOrder
                      select new WorkOrderUnitData
                      {
                          Workorder = ack.ProdNoCompany + "-" + ack.OpenSeq.ToString(),
                          Batch = dpd.BatchID,
                          Seq = (int)dpd.printedOrder,
                          Unit = $"{dpd.BatchID}_{dpd.printedOrder}",
                          Thumbnail = $"{dpd.PO}_{dpd.printedOrder}.jpg",
                          Quantity = 1,
                          Flag = "I",
                          ProductId = dih.Style,
                          Size = dih.Size,
                      }).ToListAsync();
    }

    /// <summary>
    /// compare magic units to shopfloor units and return missing ones
    /// </summary>
    /// <param name="batchId"></param>
    /// <returns></returns>
    private async Task<List<MagicUnit>> getMissingBatches(string batchId)
    {
        List<string> unitsInShopfloor = await getUnitsFromShopfloor(batchId);
        List<MagicUnit> unitsInMagic = await getUnitsFromMagic(batchId);
        return unitsInMagic
            .Where(d => !unitsInShopfloor.Contains($"{d.BatchID}_{d.PrintOrder}"))
            .OrderBy(d => d.PrintOrder)
            .ToList();
    }

    /// <summary>
    /// return just the alphanumid for a simple match
    /// </summary>
    /// <param name="batchId"></param>
    /// <returns></returns>
    private async Task<List<string>> getUnitsFromShopfloor(string batchId)
    {
        return await _context.Units
            .Where(u => u.BatchId == batchId)
            .Select(u => u.AlphaNumId)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// get units from Magic database for the specified batch ID
    /// </summary>
    /// <param name="batchId"></param>
    /// <returns></returns>
    private async Task<List<MagicUnit>> getUnitsFromMagic(string batchId)
    {
        var rawResults = await _magicDbContext.DyePrintDetails
            .Where(dpd => dpd.BatchID == batchId)
            .Join(
                _magicDbContext.ExentaPOLinesWithAckNos,
                dpd => new { dpd.PO, LineNo = (int)dpd.Ln_No },
                ack => new { PO = ack.PO, LineNo = ack.LN_NO },
                (dpd, ack) => new { dpd, ack }
            )
            .AsNoTracking()
            .ToListAsync();

        var magicUnits = rawResults
            .Select(x => new MagicUnit
            {
                ProdNoCompany = int.TryParse(x.ack.ProdNoCompany?.Trim(), out var prodNo) ? prodNo : 0,
                OpenSeq = x.ack.OpenSeq,
                BatchID = x.dpd.BatchID!,
                PrintOrder = x.dpd.printedOrder.HasValue ? (short)x.dpd.printedOrder.Value : x.dpd.PrintOrder
            })
            .ToList();

        return magicUnits;
    }

    /// <summary>
    /// determine the mounted drive to write the workorder file to based on batch prefix
    /// </summary>
    /// <param name="batchID">batch ID string</param>
    /// <returns>correct mounted drive location</returns>
    /// <exception cref="InvalidOperationException"></exception>
    private string getPath(string batchID)
    {
        var shopfloor = _configuration.GetSection("Shopfloor");
        string? path = shopfloor?[batchID.Substring(0, 2).ToLower()];
        if (string.IsNullOrEmpty(path))
            path = shopfloor?["mww"];
        if (string.IsNullOrEmpty(path))
            throw new InvalidOperationException("No valid Shopfloor path found in configuration");
        return path;
    }
}