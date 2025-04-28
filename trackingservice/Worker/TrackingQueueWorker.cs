namespace TrackingService.Worker;

using TrackingService.Data;
using TrackingService.Models;
using System.Text.Json;
using StackExchange.Redis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
public class TrackingServiceWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TrackingServiceWorker> _logger;
    private readonly IDatabase _redisDB;

    public TrackingServiceWorker(IServiceProvider serviceProvider, ILogger<TrackingServiceWorker> logger, IConnectionMultiplexer redis)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _redisDB = redis.GetDatabase();
        Console.WriteLine("TrackingServiceWorker started.");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Check if there is any data in the Redis queue
            var data = await _redisDB.ListLeftPopAsync("queue");
            if (!data.IsNullOrEmpty)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<TrackingDBContext>();

                    // Deserialize data from the queue
                    var dataFromQueue = JsonSerializer.Deserialize<OrderExtract>(data);
                    var orderid = Guid.Parse(dataFromQueue.Id.ToString());
                    var ordertype = dataFromQueue.Type;

                    if (ordertype == "push")
                    {
                        // Create a new model to store order ID and type
                        var trackingData = new TrackingData
                        {
                            Id = orderid,
                            Type = ordertype,
                            Status = "Dispatched",
                            Location = "In warehouse"
                        };

                        // Add to the database
                        await dbContext.Tracking.AddAsync(trackingData);
                        await dbContext.SaveChangesAsync();
                        _logger.LogInformation($"Order ID: {orderid}");
                        _logger.LogInformation($"Order Type: {ordertype}");
                        Console.WriteLine("Order tracking added.");
                    }
                    else if (ordertype == "track")
                    {
                        // Get order from the database and push to queue2
                        var trackingdetails = await dbContext.Tracking.FirstOrDefaultAsync(x => x.Id == orderid);
                        if (trackingdetails != null)
                        {
                            var trackingData = new { Id = trackingdetails.Id, Type = trackingdetails.Type, Status = trackingdetails.Status, Location = trackingdetails.Location };
                            var trackingDataJson = JsonSerializer.Serialize(trackingData);
                            await _redisDB.ListRightPushAsync("queue2", trackingDataJson);
                            Console.WriteLine("Order tracking details pushed to queue2.");
                        }
                    }
                }
            }
            else
            {
                // _logger.LogInformation("No data in the queue.");
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}



