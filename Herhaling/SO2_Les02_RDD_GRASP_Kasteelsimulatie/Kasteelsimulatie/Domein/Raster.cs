namespace Kasteelsimulatie.Domein;

public class Raster
{
    private readonly List<Kasteel> _kastelen = new();

    private int _breedte;
    private int _hoogte;

    public int Breedte
    {
        get => _breedte;
        init
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value), "De breedte moet groter zijn dan nul.");
            }

            _breedte = value;
        }
    }

    public int Hoogte
    {
        get => _hoogte;
        init
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value), "De hoogte moet groter zijn dan nul.");
            }

            _hoogte = value;
        }
    }

    public IReadOnlyList<Kasteel> Kastelen => _kastelen.AsReadOnly();

    public Raster(int breedte, int hoogte)
    {
        Breedte = breedte;
        Hoogte = hoogte;
    }

    public bool Bevat(Coordinate coordinate)
    {
        ArgumentNullException.ThrowIfNull(coordinate);

        return coordinate.X >= 0
            && coordinate.Y >= 0
            && coordinate.X < Breedte
            && coordinate.Y < Hoogte;
    }

    public Kasteel? ZoekKasteelOp(Coordinate positie)
    {
        ArgumentNullException.ThrowIfNull(positie);

        return _kastelen.FirstOrDefault(kasteel =>
            kasteel.Positie.X == positie.X
            && kasteel.Positie.Y == positie.Y);
    }

    /// <summary>
    /// Bestaande methode uit de vorige les. Raster bewaakt zijn eigen collectie.
    /// </summary>
    public void VoegKasteelToe(Kasteel kasteel)
    {
        ArgumentNullException.ThrowIfNull(kasteel);

        if (!Bevat(kasteel.Positie))
        {
            throw new ArgumentException(
                "Een kasteel moet binnen de grenzen van het raster liggen.",
                nameof(kasteel));
        }

        if (ZoekKasteelOp(kasteel.Positie) is not null)
        {
            throw new InvalidOperationException(
                "Deze positie is al ingenomen door een kasteel.");
        }

        _kastelen.Add(kasteel);
    }

    /// <summary>
    /// Systeemgerichte variant gebruikt door DomeinController.
    ///
    /// GRASP - Creator:
    /// Raster bevat en beheert kastelen en is daarom een natuurlijke creator.
    /// De bewoners zelf worden door BewonerFactory gemaakt en door Kasteel beheerd.
    /// </summary>
    public void VoegKasteelToe(int x, int y, int aantalBewoners)
    {
        if (aantalBewoners < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(aantalBewoners),
                "Het aantal bewoners mag niet negatief zijn.");
        }

        var positie = new Coordinate(x, y);

        // Controleer vóór we bewoners genereren. Zo doen we geen nutteloos werk
        // wanneer de positie sowieso ongeldig of al bezet is.
        if (!Bevat(positie))
        {
            throw new ArgumentException(
                "Een kasteel moet binnen de grenzen van het raster liggen.",
                nameof(x));
        }

        if (ZoekKasteelOp(positie) is not null)
        {
            throw new InvalidOperationException(
                "Deze positie is al ingenomen door een kasteel.");
        }

        var kasteel = new Kasteel(positie);

        // GRASP - Pure Fabrication / Creator:
        // BewonerFactory kent de creatielogica van concrete bewoners.
        var bewoners = BewonerFactory.MaakWillekeurigeBewoners(aantalBewoners);

        // GRASP - Information Expert:
        // Kasteel beheert zijn eigen bewonerscollectie.
        kasteel.VoegBewonersToe(bewoners);

        // Hergebruik de bestaande methode zodat de regels rond de
        // kastelencollectie op één plaats blijven staan.
        VoegKasteelToe(kasteel);
    }

    public bool IsKasteelBewoond(int x, int y)
    {
        // Raster is expert voor de locatie van kastelen.
        var kasteel = ZoekKasteelOp(new Coordinate(x, y))
            ?? throw new InvalidOperationException(
                $"Er staat geen kasteel op positie ({x}, {y}).");

        // Kasteel is vervolgens expert voor zijn bewoners.
        return kasteel.IsBewoond();
    }

    public void DoodWillekeurigeBewoner()
    {
        // We willen een willekeurige BEWONER kiezen, niet eerst een willekeurig
        // kasteel. Anders zouden bewoners van een klein kasteel meer kans kunnen
        // krijgen dan bewoners van een groot kasteel.
        var totaalAantalBewoners = _kastelen.Sum(kasteel => kasteel.AantalBewoners);

        if (totaalAantalBewoners == 0)
        {
            throw new InvalidOperationException(
                "Er zijn geen bewoners die gedood kunnen worden.");
        }

        var gekozenIndex = Random.Shared.Next(totaalAantalBewoners);

        foreach (var kasteel in _kastelen)
        {
            if (gekozenIndex < kasteel.AantalBewoners)
            {
                // Raster kiest enkel welk kasteel en welke lokale index.
                // Het wijzigen van de bewonerscollectie blijft de
                // verantwoordelijkheid van Kasteel.
                kasteel.DoodBewonerOpIndex(gekozenIndex);
                return;
            }

            gekozenIndex -= kasteel.AantalBewoners;
        }

        // Door de controles hierboven hoort dit pad nooit bereikbaar te zijn.
        throw new InvalidOperationException(
            "Er kon onverwacht geen bewoner geselecteerd worden.");
    }
}
