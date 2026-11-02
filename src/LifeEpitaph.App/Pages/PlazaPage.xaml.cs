using LifeEpitaph.App.Services;
using LifeEpitaph.Shared.Models;
using Microsoft.Maui.Controls.Shapes;

namespace LifeEpitaph.App.Pages;

public partial class PlazaPage : ContentPage
{
    private readonly ApiClient _api;

    public PlazaPage()
    {
        InitializeComponent();
        _api = ServiceHelper.GetRequiredService<ApiClient>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        PlazaList.Children.Clear();

        var cards = await _api.GetPlazaAsync();
        if (cards.Count == 0)
        {
            cards =
            [
                new PlazaCardDto
                {
                    TombstoneId = Guid.Parse("11111111-1111-4111-8111-111111111111"),
                    DisplayName = "雾里的猫",
                    Epitaph = "路过人间，轻轻放下一束花。",
                    Status = "发呆",
                    SkinId = "moss-kitty",
                    FlowerCount = 12,
                    RecentChapterTitles = ["雨停的那一晚", "把钥匙放在口袋里"],
                },
            ];
        }

        foreach (var card in cards)
            PlazaList.Children.Add(BuildCard(card));
    }

    private View BuildCard(PlazaCardDto card)
    {
        var titles = card.RecentChapterTitles.Count == 0
            ? "（暂无近章）"
            : string.Join(" · ", card.RecentChapterTitles);

        var flowerLabel = new Label
        {
            Text = $"皮肤 {card.SkinId} · 献花 {card.FlowerCount}",
            FontSize = 12,
            TextColor = (Color)Application.Current!.Resources["MutedInk"],
        };

        var flowerButton = new Button
        {
            Text = "献一束花",
            Style = (Style)Application.Current.Resources["QuietButton"],
            HorizontalOptions = LayoutOptions.Start,
            Padding = new Thickness(14, 8),
            FontSize = 13,
        };

        flowerButton.Clicked += async (_, _) =>
        {
            flowerButton.IsEnabled = false;
            try
            {
                var updated = await _api.OfferFlowerAsync(card.TombstoneId);
                card.FlowerCount = updated.FlowerCount;
                flowerLabel.Text = $"皮肤 {card.SkinId} · 献花 {card.FlowerCount}";
                flowerButton.Text = "已献花";
                await Task.Delay(600);
                flowerButton.Text = "再献一束";
            }
            finally
            {
                flowerButton.IsEnabled = true;
            }
        };

        return new Border
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
                        Text = $"{card.DisplayName}  ·  {card.Status}",
                        FontSize = 16,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = (Color)Application.Current.Resources["SoftInk"],
                    },
                    new Label
                    {
                        Text = $"「{card.Epitaph}」",
                        FontSize = 14,
                        TextColor = (Color)Application.Current.Resources["SoftInk"],
                    },
                    flowerLabel,
                    new Label
                    {
                        Text = $"近章：{titles}",
                        FontSize = 12,
                        TextColor = (Color)Application.Current.Resources["MutedInk"],
                    },
                    flowerButton,
                },
            },
        };
    }
}
