using JokeApp.Domain.Models;

namespace JokeApp.Domain.Interfaces;

/// <summary>
/// Protected Variations: het domein bepaalt welke opslagbewerkingen nodig zijn.
/// De persistentielaag implementeert dit contract; het domein kent geen LiteDB.
/// </summary>
public interface IJokeRepository
{
    // Read-only slaat hier op de toegang tot de collectie. Het is geen belofte
    // over de volgorde en maakt de objecten op zichzelf niet onveranderlijk.
    IReadOnlyList<Joke> GetAll();

    // De opslag telt zelf, zonder alle grappen als objecten terug te geven.
    int Count();

    // Zoek letterlijk in de tekst, zonder onderscheid tussen hoofdletters en
    // kleine letters. null, lege tekst en witruimte betekenen: alle teksten.
    // Een categorie beperkt de resultaten; sorteren wijzigt de opslag niet.
    IReadOnlyList<Joke> Search(
        string? searchText, JokeCategory? category, bool sortAlphabetically);

    // null is een normaal antwoord wanneer de gevraagde id niet bestaat.
    Joke? GetById(Guid id);

    void Add(Joke joke);

    // false betekent: deze id was niet aanwezig; er is niets verwijderd.
    bool Remove(Guid id);
}
