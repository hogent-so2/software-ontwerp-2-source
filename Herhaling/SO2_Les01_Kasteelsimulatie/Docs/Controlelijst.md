# Controlelijst — basisoplossing

Dit zijn verwachte resultaten op basis van de code, geen verslag van een uitgevoerde
.NET-run. Voer Program.cs lokaal uit en vergelijk de uitvoer. Tekst die door .NET
zelf aan uitzonderingsberichten wordt toegevoegd, kan per taalinstelling verschillen.

## Verwachte resultaten van Program.cs

| Stap | Verwacht |
| --- | --- |
| Twee geldige kastelen toevoegen | Raster.Kastelen.Count is 2. |
| Rood kasteel vóór bewoners toevoegen | IsBewoond() is False. |
| Bewoners toegevoegd | Blauw bevat Emma, Arthur, Nora en Sam; rood bevat Lotte en Bas. |
| Aantallen bewoners | Blauw heeft 4 bewoners, rood heeft 2 bewoners. |
| Rood kasteel na bewoners toevoegen | IsBewoond() is True. |
| HangVlagBij op blauw | Het aantal stijgt van 2 naar 3. |
| Strijders van blauw | Alleen Arthur en Sam produceren strijd-uitvoer. |
| Zoek met een nieuw Coordinate(1, 1) | Het blauwe kasteel wordt gevonden. |
| Zoek op (0, 0) | null; het vak ligt binnen het raster maar is leeg. |
| Bevat (0, 0) en (4, 3) | True voor beide posities. |
| Bevat (5, 0), (0, 4), (-1, 0) en (0, -1) | False voor alle vier. |
| Voeg een kasteel op (5, 0) toe | ArgumentException, geen toevoeging. |
| Voeg een tweede kasteel op (1, 1) toe | InvalidOperationException, geen toevoeging. |
| Aantal na geweigerde toevoegingen | Nog steeds 2. |
| new Raster(0, 4) | ArgumentOutOfRangeException. |
| new Kok("   ", "Soep") | ArgumentException. |
| new Raster(5, 4) { Breedte = 0 } | ArgumentOutOfRangeException. |

Geen enkele regel in de uitvoer hoort met `ONVERWACHT` te beginnen.

## Extra gevallen om zelf te onderzoeken

| Controle | Verwacht |
| --- | --- |
| Breedte of hoogte negatief; hoogte nul | ArgumentOutOfRangeException. |
| Lege prinsessenbeschrijving of kokspecialiteit | ArgumentException. |
| Zwaardlengte nul of negatief | ArgumentOutOfRangeException. |
| Negatief aantal vlaggetjes | ArgumentOutOfRangeException. |
| Ongeldige kleur via (KasteelKleur)999 | ArgumentOutOfRangeException. |
| Hetzelfde kasteel nogmaals aan hetzelfde raster toevoegen | InvalidOperationException wegens bezette positie. |
| ZoekKasteelOp met een positie buiten het raster | null. |
| Ongeldige Naam of ZwaardLengte via een object initializer | Dezelfde validatiefout als bij de constructor. |
| HangVlagBij bij int.MaxValue vlaggetjes | InvalidOperationException; het aantal blijft ongewijzigd. |
| Read-only lijst bewaren en daarna via de eigenaar toevoegen | De bewaarde leesweergave toont de nieuwe inhoud ook. |

De methoden die een object nodig hebben, weigeren een null-argument met
ArgumentNullException. Nullable reference types helpen al tijdens het schrijven
van gewone aanroepende code om zulke ongeldige argumenten te herkennen.

## Opzettelijk niet-compilerende voorbeelden

In Program.cs staan commentaarregels die een publieke setter of een publieke
wijzigbare lijst proberen te gebruiken. Haal tijdelijk één zo'n commentaar weg
om de compilerfout te bekijken, en plaats het commentaar daarna terug.
Dit is een controle van de publieke API, niet van een runtime-uitzondering.
