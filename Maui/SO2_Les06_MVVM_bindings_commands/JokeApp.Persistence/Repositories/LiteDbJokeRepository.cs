using JokeApp.Domain.Interfaces;
using JokeApp.Domain.Models;
using LiteDB;
using System.Text.RegularExpressions;

namespace JokeApp.Persistence.Repositories;

/// <summary>
/// Bewaart grappen in één lokaal LiteDB-bestand. Alleen deze klasse kent
/// de database-API; pagina's en de service kennen uitsluitend het contract.
/// Pure Fabrication: opslag krijgt een eigen klasse om het model en de
/// service vrij te houden van databasecode (High Cohesion, Low Coupling).
/// </summary>
public class LiteDbJokeRepository : IJokeRepository, IDisposable
{
    private readonly LiteDatabase _database;
    private readonly ILiteCollection<Joke> _jokes;

    public LiteDbJokeRepository(string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);

        // Het pad komt van buitenaf. Deze repository kent dus geen MAUI,
        // FileSystem.AppDataDirectory of Windows-/Android-specifieke code.
        // Ook een test kan hierdoor een eigen tijdelijk bestand gebruiken.
        _database = new LiteDatabase(databasePath);
        _jokes = _database.GetCollection<Joke>("jokes");

        // Niet automatisch seeden: een nieuwe database mag leeg beginnen.
        // Ook nadat de laatste grap verwijderd is, moet ze leeg blijven.
    }

    public IReadOnlyList<Joke> GetAll()
    {
        // Het resultaat is een momentopname, geen tweede beheerde bibliotheek.
        // LiteDB maakt bij het lezen Joke-objecten van de bewaarde documenten.
        return _jokes.FindAll()
                     .ToList()
                     .AsReadOnly();
    }

    public Joke? GetById(Guid id) 
        => _jokes.FindById(id);

    public int Count() => _jokes.Count();

    public IReadOnlyList<Joke> Search(string? searchText, JokeCategory? category, bool sortAlphabetically)
    {
        string term = searchText?.Trim() ?? string.Empty;
        var query = _jokes.Query();

        if (term.Length > 0)
        {
            // Een LiteDB-expressie is geen gewone LINQ-to-Objects-aanroep.
            // Contains wordt vertaald naar LIKE; % en _ zouden dan wildcards zijn.
            // Escape maakt ALLE invoer letterlijk. UPPER en ToUpperInvariant
            // zorgen voor hoofdletterongevoelig zoeken zonder cultureel regexgedrag.
            // @0 is een parameter: gebruikersinvoer wordt nooit querycode.
            string pattern = Regex.Escape(term.ToUpperInvariant());
            query = query.Where(BsonExpression.Create(
                "IS_MATCH(UPPER($.Text), @0)", pattern));
        }

        if (category.HasValue)
            query = query.Where(joke => joke.Category == category.Value);

        if (sortAlphabetically)
            query = query.OrderBy(joke => joke.Text);

        // Where en OrderBy bouwen een LiteDB-query, geen LINQ-filter op GetAll().
        // Pas ToList voert ze uit; alleen de resultaten worden Joke-objecten.
        return query.ToList()
                    .AsReadOnly();
    }

    public void Add(Joke joke)
    {
        ArgumentNullException.ThrowIfNull(joke);

        // LiteDB herkent Id als document-id. De bestaande constructor en
        // init-properties blijven behouden; we maken geen ongeldige Joke()
        // uitsluitend om de opslagbibliotheek tevreden te stellen.
        _jokes.Insert(joke);
    }

    public bool Remove(Guid id) 
        => _jokes.Delete(id);

    public void Dispose()
    {
        // Wie het object beheert, sluit het ook: in de app is dat de container.
        // Een pagina gebruikt dus GEEN using rond haar geïnjecteerde service.
        // In een integratietest beheert de test zelf de repository met using.
        _database.Dispose();
    }
}
