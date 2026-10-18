using JokeApp.Validation;

namespace JokeApp.Models;

/// <summary>
/// De gegevens en geldigheidsregels van één grap, los van de gebruikersinterface.
/// </summary>
public class Joke
{
    private string _text = string.Empty;

    // Identiteit staat los van de tekst. De service beslist of toevoegen
    // van dezelfde tekst toegestaan is; dat kan één Joke zelf niet weten.
    public Guid Id { get; init; }

    public string Text
    {
        get => _text;
        init
        {
            var validation = JokeTextValidator.Validate(value);

            if (!validation.IsValid)
                throw new ArgumentException(validation.ErrorMessage, nameof(value));

            // Eerst controleren, pas daarna wijzigen. Bij een ongeldige
            // toekenning behoudt een bestaand object dus zijn vorige geldige tekst.
            _text = value.Trim();
        }
    }

    public JokeCategory Category { get; init; }

    public Joke(string text, JokeCategory category)
    {
        Id = Guid.NewGuid();

        // Gebruik de property, niet rechtstreeks _text. Zo kan ook de
        // constructor de tekstregels van het model niet omzeilen.
        Text = text;
        Category = category;
    }
}
