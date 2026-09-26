namespace Kasteelsimulatie.Domein;

public class Kasteel
{
    private readonly List<Bewoner> _bewoners = new();

    public Coordinate Positie
    {
        get => field;
        init
        {
            // Een kasteel kan niet zonder positie bestaan.
            // Ook een object initializer mag hier geen null invullen.
            ArgumentNullException.ThrowIfNull(value);

            // 'field' is een korte manier om te vermijden
            // dat een class-level variabele _positie gemaakt moet worden
            field = value;
        }
    }

    public KasteelKleuren Kleur
    {
        get => field;
        init
        {
            // Een cast zoals (KasteelKleur)999 is syntactisch mogelijk,
            // maar stelt geen kleur uit onze enum voor.
            if (!Enum.IsDefined(typeof(KasteelKleuren), value))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value), "Kies een kleur die in KasteelKleur bestaat.");
            }

            field = value;
        }
    }

    // Het aantal mag na constructie veranderen, maar alleen via deze klasse.
    // Daarom gebruiken we private set en niet init of een publieke set.
    public int AantalVlaggetjes { get; private set; }

    // Eén collectie voor ALLE bewoners, dus geen aparte lijst per bewonertype.
    public IReadOnlyList<Bewoner> Bewoners => _bewoners.AsReadOnly();

    public Kasteel(
        Coordinate positie,
        KasteelKleuren kleur,
        int aantalVlaggetjes)
    {
        ArgumentNullException.ThrowIfNull(positie);

        if (aantalVlaggetjes < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(aantalVlaggetjes), "Het aantal vlaggetjes mag niet negatief zijn.");
        }

        // De positie is hierboven gecontroleerd. We initialiseren het veld
        // rechtstreeks, zodat het zeker een niet-null waarde krijgt.
        Positie = positie;
        Kleur = kleur;
        AantalVlaggetjes = aantalVlaggetjes;
    }

    public void HangVlagBij()
    {
        // Ook aan de bovengrens van int mag het aantal niet negatief worden.
        if (AantalVlaggetjes == int.MaxValue)
        {
            throw new InvalidOperationException("Het maximaal voorstelbare aantal vlaggetjes is bereikt.");
        }

        AantalVlaggetjes++;
    }

    public void VoegBewonerToe(Bewoner bewoner)
    {
        ArgumentNullException.ThrowIfNull(bewoner);

        if (_bewoners.Any(x => x is Prinses))
            throw new InvalidOperationException("Er is al een prinses");

        // Deze methode hoeft niet te weten of dit een ridder, kok, ... is.
        // Elk object dat een Bewoner is, past in de bewonerscollectie.
        _bewoners.Add(bewoner);
    }

    public bool IsBewoond()
    {
        // We leiden dit af uit de lijst in plaats van een extra bool bij te houden.
        // Anders zouden die bool en de lijst elkaar kunnen tegenspreken.
        return _bewoners.Count > 0;
    }
}
