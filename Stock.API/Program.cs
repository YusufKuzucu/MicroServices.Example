using MassTransit;
using Shared;
using Stock.API.Consumers;
using Stock.API.Services;
using MongoDB.Driver;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<OrderCreatedEventConsumer>();
    configurator.UsingRabbitMq((context, _configurator) =>
    {
        _configurator.Host(builder.Configuration["RabbitMQ"]);
        _configurator.ReceiveEndpoint(RabbitMQSettings.Stock_OrderCreatedEventQueue,e=>e.ConfigureConsumer<OrderCreatedEventConsumer>(context));
    });
});
builder.Services.AddSingleton<MongoDbService>();

#region harici -MongoDb'ye seed data ekledik
using IServiceScope scope = builder.Services.BuildServiceProvider().CreateScope();
MongoDbService mongoDbService = scope.ServiceProvider.GetService<MongoDbService>();
var collection = mongoDbService.GetCollection<Stock.API.Models.Entities.Stock>();
if (!collection.FindSync(s => true).Any())
{
    await collection.InsertOneAsync(new() { ProductId ="1", Count = 2000 });
    await collection.InsertOneAsync(new() { ProductId = "2", Count = 1000 });
}
#endregion






// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
