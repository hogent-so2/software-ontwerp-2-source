namespace JokeApp.Navigation;

/// <summary>
/// Verzorgt schermnavigatie. Deze klasse kent MAUI Shell en hoort daarom
/// bij de presentatie, niet bij de domeinservices.
/// </summary>
public sealed class NavigationService
{
    // De Task laat de pagina wachten tot de navigatie klaar is.
    // Neem deze Async-methoden voorlopig als navigatierecept over.
    public Task GoBackAsync() => Shell.Current.GoToAsync("..");

    public Task ShowAddJokeAsync() => Shell.Current.GoToAsync(nameof(AddJokePage));
}
