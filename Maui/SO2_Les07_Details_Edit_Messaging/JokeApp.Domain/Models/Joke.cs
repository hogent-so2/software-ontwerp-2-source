using JokeApp.Domain.Validation;

namespace JokeApp.Domain.Models;

/// <summary>
/// Information Expert: bewaakt de tekstregels van één grap, los van de UI.
/// </summary>
public class Joke
{
    private string _text = string.Empty;

    // Identiteit staat los van de tekst. De service beslist of toevoegen
    // van dezelfde tekst toegestaan is; dat kan één Joke zelf niet weten.
    public Guid Id { get; init; }

    // Tekst en categorie kunnen wijzigen. De setters bewaken de geldigheid
    // bij het maken én bij het aanpassen van een grap.
    public string Text
    {
        get => _text;
        set
        {
            var validation = JokeTextValidator.Validate(value);

            if (!validation.IsValid)
                throw new ArgumentException(validation.ErrorMessage, nameof(value));

            _text = value.Trim();
        }
    }

    private JokeCategory _category;

    public JokeCategory Category
    {
        get => _category;
        set
        {
            if (!Enum.IsDefined(value))
                throw new ArgumentOutOfRangeException(nameof(value), "Onbekende categorie.");
            _category = value;
        }
    }

    public Joke(string text, JokeCategory category)
    {
        Id = Guid.NewGuid();

        // Gebruik de property, niet rechtstreeks _text. Zo kan ook de
        // constructor de tekstregels van het model niet omzeilen.
        Text = text;
        Category = category;
    }
}
