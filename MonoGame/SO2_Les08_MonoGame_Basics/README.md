# Les 08 - Eerste MonoGame-les, inclusief oefeningen

Deze oplossing hoort bij [MonoGame - Een eerste game met een game loop](https://hogent-so2.github.io/so2/30-monogame-basics.html). Ze bevat het eindpunt van de les én alle Must-, Should- en Could-oefeningen. De laatste Could-opdracht is een ontwerpvoorstel en staat hieronder.

## Openen in Visual Studio

1. Installeer de MonoGame-extensie via **Extensions > Manage Extensions**, zoals beschreven in de cursus.
2. Open `MonoGame_Pikachu.sln`. Het project gebruikt .NET 10, net zoals de andere oplossingen in deze repository.
3. Kies **Build > Build Solution**. De MonoGame-buildtaak herstelt de lokale contenttools en bouwt de afbeeldingen en het font.
4. Start met **F5**.

De drie PNG-bestanden en `game-font.spritefont` zijn ongewijzigd overgenomen uit `MonoGame/Assets`. Ze staan in de eigen `Content`-map en zijn geregistreerd in `Content.mgcb`. Dubbelklik op dat bestand om de MGCB Editor te openen. Het meegeleverde font gebruikt Arial.

## Bediening en lesgrenzen

| Toets | Gedrag |
| --- | --- |
| Enter | Starten vanuit het menu of hervatten vanuit pauze |
| Pijltjestoetsen of ZQSD | Pikachu bewegen; twee richtingen tegelijk is mogelijk |
| P | Pauzeren tijdens het spelen |
| Escape | Afsluiten vanuit elke toestand |

`Game1` gebruikt de enum `States` en `if/else`, zoals in de eerste les. `GameOver` is alvast een enumwaarde voor later. Botsingen, levens en stateklassen komen in volgende lessen.

De game loop blijft ook in het menu en tijdens pauze `Update` en `Draw` aanroepen. Alleen de `Play`-tak werkt de wereld en de spawnteller bij. Hervatten bewaart de bestaande spelgegevens.

## Uitwerking van de oefeningen

De constanten gebruiken PascalCase, zoals in de bestaande cursuscode. `PlayerScale` komt bijvoorbeeld overeen met `PLAYER_SCALE` in de cursus.

| Opdracht | Waar en waarom? |
| --- | --- |
| Must: speler binnen het venster | `KeepPlayerInsideWindow` begrenst na de beweging de linkerbovenhoek met de vensterafmetingen min de geschaalde spelerafmetingen. |
| Must: onzichtbare haaien verwijderen | `RemoveSharksOutsideWindow` kijkt naar de rechterkant van de haai. Achterwaarts doorlopen voorkomt overgeslagen items door opschuivende indices. Verwijderen in een `foreach` zou de enumeratie ongeldig maken. |
| Must: maximaal acht haaien | `SpawnSharkIfNeeded` bewaakt `MaximumSharkCount`. Ook wanneer de lijst vol is, wordt de teller op nul gezet; een overgeslagen spawn wordt niet ingehaald zodra er plaats vrijkomt. |
| Should: willekeurige hoogte | `SpawnSharkIfNeeded` kiest één Y-waarde tussen nul en de vensterhoogte min de geschaalde haaihoogte. `UpdateSharkPositions` verandert daarna alleen X. |
| Should: horizontaal gecentreerde tekst | `GetCenteredTextPosition` trekt de gemeten tekstbreedte af van de vensterbreedte en deelt de resterende ruimte in twee gelijke marges. |
| Should: eenvoudige tekstaanroep | `SpriteBatchExtensions.DrawString` vult `Color.White` in. Zowel menu als pauze gebruiken deze overload. |
| Could: herhalend water | `DrawBackground` tekent twee aansluitende exemplaren. `UpdateBackgroundPosition` schuift de eerste positie een texturebreedte op zodra het eerste exemplaar verdwenen is, met behoud van de overschreden afstand. |
| Could: pixels per seconde | `Update` haalt `TotalSeconds` op. De drie bewegingsmethoden vermenigvuldigen hun snelheid met die tijd. 300, 120 en 120 pixels per seconde behouden de snelheid uit de les bij 60 updates per seconde. |

Haaien bewegen en worden opgeruimd vóór de spawncontrole, zodat een volledig verdwenen haai geen plaats bezet bij een nieuw spawnmoment. Elke nieuwe haai start op de rechterrand. De spawnteller volgt dezelfde eenvoudige meting als in de cursus: na een bereikt interval begint de meting opnieuw vanaf nul.

Voor de herhalende achtergrond berekent `LoadContent` de schaal uit de vensterhoogte en de werkelijke texturehoogte. `water.png` is 2154 pixels hoog; de oorspronkelijke schaal `0.25` geeft slechts 538,5 pixels voor een venster van 540 pixels. Zo verdwijnt ook de smalle lege strook onderaan. De geschaalde waterafbeelding is breder dan dit venster, waardoor twee exemplaren volstaan.

## Could: ontwerpvoorstel voor haaien met eigen snelheid en schaal

Wanneer elke haai een eigen snelheid en schaal krijgt, volstaat een `List<Vector2>` niet meer. Losse lijsten voor posities, snelheden en schalen zouden telkens in dezelfde volgorde moeten blijven staan. Een `Shark`-object houdt de gegevens van één haai samen.

| Onderdeel | Gegevens en verantwoordelijkheden |
| --- | --- |
| `Shark` | Bewaart `Position`, `Speed` en `Scale`. `Update(float elapsedSeconds)` berekent de eigen beweging. `IsOutsideWindow(float textureWidth)` bepaalt met de positie en de geschaalde breedte of de haai volledig links verdwenen is. Alleen de haai zelf wijzigt haar positie. |
| `Game1` | Beheert een `List<Shark>`, de spawnteller en de limiet van acht. Kiest bij het aanmaken de beginpositie, snelheid en schaal. Laadt één gedeelde haaientexture, laat elke haai bewegen en tekent haar met de eigen positie en schaal. |
| `SpriteBatchExtensions` | Behoudt de gedeelde tekeninstellingen. Kent geen spawntimer, collectie of speltoestand. |

**Information Expert:** de haai kent haar eigen positie, snelheid en schaal en kan dus de eigen beweging en rechterrand berekenen. `Game1` kent de collectie en de verstreken speeltijd en bewaakt daarom de spawnlimiet en het spawnmoment.

**High Cohesion:** gegevens en gedrag van één haai komen samen, terwijl het beheer van alle haaien bij de game blijft. De gedeelde texture wordt één keer geladen. Dit is het gevraagde ontwerpvoorstel; de uitvoerbare versie behoudt de `List<Vector2>` uit deze les.

## Controleren tijdens het spelen

- Wacht in het menu: de wereld en spawnteller blijven ongewijzigd.
- Beweeg naar alle randen en hoeken, ook diagonaal. Pikachu blijft volledig zichtbaar.
- Controleer dat haaien op verschillende geldige hoogtes verschijnen, hun hoogte behouden en pas verdwijnen wanneer hun rechterkant links uit beeld is.
- Verkort tijdelijk `SharkSpawnTimeInMs` om acht haaien te bereiken. Controleer dat er na een overgeslagen spawn geen negende haai of onmiddellijke vervangende spawn verschijnt. Herstel daarna 3000.
- Pauzeer vóór een spawn, wacht en hervat. Posities, aantal haaien en de al verzamelde speeltijd blijven behouden.
- Laat het water meerdere volledige breedtes doorschuiven: er ontstaat geen lege strook.
- Controleer dat beide teksten gelijke linker- en rechtermarges hebben.
