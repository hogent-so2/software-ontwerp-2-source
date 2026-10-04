namespace Kasteelsimulatie.Domein;

public class Kasteel
{
    private readonly List<Bewoner> _bewoners = new();

    public Coordinate Positie
    {
        get => field;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    }

    public KasteelKleuren Kleur
    {
        get => field;
        init
        {
            if (!Enum.IsDefined(typeof(KasteelKleuren), value))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value), "Kies een kleur die in KasteelKleuren bestaat.");
            }

            field = value;
        }
    }

    public int AantalVlaggetjes { get; private set; }

    // De collectie blijft private. Andere klassen mogen bewoners bekijken,
    // maar toevoegen/verwijderen gebeurt via gedrag van Kasteel.
    public IReadOnlyList<Bewoner> Bewoners => _bewoners.AsReadOnly();

    // Handig voor Raster: het heeft voor sommige verantwoordelijkheden alleen
    // het aantal bewoners nodig, niet de volledige wijzigbare collectie.
    public int AantalBewoners => _bewoners.Count;

    /// <summary>
    /// Constructor met de standaardwaarden uit de les.
    /// Kasteel kent zijn eigen geldige defaults beter dan de controller.
    /// </summary>
    public Kasteel(Coordinate positie)
        : this(positie, KasteelKleuren.Grijs, aantalVlaggetjes: 3)
    {
    }

    public Kasteel(
        Coordinate positie,
        KasteelKleuren kleur,
        int aantalVlaggetjes)
    {
        ArgumentNullException.ThrowIfNull(positie);

        if (aantalVlaggetjes < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(aantalVlaggetjes),
                "Het aantal vlaggetjes mag niet negatief zijn.");
        }

        Positie = positie;
        Kleur = kleur;
        AantalVlaggetjes = aantalVlaggetjes;
    }

    public void HangVlagBij()
    {
        if (AantalVlaggetjes == int.MaxValue)
        {
            throw new InvalidOperationException(
                "Het maximaal voorstelbare aantal vlaggetjes is bereikt.");
        }

        AantalVlaggetjes++;
    }

    public void VoegBewonerToe(Bewoner bewoner)
    {
        ArgumentNullException.ThrowIfNull(bewoner);
        _bewoners.Add(bewoner);
    }

    /// <summary>
    /// Voegt meerdere bewoners toe zonder dat de aanroeper toegang krijgt
    /// tot de interne List. Kasteel blijft verantwoordelijk voor zijn collectie.
    /// </summary>
    public void VoegBewonersToe(IEnumerable<Bewoner> bewoners)
    {
        ArgumentNullException.ThrowIfNull(bewoners);

        foreach (var bewoner in bewoners)
        {
            VoegBewonerToe(bewoner);
        }
    }

    public bool IsBewoond()
    {
        // GRASP - Information Expert:
        // Kasteel bezit de bewonerscollectie en is dus de beste klasse
        // om te antwoorden of het bewoond is.
        return _bewoners.Count > 0;
    }

    /// <summary>
    /// Verwijdert de bewoner op een gegeven index.
    /// Raster gebruikt dit om één bewoner uit de volledige simulatie te kiezen,
    /// zonder zelf toegang te krijgen tot de wijzigbare bewonerslijst.
    /// </summary>
    public void DoodBewonerOpIndex(int index)
    {
        if (index < 0 || index >= _bewoners.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(index),
                "Er bestaat geen bewoner op deze positie in het kasteel.");
        }

        _bewoners.RemoveAt(index);
    }
}
