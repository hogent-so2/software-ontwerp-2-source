# Softwareontwerp 2 — Les 1: Kasteelsimulatie

Uitgewerkte oefening bij "Herhaling OO — Klassen, Objecten & Relaties".
De basisoplossing bevat alle Must- en Should-punten. De Could-uitbreiding staat
apart, zodat de eerste oplossing bij de behandelde basisconcepten blijft.

## Starten

Pak de volledige ZIP uit en open `Kasteelsimulatie.sln` in Visual Studio met
ondersteuning voor .NET 10 en de .NET 10 SDK. Start het project `Kasteelsimulatie`
met Ctrl+F5. Er zijn geen bijkomende NuGet-packages nodig.

Vanuit de map waarin de solution staat, kan je ook uitvoeren:

```sh
dotnet build Kasteelsimulatie.sln
dotnet run --project Kasteelsimulatie/Kasteelsimulatie.csproj
```

Dit is een consoletoepassing met een vast scenario, geen applicatie met een menu.
Er worden geen bestanden opgeslagen en er is geen installatie of configuratie nodig.

## Leesvolgorde

Begin met `Domein/Coordinate.cs` en `Domein/KasteelKleur.cs`. Bekijk daarna
`Domein/Raster.cs` en `Domein/Kasteel.cs`. Lees vervolgens `Domein/Bewoner.cs`,
de vier concrete bewoners en `Domein/IStrijder.cs`. In `Program.cs` zie je
hoe de objecten samenwerken en hoe ongeldige handelingen worden geweigerd.

`Docs/DCD.svg` is het leesbare klassendiagram. `Docs/DCD.puml` bevat dezelfde
modelinhoud in bewerkbare PlantUML-notatie. `Docs/DCD.dot` is de bron van de SVG.
De basisoplossing wordt ook samengebracht in `Docs/Oplossing.md` voor wie de code
liever in één document doorneemt.

## Dekking van de oefening

| Onderdeel | Uitwerking |
| --- | --- |
| Klassen en objecten | Coordinate, Raster, Kasteel, abstracte Bewoner en vier concrete bewoners. |
| Vaste toestand | X, Y, rasterafmetingen, positie, kleur en bewonersgegevens gebruiken init. |
| Veranderlijke toestand | AantalVlaggetjes heeft een private setter; wijziging gebeurt via HangVlagBij(). |
| Collecties afschermen | Beide lijsten zijn private; alleen IReadOnlyList via AsReadOnly is publiek. |
| Kastelen toevoegen | Raster controleert grenzen en een reeds bezette positie vóór de toevoeging. |
| Bewoners beheren | Kasteel.VoegBewonerToe() beheert één lijst van Bewoner-objecten. |
| Overerving | Prinses, Ridder, Kok en Stalknecht erven van Bewoner. |
| Interface en polymorfisme | Ridder en Stalknecht implementeren IStrijder; Program spreekt alleen dat contract aan. |
| Testscenario | Twee kastelen, alle vier bewonertypes, zoeken, grenzen, vlaggetjes en geweigerde acties. |
| DCD | Bijgewerkt diagram van het uiteindelijke domeinmodel, met multipliciteiten en visibility. |
| Should: zoeken | ZoekKasteelOp() geeft het gevonden kasteel of null terug. |
| Should: bewoond | IsBewoond() leidt het antwoord af uit de bewonerslijst. |
| Should: strijden | Alle strijders van het blauwe kasteel strijden, zonder concrete types te controleren. |
| Should: validatie | Positieve afmetingen en zwaardlengte, niet-lege teksten, geldige kleur, positie en vlaggetjes. |
| Could | Losse vervangbestanden voor value equality, een aangepast DCD en een vergelijking in Could/README.md. |

## Ontwerpkeuzes die je moet kunnen uitleggen

### Raster bewaakt de plaatsing

Een Coordinate stelt een positie voor. Het kent geen raster. Raster kent zijn
eigen afmetingen en de al aanwezige kastelen, en beslist daarom of een kasteel
kan worden toegevoegd. Program roept dat gedrag aan; Program bevat de regels niet.
`VoegKasteelToe()` hergebruikt `ZoekKasteelOp()` voor de controle op een bezet vak.

`Bevat()` betekent "binnen de grenzen", niet "hier staat een kasteel".
`ZoekKasteelOp()` geeft null bij een leeg vak. Een zoekpositie buiten het raster
levert eveneens null op: daar kan geen toegevoegd kasteel staan. Een null-argument
is daarentegen een fout en wordt geweigerd.

### Eén bron voor de bewoners

De prinses is in het uiteindelijke model ook een Bewoner. Er is geen afzonderlijke
Prinses-property naast de bewonerslijst. Zo houden we dezelfde relatie niet op
twee plaatsen bij. Ook IsBewoond is geen opgeslagen bool: de bewonerslijst bevat
al de informatie die we nodig hebben.

Deze oplossing voegt geen bijkomende regels toe over unieke bewonersnamen,
maximaal één prinses, of exclusief eigenaarschap van bewoners of kastelen.
Dat zijn eventuele volgende requirements, geen impliciete garanties van dit model.

### Wat vastligt en wat mag veranderen

Vaste properties gebruiken init. Het aantal vlaggetjes kan later veranderen en
heeft daarom een private setter met gericht gedrag. De positie van een geplaatst
kasteel blijft vast: zowel Kasteel.Positie als Coordinate.X en Coordinate.Y hebben init.

Een read-only lijst maakt niet alle objecten in die lijst onveranderlijk.
Aanroepende code kan bijvoorbeeld nog HangVlagBij() aanroepen op een kasteel dat
ze via Raster.Kastelen heeft gevonden. Ze kan de kastelenlijst zelf niet leegmaken.

### Waarom sommige properties een uitgeschreven init hebben

Een publieke init-accessor kan niet alleen in de constructor worden gebruikt.
Een object initializer kan tijdens de creatie nog een andere waarde instellen:

```csharp
var raster = new Raster(5, 4) { Breedte = 0 };
```

Alleen validatie in de constructor zou hier onvoldoende zijn. Daarom staan de
relevante controles in de init-accessors, met gewone private backingfields.
Bij Positie controleert ook de constructor het argument vóór de eerste veldtoekenning.
Dit zijn geen extra architectuurconcepten: het zijn properties en encapsulatie.

### Overerving is niet hetzelfde als een interface

Iedere concrete bewoner heeft een naam en kan als Bewoner worden gebruikt.
Niet iedere bewoner strijdt. IStrijder wordt daarom niet aan Bewoner opgelegd,
maar alleen aan Ridder en Stalknecht. Program hoeft niet te weten hoe elk van
hen strijdt en controleert niet op hun concrete klassen.

Het strijdgedrag schrijft, zoals in de les, een bericht naar de console.
Er is geen aanvullend gevechtssysteem, geen domeincontroller, geen dependency
injection en geen UI-framework toegevoegd.

## Gekozen validatieregels

Rasterbreedte en -hoogte moeten groter zijn dan nul. We tellen posities vanaf nul:
in een raster van 5 bij 4 zijn X-waarden 0 t/m 4 en Y-waarden 0 t/m 3 geldig.
Negatieve coördinaten kunnen als Coordinate bestaan, maar liggen niet in zo'n raster.

Naam, prinsessenbeschrijving en kokspecialiteit mogen niet null, leeg of alleen
witruimte zijn. Een zwaardlengte moet positief zijn; in deze uitwerking is de eenheid
centimeter. Een kasteel moet een positie en een gedefinieerde KasteelKleur hebben.
Het aantal vlaggetjes begint op nul of hoger. HangVlagBij weigert ook de bovengrens
van int te overschrijden.

## Het DCD vergelijken met de les

Het nieuwe diagram volgt de uiteindelijke tekst en code, niet verouderde tussenstappen
uit sommige lesafbeeldingen. Er staat dus `Kleur: KasteelKleur`, geen `KleurVanDeMuren: Color`.
De rastergrenzen worden gecontroleerd met `Raster.Bevat()`, niet met een methode op Coordinate.
Bewoner is abstract en zijn constructor is protected. Read-only collecties en init/private set
zijn zichtbaar, evenals ZoekKasteelOp en IsBewoond.

Associatiepijlen tonen alleen de geïmplementeerde navigatierichting. Bij het doel staat
`0..*` voor de kastelen en de bewoners, en `1` voor de positie. Het diagram claimt geen
niet-geïmplementeerde terugverwijzingen of exclusieve compositie. Private backingfields
van properties worden niet nogmaals getekend; de twee private beheercollecties wel.
Program is het uitvoerscenario en maakt geen deel uit van dit domeindiagram.

## Controleren

`Docs/Controlelijst.md` beschrijft de verwachte resultaten en extra controlegevallen.
De verwachte waarden in Program zijn bedoeld om de uitvoer te controleren; dit is
geen geautomatiseerde unit-test-suite.

De bestanden zijn inhoudelijk en structureel nagekeken. In de omgeving waarin dit
pakket werd samengesteld was geen .NET SDK beschikbaar en kon die niet worden
opgehaald. De C#-code is daar dus niet gecompileerd of uitgevoerd. Voer de bovenstaande
build- en run-opdrachten uit om dat lokaal te verifiëren.

## Bronnen

- [Les en oefening: Klassen, Objecten & Relaties](https://hogent-so2.github.io/so2/01-klassen-objecten.html)
- [Microsoft: init-accessors en object initializers](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/init)
- [Microsoft: List<T>.AsReadOnly](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1.asreadonly?view=net-10.0)
- [Microsoft: Enumerable.OfType](https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.oftype?view=net-10.0)
- [Microsoft: IEquatable<T>](https://learn.microsoft.com/en-us/dotnet/api/system.iequatable-1?view=net-10.0)
