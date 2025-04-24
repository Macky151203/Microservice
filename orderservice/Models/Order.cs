namespace OrderService.Models;

public class Orders{
    public Guid Id { get; set; }//order id
    public string CustomerName { get; set; } 
    public string CustomerEmail { get; set; } 
    public string CustomerPhone { get; set; } 
    public string Productname { get; set; }
    public decimal TotalAmount { get; set; } 
}