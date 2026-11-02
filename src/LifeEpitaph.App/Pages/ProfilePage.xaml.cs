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
        PublicSwitch.IsToggled = tombstone.IsListedPublic != 0;
        _suppressToggle = false;
        ApplyPublicStatus(tombstone.IsListedPublic != 0);

        await ReloadJournalAsync();
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
                Text = "还没有日志。第一笔可以很短。",
                FontSize = 13,
                TextColor = (Color)Application.Current!.Resources["MutedInk"],
            });
            return;
        }

        foreach (var entry in entries)
        {
            JournalList.Children.Add(new Border
            {
                StrokeThickness = 0,
                BackgroundColor = (Color)Application.Current!.Resources["StoneSurface"],
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                Padding = new Thickness(14),
                Content = new VerticalStackLayout
                {
                    Spacing = 4,
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
                    },
                },
            });
        }
    }

    private async void OnPublicToggled(object? sender, ToggledEventArgs e)
    {
        if (_suppressToggle)
            return;

        var tombstone = await _store.GetTombstoneAsync();
        tombstone.IsListedPublic = e.Value ? 1 : 0;
        await _store.SaveTombstoneAsync(tombstone);

        PublicStatusLabel.Text = e.Value ? "正在同步到广场…" : "正在从广场撤回…";
        await _session.EnsureSessionAsync();
        await _api.SetPlazaVisibilityAsync(e.Value);
        ApplyPublicStatus(e.Value);
    }

    private void ApplyPublicStatus(bool listed)
    {
        PublicStatusLabel.Text = listed
            ? "当前：已出现在墓碑广场（最上方）"
            : "当前：仅自己可见";
        GoPlazaButton.IsVisible = listed;
    }

    private async void OnGoPlazaClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("//plaza");

    private static string Short(string value)
        => value.Length <= 12 ? value : $"{value[..6]}…{value[^4..]}";
}
