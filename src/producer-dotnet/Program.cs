using ProducerDotnet.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient<ICloudEventPublisher, CloudEventPublisher>();

var app = builder.Build();

app.MapControllers();
app.Run();
