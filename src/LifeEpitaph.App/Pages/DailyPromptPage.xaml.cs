using LifeEpitaph.App.Models;
using LifeEpitaph.App.Services;
using LifeEpitaph.Shared.Models;

namespace LifeEpitaph.App.Pages;

public partial class DailyPromptPage : ContentPage
{
    private readonly LocalStore _store;
    private readonly ApiClient _api;
    private DailyPromptDto? _prompt;
    private CancellationTokenSource? _saveCts;
    private bool _suppressTextChanged;

    public DailyPromptPage()
    {
        InitializeComponent();
        _store = ServiceHelper.GetRequiredService<LocalStore>();
        _api = ServiceHelper.GetRequiredService<ApiClient>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _prompt = await _api.GetTodayPromptAsync() ?? MockPromptFallback.ForToday();
        PromptLabel.Text = _prompt.Text;

        var today = DateOnly.FromDateTime(DateTime.Today);
        var existing = await _store.GetAnswerForDayAsync(today);
        _suppressTextChanged = true;
        AnswerEditor.Text = existing?.Body ?? string.Empty;
        _suppressTextChanged = false;
        SaveHintLabel.Text = existing is null ? "" : $"已保存 · {existing.UpdatedAt}";

        WeekFragments.Children.Clear();
        foreach (var answer in await _store.ListRecentAnswersAsync(7))
        {
            WeekFragments.Children.Add(new Label
            {
                Text = $"{answer.AnsweredOn}  {Trim(answer.Body, 48)}",
                FontSize = 13,
                TextColor = (Color)Application.Current!.Resources["MutedInk"],
            });
        }
    }

    private async void OnAnswerChanged(object? sender, TextChangedEventArgs e)
    {
        if (_suppressTextChanged || _prompt is null)
            return;

        _saveCts?.Cancel();
        _saveCts = new CancellationTokenSource();
        var token = _saveCts.Token;

        try
        {
            await Task.Delay(450, token);
            var now = DateTimeOffset.Now.ToString("HH:mm:ss");
            var day = DateOnly.FromDateTime(DateTime.Today);
            var local = new LocalAnswer
            {
                AnsweredOn = day.ToString("yyyy-MM-dd"),
                PromptId = _prompt.Id.ToString("N"),
                PromptText = _prompt.Text,
                Body = AnswerEditor.Text ?? string.Empty,
                UpdatedAt = now,
            };
            await _store.SaveAnswerAsync(local);
            await _api.SaveAnswerRemoteAsync(new SaveAnswerRequest
            {
                PromptId = _prompt.Id,
                Body = local.Body,
                AnsweredOn = day,
            });
            SaveHintLabel.Text = $"已自动保存 · {now}";
        }
        catch (TaskCanceledException)
        {
            // debounce
        }
    }

    private static string Trim(string value, int max)
        => string.IsNullOrWhiteSpace(value)
            ? "（空）"
            : value.Length <= max ? value : value[..max] + "…";
}
