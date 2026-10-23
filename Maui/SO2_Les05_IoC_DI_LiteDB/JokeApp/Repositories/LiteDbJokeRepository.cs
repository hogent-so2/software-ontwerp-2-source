using JokeApp.Interfaces;
using JokeApp.Models;
using LiteDB;

namespace JokeApp.Repositories;

/// <summary>
/// Bewaart grappen in één lokaal LiteDB-bestand. Alleen deze klasse kent
/// de database-API; pagina's en de service kennen uitsluitend het contract.
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
        return _jokes.FindAll().ToList().AsReadOnly();
    }

    public Joke? GetById(Guid id) => _jokes.FindById(id);

    public void Add(Joke joke)
    {
        ArgumentNullException.ThrowIfNull(joke);

        // LiteDB herkent Id als document-id. De bestaande constructor en
        // init-properties blijven behouden; we maken geen ongeldige Joke()
        // uitsluitend om de opslagbibliotheek tevreden te stellen.
        _jokes.Insert(joke);
    }

    public bool Remove(Guid id) => _jokes.Delete(id);

    public void Dispose()
    {
        // Wie het object beheert, sluit het ook: in de app is dat de container.
        // Een pagina gebruikt dus GEEN using rond haar geïnjecteerde service.
        // In een integratietest beheert de test zelf de repository met using.
        _database.Dispose();
    }
}
