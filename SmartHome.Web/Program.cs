using Serilog;
using SmartHome.Connection.Interfaces;
using SmartHome.Connection.Services;
using System.Reflection;

namespace SmartHome.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            string currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? string.Empty;

            Log.Logger = new LoggerConfiguration()
                    .WriteTo.Console()
                    .WriteTo.File($"{currentDirectory}\\logs\\SmartHome.txt",
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true)
                    .CreateLogger();

            Log.Logger.Information("Application starting.");

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();
            builder.Services.AddSingleton<ISmartDeviceService, TPLinkService>();
            builder.Services.AddSingleton<DelayService>();
            builder.Services.AddSingleton<ModeService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");

            ISmartDeviceService smartDeviceService = app.Services.GetRequiredService<ISmartDeviceService>();

            await smartDeviceService.DiscoverDevices();

            try
            {
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Logger.Fatal("An exception occurred, shutting down!");
                Log.Logger.Fatal(ex.ToString());
            }
        }
    }
}