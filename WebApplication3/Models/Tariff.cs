using System.ComponentModel.DataAnnotations;

public class Tariff
{
    [Key]
    public int CityCode { get; set; }
    public string CityName { get; set; }
    public decimal PricePerMinute { get; set; }
}