using LifeEpitaph.App.Models;
using LifeEpitaph.App.Services;
using Microsoft.Maui.Controls.Shapes;

namespace LifeEpitaph.App.Pages;

public partial class ProfilePage : ContentPage
{
    private readonly LocalStore _store;
    private readonly AnonymousSessionService _session;
    private readonly ApiClient _api;
    private bool _suppressToggle;

    public ProfilePage()
    {
        InitializeComponent();
        _store = ServiceHelper.GetRequiredService<LocalStore>();
        _session = ServiceHelper.GetRequiredService<AnonymousSessionService>();
        _api = ServiceHelper.GetRequiredService<ApiClient>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var session = _session.Current ?? await _session.EnsureSessionAsync();
        DeviceIdLabel.Text = $"设备  {Short(session.DeviceId)}";
        UserIdLabel.Text = $"用户  {Short(session.UserId)}";

        var tombstone = await _store.GetTombstoneAsync();
        _suppressToggle = true;
        PublicSwitch.IsToggled = tombstone.IsListedPublic == 1;
        _suppressToggle = false;
        PrivacyHintLabel.Text = PublicSwitch.IsToggled
            ? "开启：分享出去的内容会出现在广场。"
            : "关闭：内容仍可点分享，但不会主动推到广场流。";

        await ReloadJournalAsync();
    }

    private async void OnPublicToggled(object? sender, ToggledEventArgs e)
    {
        if (_suppressToggle)
            return;

        var tombstone = await _store.GetTombstoneAsync();
        tombstone.IsListedPublic = e.Value ? 1 : 0;
        await _store.SaveTombstoneAsync(tombstone);
        PrivacyHintLabel.Text = e.Value
            ? "开启：分享出去的内容会出现在广场。"
            : "关闭：内容仍可点分享，但不会主动推到广场流。";
        try { await _api.SetPlazaVisibilityAsync(e.Value); } catch { }
    }

    private async void OnSaveJournalClicked(object? sender, EventArgs e)
    {
        var body = JournalEditor.Text?.Trim() ?? string.Empty;
        if (body.Length == 0)
        {
            JournalHintLabel.Text = "先写一点再写入";
            return;
        }

        await _store.AddJournalEntryAsync(body);
        JournalEditor.Text = string.Empty;
        JournalHintLabel.Text = $"已写入 · {DateTime.Now:HH:mm:ss}";
        await ReloadJournalAsync();
    }

    private async Task ReloadJournalAsync()
    {
        JournalList.Children.Clear();
        var entries = await _store.ListJournalEntriesAsync();
        if (entries.Count == 0)
        {
            JournalList.Children.Add(new Label
            {
                Text = "还没有日志。写一条后可点「分享到广场」。",
                FontSize = 13,
                TextColor = (Color)Application.Current!.Resources["MutedInk"],
            });
            return;
        }

        foreach (var entry in entries)
        {
            var shareButton = new Button
            {
                Text = "分享到广场",
                Style = (Style)Application.Current!.Resources["QuietButton"],
                HorizontalOptions = LayoutOptions.Start,
                Padding = new Thickness(14, 8),
                FontSize = 13,
            };
            var captured = entry;
            shareButton.Clicked += async (_, _) =>
            {
                shareButton.IsEnabled = false;
                var post = await _store.ShareJournalToPlazaAsync(captured);
                var card = await _store.ToPlazaCardAsync(post);
                try { await _api.SetPlazaVisibilityAsync(true, card); } catch { }
                shareButton.Text = "已分享";
                JournalHintLabel.Text = "已分享到广场，去「广场 → 我的」可看献花";
            };

            JournalList.Children.Add(new Border
            {
                StrokeThickness = 0,
                BackgroundColor = (Color)Application.Current.Resources["StoneSurface"],
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                Padding = new Thickness(14),
                Content = new VerticalStackLayout
                {
                    Spacing = 8,
                    Children =
                    {
                        new Label
                        {
                            Text = entry.CreatedAt,
                            FontSize = 12,
                            TextColor = (Color)Application.Current.Resources["MutedInk"],
                        },
                        new Label
                        {
                            Text = entry.Body,
                            FontSize = 14,
                            TextColor = (Color)Application.Current.Resources["SoftInk"],
                            LineBreakMode = LineBreakMode.WordWrap,
                        },
                        shareButton,
                    },
                },
            });
        }
    }

    private static string Short(string value)
        => value.Length <= 12 ? value : $"{value[..6]}…{value[^4..]}";
}
