# Les 5 — IoC, Dependency Injection en LiteDB

Dit is de voorbeeldoplossing **na les 5, inclusief de uitgewerkte thuisopdracht**.
Verwijderen, de bevestigingsdialoog en de verschillende lege toestanden zijn al
uitgewerkt. Gebruik deze versie als referentie of als vertrekpunt voor de
volgende les over MVVM.

## Drie projecten, drie verantwoordelijkheden

| Project | Verantwoordelijkheid | Belangrijkste onderdelen |
| --- | --- | --- |
| `JokeApp` | Presentatie en samenstellen van de applicatie | Pagina's, XAML, styles, navigatie, `MauiProgram` |
| `JokeApp.Domain` | Modellen, geldigheidsregels en applicatiebewerkingen | `Joke`, validatie, `IJokeService`, `JokeService`, `IJokeRepository` |
| `JokeApp.Persistence` | Opslag achter het domeincontract | `InMemoryJokeRepository`, `LiteDbJokeRepository` |

De twee classlibraries gebruiken gewone .NET en hebben geen MAUI-workload nodig.
Alleen Persistence verwijst rechtstreeks naar het NuGet-package LiteDB.

De **projectreferenties** lopen als volgt:

```text
JokeApp ────────────────> JokeApp.Domain
   └──> JokeApp.Persistence ───> JokeApp.Domain
```

`IJokeRepository` staat in Domain: het domein bepaalt welke opslagbewerkingen
het nodig heeft. Persistence implementeert dat contract. Domain verwijst dus
niet naar Persistence, MAUI of LiteDB.

Tijdens de **uitvoering** roept een pagina de service aan en gebruikt de service
de repository via `IJokeRepository`. Die aanroepvolgorde verschilt van de richting
van de projectreferenties. De interface maakt die ontkoppeling mogelijk.

Binnen de presentatie kent alleen `MauiProgram` de concrete persistentieklassen.
Dit samenstelpunt kiest de implementaties en hun lifetimes. De pagina's
gebruiken `IJokeService`; de projectreferentie is geen uitnodiging om vanuit
een pagina rechtstreeks de database aan te spreken.

Zoeken en sorteren horen bij de repository: de service geeft de zoekcriteria
door via het contract. De in-memory repository gebruikt LINQ op haar lijst.
De LiteDB-repository bouwt een databasequery en haalt alleen de resultaten
als objecten op. Een substringzoekopdracht kan nog steeds documenten in de
database doorlopen; ze laadt niet eerst de volledige collectie in de service.

De zoektekst wordt letterlijk en hoofdletterongevoelig gezocht. De LiteDB-query
gebruikt daarvoor een ge-escapet patroon als parameter, zodat ook procenttekens,
underscores en andere bijzondere tekens gewone zoektekst blijven. Alfabetisch
sorteren gebruikt in LiteDB de databasecollation (standaard de cultuur bij het
maken van de database, zonder onderscheid tussen hoofdletters en kleine letters).
De in-memory repository gebruikt de huidige cultuur.

Ook de totaalteller gebruikt een repositorybewerking: Count telt in de opslag,
zonder alle grappen voor de teller op te halen.

## Verband met de vorige lessen

- **OO en encapsulatie:** `Joke` bewaakt zijn tekst en gebruikt `init`.
  Collecties zijn alleen read-only toegankelijk.
- **GRASP Controller:** `JokeService` ontvangt en coördineert applicatiebewerkingen.
- **Information Expert:** één grap bewaakt haar tekst; de service controleert
  unieke tekst over meerdere grappen.
- **High Cohesion en Low Coupling:** schermcode, applicatieregels en opslag
  hebben ieder een eigen plaats.
- **Protected Variations en polymorfisme:** dezelfde service werkt met beide
  implementaties van `IJokeRepository`, zonder op het concrete type te testen.
- **XAML, controls en events:** de pagina's blijven met code-behind werken.
  De item-bindings in de lijst zijn nog geen MVVM-architectuur.
- **Layout, styling en validatie:** categorieën, zoeken, sorteren, styles,
  formulierfeedback en het bewaren van de toegepaste zoekopdracht blijven bestaan.

De UI gebruikt de gedeelde validator voor feedback vóór ze een model maakt.
Het model gebruikt dezelfde regels om zichzelf te beschermen. Twee aanroepen
van de validator betekenen niet dat de regels tweemaal beschreven zijn.

## Van klik naar opslag

Volg bij toevoegen met breakpoints deze weg:

```text
AddJokePage → JokeTextValidator → new Joke(...) → IJokeService.Add(...)
           → JokeService → IJokeRepository.Add(...) → LiteDbJokeRepository
```

Bij een fout blijft de invoer staan. Na succes navigeert de pagina terug via
`NavigationService`. `MainPage.OnAppearing()` vernieuwt lijst en teller met
behoud van de toegepaste filters.

Een DI-registratie bepaalt hoe een object ontstaat en hoe lang het gedeeld wordt.
Een route in `AppShell` koppelt een navigatiebestemming aan een pagina.
Een `GoToAsync`-aanroep voert de navigatie uit. Dit zijn drie verschillende taken.

## Starten en opslag vergelijken

Open `JokeApp.sln`, kies `JokeApp` als opstartproject en start op Windows of Android.
Het MAUI-project vereist de bijbehorende .NET 10-workload.

LiteDB is de actieve registratie. Het bestand `jokes.db` staat in
`FileSystem.AppDataDirectory`; een nieuwe database begint leeg.

Om in-memory opslag te proberen, vervang je in `MauiProgram` het volledige
registratieblok voor `IJokeRepository` door de aangegeven in-memory registratie.
Registreer steeds één implementatie. Dit wisselt de opslag, maar migreert geen data.

De repository is singleton; de service en pagina's zijn transient.
Verschillende services delen daardoor dezelfde repository. Een singleton
bewaart op zichzelf geen gegevens na het afsluiten van de app.

Controleer toevoegen, duplicaten, zoeken, verwijderen en terugkeren met een
toegepast filter bij beide implementaties. Sluit de app volledig af en start
opnieuw om het verschil tussen in-memory opslag en LiteDB te zien.
