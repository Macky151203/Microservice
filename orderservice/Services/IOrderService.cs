namespace OrderService.Services;
using OrderService.Models;
public interface IOrderServices
{
    Task<List<Orders>> GetAllAsync();
    Task<Orders> CreateOrderAsync(Orders order);
    
}