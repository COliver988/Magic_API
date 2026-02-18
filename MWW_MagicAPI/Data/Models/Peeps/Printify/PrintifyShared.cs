namespace MWW_Api.Models.Peeps.Printify;

public static class PrintifyShared
{
    public enum PrintifyStatuses
    {
        created,
        picked,
        printed,
        packaged,
        shipped,
        cancelled
    }

    public static string FacilityName = "MWW";
}
