namespace Kasteelsimulatie.Domein;

// Een Prinses IS EEN Bewoner. De naam en de naamvalidatie komen uit Bewoner.
// sealed: we voorzien in dit ontwerp geen verdere afgeleide prinsessentypes.
public class Prinses : Bewoner
{
    private string _beschrijving = string.Empty;

    public string Beschrijving
    {
        get => _beschrijving;
        init
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Een prinses moet een niet-lege beschrijving hebben.", nameof(value));
            }

            _beschrijving = value;
        }
    }

    public Prinses(string naam, string beschrijving) : base(naam)
    {
        // base(naam) laat de basisklasse het gemeenschappelijke deel opbouwen.
        // Deze constructor zorgt alleen voor de bijkomende prinsessengegevens.
        Beschrijving = beschrijving;
    }
}
