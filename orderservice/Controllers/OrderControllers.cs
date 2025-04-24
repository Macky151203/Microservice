namespace OrderService.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using OrderService.Models;
using OrderService.Data;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;
using System;

[Route("/api/[controller]")]
[ApiController]

public class OrdersController : ControllerBase
{
    private readonly OrderDBContext _orderDBContext;
    private readonly IDatabase _redisDB;
    private const string RedisKey = "queue";
    private const string RedisKey2 = "queue2";
    public OrdersController(OrderDBContext orderDBContext, IConnectionMultiplexer redis)//injecting the db context that we have registered in program.cs
    {
        _orderDBContext = orderDBContext;
        _redisDB = redis.GetDatabase();
    }
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orders = await _orderDBContext.Order.ToListAsync();
        if (orders.Count == 0)
        {
            return NotFound("No orders found");
        }
        return Ok(orders);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Orders order)
    {
        if (order == null)
        {
            return BadRequest("Invalid order data.");
        }
        await _orderDBContext.Order.AddAsync(order);
        await _orderDBContext.SaveChangesAsync();
        //after adding to order make a push to the redis queue with orderid and type as push
        var orderId = order.Id.ToString();
        var orderType = "push";
        var orderData = new { Id = orderId, Type = orderType };
        var orderDataJson = JsonSerializer.Serialize(orderData);
        await _redisDB.ListRightPushAsync(RedisKey, orderDataJson);

        //try popping to check push and also need to make new class obj for incoming response
        // var data = await _redisDB.ListLeftPopAsync(RedisKey);
        // var orderDataFromQueue = JsonSerializer.Deserialize<OrderExtract>(data);
        // if (orderDataFromQueue == null)
        // {
        //     return BadRequest("Failed to pop order data from queue.");
        // }
        // Console.WriteLine($"Order ID: {Guid.Parse(orderDataFromQueue.Id.ToString())}");
        // //create a new model to store orderid and type
        // Console.WriteLine($"Order Type: {orderDataFromQueue.Type}");

        //It is getting popped , so now do the same in the other service by popping and adding to tracking db
        //To send data back to the order service, listen to a queue after send get tracking requesting to the main queue

        return CreatedAtAction(nameof(GetAll), new { id = order.Id }, order);
    }


    [HttpGet("/track/{id}")]
    public async Task<IActionResult> GetDetailsByd(Guid id)
    {
        //push to the mq with type as track 
        var trackingdata = new { Id = id.ToString(), Type = "track" };
        var trackingdataJson = JsonSerializer.Serialize(trackingdata);
        await _redisDB.ListRightPushAsync(RedisKey, trackingdataJson);


        //listen from another queue for response
        // var result = await _redisDB.ListLeftPopAsync(new RedisKey[] {RedisKey2},TimeSpan.FromSeconds(10));
        // if (result.IsNullOrEmpty)
        // {
        //     return NotFound("No tracking data found");
        // }
        // var trackingDataFromQueue= JsonSerializer.Deserialize<OrderExtract>(result);
        // Console.WriteLine($"Order ID: {Guid.Parse(trackingDataFromQueue.Id.ToString())}");
        // Console.WriteLine($"Order Type: {trackingDataFromQueue.Type}");

        // return NoContent();
        var timeout = TimeSpan.FromSeconds(10);
        var startTime = DateTime.UtcNow;

        while ((DateTime.UtcNow - startTime) < timeout)
        {
            var result = await _redisDB.ListLeftPopAsync(RedisKey2);
            if (!result.IsNullOrEmpty)
            {
                //need to change object for receiving response from queue
                var trackingDataFromQueue = JsonSerializer.Deserialize<TrackingData>(result);
                Console.WriteLine($"Order ID: {Guid.Parse(trackingDataFromQueue.Id.ToString())}");
                Console.WriteLine($"Order Type: {trackingDataFromQueue.Type}");
                Console.WriteLine($"Order Status: {trackingDataFromQueue.Status}");
                Console.WriteLine($"Order Location: {trackingDataFromQueue.Location}");
                return Ok(trackingDataFromQueue);
            }

            await Task.Delay(500);
        }

        return NotFound("No tracking data found within the timeout period.");
    }
}
public class TrackingData{
    public Guid Id { get; set; } //order id
    public string Type { get; set; } 
    public string Status { get; set; } 
    public string Location { get; set; } //location of the order
}