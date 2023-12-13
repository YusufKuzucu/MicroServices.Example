using MassTransit;
using Shared.Events;

namespace Payment.API.Consumers
{
    public class StockReservedEventConsumer : IConsumer<StockReservedEvent>
    {
        readonly IPublishEndpoint _publishEndpoint;

        public StockReservedEventConsumer(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<StockReservedEvent> context)
        {
            //ödeme işlemleri
            if (true)
            {
                PaymentCompletedEvent paymanetCompleted = new()
                {
                    OrderId = context.Message.OrderId
                };
                await _publishEndpoint.Publish(paymanetCompleted);
                Console.WriteLine("Ödeme başarılı");
            }
            else
            {
                PaymentFailedEvent paymentFailedEvent = new()
                {
                    OrderId = context.Message.OrderId,
                    Message = "Bakiye Yetersiz"
                };
                await _publishEndpoint.Publish(paymentFailedEvent);
            }

        }
    }
}
