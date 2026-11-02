using LifeEpitaph.App.Services;
using LifeEpitaph.Shared.Models;
using Microsoft.Maui.Controls.Shapes;

namespace LifeEpitaph.App.Pages;

public partial class PlazaPage : ContentPage
{
    private readonly ApiClient _api;
    private readonly LocalStore _store;

    public PlazaPage()
    {
        InitializeComponent();
        _api = ServiceHelper.GetRequiredService<ApiClient>();
        _store = ServiceHelper.GetRequiredService<LocalStore>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        PlazaList.Children.Clear();

        var cards = (await _api.GetPlazaAsync()).ToList();
        var mine = await _store.GetTombstoneAsync();
        if (mine.IsListedPublic != 0)
        {
            var myCard = await _store.BuildSharedPlazaCardAsync();
            cards.RemoveAll(c => c.TombstoneId == myCard.TombstoneId
                || (c.IsMine)
                || (!string.IsNullOrEmpty(myCard.DisplayName)
                    && c.DisplayName == myCard.DisplayName
                    && c.Epitaph == myCard.Epitaph));
            cards.Insert(0, myCard);
        }

        foreach (var card in cards)
            PlazaList.Children.Add(BuildCard(card));
    }

    private View BuildCard(PlazaCardDto card)
    {
        var stack = new VerticalStackLayout { Spacing = 8 };
        foreach (var view in PlazaCardViews.BuildDetailViews(card))
            stack.Children.Add(view);

        var flowerButton = new Button
        {
            Text = "献一束花",
            Style = (Style)Application.Current!.Resources["QuietButton"],
            HorizontalOptions = LayoutOptions.Start,
            Padding = new Thickness(14, 8),
            FontSize = 13,
        };

        var flowerCountLabel = stack.Children.OfType<Label>().LastOrDefault(l => l.Text.StartsWith("献花", StringComparison.Ordinal));

        flowerButton.Clicked += async (_, _) =>
        {
            flowerButton.IsEnabled = false;
            try
            {
                var updated = await _api.OfferFlowerAsync(card.TombstoneId);
                card.FlowerCount = updated.FlowerCount;
                if (flowerCountLabel is not null)
                    flowerCountLabel.Text = $"献花  {card.FlowerCount}";
                flowerButton.Text = "已献花";
                await Task.Delay(400);
                flowerButton.Text = "再献一束";
            }
            catch
            {
                card.FlowerCount += 1;
                if (flowerCountLabel is not null)
                    flowerCountLabel.Text = $"献花  {card.FlowerCount}";
                flowerButton.Text = "已献花（本地）";
            }
            finally
            {
                flowerButton.IsEnabled = true;
            }
        };

        stack.Children.Add(flowerButton);

        return new Border
        {
            StrokeThickness = 0,
            BackgroundColor = (Color)Application.Current.Resources["StoneSurface"],
            StrokeShape = new RoundRectangle { CornerRadius = 14 },
            Padding = new Thickness(18),
            Content = stack,
        };
    }
}
