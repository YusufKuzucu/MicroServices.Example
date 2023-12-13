using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.API.Models;
using Order.API.Models.Entities;
using Order.API.Models.Enums;
using Order.API.ViewModels;
using Shared.Events;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        readonly OrderAPIDbContext _Context;
        readonly IPublishEndpoint _publishEndpoint;

        public OrdersController(OrderAPIDbContext orderAPIDbContext, IPublishEndpoint publishEndpoint)
        {
            _Context = orderAPIDbContext;
            _publishEndpoint = publishEndpoint;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(CreateOrderVM createOrderVM)
        {
            Order.API.Models.Entities.Order order = new()
            {
                OrderId = Guid.NewGuid(),
                CreatedDate = DateTime.Now,
                BuyerId=createOrderVM.BuyerId,
                OrderStatus = OrderStatus.Suspend,
            };
            order.OrderItems = createOrderVM.OrderItems.Select(oi => new OrderItem
            {
                Count=oi.Count,
                Price=oi.Price,
                ProductId=oi.ProductId,
            }).ToList();
            order.TotalPrice=createOrderVM.OrderItems.Sum(oi=> (oi.Price*oi.Count));
            await _Context.AddAsync(order);
            await _Context.SaveChangesAsync();

            OrderCreatedEvent orderCreatedEvent = new()
            {
                BuyerId = order.BuyerId,
                OrderId = order.OrderId,
                OrderItems = order.OrderItems.Select(oi => new Shared.Messages.OrderItemMessage {
                    Count=oi.Count,
                    ProductId=oi.ProductId,
                }).ToList() ,
                TotalPrice=order.TotalPrice,
                
            };
           await _publishEndpoint.Publish(orderCreatedEvent);    
            return Ok();
        }
    }
}
