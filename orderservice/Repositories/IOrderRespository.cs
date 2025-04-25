namespace OrderService.Repositories;

using OrderService.Models;

public interface IOrderRepository{
    Task<List<Orders>> GetAllAsync();
    Task<Orders> CreateOrderAsync(Orders order);
    Task<TrackingData> GetTrackingAsync(Guid id);  

}