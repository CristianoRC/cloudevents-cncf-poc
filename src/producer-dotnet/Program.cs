using ProducerDotnet.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient<ICloudEventPublisher, CloudEventPublisher>();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Producer Dotnet - CloudEvents",
        Version = "v1",
        Description = "API que produz CloudEvents de pedidos (order.created, order.shipped)"
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.Run();
