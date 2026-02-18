using MWW_Api.Models.Peeps.Printify;

namespace MWW_Api.Services.Peeps.PrintifyServices;
public interface IPrintifyHttpClientService
{
    Task<bool> SendPrintifyUpdateAsync(PrintifyOrder order, string status);
}