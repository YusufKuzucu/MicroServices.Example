using MassTransit;
using MongoDB.Driver;
using Shared;
using Shared.Events;
using Shared.Messages;
using Stock.API.Services;

namespace Stock.API.Consumers
{
    public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
    {
        IMongoCollection<Stock.API.Models.Entities.Stock> _stockCollection;
        readonly ISendEndpointProvider _sendEndPointProvider;
        readonly IPublishEndpoint _publishEndpoint;
        public OrderCreatedEventConsumer(MongoDbService mongoDbService, ISendEndpointProvider sendEndPointProvider, IPublishEndpoint publishEndpoint)
        {
            _stockCollection = mongoDbService.GetCollection<Stock.API.Models.Entities.Stock>();
            _sendEndPointProvider = sendEndPointProvider;
            _publishEndpoint = publishEndpoint;
        }
        public  async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            List<bool> stockResult = new();
            foreach (OrderItemMessage orderItems in context.Message.OrderItems)
            { 
                stockResult.Add((await _stockCollection.FindAsync(s => s.ProductId == orderItems.ProductId && s.Count >= orderItems.Count)).Any());
            }
            if (stockResult.TrueForAll(sr => sr.Equals(true)))
            {
                foreach (OrderItemMessage orderItems in context.Message.OrderItems)
                {
                    Stock.API.Models.Entities.Stock stock=(await(await _stockCollection.FindAsync(c => c.ProductId == orderItems.ProductId)).FirstOrDefaultAsync());
                    stock.Count -= orderItems.Count;
                    await _stockCollection.FindOneAndReplaceAsync(s => s.ProductId == orderItems.ProductId, stock);
                }
                //payment....
                StockReservedEvent stockReservedEvent = new()
                {
                    BuyerId = context.Message.BuyerId,
                    OrderId = context.Message.OrderId,
                    TotalPrice = context.Message.TotalPrice,
                };
                  ISendEndpoint sendEndpoint= await _sendEndPointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.Payment_StockReservedEventQueue}"));
                await sendEndpoint.Send(stockReservedEvent);//yayınladık
                Console.WriteLine("Stock Başarılı");
            }
            else
            {
                //Siparişin tutarsız/geçersiz olduğuna dair işlemler
                StockNotReservedEvent stockNotReservedEvent = new()
                {
                    BuyerId = context.Message.BuyerId,
                    OrderId = context.Message.OrderId,
                    Message = "...."
                };
               await _publishEndpoint.Publish(stockNotReservedEvent);
                Console.WriteLine("Stock Başarısız");
            }
        }
    }
}
