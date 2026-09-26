# Could — value equality voor Coordinate

Deze uitbreiding hoort niet bij de basis die je nodig hebt om met de volgende les
te beginnen. Ze werkt de optionele onderzoeksvraag uit zonder de eerste oplossing
met gelijkheidsoperators en hashcodes te belasten.

## Twee betekenissen van gelijk

Twee objecten kunnen verschillende instanties zijn en toch dezelfde waarden bevatten.
In de basisversie hebben twee apart gemaakte Coordinate-objecten geen aangepaste
value equality. Raster vergelijkt daarom zelf de X- en Y-waarden.

In deze uitbreiding zegt Coordinate zelf wanneer twee coördinaten gelijk zijn.
De klasse implementeert IEquatable<Coordinate>, overschrijft Equals(object) en
GetHashCode(), en definieert ook == en !=. Alle vergelijkingen gebruiken dezelfde
X- en Y-waarden. Een hashcode is geen unieke identificatie van een coördinaat:
verschillende coördinaten kunnen dezelfde hashcode hebben.

## De uitbreiding toepassen

Vervang de volledige inhoud van `Kasteelsimulatie/Domein/Coordinate.cs` door
`Could/Coordinate.cs.txt`. Vervang ook `Kasteelsimulatie/Domein/Raster.cs` door
`Could/Raster.cs.txt`. Behoud de bestaande .cs-bestandsnamen in het project.
Voeg geen tweede klasse met dezelfde naam toe.

De bestanden in deze map eindigen bewust op `.cs.txt`: ze worden niet automatisch
meegecompileerd. De basisoplossing blijft daardoor onafhankelijk uitvoerbaar.

Het oorspronkelijke Program.cs kan ongewijzigd blijven. Voeg voor het onderzoek
onderaan tijdelijk deze regels toe:

```csharp
var eerste = new Coordinate(x: 1, y: 1);
var tweede = new Coordinate(x: 1, y: 1);
var derde = new Coordinate(x: 2, y: 1);

Console.WriteLine(eerste == tweede);                  // Verwacht: True.
Console.WriteLine(eerste.Equals(tweede));             // Verwacht: True.
Console.WriteLine(object.ReferenceEquals(eerste, tweede)); // Verwacht: False.
Console.WriteLine(eerste == derde);                   // Verwacht: False.
Console.WriteLine(eerste != derde);                   // Verwacht: True.
Console.WriteLine(eerste.GetHashCode() == tweede.GetHashCode()); // Verwacht: True.

Coordinate? ontbreekt = null;
Console.WriteLine(eerste == ontbreekt);               // Verwacht: False.
Console.WriteLine(ontbreekt == null);                 // Verwacht: True.
```

De controle op een bezette positie blijft via ZoekKasteelOp lopen. In die methode
verandert de vergelijking naar:

```csharp
if (kasteel.Positie == positie)
{
    return kasteel;
}
```

Die kortere vergelijking is pas correct nadat ook de Coordinate-uitbreiding is
toegepast. Alleen Raster vervangen zou de basisversie fout maken: twee verschillende
Coordinate-objecten met dezelfde waarden zouden dan niet als dezelfde positie tellen.

## Belangrijke commentaren in de code

De ==-operator gebruikt `is null` om null te controleren. `== null` zou binnen
deze operator opnieuw dezelfde operator aanroepen. GetHashCode gebruikt dezelfde
properties als Equals. Coordinate is sealed, zodat een afgeleide klasse niet
ongemerkt extra betekenis aan gelijkheid kan toevoegen.

Dit is één mogelijke uitwerking. Er zijn ook andere manieren om waardegelijkheid
te modelleren; deze versie houdt expliciet vast aan een klasse met init-properties.

## Het DCD na de implementatie

`DCD-value-equality.puml` en `DCD-value-equality.svg` beschrijven deze versie.
Vergelijk ze met het basisdiagram in Docs. Coordinate is nu sealed, implementeert
IEquatable<Coordinate> en heeft twee Equals-methoden, GetHashCode en de twee operators.
De andere domeinklassen, relaties en publieke methodesignatures blijven hetzelfde.
Raster kent nog steeds geen extra eigenschappen van coördinaten en beheert nog
steeds zelf de kastelen. Alleen de manier waarop een positie wordt vergeleken, verandert.

Let op: ook deze vervangbestanden zijn in de samenstellingsomgeving niet met een
.NET-compiler gebouwd of uitgevoerd. Controleer de basisgevallen én de bovenstaande
vergelijkingen lokaal nadat je beide bestanden hebt vervangen.

## Bronnen

- [Onderzoeksvraag in de oefening](https://hogent-so2.github.io/so2/01-klassen-objecten.html)
- [Microsoft: IEquatable<T> en consistente Equals/GetHashCode-implementaties](https://learn.microsoft.com/en-us/dotnet/api/system.iequatable-1?view=net-10.0)
