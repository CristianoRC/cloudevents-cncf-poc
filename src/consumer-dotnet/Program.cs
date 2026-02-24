using ConsumerDotnet.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<IEventStore, InMemoryEventStore>();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Consumer Dotnet - CloudEvents",
        Version = "v1",
        Description = "API que recebe e armazena CloudEvents de qualquer producer"
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.Run();
