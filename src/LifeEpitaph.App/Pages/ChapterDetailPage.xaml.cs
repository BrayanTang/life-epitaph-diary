using LifeEpitaph.App.Models;
using LifeEpitaph.App.Services;

namespace LifeEpitaph.App.Pages;

[QueryProperty(nameof(ChapterId), "id")]
public partial class ChapterDetailPage : ContentPage
{
    private readonly LocalStore _store;
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
            return;
        }

        TitleLabel.Text = _chapter.Title;
        MetaLabel.Text = $"{_chapter.WeekStart} · {_chapter.Status}";
        StoryLabel.Text = _chapter.Body;
        EnrichEditor.Text = await _store.GetChapterEnrichmentAsync(_chapter.Id) ?? string.Empty;
        SaveHintLabel.Text = string.IsNullOrWhiteSpace(EnrichEditor.Text)
            ? "写下补充后会自动保存"
            : "已保存的丰富笔迹";
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

            await _store.SaveChapterEnrichmentAsync(_chapter.Id, EnrichEditor.Text ?? string.Empty);
            SaveHintLabel.Text = $"已保存 · {DateTime.Now:HH:mm:ss}";
        }
        catch (TaskCanceledException)
        {
        }
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
