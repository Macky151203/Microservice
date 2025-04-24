using Microsoft.EntityFrameworkCore;
using OrderService.Models;
namespace OrderService.Data;

public class OrderDBContext : DbContext
{
   
    public OrderDBContext(DbContextOptions<OrderDBContext> options) : base(options)
    {
        Console.WriteLine("OrderDBContext constructor called.");
    }

    public DbSet<Orders> Order { get; set; }

}
