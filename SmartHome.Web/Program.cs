using SmartHome.Connection.Interfaces;
using SmartHome.Connection.Services;

namespace SmartHome.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

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

            app.Run();
        }
    }
}