using JokeApp.Models;

namespace JokeApp.Interfaces;

/// <summary>
/// Het opslagcontract voor deze app. De aanroeper hoeft niet te weten
/// of de grappen in het geheugen of in een databasebestand zitten.
/// </summary>
public interface IJokeRepository
{
    // Read-only slaat hier op de toegang tot de collectie. Het is geen belofte
    // over de volgorde en maakt de objecten op zichzelf niet onveranderlijk.
    IReadOnlyList<Joke> GetAll();

    // null is een normaal antwoord wanneer de gevraagde id niet bestaat.
    Joke? GetById(Guid id);

    void Add(Joke joke);

    // false betekent: deze id was niet aanwezig; er is niets verwijderd.
    bool Remove(Guid id);
}
