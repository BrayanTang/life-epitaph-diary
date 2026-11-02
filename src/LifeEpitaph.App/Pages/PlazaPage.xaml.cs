using LifeEpitaph.App.Services;
using LifeEpitaph.Shared.Models;
using Microsoft.Maui.Controls.Shapes;

namespace LifeEpitaph.App.Pages;

public partial class PlazaPage : ContentPage
{
    private readonly ApiClient _api;
    private readonly LocalStore _store;
    private bool _mineOnly;

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

    private async void OnAllTabClicked(object? sender, EventArgs e)
    {
        _mineOnly = false;
        StyleTabs();
        await ReloadAsync();
    }

    private async void OnMineTabClicked(object? sender, EventArgs e)
    {
        _mineOnly = true;
        StyleTabs();
        await ReloadAsync();
    }

    private void StyleTabs()
    {
        if (_mineOnly)
        {
            MineTabButton.Style = (Style)Application.Current!.Resources["QuietButton"];
            AllTabButton.BackgroundColor = Colors.Transparent;
            AllTabButton.TextColor = (Color)Application.Current.Resources["AccentMoss"];
            SectionHint.Text = "你分享出去的内容，以及收到的献花";
        }
        else
        {
            AllTabButton.Style = (Style)Application.Current!.Resources["QuietButton"];
            MineTabButton.BackgroundColor = Colors.Transparent;
            MineTabButton.TextColor = (Color)Application.Current.Resources["AccentMoss"];
            SectionHint.Text = "路过别人的石头，献一束花就好";
        }
    }

    private async Task ReloadAsync()
    {
        StyleTabs();
        PlazaList.Children.Clear();

        if (_mineOnly)
        {
            var mine = await _store.ListMyPlazaPostsAsync();
            if (mine.Count == 0)
            {
                PlazaList.Children.Add(new Label
                {
                    Text = "还没有分享。去「我的」写日志，或去「剧情」分享一章。",
                    FontSize = 14,
                    TextColor = (Color)Application.Current!.Resources["MutedInk"],
                });
                return;
            }

            foreach (var post in mine)
            {
                var card = await _store.ToPlazaCardAsync(post);
                PlazaList.Children.Add(BuildCard(card, allowFlower: false));
            }
            return;
        }

        var cards = (await _api.GetPlazaAsync()).ToList();
        foreach (var post in await _store.ListMyPlazaPostsAsync())
        {
            var mineCard = await _store.ToPlazaCardAsync(post);
            cards.RemoveAll(c => c.PostId == mineCard.PostId);
            cards.Insert(0, mineCard);
        }

        if (cards.Count == 0)
        {
            PlazaList.Children.Add(new Label
            {
                Text = "广场还空着。先分享一条日志或一章剧情，它会出现在这里。",
                FontSize = 14,
                TextColor = (Color)Application.Current!.Resources["MutedInk"],
            });
            return;
        }

        foreach (var card in cards)
            PlazaList.Children.Add(BuildCard(card, allowFlower: !card.IsMine));
    }

    private View BuildCard(PlazaCardDto card, bool allowFlower)
    {
        var soft = (Color)Application.Current!.Resources["SoftInk"];
        var muted = (Color)Application.Current.Resources["MutedInk"];
        var kindLabel = card.Kind == "chapter" ? "人生剧情" : "自由日志";

        var stack = new VerticalStackLayout
        {
            Spacing = 8,
            Children =
            {
                new Label
                {
                    Text = card.IsMine ? $"我的{kindLabel}" : $"{card.DisplayName} · {kindLabel}",
                    FontSize = 12,
                    TextColor = (Color)Application.Current.Resources["AccentMoss"],
                },
                new Label
                {
                    Text = string.IsNullOrWhiteSpace(card.Title) ? "分享" : card.Title,
                    FontSize = 17,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = soft,
                },
                new Label
                {
                    Text = card.Body,
                    FontSize = 14,
                    LineHeight = 1.35,
                    TextColor = soft,
                },
                new Label
                {
                    Text = $"献花 {card.FlowerCount}" + (string.IsNullOrWhiteSpace(card.CreatedAt) ? "" : $" · {card.CreatedAt}"),
                    FontSize = 12,
                    TextColor = muted,
                },
            },
        };

        if (allowFlower)
        {
            var flowerButton = new Button
            {
                Text = "献一束花",
                Style = (Style)Application.Current.Resources["QuietButton"],
                HorizontalOptions = LayoutOptions.Start,
                Padding = new Thickness(14, 8),
                FontSize = 13,
            };
            var countLabel = (Label)stack.Children[^1];
            flowerButton.Clicked += async (_, _) =>
            {
                flowerButton.IsEnabled = false;
                try
                {
                    if (!string.IsNullOrWhiteSpace(card.PostId))
                    {
                        var updated = await _store.AddFlowerToMyPostAsync(card.PostId);
                        if (updated is not null)
                            card.FlowerCount = updated.FlowerCount;
                    }

                    try
                    {
                        var remote = await _api.OfferFlowerAsync(card.TombstoneId);
                        card.FlowerCount = Math.Max(card.FlowerCount, remote.FlowerCount);
                    }
                    catch
                    {
                        if (string.IsNullOrWhiteSpace(card.PostId))
                            card.FlowerCount += 1;
                    }

                    countLabel.Text = $"献花 {card.FlowerCount}" + (string.IsNullOrWhiteSpace(card.CreatedAt) ? "" : $" · {card.CreatedAt}");
                    flowerButton.Text = "已献花";
                }
                finally
                {
                    flowerButton.IsEnabled = true;
                }
            };
            stack.Children.Add(flowerButton);
        }

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
