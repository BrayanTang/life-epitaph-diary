using LifeEpitaph.App.Models;
using LifeEpitaph.App.Services;
using LifeEpitaph.Shared.Models;
using Microsoft.Maui.Controls.Shapes;

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
        SaveHintLabel.Text = existing is null ? "写一点就会自动保存" : $"已保存 · {existing.UpdatedAt}";

        WeekFragments.Children.Clear();
        var answers = await _store.ListRecentAnswersAsync(7);
        if (answers.Count == 0)
        {
            WeekFragments.Children.Add(new Label
            {
                Text = "本周还没有碎片。今天的一句，就是开场。",
                FontSize = 13,
                TextColor = (Color)Application.Current!.Resources["MutedInk"],
            });
            return;
        }

        foreach (var answer in answers)
        {
            WeekFragments.Children.Add(new Border
            {
                StrokeThickness = 0,
                BackgroundColor = (Color)Application.Current!.Resources["StoneSurface"],
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                Padding = new Thickness(14, 12),
                Content = new VerticalStackLayout
                {
                    Spacing = 4,
                    Children =
                    {
                        new Label
                        {
                            Text = answer.AnsweredOn,
                            FontSize = 12,
                            TextColor = (Color)Application.Current.Resources["MutedInk"],
                        },
                        new Label
                        {
                            Text = string.IsNullOrWhiteSpace(answer.PromptText) ? "" : answer.PromptText,
                            FontSize = 12,
                            TextColor = (Color)Application.Current.Resources["MutedInk"],
                        },
                        new Label
                        {
                            Text = Trim(answer.Body, 80),
                            FontSize = 14,
                            TextColor = (Color)Application.Current.Resources["SoftInk"],
                        },
                    },
                },
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
        }
    }

    private static string Trim(string value, int max)
        => string.IsNullOrWhiteSpace(value)
            ? "（空）"
            : value.Length <= max ? value : value[..max] + "…";
}
