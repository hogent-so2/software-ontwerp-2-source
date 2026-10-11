namespace JokeApp.Models;

/// <summary>
/// De gegevens van één grap, onafhankelijk van de gebruikersinterface.
/// </summary>
public sealed class Joke
{
    // De identiteit staat los van de tekst: twee grappen kunnen dezelfde tekst hebben.
    // Met init kan Id na het aanmaken van het object niet meer worden toegewezen.
    public Guid Id { get; init; }

    public string Text { get; set; }
    public JokeCategory Category { get; set; }

    public Joke(string text, JokeCategory category)
    {
        // Tekst en categorie horen bij hetzelfde object. Een label op het scherm
        // is alleen een weergave van deze gegevens, niet de plaats waar we ze bewaren.
        Id = Guid.NewGuid();
        Text = text;
        Category = category;
    }

    // Net zoals in de les houden we dit model voorlopig eenvoudig.
    // Uitgebreide modelvalidatie wordt in een volgende les toegevoegd.
}
