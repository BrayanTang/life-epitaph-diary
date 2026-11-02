using LifeEpitaph.App.Services;

namespace LifeEpitaph.App.Pages;

public partial class ProfilePage : ContentPage
{
    private readonly LocalStore _store;
    private readonly AnonymousSessionService _session;
    private bool _suppressToggle;

    public ProfilePage()
    {
        InitializeComponent();
        _store = ServiceHelper.GetRequiredService<LocalStore>();
        _session = ServiceHelper.GetRequiredService<AnonymousSessionService>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var session = _session.Current ?? await _session.EnsureSessionAsync();
        DeviceIdLabel.Text = $"设备  {Short(session.DeviceId)}";
        UserIdLabel.Text = $"用户  {Short(session.UserId)}";

        var tombstone = await _store.GetTombstoneAsync();
        _suppressToggle = true;
        PublicSwitch.IsToggled = tombstone.IsListedPublic != 0;
        _suppressToggle = false;
    }

    private async void OnPublicToggled(object? sender, ToggledEventArgs e)
    {
        if (_suppressToggle)
            return;

        var tombstone = await _store.GetTombstoneAsync();
        tombstone.IsListedPublic = e.Value ? 1 : 0;
        await _store.SaveTombstoneAsync(tombstone);
    }

    private static string Short(string value)
        => value.Length <= 12 ? value : $"{value[..6]}…{value[^4..]}";
}
