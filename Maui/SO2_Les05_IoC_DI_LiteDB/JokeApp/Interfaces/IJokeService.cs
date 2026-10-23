using JokeApp.Models;
using JokeApp.Validation;

namespace JokeApp.Interfaces;

/// <summary>
/// De bewerkingen die pagina's mogen gebruiken, zonder kennis van de opslag.
/// </summary>
public interface IJokeService
{
    IReadOnlyList<Joke> GetAll();
    IReadOnlyList<Joke> Search(string? searchText);

    // De werkende categorie- en sorteeruitbreidingen uit de bijlage blijven
    // beschikbaar. null betekent: geen beperking tot één categorie.
    IReadOnlyList<Joke> Search(
        string? searchText, JokeCategory? category, bool sortAlphabetically);

    Joke? GetById(Guid id);
    ValidationResult Add(Joke joke);
    ValidationResult Remove(Guid id);
}
