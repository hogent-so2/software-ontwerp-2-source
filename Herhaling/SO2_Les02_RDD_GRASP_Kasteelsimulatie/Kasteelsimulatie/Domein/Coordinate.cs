namespace Kasteelsimulatie.Domein;

// Een Coordinate bundelt twee waarden die samen één positie voorstellen.
// Deze klasse hoeft niet te weten of er een raster bestaat of hoe groot dat is.
public class Coordinate
{
    // Na de constructie ligt de positie vast. Een publieke set zou toelaten
    // om een geplaatst kasteel achteraf buiten het raster te verplaatsen.
    public int X { get; init; }
    public int Y { get; init; }

    public Coordinate(int x, int y)
    {
        X = x;
        Y = y;
    }

    // Negatieve coördinaten zijn op zichzelf toegestaan.
    // Of een positie in een bepaald raster ligt, wordt door Raster gecontroleerd.
}
