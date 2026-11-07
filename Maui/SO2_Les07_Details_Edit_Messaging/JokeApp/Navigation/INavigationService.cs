namespace JokeApp.Navigation;

/// <summary>
/// Het ViewModel kent de bedoeling van navigatie, niet Shell of een pagina.
/// Een test kan dit contract implementeren zonder een venster te openen.
/// </summary>
public interface INavigationService
{
    Task ShowAddJokeAsync();
    Task ShowJokeDetailAsync(Guid jokeId);
    Task GoBackAsync();
}
