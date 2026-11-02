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
            ChapterList.Children.Add(new Border
            {
                StrokeThickness = 0,
                BackgroundColor = (Color)Application.Current!.Resources["StoneSurface"],
                StrokeShape = new RoundRectangle { CornerRadius = 14 },
                Padding = new Thickness(18),
                Content = new VerticalStackLayout
                {
                    Spacing = 6,
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
                            Text = chapter.Body,
                            FontSize = 14,
                            TextColor = (Color)Application.Current.Resources["SoftInk"],
                            LineBreakMode = LineBreakMode.WordWrap,
                        },
                    },
                },
            });
        }
    }
}
