using WebApp.Services;
using WebApp.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ITimeEntryService, TimeEntryService>();
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));


var app = builder.Build();

app.MapGet("/", context =>
{
    context.Response.Redirect("/api/my/html");
    return Task.CompletedTask;
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllers();

app.Run();