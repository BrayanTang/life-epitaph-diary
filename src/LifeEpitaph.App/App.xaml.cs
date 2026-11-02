using LifeEpitaph.App.Services;

namespace LifeEpitaph.App;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();
    }

    protected override async void OnStart()
    {
        base.OnStart();
        var session = ServiceHelper.GetRequiredService<AnonymousSessionService>();
        await session.EnsureSessionAsync();
    }
}
