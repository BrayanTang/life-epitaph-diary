using LifeEpitaph.App.Services;

namespace LifeEpitaph.App.Pages;

public partial class TombstoneHomePage : ContentPage
{
    private readonly LocalStore _store;
    private readonly ApiClient _api;

    public TombstoneHomePage()
    {
        InitializeComponent();
        _store = ServiceHelper.GetRequiredService<LocalStore>();
        _api = ServiceHelper.GetRequiredService<ApiClient>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var tombstone = await _store.GetTombstoneAsync();
        BindTombstone(tombstone);

        var needsSetup = tombstone.DisplayName == "无名之石"
            || tombstone.Epitaph == "在这里，安静地记下一周的自己。";
        SetupPanel.IsVisible = needsSetup;
        if (needsSetup)
        {
            NameEntry.Text = tombstone.DisplayName;
            EpitaphEntry.Text = tombstone.Epitaph;
            StatusEntry.Text = tombstone.Status;
        }

        var prompt = await _api.GetTodayPromptAsync() ?? MockPromptFallback.ForToday();
        TodayPromptLabel.Text = prompt.Text;

        ChaptersPreview.Children.Clear();
        var chapters = (await _store.ListChaptersAsync()).Take(3).ToList();
        if (chapters.Count == 0)
        {
            ChaptersPreview.Children.Add(new Label
            {
                Text = "还没有章节。答几天今日提问，故事会自己长出来。",
                TextColor = (Color)Application.Current!.Resources["MutedInk"],
                FontSize = 14,
            });
        }
        else
        {
            foreach (var chapter in chapters)
            {
                var id = chapter.Id;
                var row = new Border
                {
                    StrokeThickness = 0,
                    BackgroundColor = (Color)Application.Current!.Resources["EditorSurface"],
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
                    Padding = new Thickness(14, 12),
                    Content = new VerticalStackLayout
                    {
                        Spacing = 4,
                        Children =
                        {
                            new Label
                            {
                                Text = chapter.Title,
                                FontSize = 15,
                                FontAttributes = FontAttributes.Bold,
                                TextColor = (Color)Application.Current.Resources["SoftInk"],
                            },
                            new Label
                            {
                                Text = chapter.Body.Length <= 40 ? chapter.Body : chapter.Body[..40] + "…",
                                FontSize = 13,
                                TextColor = (Color)Application.Current.Resources["MutedInk"],
                            },
                        },
                    },
                };
                var tap = new TapGestureRecognizer();
                tap.Tapped += async (_, _) =>
                    await Shell.Current.GoToAsync($"{nameof(ChapterDetailPage)}?id={Uri.EscapeDataString(id)}");
                row.GestureRecognizers.Add(tap);
                ChaptersPreview.Children.Add(row);
            }
        }
    }

    private void BindTombstone(Models.LocalTombstone tombstone)
    {
        DisplayNameLabel.Text = tombstone.DisplayName;
        StatusLabel.Text = $"此刻 · {tombstone.Status}";
        EpitaphLabel.Text = $"「{tombstone.Epitaph}」";
        SkinLabel.Text = $"皮肤 · {SkinDisplay(tombstone.SkinId)}";
    }

    private async void OnSaveSetupClicked(object? sender, EventArgs e)
    {
        var tombstone = await _store.GetTombstoneAsync();
        var name = NameEntry.Text?.Trim();
        var epitaph = EpitaphEntry.Text?.Trim();
        var status = StatusEntry.Text?.Trim();
        if (!string.IsNullOrWhiteSpace(name))
            tombstone.DisplayName = name;
        if (!string.IsNullOrWhiteSpace(epitaph))
            tombstone.Epitaph = epitaph;
        if (!string.IsNullOrWhiteSpace(status))
            tombstone.Status = status;
        await _store.SaveTombstoneAsync(tombstone);
        BindTombstone(tombstone);
        SetupPanel.IsVisible = false;
    }

    private static string SkinDisplay(string skinId) => skinId switch
    {
        "soft-stone" => "软石头",
        "moss-kitty" => "苔藓猫",
        "lantern-night" => "夜灯",
        _ => skinId,
    };

    private async void OnGoDailyPrompt(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("//daily");

    private async void OnGoChapters(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("//chapters");

    private async void OnGoProfile(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("//profile");

    private async void OnGoPlaza(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("//plaza");
}
