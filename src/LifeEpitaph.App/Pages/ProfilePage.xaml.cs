using LifeEpitaph.App.Models;
using LifeEpitaph.App.Services;
using LifeEpitaph.Shared.Models;
using Microsoft.Maui.Controls.Shapes;

namespace LifeEpitaph.App.Pages;

public partial class ProfilePage : ContentPage
{
    private readonly LocalStore _store;
    private readonly AnonymousSessionService _session;
    private readonly ApiClient _api;
    private bool _suppressShareUi;

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

        await LoadShareChecksAsync();
        await ReloadJournalAsync();
        await RefreshPreviewAsync();
        await RefreshPublishButtonsAsync();
    }

    private async Task LoadShareChecksAsync()
    {
        var share = await _store.GetPlazaShareAsync();
        _suppressShareUi = true;
        ShareNameCheck.IsChecked = share.ShareDisplayName != 0;
        ShareEpitaphCheck.IsChecked = share.ShareEpitaph != 0;
        ShareStatusCheck.IsChecked = share.ShareStatus != 0;
        ShareSkinCheck.IsChecked = share.ShareSkin != 0;
        ShareChaptersCheck.IsChecked = share.ShareChapterTitles != 0;
        ShareMoodCheck.IsChecked = share.ShareMoodSnippet != 0;
        _suppressShareUi = false;
    }

    private async void OnShareChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (_suppressShareUi)
            return;

        await _store.SavePlazaShareAsync(ReadShareFromUi());
        await RefreshPreviewAsync();
    }

    private LocalPlazaShare ReadShareFromUi() => new()
    {
        ShareDisplayName = ShareNameCheck.IsChecked ? 1 : 0,
        ShareEpitaph = ShareEpitaphCheck.IsChecked ? 1 : 0,
        ShareStatus = ShareStatusCheck.IsChecked ? 1 : 0,
        ShareSkin = ShareSkinCheck.IsChecked ? 1 : 0,
        ShareChapterTitles = ShareChaptersCheck.IsChecked ? 1 : 0,
        ShareMoodSnippet = ShareMoodCheck.IsChecked ? 1 : 0,
    };

    private async Task RefreshPreviewAsync()
    {
        var card = await _store.BuildSharedPlazaCardAsync();
        PreviewStack.Children.Clear();
        foreach (var view in PlazaCardViews.BuildDetailViews(card, preview: true))
            PreviewStack.Children.Add(view);
    }

    private async Task RefreshPublishButtonsAsync()
    {
        var tombstone = await _store.GetTombstoneAsync();
        var listed = tombstone.IsListedPublic != 0;
        PublicStatusLabel.Text = listed ? "当前：已发布到广场（按上方勾选项展示）" : "当前：未发布到广场";
        PublishButton.Text = listed ? "更新广场内容" : "发布到广场";
        UnpublishButton.IsVisible = listed;
        GoPlazaButton.IsVisible = listed;
    }

    private async void OnPublishClicked(object? sender, EventArgs e)
    {
        await _store.SavePlazaShareAsync(ReadShareFromUi());
        var card = await _store.BuildSharedPlazaCardAsync();
        if (card.SharedLabels.Count == 0)
        {
            PublicStatusLabel.Text = "请至少勾选一项再发布";
            return;
        }

        var tombstone = await _store.GetTombstoneAsync();
        tombstone.IsListedPublic = 1;
        await _store.SaveTombstoneAsync(tombstone);
        await RefreshPublishButtonsAsync();
        PublicStatusLabel.Text = "已发布到广场";

        try
        {
            await _api.SetPlazaVisibilityAsync(true, card);
        }
        catch
        {
            PublicStatusLabel.Text = "已本地发布（广场同步未完成，仍可在本机广场看到）";
        }
    }

    private async void OnUnpublishClicked(object? sender, EventArgs e)
    {
        var tombstone = await _store.GetTombstoneAsync();
        tombstone.IsListedPublic = 0;
        await _store.SaveTombstoneAsync(tombstone);
        await RefreshPublishButtonsAsync();
        PublicStatusLabel.Text = "已从广场撤回";

        try
        {
            await _api.SetPlazaVisibilityAsync(false);
        }
        catch
        {
            PublicStatusLabel.Text = "已本地撤回（广场同步未完成）";
        }
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
        await RefreshPreviewAsync();
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

    private async void OnGoPlazaClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("//plaza");

    private static string Short(string value)
        => value.Length <= 12 ? value : $"{value[..6]}…{value[^4..]}";
}
