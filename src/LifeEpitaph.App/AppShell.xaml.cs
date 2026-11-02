using LifeEpitaph.App.Pages;

namespace LifeEpitaph.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(ChapterDetailPage), typeof(ChapterDetailPage));
    }
}
