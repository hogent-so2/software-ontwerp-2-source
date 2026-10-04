namespace Kasteelsimulatie.Domein;

// abstract: "bewoner" is hier een gemeenschappelijk concept.
// We maken concrete bewoners, bijvoorbeeld een Kok, geen losse Bewoner.
public abstract class Bewoner
{
    private string _naam = string.Empty;

    public string Naam
    {
        get => _naam;
        init
        {
            // Ook null, een lege string en alleen spaties zijn geen geldige naam.
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Een bewoner moet een niet-lege naam hebben.", nameof(value));
            }

            _naam = value;
        }
    }

    // protected: afgeleide klassen mogen deze constructor aanroepen met base(...).
    // Aanroepende code maakt een concreet bewonertype aan.
    protected Bewoner(string naam)
    {
        Naam = naam;
    }
}
