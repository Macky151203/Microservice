namespace OrderService.Services;
using OrderService.Models;
using OrderService.Repositories;
public class OrderServices : IOrderServices
{
    //inject repository here and call its functions
    private readonly IOrderRepository _orderRepository;
    public OrderServices(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }
    public async Task<List<Orders>> GetAllAsync()
    {
        return await _orderRepository.GetAllAsync();
    }
    public async Task<Orders> CreateOrderAsync(Orders order)
    {
        return await _orderRepository.CreateOrderAsync(order);
    }

}