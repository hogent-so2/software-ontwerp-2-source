using JokeApp.Domain.Interfaces;
using JokeApp.Domain.Models;
using JokeApp.Domain.Validation;

namespace JokeApp.Domain.Services;

/// <summary>
/// GRASP Controller: ontvangt bewerkingen vanuit de UI en coördineert ze.
/// Bewaakt regels over meerdere grappen, zoals unieke tekst.
/// De service heeft geen eigen lijst, database of schermtoestand.
/// </summary>
public sealed class JokeService : IJokeService
{
    private readonly IJokeRepository _jokeRepository;

    public JokeService(IJokeRepository jokeRepository)
    {
        ArgumentNullException.ThrowIfNull(jokeRepository);

        // Constructor injection: de service krijgt zijn dependency.
        // Hij kiest niet zelf met new welke opslagimplementatie hij gebruikt.
        _jokeRepository = jokeRepository;
    }

    public IReadOnlyList<Joke> GetAll() 
        => _jokeRepository.GetAll();

    public int Count()
        => _jokeRepository.Count();

    public Joke? GetById(Guid id) 
        => _jokeRepository.GetById(id);

    public IReadOnlyList<Joke> Search(string? searchText) =>
        Search(searchText, category: null, sortAlphabetically: false);

    // De repository kent de opslag en voert zoeken en sorteren zelf uit.
    // De service hoeft daarvoor niet eerst alle grappen op te halen.
    public IReadOnlyList<Joke> Search(string? searchText, JokeCategory? category, bool sortAlphabetically)
        => _jokeRepository.Search(searchText, category, sortAlphabetically);

    public ValidationResult Add(Joke joke)
    {
        ArgumentNullException.ThrowIfNull(joke);

        // Information Expert: de service kan alle grappen opvragen en bewaakt
        // de regel die over meerdere objecten gaat. Eén Joke kan dat niet.
        bool duplicateExists = _jokeRepository.GetAll()
                                              .Any(existing => string.Equals(existing.Text, joke.Text, StringComparison.OrdinalIgnoreCase));

        if (duplicateExists)
            return ValidationResult.Failure("Deze grap bestaat al.");

        _jokeRepository.Add(joke);
        return ValidationResult.Success();
    }

    public ValidationResult Remove(Guid id)
    {
        // De repository weet OF er iets verwijderd is. De service vertaalt
        // dat antwoord naar een betekenisvol resultaat voor zijn aanroepers.
        // Een onbekende id is hier verwachte feedback, geen exception.
        if (!_jokeRepository.Remove(id))
            return ValidationResult.Failure("Deze grap bestaat niet meer.");

        return ValidationResult.Success();
    }
}
