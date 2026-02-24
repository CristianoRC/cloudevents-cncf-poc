using ConsumerDotnet.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<IEventStore, InMemoryEventStore>();

var app = builder.Build();

app.MapControllers();
app.Run();
