using LifeEpitaph.App.Pages;
using LifeEpitaph.App.Services;
using Microsoft.Extensions.Logging;

namespace LifeEpitaph.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        SQLitePCL.Batteries_V2.Init();

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<LocalStore>();
        builder.Services.AddSingleton<AnonymousSessionService>();
        builder.Services.AddSingleton<ApiClient>();
        builder.Services.AddTransient<TombstoneHomePage>();
        builder.Services.AddTransient<DailyPromptPage>();
        builder.Services.AddTransient<ChaptersPage>();
        builder.Services.AddTransient<ChapterDetailPage>();
        builder.Services.AddTransient<PlazaPage>();
        builder.Services.AddTransient<ProfilePage>();

        builder.Services.AddHttpClient("LifeEpitaphApi", client =>
        {
            // Override via Preferences / env later. Default assumes local API on machine.
            var baseUrl = Preferences.Default.Get("api_base_url", "http://127.0.0.1:5188/");
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(8);
        });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
