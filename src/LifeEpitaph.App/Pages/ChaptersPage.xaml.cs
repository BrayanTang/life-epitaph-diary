using LifeEpitaph.App.Services;
using Microsoft.Maui.Controls.Shapes;

namespace LifeEpitaph.App.Pages;

public partial class ChaptersPage : ContentPage
{
    private readonly LocalStore _store;

    public ChaptersPage()
    {
        InitializeComponent();
        _store = ServiceHelper.GetRequiredService<LocalStore>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        ChapterList.Children.Clear();
        foreach (var chapter in await _store.ListChaptersAsync())
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

            var card = new Border
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
                        openButton,
                    },
                },
            };
            ChapterList.Children.Add(card);
        }
    }
}
