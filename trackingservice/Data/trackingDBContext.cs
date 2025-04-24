namespace TrackingService.Data;
using Microsoft.EntityFrameworkCore;
using TrackingService.Models;

public class TrackingDBContext : DbContext
{
    public TrackingDBContext(DbContextOptions<TrackingDBContext> options) : base(options)
    {
        Console.WriteLine("TrackingDBContext constructor called.");
    }

    public DbSet<TrackingData> Tracking { get; set; }
}