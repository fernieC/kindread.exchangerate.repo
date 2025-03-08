using KindRed.Exchange.Services;
var builder = WebApplication.CreateBuilder(args);
//
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register services
builder.Services.AddHttpClient();
builder.Services.AddScoped<ICurrencyService, ExchangeRateApi>();
builder.Services.AddSingleton<ICurrencyCodeService, CurrencyCodeService>();
builder.Services.AddMemoryCache();

// Add controllers
builder.Services.AddControllers();
var app = builder.Build();
app.MapControllers();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

}

app.UseHttpsRedirection();



app.Run();


