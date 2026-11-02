using Microsoft.Extensions.DependencyInjection;

namespace LifeEpitaph.App.Services;

internal static class ServiceHelper
{
    public static T GetRequiredService<T>() where T : notnull
        => Current.GetRequiredService<T>();

    private static IServiceProvider Current =>
        Application.Current?.Handler?.MauiContext?.Services
        ?? IPlatformApplication.Current?.Services
        ?? throw new InvalidOperationException("MAUI services are not ready yet.");
}
