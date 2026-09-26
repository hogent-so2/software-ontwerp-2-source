namespace Kasteelsimulatie.Domein;

public class Kok : Bewoner
{
    private string _specialiteit = string.Empty;

    public string Specialiteit
    {
        get => _specialiteit;
        init
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Een kok moet een niet-lege specialiteit hebben.", nameof(value));
            }

            _specialiteit = value;
        }
    }

    public Kok(string naam, string specialiteit) : base(naam)
    {
        Specialiteit = specialiteit;
    }

    // Kok implementeert IStrijder niet: niet iedere bewoner moet kunnen strijden.
}
