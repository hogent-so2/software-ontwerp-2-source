namespace JokeApp.Navigation;

/// <summary>
/// Alleen deze implementatie vertaalt navigatieacties naar MAUI Shell-routes.
/// Ze hoort bij de presentatie, niet bij Domain of Persistence.
/// </summary>
public class ShellNavigationService : INavigationService
{
    public Task ShowAddJokeAsync() =>
        Shell.Current.GoToAsync(nameof(AddJokePage));

    public Task GoBackAsync() =>
        Shell.Current.GoToAsync("..");
}
