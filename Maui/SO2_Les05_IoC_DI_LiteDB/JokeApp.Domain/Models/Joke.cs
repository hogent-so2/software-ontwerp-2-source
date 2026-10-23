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

    // In deze voorbeeldoplossing kiezen we bewust init:
    // tekst en categorie zijn alleen bij het maken toewijsbaar.
    // Een pagina kan een opgeslagen grap daardoor niet rechtstreeks wijzigen.
    // Bij later bewerken kan de service een vervangend model met dezelfde Id maken.
    public string Text
    {
        get => _text;
        init
        {
            var validation = JokeTextValidator.Validate(value);

            if (!validation.IsValid)
                throw new ArgumentException(validation.ErrorMessage, nameof(value));

            // Ook tijdens de initialisatie moet de tekst geldig zijn.
            // Na de initialisatie laat init geen nieuwe toekenning toe.
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
