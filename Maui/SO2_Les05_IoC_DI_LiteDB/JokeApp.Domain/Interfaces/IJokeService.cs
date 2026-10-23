using JokeApp.Domain.Models;
using JokeApp.Domain.Validation;

namespace JokeApp.Domain.Interfaces;

/// <summary>
/// De bewerkingen die pagina's mogen gebruiken, zonder kennis van de opslag.
/// </summary>
public interface IJokeService
{
    IReadOnlyList<Joke> GetAll();
    int Count();
    IReadOnlyList<Joke> Search(string? searchText);

    // Tekst en categorie kunnen samen filteren. null betekent hier:
    // geen beperking tot één categorie. Sorteren verandert de opslag niet.
    IReadOnlyList<Joke> Search(
        string? searchText, JokeCategory? category, bool sortAlphabetically);

    Joke? GetById(Guid id);
    ValidationResult Add(Joke joke);
    ValidationResult Remove(Guid id);
}
