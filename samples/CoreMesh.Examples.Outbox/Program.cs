using Confluent.Kafka;
using CoreMesh.Examples.Outbox.Data;
using CoreMesh.Examples.Outbox.Messaging;
using CoreMesh.Examples.Outbox.Messaging.Kafka;
using CoreMesh.Examples.Outbox.Todo;
using CoreMesh.Outbox.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=app.db"));

builder.Services
    .AddSingleton<IProducer<string, string>>(_ =>
    {
        var config = new ProducerConfig
        {
            BootstrapServers = builder.Configuration["KafkaOption:BootstrapServers"],
            SecurityProtocol = SecurityProtocol.Plaintext,
            Acks = Acks.All
        };
        return new ProducerBuilder<string, string>(config).Build();
    })
    .AddSingleton<IConsumer<string, string>>(_ =>
    {
        var config = new ConsumerConfig
        {
            BootstrapServers =  builder.Configuration["KafkaOption:BootstrapServers"],
            SecurityProtocol = SecurityProtocol.Plaintext,
            GroupId = builder.Configuration["KafkaOption:GroupId"],
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };
        return new ConsumerBuilder<string, string>(config).Build();
    })
    .AddHostedService<KafkaTopicInitializer>();

builder.Services.AddCoreMeshOutbox(
    [typeof(Program).Assembly],
    options =>
    {
        options.AddOutboxStore<EfCoreOutboxStore>()
               .AddOutboxWriter<EfCoreOutboxWriter>()
               .AddMessageQueue<KafkaEventPublisher, KafkaMessageSubscriber>()
               .WithConsumer();
    });

builder.Services.AddScoped<TodoService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("api/todo",
        async (TodoService todoService, CreateTodoCommand command) =>
        {
            await todoService.CreateAsync(command);
            return TypedResults.Created();
        })
    .WithName("CreateTodo");

app.Run();
