namespace Kasteelsimulatie.Domein;

/// <summary>
/// De DomeinController is het aanspreekpunt van de domeinlaag.
/// Code buiten het domein hoeft daardoor niet te weten welke domeinobjecten
/// onderling moeten samenwerken om een systeemoperatie uit te voeren.
///
/// GRASP - Controller:
/// deze klasse ontvangt de systeemoperaties en coördineert ze.
/// Let op: coördineren betekent niet dat alle domeinlogica hier thuishoort.
/// </summary>
public class DomeinController
{
    // Het Raster is een intern detail van de lopende simulatie.
    // We maken het niet publiek, zodat de buitenwereld niet rechtstreeks
    // aan de controller voorbij kan gaan om het domein te wijzigen.
    private Raster? _raster;

    public void StartNieuweSimulatie(int breedte, int hoogte)
    {
        // GRASP - Creator:
        // de controller beheert de levensduur van de huidige simulatie
        // en is daarom een logische plaats om het Raster aan te maken.
        _raster = new Raster(breedte, hoogte);
    }

    public void StopHuidigeSimulatie()
    {
        _raster = null;
    }

    public void VoegKasteelToe(int x, int y, int aantalBewoners)
    {
        // De controller geeft de opdracht door, maar kent de interne
        // kastelen- of bewonerscollecties niet. Raster neemt vanaf hier over.
        HuidigRaster.VoegKasteelToe(x, y, aantalBewoners);
    }

    public bool IsKasteelBewoond(int x, int y)
    {
        // De controller weet het antwoord zelf niet en hoeft het ook niet te weten.
        // Raster zoekt het juiste kasteel; Kasteel kent zijn bewoners.
        return HuidigRaster.IsKasteelBewoond(x, y);
    }

    public void DoodWillekeurigeBewoner()
    {
        // Opnieuw alleen coördinatie. De controller gaat niet zelf door
        // kastelen of bewoners lopen: dat zou onnodige koppeling veroorzaken.
        HuidigRaster.DoodWillekeurigeBewoner();
    }

    private Raster HuidigRaster
    {
        get
        {
            // Could: buiten de happy flow willen we een duidelijke domeinfout
            // in plaats van een NullReferenceException door _raster! te gebruiken.
            return _raster
                ?? throw new InvalidOperationException(
                    "Er is geen actieve simulatie. Start eerst een nieuwe simulatie.");
        }
    }
}
