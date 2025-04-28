using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;
using System;
using TrackingService.Models;
using TrackingService.Data;

[Route("/api/[controller]")]
[ApiController]
public class TrackingServiceController : ControllerBase
{
    private readonly TrackingDBContext _trackingDBContext;
    // private readonly IDatabase _redisDB;
    // private const string RedisKey = "queue";
    // private const string RedisKey2 = "queue2";

    public TrackingServiceController(TrackingDBContext trackingDBContext)
    {
        _trackingDBContext = trackingDBContext;
        // _redisDB = redis.GetDatabase();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var trackingData = await _trackingDBContext.Tracking.ToListAsync();
        if (trackingData.Count == 0)
        {
            return NotFound("No tracking data found");
        }
        return Ok(trackingData);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] OrderExtract Data)
    {
        if (Data == null)
        {
            return BadRequest("Invalid tracking data.");
        }
        
        var orderId = Data.Id;
        var orderType = "push";
        var trackingData = new TrackingData{ Id = orderId, Type = orderType, Status = "In  warehouse", Location = "Location" };
        // var trackingDataJson = JsonSerializer.Serialize(trackingData);
        
        await _trackingDBContext.Tracking.AddAsync(trackingData);
        await _trackingDBContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { id = trackingData.Id }, trackingData);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var trackingData = await _trackingDBContext.Tracking.FirstOrDefaultAsync(x => x.Id == id);
        if (trackingData == null)
        {
            return NotFound("Tracking data not found.");
        }
        return Ok(trackingData);
    }
}
