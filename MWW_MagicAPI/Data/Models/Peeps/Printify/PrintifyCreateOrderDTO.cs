using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MWW_Api.Models.Peeps.Printify;

/// <summary>
/// Printify Create Order DTO
/// </summary>
/// see <a href="https://supply.printify.com/integration/supply-api-specification#tag/Production-API/operation/submit-a-production-order-v2019-06">Printify API</a> 
public class PrintifyCreateOrderDTO
{
    //regex: ^[a-zA-Z0-9]{24}$
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("tags")]
    public List<string>? Tags { get; set; }

    [JsonProperty("address_to")]
    public AddressTo AddressTo { get; set; }

    [JsonProperty("address_from")]
    public AddressFrom AddressFrom { get; set; }

    [JsonProperty("shipping")]
    public Shipping Shipping { get; set; }

    [JsonProperty("sample")]
    public bool Sample { get; set; }

    [JsonProperty("reprint")]
    public bool Reprint { get; set; }

    [JsonProperty("xpc")]
    public bool Xpc { get; set; }

    // not sur ethis is in the production order
    [JsonProperty("facility_id")]
    public string? FacilityId { get; set; }

    [JsonProperty("items")]
    public List<Item> Items { get; set; }
}

public class AddressFrom
{
    [JsonProperty("address1")]
    [MaxLength(200)]
    public string Address1 { get; set; }

    [JsonProperty("address2")]
    [MaxLength(200)]
    public string Address2 { get; set; }

    [JsonProperty("city")]
    [MaxLength(200)]
    public string City { get; set; }

    [JsonProperty("zip")]
    [MaxLength(200)]
    public string Zip { get; set; }

    [JsonProperty("country")]
    [MaxLength(10)]
    public string Country { get; set; }

    [JsonProperty("region")]
    [MaxLength(200)]
    public string Region { get; set; }

    [JsonProperty("company")]
    [MaxLength(200)]
    public string Company { get; set; }

    [JsonProperty("email")]
    [MaxLength(200)]
    public string? Email { get; set; }

    [JsonProperty("phone")]
    [MaxLength(200)]
    public string? Phone { get; set; }
}

public class AddressTo
{
    [JsonProperty("address1")]
    [MaxLength(200)]
    public string Address1 { get; set; }

    [JsonProperty("address2")]
    [MaxLength(200)]
    public string Address2 { get; set; }

    [JsonProperty("city")]
    [MaxLength(200)]
    public string City { get; set; }

    // If country does not use ZIP codes it will be set to 00000
    [JsonProperty("zip")]
    [MaxLength(200)]
    public string Zip { get; set; }

    [JsonProperty("country")]
    [MaxLength(10)]
    public string Country { get; set; }

    [JsonProperty("region")]
    [MaxLength(200)]
    public string Region { get; set; }

    [JsonProperty("first_name")]
    [MaxLength(200)]
    public string FirstName { get; set; }

    [JsonProperty("last_name")]
    [MaxLength(200)]
    public string LastName { get; set; }

    [JsonProperty("email")]
    [MaxLength(200)]
    public string? Email { get; set; }

    [JsonProperty("phone")]
    [MaxLength(200)]
    public string? Phone { get; set; }
}

public class Item
{
    // regex:  ^[a-zA-Z0-9]{24}$
    [JsonProperty("id")]
    public string Id { get; set; }

    // regex: ^[a-zA-Z0-9_-]{1,40}$
    // SKU (Stock keeping unit). Important - case insensitive
    [JsonProperty("sku")]
    public string Sku { get; set; }

    [JsonProperty("preview_files")]
    public PreviewFiles PreviewFiles { get; set; }

    [JsonProperty("print_files")]
    public PrintFiles PrintFiles { get; set; }

    [JsonProperty("production_due_date")]
    public DateTime? ProductionDueDate { get; set; }

    [JsonProperty("quantity")]
    public int Quantity { get; set; }
}

public class PreviewFiles
{
    [JsonProperty("front")]
    public string? Front { get; set; }

    [JsonProperty("back")]
    public string? Back { get; set; }
}

public class PrintFiles
{
    [JsonProperty("front")]
    public string? Front { get; set; }

    [JsonProperty("back")]
    public string? Back { get; set; }

    [JsonProperty("left_sleeve")]
    public string? LeftSleeve { get; set; }
}

public class Shipping
{
    [JsonProperty("carrier")]
    [MaxLength(100)]
    public string Carrier { get; set; }

    [JsonProperty("priority")]
    [MaxLength(100)]
    public string Priority { get; set; }

    // shipping label URL
    [JsonProperty("label")]
    [MaxLength(2048)]
    public string? Label { get; set; }

    [JsonProperty("tracking_number")]
    public string? TrackingNumber { get; set; }

    /*
    Identifies the label source Print Provider should use to obtain the label.
    Values:
     * `print_provider` - Print Provider obtains the label as agreed with Printify.
     * `download_label` - Shipping Label is provided by Printify in `label` field along with `tracking_number`.
     * `easypost_proxy` - Print Provider is instructed to use Printify's EasyPost Proxy to obtain the label.
    */
    [JsonProperty("label_source")]
    [MaxLength(100)]
    public string? LabelSource { get; set; }
}


