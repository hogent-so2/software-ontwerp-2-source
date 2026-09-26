using Kasteelsimulatie.Domein;

// Dit is een vast testscenario, geen consolemenu en nog geen domeincontroller.
// Lees telkens de verwachte waarde en vergelijk die met de uitvoer (gebruik breakpoints om de code te stoppen en te inspecteren).

Console.WriteLine("1. Raster en kastelen aanmaken");

var raster = new Raster(breedte: 5, hoogte: 4);

var blauwKasteel = new Kasteel(
    positie: new Coordinate(x: 1, y: 1),
    kleur: KasteelKleuren.Blauw,
    aantalVlaggetjes: 2);

// (4, 3) is de laatste geldige positie van een raster van 5 bij 4.
var roodKasteel = new Kasteel(
    positie: new Coordinate(x: 4, y: 3),
    kleur: KasteelKleuren.Rood,
    aantalVlaggetjes: 0);

raster.VoegKasteelToe(blauwKasteel);
raster.VoegKasteelToe(roodKasteel);

Console.WriteLine($"Aantal kastelen: {raster.Kastelen.Count} (verwacht: 2)");
Console.WriteLine($"Rood kasteel bewoond: {roodKasteel.IsBewoond()} (verwacht: False)");

Console.WriteLine();
Console.WriteLine("2. Verschillende bewoners toevoegen");

blauwKasteel.VoegBewonerToe(new Prinses(naam: "Emma", beschrijving: "Zorgt voor de contacten met naburige kastelen."));

// De variabele heeft het type Bewoner, het concrete object is een Ridder.
// Een ridder kan dus gebruikt worden waar een bewoner verwacht wordt.
Bewoner ridder = new Ridder(naam: "Arthur", zwaardLengte: 90);
blauwKasteel.VoegBewonerToe(ridder);

blauwKasteel.VoegBewonerToe(new Kok(naam: "Nora", specialiteit: "Groentesoep"));
blauwKasteel.VoegBewonerToe(new Stalknecht(naam: "Sam"));

roodKasteel.VoegBewonerToe(new Ridder(naam: "David", zwaardLengte: 80));
roodKasteel.VoegBewonerToe(new Kok(naam: "Bas", specialiteit: "Vers brood"));

foreach (var kasteel in raster.Kastelen)
{
    Console.WriteLine($"{kasteel.Kleur} kasteel op ({kasteel.Positie.X}, {kasteel.Positie.Y}):");

    // We gebruiken alleen gemeenschappelijke bewonersinformatie.
    foreach (var bewoner in kasteel.Bewoners)
    {
        Console.WriteLine($"  - {bewoner.Naam}");
    }
}

Console.WriteLine($"Blauwe bewoners: {blauwKasteel.Bewoners.Count} (verwacht: 4)");
Console.WriteLine($"Rode bewoners: {roodKasteel.Bewoners.Count} (verwacht: 2)");
Console.WriteLine($"Rood kasteel bewoond: {roodKasteel.IsBewoond()} (verwacht: True)");

Console.WriteLine();
Console.WriteLine("3. Toestand wijzigen via gedrag");

blauwKasteel.HangVlagBij();
Console.WriteLine($"Blauwe vlaggetjes: {blauwKasteel.AantalVlaggetjes} (verwacht: 3)");

// Deze voorbeelden blijven commentaar: ze horen NIET te compileren.
// blauwKasteel.AantalVlaggetjes = -5;               // Setter is private.
// blauwKasteel.Positie = new Coordinate(100, 100);  // init: constructie is voorbij.
// blauwKasteel.Positie.X = 100;                    // Ook Coordinate gebruikt init.
// raster.Kastelen.Clear();                        // Geen wijzigbare lijst beschikbaar.
// blauwKasteel.Bewoners.Add(new Stalknecht("Pim")); // Gebruik VoegBewonerToe().

Console.WriteLine();
Console.WriteLine("4. Alleen de strijders van het blauwe kasteel laten strijden");

// OfType<IStrijder>() selecteert bewoners die het contract aanbieden.
// We controleren nergens op concrete types zoals Ridder of Stalknecht.
// De aanroep is dezelfde; het concrete object bepaalt de uitvoering.
foreach (var strijder in blauwKasteel.Bewoners.OfType<IStrijder>())
{
    strijder.Strijd();
}

// Verwacht: Arthur en Sam strijden. Emma en Nora niet.
// Lotte strijdt evenmin: zij woont in het rode kasteel, niet in het blauwe.

Console.WriteLine();
Console.WriteLine("5. Zoeken met een nieuw Coordinate-object");

// Dit is niet hetzelfde Coordinate-object als bij de constructie van het kasteel.
// Toch moet de zoekmethode het kasteel op deze X- en Y-waarde terugvinden.
var gevondenKasteel = raster.ZoekKasteelOp(new Coordinate(x: 1, y: 1));

if (gevondenKasteel is not null)
{
    Console.WriteLine($"Gevonden kleur: {gevondenKasteel.Kleur} (verwacht: Blauw)");
}
else
{
    Console.WriteLine("ONVERWACHT: het blauwe kasteel is niet gevonden.");
}

var leegVak = raster.ZoekKasteelOp(new Coordinate(x: 0, y: 0));
Console.WriteLine($"Leeg vak geeft null: {leegVak is null} (verwacht: True)");

Console.WriteLine();
Console.WriteLine("6. De grenzen van het raster controleren");

Console.WriteLine($"(0, 0) binnen raster: {raster.Bevat(new Coordinate(0, 0))} (verwacht: True)");
Console.WriteLine($"(4, 3) binnen raster: {raster.Bevat(new Coordinate(4, 3))} (verwacht: True)");
Console.WriteLine($"(5, 0) binnen raster: {raster.Bevat(new Coordinate(5, 0))} (verwacht: False)");
Console.WriteLine($"(0, 4) binnen raster: {raster.Bevat(new Coordinate(0, 4))} (verwacht: False)");
Console.WriteLine($"(-1, 0) binnen raster: {raster.Bevat(new Coordinate(-1, 0))} (verwacht: False)");
Console.WriteLine($"(0, -1) binnen raster: {raster.Bevat(new Coordinate(0, -1))} (verwacht: False)");

Console.WriteLine();
Console.WriteLine("7. Ongeldige toevoegingen weigeren");

try
{
    // Alleen Raster kan bepalen dat deze positie buiten zijn grenzen ligt.
    var kasteelBuitenRaster = new Kasteel(
        new Coordinate(x: 5, y: 0), KasteelKleuren.Grijs, aantalVlaggetjes: 0);

    raster.VoegKasteelToe(kasteelBuitenRaster);
    Console.WriteLine("ONVERWACHT: een kasteel buiten het raster werd aanvaard.");
}
catch (ArgumentException fout)
{
    // De regel staat in Raster. Dit scenario vangt de fout alleen op
    // zodat we daarna ook de andere gevallen nog kunnen bekijken.
    Console.WriteLine($"Verwacht geweigerd (buiten raster): {fout.Message}");
}

try
{
    var kasteelOpBezetVak = new Kasteel(
        new Coordinate(x: 1, y: 1), KasteelKleuren.Grijs, aantalVlaggetjes: 1);

    raster.VoegKasteelToe(kasteelOpBezetVak);
    Console.WriteLine("ONVERWACHT: twee kastelen op dezelfde positie werden aanvaard.");
}
catch (InvalidOperationException fout)
{
    Console.WriteLine($"Verwacht geweigerd (bezet vak): {fout.Message}");
}

Console.WriteLine($"Aantal kastelen na beide weigeringen: {raster.Kastelen.Count} (verwacht: 2)");

Console.WriteLine();
Console.WriteLine("8. Verplichte waarden valideren");

try
{
    var ongeldigRaster = new Raster(breedte: 0, hoogte: 4);
    Console.WriteLine("ONVERWACHT: een raster met breedte nul werd aanvaard.");
}
catch (ArgumentOutOfRangeException fout)
{
    Console.WriteLine($"Verwacht geweigerd (breedte): {fout.Message}");
}

try
{
    var naamlozeKok = new Kok(naam: "   ", specialiteit: "Soep");
    Console.WriteLine("ONVERWACHT: een bewoner zonder naam werd aanvaard.");
}
catch (ArgumentException fout)
{
    Console.WriteLine($"Verwacht geweigerd (naam): {fout.Message}");
}

try
{
    // Met publieke init-accessors kan een object initializer de waarden van
    // de constructor tijdens de creatie nog overschrijven. Ook dat valideren we.
    var ongeldigRaster = new Raster(breedte: 5, hoogte: 4) { Breedte = 0 };
    Console.WriteLine("ONVERWACHT: de object initializer omzeilde de validatie.");
}
catch (ArgumentOutOfRangeException fout)
{
    Console.WriteLine($"Verwacht geweigerd (object initializer): {fout.Message}");
}

Console.WriteLine();
Console.WriteLine("Einde van het testscenario.");
