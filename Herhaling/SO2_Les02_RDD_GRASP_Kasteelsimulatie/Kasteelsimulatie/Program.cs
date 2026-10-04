using Kasteelsimulatie.Domein;

// ================================================================
// TESTSCENARIO LES 03 - RDD & GRASP
// ================================================================
// Belangrijk: dit scenario spreekt UITSLUITEND de DomeinController aan.
// Program.cs maakt dus zelf geen Raster, Kasteel of Bewoner aan.
// Dat is precies wat we met de Controller-rol willen bereiken.

var controller = new DomeinController();

Console.WriteLine("1. Nieuwe simulatie starten");
controller.StartNieuweSimulatie(breedte: 5, hoogte: 4);
Console.WriteLine("Simulatie gestart met raster 5 x 4.");

Console.WriteLine();
Console.WriteLine("2. Kastelen toevoegen");

// De concrete bewoners worden willekeurig aangemaakt in de domeinlaag.
// De buitenwereld hoeft niet te weten welk type bewoner gekozen wordt.
controller.VoegKasteelToe(x: 1, y: 1, aantalBewoners: 1);
controller.VoegKasteelToe(x: 4, y: 3, aantalBewoners: 0);

Console.WriteLine($"Kasteel (1,1) bewoond: {controller.IsKasteelBewoond(1, 1)} (verwacht: True)");
Console.WriteLine($"Kasteel (4,3) bewoond: {controller.IsKasteelBewoond(4, 3)} (verwacht: False)");

Console.WriteLine();
Console.WriteLine("3. Een willekeurige bewoner doden");

// Er is op dit moment exact één bewoner in de hele simulatie.
// De operatie is dus willekeurig ontworpen, maar dit testscenario blijft
// deterministisch: na de operatie moet kasteel (1,1) leeg zijn.
controller.DoodWillekeurigeBewoner();
Console.WriteLine($"Kasteel (1,1) bewoond: {controller.IsKasteelBewoond(1, 1)} (verwacht: False)");

Console.WriteLine();
Console.WriteLine("4. Robuustheid buiten de happy flow");

try
{
    controller.DoodWillekeurigeBewoner();
    Console.WriteLine("ONVERWACHT: er werd een bewoner gedood terwijl niemand meer aanwezig is.");
}
catch (InvalidOperationException fout)
{
    Console.WriteLine($"Verwacht geweigerd (geen bewoners): {fout.Message}");
}

try
{
    controller.VoegKasteelToe(x: 1, y: 1, aantalBewoners: 2);
    Console.WriteLine("ONVERWACHT: een tweede kasteel op dezelfde positie werd toegevoegd.");
}
catch (InvalidOperationException fout)
{
    Console.WriteLine($"Verwacht geweigerd (positie bezet): {fout.Message}");
}

try
{
    controller.VoegKasteelToe(x: 5, y: 0, aantalBewoners: 2);
    Console.WriteLine("ONVERWACHT: een kasteel buiten het raster werd toegevoegd.");
}
catch (ArgumentException fout)
{
    Console.WriteLine($"Verwacht geweigerd (buiten raster): {fout.Message}");
}

try
{
    controller.IsKasteelBewoond(x: 0, y: 0);
    Console.WriteLine("ONVERWACHT: een onbestaand kasteel werd gevonden.");
}
catch (InvalidOperationException fout)
{
    Console.WriteLine($"Verwacht geweigerd (geen kasteel): {fout.Message}");
}

Console.WriteLine();
Console.WriteLine("5. Simulatie stoppen");
controller.StopHuidigeSimulatie();

try
{
    controller.IsKasteelBewoond(x: 1, y: 1);
    Console.WriteLine("ONVERWACHT: de gestopte simulatie kon nog gebruikt worden.");
}
catch (InvalidOperationException fout)
{
    Console.WriteLine($"Verwacht geweigerd (geen actieve simulatie): {fout.Message}");
}

Console.WriteLine();
Console.WriteLine("Einde van het testscenario.");
