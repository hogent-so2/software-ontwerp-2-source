using JokeApp.Models;
using JokeApp.Services;

namespace JokeApp;

public partial class MainPage : ContentPage
{
    // De standaardkeuze krijgt een naam, zodat openen en wissen dezelfde waarde gebruiken.
    private const JokeCategory DefaultCategory = JokeCategory.Programming;

    // Eén instantie per pagina: een nieuwe service per klik zou de toegevoegde grappen verliezen.
    private readonly JokeService _jokeService = new();

    // Could: dit is schermtoestand. De service hoeft niet te weten wat we eerder toonden.
    // Het vraagteken betekent dat er nog geen willekeurige grap gekozen kan zijn.
    private Joke? _lastRandomJoke;
    private Joke? _currentJoke;
    private int _randomRequestCount;

    public MainPage()
    {
        // Deze lijn code zou hier niet werken gezien de 'InitializeComponent' de variabele 'CategoryPicker' aanmaakt.
        // CategoryPicker.ItemsSource = Enum.GetValues<JokeCategory>().ToList();

        InitializeComponent();

        // De controls uit XAML bestaan pas na InitializeComponent().
        // De Picker bewaart de enumwaarden als items, niet alleen hun tekstuele namen.
        CategoryPicker.ItemsSource = Enum.GetValues<JokeCategory>().ToList();

        ResetInput();
        UpdateStatus();
        UpdateRandomRequestCount();
    }

    private void ShowRandomJoke_Clicked(object? sender, EventArgs e)
    {
        // Bewaar de verwijzing naar het object, niet alleen joke.Text.
        // Zo kunnen we later ook de bijbehorende categorie opnieuw tonen.
        _lastRandomJoke = _currentJoke;
        _currentJoke = _jokeService.GetRandom();

        _randomRequestCount++;

        ShowJoke(_currentJoke);

        if ( _lastRandomJoke is not null )
            ShowLastRandomJokeButton.IsEnabled = true;

        UpdateRandomRequestCount();
    }

    private void ShowLastRandomJoke_Clicked(object? sender, EventArgs e)
    {
        // De knop staat normaal uit wanneer de verwijzing null is.
        // Toch maken we ook in de code expliciet dat er een grap nodig is.
        if (_lastRandomJoke is null)
            return;

        // Opnieuw tonen is geen nieuwe willekeurige opvraag: de teller blijft gelijk.
        ShowJoke(_lastRandomJoke);
    }

    private void JokeText_Changed(object? sender, TextChangedEventArgs e)
    {
        UpdateAddButtonState();
        FeedbackLabel.Text = string.Empty;
    }

    private void Category_SelectionChanged(object? sender, EventArgs e)
    {
        // Dit event kan ook optreden wanneer onze code de selectie verandert,
        // bijvoorbeeld bij het openen van de pagina of bij Invoer wissen.
        UpdateSelectedCategoryLabel();
        UpdateAddButtonState();
        FeedbackLabel.Text = string.Empty;
    }

    private void JokeText_Completed(object? sender, EventArgs e)
    {
        // De Enter-/Done-toets en de knop gebruiken exact dezelfde toevoeglogica.
        TryAddJoke();
    }

    private void AddJoke_Clicked(object? sender, EventArgs e)
    {
        TryAddJoke();
    }

    private void ClearInput_Clicked(object? sender, EventArgs e)
    {
        // Wissen verandert uitsluitend de invoer, niet de collectie of de getoonde grap.
        ResetInput();
        FeedbackLabel.Text = "Invoer gewist.";
        JokeTextEntry.Focus();
    }

    private void TryAddJoke()
    {
        string? text = JokeTextEntry.Text;

        // Alleen de knop uitschakelen volstaat niet: via Completed kan deze methode
        // ook worden uitgevoerd. Beide invoerroutes moeten dezelfde controle krijgen.
        if (string.IsNullOrWhiteSpace(text))
        {
            FeedbackLabel.Text = "Typ eerst een grap.";
            JokeTextEntry.Focus();
            return;
        }

        // SelectedItem heeft type object. Met deze controle krijgen we veilig
        // de echte enumwaarde en behandelen we ook een ontbrekende selectie.
        if (CategoryPicker.SelectedItem is not JokeCategory category)
        {
            FeedbackLabel.Text = "Kies eerst een categorie.";
            CategoryPicker.Focus();
            return;
        }

        // De pagina leest de invoer; de service beheert de collectie.
        // Trim verwijdert alleen spaties aan het begin en einde van de invoer.
        var joke = new Joke(text.Trim(), category);

        _jokeService.Add(joke);

        ShowJoke(joke);
        JokeTextEntry.Text = string.Empty;
        UpdateAddButtonState();
        UpdateStatus();
        FeedbackLabel.Text = "Je grap is toegevoegd.";

        // Should: de gebruiker kan meteen verder typen.
        // De gekozen categorie blijft behouden; alleen Invoer wissen zet die terug.
        JokeTextEntry.Focus();

        // _lastRandomJoke wordt hier NIET gewijzigd: een zelf toegevoegde grap
        // is niet de laatst willekeurig opgevraagde grap.
    }

    private void ShowJoke(Joke joke)
    {
        // Eén plaats voor de weergave voorkomt dat we de tekst aanpassen,
        // maar per ongeluk de categorie van de vorige grap laten staan.
        CurrentJokeLabel.Text = joke.Text;
        CurrentJokeCategoryLabel.Text = $"Categorie: {joke.Category}";
    }

    private void ResetInput()
    {
        JokeTextEntry.Text = string.Empty;

        // De standaardkeuze wordt gevonden op waarde, niet via een magisch indexgetal.
        // Daardoor blijft dit correct wanneer de enum later een andere volgorde krijgt.
        CategoryPicker.SelectedItem = DefaultCategory;

        // Bij dezelfde waarde hoeft geen change-event te ontstaan.
        // Deze methode zorgt daarom zelf voor een volledige, consistente invoertoestand.
        UpdateSelectedCategoryLabel();
        UpdateAddButtonState();
    }

    private void UpdateAddButtonState()
    {
        AddJokeButton.IsEnabled =
            !string.IsNullOrWhiteSpace(JokeTextEntry.Text)
            && CategoryPicker.SelectedItem is JokeCategory;
    }

    private void UpdateSelectedCategoryLabel()
    {
        if (CategoryPicker.SelectedItem is JokeCategory category)
        {
            SelectedCategoryLabel.Text = $"Gekozen categorie voor de nieuwe grap: {category}";
        }
        else
        {
            SelectedCategoryLabel.Text = "Nog geen categorie geselecteerd";
        }
    }

    private void UpdateStatus()
    {
        // Leid het aantal af uit de collectie. Een tweede teller voor opgeslagen
        // grappen zou na een wijziging uit de pas kunnen lopen.
        int count = _jokeService.GetAll().Count;
        StatusLabel.Text = $"{count} grappen beschikbaar";
    }

    private void UpdateRandomRequestCount()
    {
        RandomRequestCountLabel.Text =
            $"Willekeurige grappen opgevraagd: {_randomRequestCount}";
    }
}
