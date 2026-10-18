using JokeApp.Models;
using JokeApp.Services;
using JokeApp.Validation;

namespace JokeApp;

public partial class AddJokePage : ContentPage
{
    private const JokeCategory DefaultCategory = JokeCategory.Programming;
    private readonly JokeService _jokeService;
    private bool _hasSaved;

    public AddJokePage(JokeService jokeService)
    {
        ArgumentNullException.ThrowIfNull(jokeService);
        _jokeService = jokeService;

        InitializeComponent();

        // Geen new JokeService(): beide pagina's moeten dezelfde bibliotheek gebruiken.
        CategoryPicker.ItemsSource = Enum.GetValues<JokeCategory>().ToList();
        TextRulesLabel.Text =
            $"Gebruik {JokeTextValidator.MinimumLength} tot {JokeTextValidator.MaximumLength} tekens. " +
            "Spaties aan het begin en einde tellen niet mee.";

        ResetInput();
    }

    private async void AddJoke_Clicked(object? sender, EventArgs e)
    {
        // async void hoort hier bij de event-handler. De herbruikbare bewerking geeft een Task terug, zodat de aanroeper de navigatie kan afwachten.
        // async / await zal je later nog zien.
        await TryAddJokeAsync();
    }

    private async Task TryAddJokeAsync()
    {
        // Na succesvol toevoegen wachten we alleen nog op de terugnavigatie.
        // Een tweede snelle klik mag de bewerking dan niet nogmaals uitvoeren.
        if (_hasSaved)
            return;

        ClearValidationError();
        FeedbackLabel.Text = string.Empty;

        string text = JokeTextEditor.Text ?? string.Empty;
        var validation = JokeTextValidator.Validate(text);

        if (!validation.IsValid)
        {
            ShowValidationError(validation.ErrorMessage);
            JokeTextEditor.Focus();
            return;
        }

        if (CategoryPicker.SelectedItem is not JokeCategory category)
        {
            ShowValidationError("Kies een categorie.");
            CategoryPicker.Focus();
            return;
        }

        // Verwachte invoerfouten zijn al behandeld. De setter blijft zichzelf
        // beschermen voor aanroepers die géén formuliercontrole uitvoeren.
        var joke = new Joke(text, category);
        var addResult = _jokeService.Add(joke);

        if (!addResult.IsValid)
        {
            // Bijvoorbeeld een duplicaat: blijf op de pagina, behoud tekst en
            // categorie en laat de gebruiker zijn invoer zelf aanpassen.
            ShowValidationError(addResult.ErrorMessage);
            JokeTextEditor.Focus();
            return;
        }

        // Alleen NA succes sluiten we het formulier. We schakelen de knop niet
        // uit om ongeldige invoer te verbergen, maar om dubbel opslaan te voorkomen.
        _hasSaved = true;
        AddJokeButton.IsEnabled = false;
        ClearInputButton.IsEnabled = false;
        await Navigation.PopAsync();

        // MainPage vernieuwt zichzelf in OnAppearing. Deze pagina hoeft geen
        // labels, lijst of filters van een andere pagina te kennen.
    }

    private void JokeText_Changed(object? sender, TextChangedEventArgs e)
    {
        // Alleen oude feedback wissen: geen validatie tijdens het typen.
        // Een verdwenen foutmelding betekent dus niet automatisch 'geldig'.
        FeedbackLabel.Text = string.Empty;
        ClearValidationError();
    }

    private void Category_SelectionChanged(object? sender, EventArgs e)
    {
        UpdateSelectedCategoryLabel();
        FeedbackLabel.Text = string.Empty;
        ClearValidationError();
    }

    private void ClearInput_Clicked(object? sender, EventArgs e)
    {
        ResetInput();
        FeedbackLabel.Text = "Invoer gewist.";
        JokeTextEditor.Focus();
    }

    private void ResetInput()
    {
        JokeTextEditor.Text = string.Empty;
        CategoryPicker.SelectedItem = DefaultCategory;

        // Bij dezelfde beginwaarde ontstaat niet noodzakelijk een selectie-event.
        // ResetInput zorgt daarom zelf voor een consistente volledige beginstand.
        UpdateSelectedCategoryLabel();
        FeedbackLabel.Text = string.Empty;
        ClearValidationError();
    }

    private void UpdateSelectedCategoryLabel()
    {
        SelectedCategoryLabel.Text = CategoryPicker.SelectedItem is JokeCategory category
            ? $"Gekozen categorie voor de nieuwe grap: {category}"
            : "Nog geen categorie geselecteerd";
    }

    private void ShowValidationError(string message)
    {
        ValidationLabel.Text = message;
        ValidationLabel.IsVisible = true;
    }

    private void ClearValidationError()
    {
        ValidationLabel.Text = string.Empty;
        ValidationLabel.IsVisible = false;
    }
}
