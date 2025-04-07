using System.ComponentModel.DataAnnotations;
public class Call


{
    public int Id { get; set; }
    public string OwnerFIO { get; set; }
    public int CityCode { get; set; }
    public string CityName { get; set; }
    public int DurationMinutes { get; set; }
    public decimal Cost { get; set; }
    public DateTime CallDate { get; set; }
}