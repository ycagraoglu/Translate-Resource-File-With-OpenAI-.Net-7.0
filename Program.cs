using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.GPT3.Extensions;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddLocalization(o => o.ResourcesPath = "Resources");
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOpenAIService(settings => settings.ApiKey = "your_api_key");

var supportedCultures = new[]
   {
                new CultureInfo("tr-TR"),
                new CultureInfo("de-DE"),
                new CultureInfo("en-GB"),
            };


builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
                new CultureInfo("tr-TR"),
                new CultureInfo("de-DE"),
                new CultureInfo("en-GB"),
            };
    options.DefaultRequestCulture = new RequestCulture(culture: "tr-TR", uiCulture: "tr-TR");

    // You must explicitly state which cultures your application supports.
    // These are the cultures the app supports for formatting 
    // numbers, dates, etc.

    options.SupportedCultures = supportedCultures;

    // These are the cultures the app supports for UI strings, 
    // i.e. we have localized resources for.

    options.SupportedUICultures = supportedCultures;
});


builder.Services.AddScoped<IResourceTranslator, ResourceTranslator>();

var app = builder.Build();

IOptions<RequestLocalizationOptions>? localizationOptions = app.Services.GetService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(localizationOptions?.Value);

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


