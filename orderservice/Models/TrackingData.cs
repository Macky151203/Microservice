namespace OrderService.Models;

public class TrackingData{
    public Guid Id { get; set; } //order id
    public string Type { get; set; } 
    public string Status { get; set; } 
    public string Location { get; set; } //location of the order
} 