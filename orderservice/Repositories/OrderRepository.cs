namespace OrderService.Repositories;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using StackExchange.Redis;
using System.Text.Json;
using OrderService.Models;
public class OrderRepository : IOrderRepository
{
    private readonly OrderDBContext _orderDBContext;
    private readonly IDatabase _redisDB;
    private readonly HttpClient _httpClient;
    private string RedisKey;
    private string RedisKey2;
    private readonly string _trackingserviceurl;

    private readonly IConfiguration _configuration;
    public OrderRepository(OrderDBContext orderDBContext, IConnectionMultiplexer redis, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _configuration = configuration;
        _trackingserviceurl= _configuration.GetValue<string>("TrackingService:BaseUrl");
        _orderDBContext = orderDBContext;
        _redisDB = redis.GetDatabase();
        _httpClient = httpClientFactory.CreateClient();
        RedisKey = _configuration.GetValue<string>("RedisKey:Key1");
        RedisKey2 = _configuration.GetValue<string>("RedisKey:Key2");
    }
    public async Task<List<Orders>> GetAllAsync()
    {
        return await _orderDBContext.Order.ToListAsync();
    }

    public async Task<Orders> CreateOrderAsync(Orders order)
    {
        
        await _orderDBContext.Order.AddAsync(order);
        await _orderDBContext.SaveChangesAsync();
        //after adding to order make a push to the redis queue with orderid and type as push
        var url=_trackingserviceurl;
        var orderId = order.Id;
        var orderType = "push";
        var orderData = new { Id = orderId, Type = orderType };
        var orderDataJson = JsonSerializer.Serialize(orderData);


        //for http method
        // var content = new StringContent(orderDataJson, System.Text.Encoding.UTF8, "application/json");
        // var response = await _httpClient.PostAsync(url, content);

        // if (!response.IsSuccessStatusCode)
        // {
        //     return null;
        // }
        // return order;

        //for queue logic
        await _redisDB.ListRightPushAsync(RedisKey, orderDataJson);


        //It is getting popped , so now do the same in the other service by popping and adding to tracking db
        //To send data back to the order service, listen to a queue after send get tracking requesting to the main queue

        return order;
    }
    
}