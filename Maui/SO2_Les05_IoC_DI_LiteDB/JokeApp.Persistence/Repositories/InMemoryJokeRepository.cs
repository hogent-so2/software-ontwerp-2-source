using JokeApp.Domain.Interfaces;
using JokeApp.Domain.Models;

namespace JokeApp.Persistence.Repositories;

/// <summary>
/// De eerste opslagimplementatie uit de les. De gegevens verdwijnen
/// wanneer de instantie verdwijnt; singleton betekent niet permanent.
/// </summary>
public sealed class InMemoryJokeRepository : IJokeRepository
{
    // Deze opslagimplementatie begint met enkele voorbeelden. De repository
    // beheert de collectie; de service bewaakt de applicatieregels.
    private readonly List<Joke> _jokes =
    [
        new Joke(
            "Waarom was de programmeur zo rustig? Hij had alles onder Ctrl.",
            JokeCategory.Programming),
        new Joke(
            "Wat doet een kat achter de computer? De muis zoeken.",
            JokeCategory.DadJoke),
        new Joke(
            "Waarom nam het boek vakantie? Het was toe aan een nieuw hoofdstuk.",
            JokeCategory.Other)
    ];

    public IReadOnlyList<Joke> GetAll() => _jokes.AsReadOnly();

    public int Count() => _jokes.Count;

    public IReadOnlyList<Joke> Search(string? searchText, JokeCategory? category, bool sortAlphabetically)
    {
        string term = searchText?.Trim() ?? string.Empty;
        IEnumerable<Joke> results = _jokes;

        // Deze repository bewaart haar gegevens al in een List.
        // LINQ voert de zoekopdracht daarom in het geheugen uit.
        if (term.Length > 0)
            results = results.Where(joke => joke.Text.Contains(term, StringComparison.OrdinalIgnoreCase));

        if (category.HasValue)
            results = results.Where(joke => joke.Category == category.Value);

        // OrderBy verandert de volgorde van de oorspronkelijke lijst niet.
        if (sortAlphabetically)
            results = results.OrderBy(joke => joke.Text, StringComparer.CurrentCultureIgnoreCase);

        return results.ToList()
                      .AsReadOnly();
    }

    public Joke? GetById(Guid id) =>
        _jokes.SingleOrDefault(joke => joke.Id == id);

    public void Add(Joke joke)
    {
        ArgumentNullException.ThrowIfNull(joke);

        // Geen controle op dubbele tekst: die regel hoort in JokeService,
        // zodat ze ook bij LiteDB en bij een testrepository blijft gelden.
        _jokes.Add(joke);
    }

    public bool Remove(Guid id)
    {
        Joke? joke = GetById(id);
        return joke is not null && _jokes.Remove(joke);
    }
}
