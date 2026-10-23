using JokeApp.Interfaces;
using JokeApp.Models;

namespace JokeApp.Repositories;

/// <summary>
/// De eerste opslagimplementatie uit de les. De gegevens verdwijnen
/// wanneer de instantie verdwijnt; singleton betekent niet permanent.
/// </summary>
public sealed class InMemoryJokeRepository : IJokeRepository
{
    // Dezelfde startgrappen als in de bijlage, maar nu op de juiste plaats:
    // de repository beheert de collectie, de service bewaakt applicatieregels.
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
