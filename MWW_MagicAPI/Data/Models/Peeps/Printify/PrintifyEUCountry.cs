using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace MWW_Api.Models.Peeps.Printify;

[Table("printify_eu_countries")]
public class PrintifyEUCountry
{
    [Column("id")]
    public long Id { get; set; }

    [Column("country_code")]
    [DisplayName("Country Code")]
    public string? CountryCode { get; set; }
}
