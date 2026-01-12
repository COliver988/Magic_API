namespace MWW_Api.Models.Peeps.Printify;

public class PrintifyAddress
{
    public long Id { get; set; }
    public Int64 OrderId { get; set; }
    public string? Type { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? Zip { get; set; }
    public string? Country { get; set; }
    public string? Company { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public TimeSpan CreatedAt { get; set; }
    public TimeSpan UpdatedAt { get; set; }
}
