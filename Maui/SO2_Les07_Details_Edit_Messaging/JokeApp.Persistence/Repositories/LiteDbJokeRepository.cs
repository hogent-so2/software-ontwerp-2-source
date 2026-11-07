using JokeApp.Domain.Interfaces;
using JokeApp.Domain.Models;
using LiteDB;

namespace JokeApp.Persistence.Repositories;

/// <summary>
/// Bewaart grappen in één lokaal LiteDB-bestand. Alleen deze klasse kent
/// de database-API; pagina's en de service kennen uitsluitend het contract.
/// Pure Fabrication: opslag krijgt een eigen klasse om het model en de
/// service vrij te houden van databasecode (High Cohesion, Low Coupling).
/// </summary>
public sealed class LiteDbJokeRepository : IJokeRepository, IDisposable
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
        string term = searchText?.Trim().ToUpper() ?? string.Empty;
        var query = _jokes.Query();

        if (term.Length > 0)
            query = query.Where(joke => joke.Text.ToUpper().IndexOf(term) >= 0);

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

        // LiteDB herkent Id als document-id en gebruikt de bestaande constructor
        // om een geldig Joke-object te maken bij het lezen.
        _jokes.Insert(joke);
    }

    public bool Update(Joke joke)
    {
        ArgumentNullException.ThrowIfNull(joke);
        // Update gebruikt de bestaande Id. Upsert zou bij een onbekende id
        // toevoegen, wat niet overeenkomt met het contract van onze repository.
        return _jokes.Update(joke);
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
