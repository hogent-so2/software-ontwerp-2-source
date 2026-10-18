using JokeApp.Models;
using JokeApp.Validation;

namespace JokeApp.Services;

/// <summary>
/// Beheert de volledige bibliotheek in het geheugen.
/// De service kent de collectie en is daarom Information Expert voor
/// toevoegen, duplicaten controleren, zoeken en sorteren.
/// </summary>
public class JokeService
{
    // Eén bron van waarheid. Zoeken vervangt of verwijdert deze lijst nooit.
    // readonly beschermt de verwijzing; het maakt de lijst zelf niet onveranderlijk.
    private readonly List<Joke> _jokes = new();

    public JokeService()
    {
        _jokes.Add(new Joke(
            "Waarom was de programmeur zo rustig? Hij had alles onder Ctrl.",
            JokeCategory.Programming));
        _jokes.Add(new Joke(
            "Wat doet een kat achter de computer? De muis zoeken.",
            JokeCategory.DadJoke));
        _jokes.Add(new Joke(
            "Waarom nam het boek vakantie? Het was toe aan een nieuw hoofdstuk.",
            JokeCategory.Other));
    }

    public IReadOnlyList<Joke> GetAll()
    {
        // Dit is een read-only weergave, geen kopie van de collectie.
        // De objecten zelf blijven wijzigbaar via hun publieke properties.
        return _jokes.AsReadOnly();
    }

    public ValidationResult Add(Joke joke)
    {
        ArgumentNullException.ThrowIfNull(joke);

        // Alleen de service kent alle grappen. De pagina mag deze regel
        // niet als enige bewaken: ook andere aanroepers moeten haar respecteren.
        bool alreadyExists = _jokes.Any(existingJoke => string.Equals(existingJoke.Text, joke.Text, StringComparison.OrdinalIgnoreCase));
        if (alreadyExists)
            return ValidationResult.Failure("Deze grap bestaat al.");

        // Het model heeft zijn tekst al gevalideerd en getrimd.
        // Een duplicaat verandert niets; alleen bij succes voegen we iets toe.
        _jokes.Add(joke);
        return ValidationResult.Success();
    }

    /// <summary>
    /// Must: zoek in de tekst. Een lege zoekterm levert de hele bibliotheek op.
    /// </summary>
    public IReadOnlyList<Joke> Search(string? searchText)
    {
        // De overload hergebruikt dezelfde implementatie. Geen tweede zoekalgoritme.
        return Search(searchText, category: null, sortAlphabetically: false);
    }

    /// <summary>
    /// Should/Could: combineer de tekst met een optionele categorie en sortering.
    /// null betekent hier 'niet op categorie filteren', geen nieuwe modelcategorie.
    /// </summary>
    public IReadOnlyList<Joke> Search(string? searchText, JokeCategory? category, bool sortAlphabetically)
    {
        string term = searchText?.Trim() ?? string.Empty;
        IEnumerable<Joke> results = _jokes;

        if (term.Length > 0)
        {
            // Contains zoekt een deel van de tekst. OrdinalIgnoreCase maakt
            // de vergelijking hoofdletterongevoelig, zonder de tekst te wijzigen.
            results = results.Where(joke => joke.Text.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        if (category.HasValue)
        {
            // Beide Where-controles gelden samen: tekst EN categorie.
            results = results.Where(joke => joke.Category == category.Value);
        }

        if (sortAlphabetically)
        {
            // OrderBy levert een nieuwe volgorde op. List.Sort op _jokes zou
            // de oorspronkelijke invoervolgorde wijzigen: dat willen we niet.
            results = results.OrderBy(joke => joke.Text, StringComparer.CurrentCultureIgnoreCase);
        }

        // LINQ voert deze query pas uit wanneer we haar uitlezen.
        // ToList maakt nu een momentopname van de RESULTATEN, niet een tweede
        // beheerde bibliotheek. De Joke-objecten worden niet gekopieerd.
        // Een nieuwe lijst zorgt ook dat ItemsSource bij vernieuwen echt verandert.
        return results.ToList().AsReadOnly();
    }

    public Joke GetRandom()
    {
        // Behouden uit de vorige les, maar niet meer gebruikt door MainPage.
        // Door de testmogelijkheid met een lege bibliotheek bewaken we ook dat geval.
        if (_jokes.Count == 0)
            throw new InvalidOperationException("Er zijn nog geen grappen beschikbaar.");

        return _jokes[Random.Shared.Next(_jokes.Count)];
    }
}
