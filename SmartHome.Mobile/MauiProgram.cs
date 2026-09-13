using Microsoft.Extensions.Logging;
using SmartHome.Mobile.Services;


namespace SmartHome.Mobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<SignalRService>();
            builder.Services.AddSingleton<SignalRService>();
            builder.Services.AddTransient<DashboardPage>();
            return builder.Build();
        }
    }
}
