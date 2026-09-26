namespace Kasteelsimulatie.Domein;

public class Stalknecht : Bewoner, IStrijder
{
    public Stalknecht(string naam) : base(naam)
    {
        // Voorlopig heeft een stalknecht geen extra gegevens nodig.
        // Dat is geen reden om hier kunstmatige properties te verzinnen.
    }

    public void Strijd()
    {
        // Dezelfde methode uit hetzelfde contract, maar een andere uitvoering.
        Console.WriteLine($"{Naam} verdedigt het kasteel met een mestvork.");
    }
}
