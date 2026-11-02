using LifeEpitaph.App.Models;
using LifeEpitaph.App.Services;

namespace LifeEpitaph.App.Pages;

[QueryProperty(nameof(ChapterId), "id")]
public partial class ChapterDetailPage : ContentPage
{
    private readonly LocalStore _store;
    private readonly ApiClient _api;
    private string _chapterId = string.Empty;
    private LocalChapter? _chapter;
    private CancellationTokenSource? _saveCts;

    public string ChapterId
    {
        get => _chapterId;
        set
        {
            _chapterId = value ?? string.Empty;
            _ = LoadAsync();
        }
    }

    public ChapterDetailPage()
    {
        InitializeComponent();
        _store = ServiceHelper.GetRequiredService<LocalStore>();
        _api = ServiceHelper.GetRequiredService<ApiClient>();
    }

    private async Task LoadAsync()
    {
        if (string.IsNullOrWhiteSpace(_chapterId))
            return;

        _chapter = await _store.GetChapterAsync(_chapterId);
        if (_chapter is null)
        {
            TitleLabel.Text = "找不到这一章";
            StoryLabel.Text = string.Empty;
            EnrichEditor.IsEnabled = false;
            ShareButton.IsEnabled = false;
            return;
        }

        TitleLabel.Text = _chapter.Title;
        MetaLabel.Text = $"{_chapter.WeekStart} · {_chapter.Status}";
        StoryLabel.Text = _chapter.Body;
        var enrich = await _store.GetChapterEnrichmentAsync(_chapter.Id) ?? string.Empty;
        EnrichEditor.Text = enrich;
        ApplyEnrichPreview(enrich);
        SaveHintLabel.Text = string.IsNullOrWhiteSpace(enrich)
            ? "写下补充后会自动保存，并出现在故事下方"
            : "已保存的丰富笔迹";
    }

    private void ApplyEnrichPreview(string enrich)
    {
        var has = !string.IsNullOrWhiteSpace(enrich);
        EnrichDivider.IsVisible = has;
        EnrichPreviewLabel.IsVisible = has;
        EnrichPreviewLabel.Text = has ? $"你添的一笔\n{enrich.Trim()}" : string.Empty;
    }

    private void OnEnrichChanged(object? sender, TextChangedEventArgs e)
    {
        _saveCts?.Cancel();
        _saveCts = new CancellationTokenSource();
        var token = _saveCts.Token;
        _ = DebouncedSaveAsync(token);
    }

    private async Task DebouncedSaveAsync(CancellationToken token)
    {
        try
        {
            await Task.Delay(450, token);
            if (_chapter is null)
                return;

            var text = EnrichEditor.Text ?? string.Empty;
            await _store.SaveChapterEnrichmentAsync(_chapter.Id, text);
            ApplyEnrichPreview(text);
            SaveHintLabel.Text = $"已保存 · {DateTime.Now:HH:mm:ss}";
        }
        catch (TaskCanceledException)
        {
        }
    }

    private async void OnShareClicked(object? sender, EventArgs e)
    {
        if (_chapter is null)
            return;

        ShareButton.IsEnabled = false;
        var post = await _store.ShareChapterToPlazaAsync(_chapter);
        var card = await _store.ToPlazaCardAsync(post);
        try { await _api.SetPlazaVisibilityAsync(true, card); } catch { }
        ShareButton.Text = "已分享到广场";
    }

    private async void OnBackClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("..");
}
