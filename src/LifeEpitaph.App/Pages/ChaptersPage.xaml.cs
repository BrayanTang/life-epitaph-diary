using LifeEpitaph.App.Services;
using Microsoft.Maui.Controls.Shapes;

namespace LifeEpitaph.App.Pages;

public partial class ChaptersPage : ContentPage
{
    private readonly LocalStore _store;
    private readonly ApiClient _api;

    public ChaptersPage()
    {
        InitializeComponent();
        _store = ServiceHelper.GetRequiredService<LocalStore>();
        _api = ServiceHelper.GetRequiredService<ApiClient>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ReloadAsync();
    }

    private async void OnGenerateClicked(object? sender, EventArgs e)
    {
        GenerateButton.IsEnabled = false;
        GenerateHintLabel.Text = "正在织故事…";
        try
        {
            var local = await _store.GenerateWeekChapterAsync();
            _ = await _api.GenerateChapterAsync();
            GenerateHintLabel.Text = $"已织好 · {local.Title}";
            await ReloadAsync();
        }
        finally
        {
            GenerateButton.IsEnabled = true;
        }
    }

    private async Task ReloadAsync()
    {
        ChapterList.Children.Clear();
        var chapters = await _store.ListChaptersAsync();
        if (chapters.Count == 0)
        {
            ChapterList.Children.Add(new Label
            {
                Text = "还没有章节。先去答几句今日提问，再点上面的「织成本周剧情」。",
                FontSize = 14,
                TextColor = (Color)Application.Current!.Resources["MutedInk"],
            });
            return;
        }

        foreach (var chapter in chapters)
        {
            var preview = chapter.Body.Length <= 48
                ? chapter.Body
                : chapter.Body[..48] + "…";

            var chapterId = chapter.Id;
            var openButton = new Button
            {
                Text = "打开故事",
                Style = (Style)Application.Current!.Resources["QuietButton"],
                HorizontalOptions = LayoutOptions.Start,
                Padding = new Thickness(14, 8),
                FontSize = 13,
            };
            openButton.Clicked += async (_, _) =>
            {
                await Shell.Current.GoToAsync($"{nameof(ChapterDetailPage)}?id={Uri.EscapeDataString(chapterId)}");
            };

            var shareButton = new Button
            {
                Text = "分享到广场",
                BackgroundColor = Colors.Transparent,
                TextColor = (Color)Application.Current.Resources["AccentMoss"],
                HorizontalOptions = LayoutOptions.Start,
                Padding = new Thickness(0, 8),
                FontSize = 13,
            };
            var captured = chapter;
            shareButton.Clicked += async (_, _) =>
            {
                shareButton.IsEnabled = false;
                var post = await _store.ShareChapterToPlazaAsync(captured);
                var card = await _store.ToPlazaCardAsync(post);
                try { await _api.SetPlazaVisibilityAsync(true, card); } catch { }
                shareButton.Text = "已分享";
            };

            ChapterList.Children.Add(new Border
            {
                StrokeThickness = 0,
                BackgroundColor = (Color)Application.Current.Resources["StoneSurface"],
                StrokeShape = new RoundRectangle { CornerRadius = 14 },
                Padding = new Thickness(18),
                Content = new VerticalStackLayout
                {
                    Spacing = 8,
                    Children =
                    {
                        new Label
                        {
                            Text = chapter.Title,
                            FontSize = 16,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = (Color)Application.Current.Resources["SoftInk"],
                        },
                        new Label
                        {
                            Text = $"{chapter.WeekStart} · {chapter.Status}",
                            FontSize = 12,
                            TextColor = (Color)Application.Current.Resources["MutedInk"],
                        },
                        new Label
                        {
                            Text = preview,
                            FontSize = 14,
                            TextColor = (Color)Application.Current.Resources["SoftInk"],
                            LineBreakMode = LineBreakMode.WordWrap,
                        },
                        new HorizontalStackLayout
                        {
                            Spacing = 16,
                            Children = { openButton, shareButton },
                        },
                    },
                },
            });
        }
    }
}
