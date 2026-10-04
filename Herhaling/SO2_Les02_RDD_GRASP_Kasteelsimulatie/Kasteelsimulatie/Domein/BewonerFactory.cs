namespace Kasteelsimulatie.Domein;

/// <summary>
/// Maakt bewoners aan wanneer het concrete bewonertype op voorhand niet gekend is.
///
/// GRASP - Pure Fabrication / Creator:
/// geen bestaande domeinklasse is een goede plaats voor alle creatielogica
/// van de verschillende concrete bewoners. Door dit apart te houden blijven
/// Raster, Kasteel en DomeinController samenhangend en leesbaar.
/// </summary>
public static class BewonerFactory
{
    public static Bewoner MaakWillekeurigeBewoner()
    {
        var naam = $"Bewoner-{MaakWillekeurigeString(5)}";

        // Polymorfisme zorgt ervoor dat de aanroepende code nadien gewoon
        // met Bewoner kan werken, ongeacht welk concreet type hier gekozen werd.
        return Random.Shared.Next(4) switch
        {
            0 => new Prinses(naam, $"Prinses {MaakWillekeurigeString(8)}"),
            1 => new Ridder(naam, Random.Shared.Next(60, 121)),
            2 => new Stalknecht(naam),
            3 => new Kok(naam, $"Gerecht-{MaakWillekeurigeString(6)}"),
            _ => throw new InvalidOperationException("Onverwachte willekeurige waarde.")
        };
    }

    public static List<Bewoner> MaakWillekeurigeBewoners(int aantal)
    {
        if (aantal < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(aantal),
                "Het aantal bewoners mag niet negatief zijn.");
        }

        var bewoners = new List<Bewoner>();

        for (var i = 0; i < aantal; i++)
        {
            bewoners.Add(MaakWillekeurigeBewoner());
        }

        return bewoners;
    }

    private static string MaakWillekeurigeString(int lengte)
    {
        const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var tekens = new char[lengte];

        for (var i = 0; i < lengte; i++)
        {
            tekens[i] = letters[Random.Shared.Next(letters.Length)];
        }

        return new string(tekens);
    }
}
