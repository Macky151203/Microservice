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
}
