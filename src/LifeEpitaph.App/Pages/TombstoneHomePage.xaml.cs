using LifeEpitaph.App.Services;

namespace LifeEpitaph.App.Pages;

public partial class TombstoneHomePage : ContentPage
{
    private readonly LocalStore _store;

    public TombstoneHomePage()
    {
        InitializeComponent();
        _store = ServiceHelper.GetRequiredService<LocalStore>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var tombstone = await _store.GetTombstoneAsync();
        DisplayNameLabel.Text = tombstone.DisplayName;
        StatusLabel.Text = $"状态 · {tombstone.Status}";
        SkinLabel.Text = $"皮肤 {tombstone.SkinId}";
        EpitaphLabel.Text = $"「{tombstone.Epitaph}」";

        ChaptersPreview.Children.Clear();
        foreach (var chapter in (await _store.ListChaptersAsync()).Take(3))
        {
            ChaptersPreview.Children.Add(new Label
            {
                Text = $"· {chapter.Title}",
                TextColor = (Color)Application.Current!.Resources["MutedInk"],
                FontSize = 14,
            });
        }
    }

    private async void OnGoDailyPrompt(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("//daily");
}
