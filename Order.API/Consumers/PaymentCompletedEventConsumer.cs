using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Models;
using Order.API.Models.Enums;
using Shared.Events;

namespace Order.API.Consumers
{
    public class PaymentCompletedEventConsumer : IConsumer<PaymentCompletedEvent>
    {
        readonly OrderAPIDbContext _context;

        public PaymentCompletedEventConsumer(OrderAPIDbContext context)
        {
            _context = context;
        }

        public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
        {
            Order.API.Models.Entities.Order order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == context.Message.OrderId);
            order.OrderStatus = OrderStatus.Completed;
            await _context.SaveChangesAsync();  
        }
    }
}
