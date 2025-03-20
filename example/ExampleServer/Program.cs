using ExampleServer.Data;
using ExampleServer.ViewModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace ExampleServer;

public class Program
{
    public static void Main(string[] args)
    {
        //Log.Logger = new LoggerConfiguration()
        //    .MinimumLevel.Debug()
        //    .WriteTo.BrowserConsole()
        //    .CreateLogger();
        //
        //Log.Debug("Hello, browser!");
        //Log.Warning("Received strange response {@Response} from server", new { Username = "example", Cats = 7 });

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();
        builder.Services.AddSingleton<WeatherForecastService>();

        builder.Services.AddScoped<InjTestVm>();

        var loggerConfig = new LoggerConfiguration();
        if (!builder.Environment.IsProduction())
        {
            loggerConfig
                .WriteTo.BrowserConsole();
        }
        var logger = loggerConfig.CreateLogger();
        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(logger);

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.UseRouting();

        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");

        app.Run();
    }
}