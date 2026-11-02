# Les 6 — MVVM, binding en commands

Voorbeeldoplossing voor Softwareontwerp 2, eerste jaar hoger onderwijs,
tweede semester. Gebaseerd op `SO2_Les05_IoC_DI_LiteDB.zip` en cursuspagina 26:
[MAUI — MVVM, binding & commands](https://hogent-so2.github.io/so2/26-maui_mvvm.html).

De lesopbouw en de Must-, Should- en Could-uitbreidingen zijn in dezelfde app
verwerkt. Dit is een refactoring, geen nieuwe JokeApp. De drie bestaande
applicatieprojecten, namespaces, styles, platformconfiguratie en database-identiteit
blijven behouden. De tijdelijke proefpagina's uit de les zijn niet in de eindapp
opgenomen; de les vraagt ze na de proef weer te verwijderen.

## Openen in Visual Studio 2026

1. Pak de ZIP volledig uit in een gewone schrijfbare map.
2. Open `JokeApp.sln` in Visual Studio 2026 met .NET 10 en de workload
   **.NET Multi-platform App UI development**.
3. Laat NuGet de packages herstellen. Stel `JokeApp` in als opstartproject.
4. Kies eerst **Windows Machine** en start met F5. Android blijft een tweede doel.

De Windows-versie is, net als de bijlage, unpackaged: een eigen MSIX-certificaat
is niet nodig. De bestaande packageversies zijn behouden:
`Microsoft.Maui.Controls` 10.0.110 en `LiteDB` 5.0.21.

LiteDB is actief. Een nieuwe `jokes.db` begint leeg. Voeg via **Nieuwe grap**
voorbeelden toe, bijvoorbeeld:

| Tekst | Categorie |
| --- | --- |
| Wat doet een kat achter de computer? De muis zoeken. | DadJoke |
| Waarom nam het boek vakantie? Het was toe aan een nieuw hoofdstuk. | Other |
| Waarom was de programmeur zo rustig? Hij had alles onder Ctrl. | Programming |

Het bestand staat nog steeds in `FileSystem.AppDataDirectory`. Dezelfde
application-id en bestandsnaam zijn behouden; bestaande gegevens worden niet
bewust gewist, gemigreerd of opnieuw met voorbeelden gevuld. Een andere
installatiecontext kan uiteraard een andere appdatamap gebruiken.

## Projecten

| Project | Verantwoordelijkheid |
| --- | --- |
| `JokeApp` | XAML, ViewModels, navigatie, dialoog en het samenstelpunt `MauiProgram`. |
| `JokeApp.Domain` | Modellen, validator, service en opslagcontract. Geen MAUI of LiteDB. |
| `JokeApp.Persistence` | Beide implementaties van het opslagcontract. |
| `JokeApp.Checks` | Optionele consolecontroles; geen extra laag in de app. |

De afhankelijkheden tijdens uitvoering zijn nu:

```text
MainPage / AddJokePage
    └── bindings en commands → JokeListViewModel / AddJokeViewModel
                                  ├── IJokeService → JokeService
                                  │                    └── IJokeRepository
                                  │                          ├── InMemoryJokeRepository
                                  │                          └── LiteDbJokeRepository
                                  ├── INavigationService → ShellNavigationService
                                  └── IDialogService → ShellDialogService
```

Het diagram toont aanroepen tijdens uitvoering, niet de richting van alle
projectreferenties. `IJokeRepository` blijft in Domain; Persistence verwijst
naar Domain en niet omgekeerd. Binnen het MAUI-project kent alleen
`MauiProgram` de concrete repositoryklassen.

## Wat is uitgewerkt?

**Lesopbouw.** Een eigen `ViewModelBase` met `INotifyPropertyChanged`, `SetProperty`
en `CallerMemberName`; twee ViewModels; expliciete `TwoWay`-bindings voor invoer
en selectie; een blijvende `ObservableCollection<Joke>` voor het overzicht;
de ingebouwde MAUI-`Command`; navigatie achter `INavigationService`;
constructorinjectie en transient registraties van ViewModels en pagina's.
Er is geen MVVM-toolkit of source generator toegevoegd.

**Must.** `DeleteJokeCommand` gebruikt `CanExecute` en `ChangeCanExecute`.
De service verwijdert op id. Fouten en succes worden via `StatusMessage`
getoond. De lijst, tellers, selectie en details worden vernieuwd zonder
voorbereide zoekinvoer toe te passen. `CharacterCountText` meldt wijzigingen
tijdens typen. Opslaan blijft beschikbaar bij ongeldige invoer.

**Should.** De bevestigingsdialoog gebruikt een klein `IDialogService`-contract.
De consolecontroles onderzoeken onder meer de propertynamen in
`PropertyChanged`, lege resultaten en het verwijderen van de laatste grap
met beide repositories. Het handmatige testplan bevat de aanvullende
UI- en breakpointcontroles.

**Could.** `ClearInputCommand` wist tekst en foutfeedback, herstelt Programming
als standaardcategorie en toont opnieuw een teller van nul, zonder op te slaan.

Ook de bestaande categoriefilter, categorieteller, alfabetische sortering,
lege-bibliotheekmelding, duplicatecontrole en styles zijn behouden.

## Aanbevolen leesvolgorde

Begin bij `ViewModels/ViewModelBase.cs`. Volg daarna `SearchCommand` in
`JokeListViewModel` en de bijbehorende binding in `MainPage.xaml`.
Bekijk vervolgens `SelectedJoke` en `DeleteJokeCommand`. Werk tot slot
`AddJokeViewModel` en `AddJokePage.xaml` door.

De code-behind van MainPage stelt alleen de context in en geeft `OnAppearing`
door. De code-behind van AddJokePage stelt alleen de context in. De pagina's
lezen geen invoercontrols uit, valideren niet en spreken geen service aan.

## De belangrijkste afspraken

Een formulier mag tijdelijk ongeldige invoer bevatten. Een `Joke` mag dat niet.
Daarom bindt de editor aan `AddJokeViewModel.Text`, niet rechtstreeks aan de
`Text`-property van een opgeslagen domeinobject. Pas **Opslaan** valideert,
maakt het model en vraagt de service om toevoeging. Duplicaten blijven een
regel van de service. Tijdens typen verdwijnt alleen oude foutfeedback.

De tekenteller gebruikt `Text.Trim().Length`, net als de validator. Begin- en
eindwitruimte tellen niet mee; er is geen `MaxLength` dat lange invoer afkapt.

`SearchText` is nog geen uitgevoerde zoekopdracht. Alleen **Zoeken** neemt die
waarde en de voorbereide categorie over. `Reload()` gebruikt altijd de laatst
toegepaste criteria, ook na toevoegen, verwijderen of terugnavigeren.

## Opslag vergelijken

Vervang in `MauiProgram` het volledige actieve LiteDB-registratieblok door:

```csharp
builder.Services.AddSingleton<IJokeRepository, InMemoryJokeRepository>();
```

Registreer steeds maar één repository. Start de app opnieuw na zo'n wijziging.
Dit wisselt van opslag, maar migreert geen gegevens. Zet voor de eindversie
LiteDB terug actief. De repository is singleton; service, navigatie, dialoog,
ViewModels en pagina's zijn transient.

## Consolecontroles uitvoeren

Vanuit de solutionmap:

```sh
dotnet run --project JokeApp.Checks/JokeApp.Checks.csproj
```

Dit is een consoleproject, geen xUnit/MSTest-project. Gebruik dus `dotnet run`,
niet `dotnet test`. Het print per scenario OK of FOUT en eindigt met exitcode 1
wanneer een controle mislukt.

De controles linken rechtstreeks naar de echte ViewModel-bronbestanden en
gebruiken de echte MAUI-`Command` uit de platformneutrale package-assets.
Alleen navigatie en dialoog worden vervangen door eenvoudige testimplementaties.
Dezelfde scenario's gebruiken beide echte repositories. De LiteDB-controles
maken tijdelijke databases; ze raken de database van de app niet aan.

## Documentatie en verificatiestatus

- `Documentatie/Opdrachtdekking.md`: elke opdracht gekoppeld aan de implementatie.
- `Documentatie/MVVM-keuzes.md`: verantwoordelijkheden en lastige ontwerpkeuzes.
- `Documentatie/Testplan.md`: handmatige scenario's en breakpointaanwijzingen.
- `Documentatie/Wijzigingen.md`: verschillen met jouw specifieke bijlage.
- `Documentatie/Controleverslag.md`: uitgevoerde controles en beperkingen.

De bestanden en koppelingen zijn statisch gecontroleerd. In de opleveromgeving
was geen .NET SDK of MAUI-workload beschikbaar. De solution en de meegeleverde
C#-controles zijn daarom hier niet gecompileerd of uitgevoerd. Een statische
controle is geen vervanging voor een build en een test op Windows/Android.
