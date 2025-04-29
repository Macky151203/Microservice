using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using TrackingService.Data;
// using TrackingService.Worker;
using System.Net.Http;
using System.Text.Json;
using TrackingService.Models;

var httpclient = new HttpClient();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect("localhost:6379"));
builder.Services.AddDbContext<TrackingDBContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
// builder.Services.AddHostedService<TrackingServiceWorker>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

_ = Task.Run(async () =>
{
    while (true)
    {
        try
        {
            var redis = app.Services.GetRequiredService<IConnectionMultiplexer>();
            var redisDB = redis.GetDatabase();
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TrackingDBContext>();
                await checkqueue(httpclient, redisDB, dbContext);
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        await Task.Delay(1000);
    }
});

async Task checkqueue(HttpClient httpclient, IDatabase redisDB, TrackingDBContext dbContext)
{
    var data = await redisDB.ListLeftPopAsync("queue");
    if (!data.IsNullOrEmpty)
    {



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
            // _logger.LogInformation($"Order ID: {orderid}");
            // _logger.LogInformation($"Order Type: {ordertype}");
            Console.WriteLine("Order tracking added.");
        }
    
    }
}

app.Run();

