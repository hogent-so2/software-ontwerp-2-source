using JokeApp.Models;

namespace JokeApp.Services;

/// <summary>
/// Beheert de grappen in het geheugen en kiest een willekeurige grap.
/// Deze klasse kent geen pagina's, labels of andere MAUI-controls.
/// </summary>
public sealed class JokeService
{
    // Encapsulatie: alleen deze service mag items aan de interne lijst toevoegen.
    // readonly beschermt de verwijzing naar de lijst, niet de inhoud van de lijst.
    private readonly List<Joke> _jokes = new()
    {
        new Joke(
            "Waarom was de programmeur zo rustig? Hij had alles onder Ctrl.",
            JokeCategory.Programming),
        new Joke(
            "Wat doet een kat achter de computer? De muis zoeken.",
            JokeCategory.DadJoke),
        new Joke(
            "Waarom nam het boek vakantie? Het was toe aan een nieuw hoofdstuk.",
            JokeCategory.Other)
    };

    public IReadOnlyList<Joke> GetAll()
    {
        // AsReadOnly levert een beschermde weergave van de collectie, geen kopie.
        // De aanroeper kan tellen en lezen, maar geen items toevoegen of verwijderen.
        // De Joke-objecten zelf blijven wel wijzigbaar via hun publieke properties.
        return _jokes.AsReadOnly();
    }

    public Joke GetRandom()
    {
        // Information Expert: deze service kent de collectie en kiest dus zelf.
        // De pagina hoeft niets te weten over indexen of de interne List<Joke>.
        // Er zijn altijd startgrappen en deze versie biedt geen verwijderfunctie.
        int index = Random.Shared.Next(_jokes.Count);
        return _jokes[index];
    }

    public void Add(Joke joke)
    {
        ArgumentNullException.ThrowIfNull(joke);

        // We bewaren het volledige object, dus ook zijn categorie en identiteit.
        _jokes.Add(joke);
    }
}
