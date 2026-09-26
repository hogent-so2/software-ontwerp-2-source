namespace Kasteelsimulatie.Domein;

// Ridder is een Bewoner EN biedt het gedrag van een IStrijder aan.
public class Ridder : Bewoner, IStrijder
{
    private int _zwaardLengte;

    // In deze uitwerking drukken we de zwaardlengte uit in centimeter.
    public int ZwaardLengte
    {
        get => _zwaardLengte;
        init
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "De zwaardlengte moet groter zijn dan nul.");
            }

            _zwaardLengte = value;
        }
    }

    public Ridder(string naam, int zwaardLengte) : base(naam)
    {
        ZwaardLengte = zwaardLengte;
    }

    public void Strijd()
    {
        // Zoals in de les maken we het gedrag zichtbaar met console-uitvoer.
        // We bouwen hier nog geen gevechtssysteem met schade of levenspunten.
        Console.WriteLine($"{Naam} vecht met een zwaard van {ZwaardLengte} cm.");
    }
}
