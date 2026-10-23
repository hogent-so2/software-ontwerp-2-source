using JokeApp.Interfaces;
using JokeApp.Models;
using JokeApp.Validation;

namespace JokeApp.Services;

/// <summary>
/// Coördineert bewerkingen en bewaakt regels over meerdere grappen.
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

    public IReadOnlyList<Joke> GetAll() => _jokeRepository.GetAll();

    public Joke? GetById(Guid id) => _jokeRepository.GetById(id);

    public IReadOnlyList<Joke> Search(string? searchText) =>
        Search(searchText, category: null, sortAlphabetically: false);

    public IReadOnlyList<Joke> Search(
        string? searchText, JokeCategory? category, bool sortAlphabetically)
    {
        string term = searchText?.Trim() ?? string.Empty;
        IEnumerable<Joke> results = _jokeRepository.GetAll();

        // Voor deze kleine bibliotheek zoeken we in de service. De repository
        // hoeft daardoor geen UI-filters of sorteervoorkeuren te kennen.
        // Bij veel gegevens kan het opslagcontract later gerichter worden.
        if (term.Length > 0)
        {
            results = results.Where(joke =>
                joke.Text.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        if (category.HasValue)
            results = results.Where(joke => joke.Category == category.Value);

        if (sortAlphabetically)
        {
            // OrderBy verandert de broncollectie niet. Zonder deze optie
            // gebruiken we de volgorde die de repository teruggeeft.
            results = results.OrderBy(
                joke => joke.Text, StringComparer.CurrentCultureIgnoreCase);
        }

        return results.ToList().AsReadOnly();
    }

    public ValidationResult Add(Joke joke)
    {
        ArgumentNullException.ThrowIfNull(joke);

        // Information Expert: de service kan alle grappen opvragen en bewaakt
        // de regel die over meerdere objecten gaat. Eén Joke kan dat niet.
        bool duplicateExists = _jokeRepository.GetAll().Any(existing =>
            string.Equals(existing.Text, joke.Text,
                StringComparison.OrdinalIgnoreCase));

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
