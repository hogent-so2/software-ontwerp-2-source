namespace JokeApp.Navigation;

/// <summary>Alleen deze adapter kent Shell en zijn routes.</summary>
public sealed class ShellNavigationService : INavigationService
{
    public Task ShowAddJokeAsync() => Shell.Current.GoToAsync(nameof(AddJokePage));

    public Task ShowJokeDetailAsync(Guid jokeId) =>
        Shell.Current.GoToAsync(nameof(JokeDetailPage), new Dictionary<string, object>
        {
            // We delen geen modelobject met het formulier; alleen de identiteit.
            ["JokeId"] = jokeId
        });

    public Task GoBackAsync() => Shell.Current.GoToAsync("..");
}
