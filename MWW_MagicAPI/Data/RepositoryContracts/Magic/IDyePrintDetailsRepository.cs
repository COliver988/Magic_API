namespace MWW_Api.Repositories.Magic;

public interface IDyePrintDetailsRepository
{
     Task UpdateDyePrintDetailsStatusAsync(string po, string co, string lnNo, string status);       
}