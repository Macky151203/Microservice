namespace OrderService.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using OrderService.Models;
using OrderService.Data;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;
using System;
using OrderService.Services;

[Route("/api/[controller]")]
[ApiController]

public class OrdersController : ControllerBase
{
    private readonly IOrderServices _orderService;
    public OrdersController(IOrderServices orderService)//injecting the db context that we have registered in program.cs
    {
        _orderService=orderService;
    }
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orders = await _orderService.GetAllAsync();
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
        var neworder=await _orderService.CreateOrderAsync(order);

        return CreatedAtAction(nameof(GetAll), new { id = order.Id }, order);
    }

    
}

