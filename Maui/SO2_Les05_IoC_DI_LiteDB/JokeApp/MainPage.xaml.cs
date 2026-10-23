using JokeApp.Domain.Interfaces;
using JokeApp.Domain.Models;
using JokeApp.Domain.Validation;
using JokeApp.Navigation;
using JokeApp.Presentation;

namespace JokeApp;

public partial class MainPage : ContentPage
{
    // Low Coupling: voor applicatiebewerkingen kent het scherm IJokeService.
    // De concrete domeinservice en repositories blijven buiten de pagina.
    private readonly IJokeService _jokeService;
    private readonly NavigationService _navigationService;

    // De invoercontrols bevatten een VOORBEREIDE zoekopdracht. Alleen deze
    // fields bevatten de WERKELIJK TOEGEPASTE tekst, categorie en sorteervoorkeur.
    private string _appliedSearchText = string.Empty;
    private JokeCategory? _appliedCategory;
    private bool _sortAlphabetically;
    private bool _isInitialized;
    private bool _isOpeningAddPage;
    private bool _isDeleting;

    private bool IsActionInProgress => _isOpeningAddPage || _isDeleting;

    private readonly CategoryFilterOption _allCategoriesOption =
        new("Alle categorieën", null);

    public MainPage(IJokeService jokeService, NavigationService navigationService)
    {
        ArgumentNullException.ThrowIfNull(jokeService);
        ArgumentNullException.ThrowIfNull(navigationService);

        _jokeService = jokeService;
        _navigationService = navigationService;

        InitializeComponent();
        InitializeCategoryFilter();
        _isInitialized = true;
        UpdateActionAvailability();

        // OnAppearing verzorgt zowel de eerste weergave als het vernieuwen
        // wanneer we van het toevoegformulier terugkeren.
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        RefreshJokes();
    }

    private void InitializeCategoryFilter()
    {
        var options = new List<CategoryFilterOption> { _allCategoriesOption };

        foreach (JokeCategory category in Enum.GetValues<JokeCategory>())
            options.Add(new CategoryFilterOption(category.ToString(), category));

        FilterCategoryPicker.ItemsSource = options;
        FilterCategoryPicker.SelectedItem = _allCategoriesOption;
    }

    private void Search_Clicked(object? sender, EventArgs e)
    {
        if (IsActionInProgress)
            return;

        _appliedSearchText = SearchTextEntry.Text?.Trim() ?? string.Empty;
        _appliedCategory =
            (FilterCategoryPicker.SelectedItem as CategoryFilterOption)?.Category;

        SearchTextEntry.Unfocus();
        RefreshJokes();
    }

    private void ShowAll_Clicked(object? sender, EventArgs e)
    {
        if (IsActionInProgress)
            return;

        // Zowel de zichtbare invoer als de toegepaste filters worden gewist.
        SearchTextEntry.Text = string.Empty;
        FilterCategoryPicker.SelectedItem = _allCategoriesOption;
        _appliedSearchText = string.Empty;
        _appliedCategory = null;

        // Toon alles verwijdert filters, niet de gekozen sorteervoorkeur.
        SearchTextEntry.Unfocus();
        RefreshJokes();
    }

    private void Alphabetical_CheckedChanged(object? sender, CheckedChangedEventArgs e)
    {
        // XAML kan al events veroorzaken terwijl InitializeComponent nog bezig is.
        if (!_isInitialized || IsActionInProgress)
            return;

        _sortAlphabetically = e.Value;

        // Sorteren past geen intussen getypte, maar nog niet gezochte tekst toe.
        RefreshJokes();
    }

    private void RefreshJokes()
    {
        ClearActionFeedback();

        // Lees hier NIET uit de invoercontrols. Ook na verwijderen en terugkeer
        // blijft alleen de eerder toegepaste zoekopdracht bepalend.
        IReadOnlyList<Joke> results = _jokeService.Search(
            _appliedSearchText, _appliedCategory, _sortAlphabetically);
        int totalCount = _jokeService.Count();

        // Na vernieuwen kan de geselecteerde grap verdwenen of weggefilterd zijn.
        // Wis daarom de selectie en de bijbehorende details.
        // Bij LiteDB krijgen we bovendien nieuwe objectinstanties na het lezen.
        JokesCollectionView.SelectedItem = null;
        JokesCollectionView.ItemsSource = results;
        UpdateDetails(null);
        UpdateStatus(results, totalCount);
        UpdateEmptyState(totalCount);
        UpdateAppliedFiltersLabel();
        UpdateActionAvailability();
    }

    private void Joke_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!_isInitialized)
            return;

        UpdateDetails(e.CurrentSelection.FirstOrDefault() as Joke);
        UpdateActionAvailability();
    }

    private void UpdateDetails(Joke? joke)
    {
        if (joke is null)
        {
            CurrentJokeCategoryLabel.Text = "Nog geen grap gekozen";
            CurrentJokeLabel.Text = "Selecteer een grap om de volledige tekst te lezen.";
            return;
        }

        CurrentJokeLabel.Text = joke.Text;
        CurrentJokeCategoryLabel.Text = $"Categorie: {joke.Category}";
    }

    private void UpdateActionAvailability()
    {
        // Beschikbaarheid is AFGELEID van de selectie en de lopende actie.
        // Bij elke selectiewijziging én vernieuwing berekenen we ze opnieuw.
        DeleteJokeButton.IsEnabled =
            !IsActionInProgress && JokesCollectionView.SelectedItem is Joke;
        OpenAddJokeButton.IsEnabled = !IsActionInProgress;

        // Tijdens de bevestiging kan de gebruiker niet ongemerkt een andere
        // selectie of filter toepassen. De handler controleert dit ook zelf.
        SearchPanel.IsEnabled = !IsActionInProgress;
        JokesCollectionView.IsEnabled = !IsActionInProgress;
    }

    private async void DeleteJoke_Clicked(object? sender, EventArgs e)
    {
        // async void hoort bij het event; de bewerking zelf geeft een Task terug.
        await DeleteSelectedJokeAsync();
    }

    private async Task DeleteSelectedJokeAsync()
    {
        // IsEnabled beschermt de klikroute, niet iedere denkbare aanroep.
        // Controleer de voorwaarden daarom ook in de eigenlijke bewerking.
        if (IsActionInProgress || JokesCollectionView.SelectedItem is not Joke selectedJoke)
            return;

        // Bewaar het gekozen object vóór await. We verwijderen straks de id
        // waarover deze bevestiging ging, niet een eventueel latere selectie.
        _isDeleting = true;
        ClearActionFeedback();
        UpdateActionAvailability();

        try
        {
            bool confirmed = await DisplayAlertAsync(
                "Grap verwijderen?",
                $"Wil je deze grap definitief verwijderen?\n\n{selectedJoke.Text}",
                "Verwijderen",
                "Annuleren");

            if (!confirmed)
                return; // Geen service-aanroep, geen wijziging aan lijst of selectie.

            // De service voert de bewerking uit en geeft feedback terug.
            // Alleen ItemsSource aanpassen zou de opgeslagen grap niet verwijderen.
            ValidationResult result = _jokeService.Remove(selectedJoke.Id);

            // Ook bij een intussen verdwenen id verversen we een mogelijk verouderd
            // scherm. De fout blijft daarna zichtbaar via het ValidationResult.
            RefreshJokes();
            ShowDeleteFeedback(result);
        }
        finally
        {
            // Ook bij annuleren herstellen we de bediening. We gebruiken hier
            // geen catch(Exception) die technische fouten zou verbergen.
            _isDeleting = false;
            UpdateActionAvailability();
        }
    }

    private void ShowDeleteFeedback(ValidationResult result)
    {
        DeleteFeedbackLabel.Text = result.IsValid ? "Grap verwijderd." : string.Empty;
        DeleteFeedbackLabel.IsVisible = result.IsValid;
        DeleteErrorLabel.Text = result.ErrorMessage;
        DeleteErrorLabel.IsVisible = !result.IsValid;
    }

    private void ClearActionFeedback()
    {
        DeleteFeedbackLabel.Text = string.Empty;
        DeleteFeedbackLabel.IsVisible = false;
        DeleteErrorLabel.Text = string.Empty;
        DeleteErrorLabel.IsVisible = false;
    }

    private void UpdateStatus(IReadOnlyList<Joke> results, int totalCount)
    {
        StatusLabel.Text = $"{results.Count} van {totalCount} grappen";

        int categoryCount = results.Select(joke => joke.Category).Distinct().Count();
        string noun = categoryCount == 1 ? "categorie" : "categorieën";
        ResultCategoriesLabel.Text = $"{categoryCount} {noun} in de resultaten";
    }

    private void UpdateEmptyState(int totalCount)
    {
        // Nul RESULTATEN is niet hetzelfde als nul OPGESLAGEN grappen.
        // Na verwijderen van het laatste zoekresultaat kan de bibliotheek
        // nog andere grappen bevatten die niet aan de huidige filters voldoen.
        if (totalCount == 0)
        {
            EmptyStateTitleLabel.Text = "Je bibliotheek is leeg";
            EmptyStateMessageLabel.Text =
                "Gebruik Nieuwe grap om je eerste grap toe te voegen.";
        }
        else
        {
            EmptyStateTitleLabel.Text = "Geen grappen gevonden";
            EmptyStateMessageLabel.Text =
                "Pas je zoektekst of categorie aan en klik op Zoeken, of kies Toon alles.";
        }
    }

    private void UpdateAppliedFiltersLabel()
    {
        string textDescription = _appliedSearchText.Length == 0
            ? "alle teksten"
            : $"tekst bevat ‘{_appliedSearchText}’";
        string categoryDescription = _appliedCategory?.ToString() ?? "alle categorieën";

        AppliedFiltersLabel.Text = $"Toegepast: {textDescription} · {categoryDescription}";
    }

    private async void OpenAddJoke_Clicked(object? sender, EventArgs e)
    {
        if (IsActionInProgress)
            return;

        _isOpeningAddPage = true;
        UpdateActionAvailability();

        try
        {
            await _navigationService.ShowAddJokeAsync();
        }
        finally
        {
            _isOpeningAddPage = false;
            UpdateActionAvailability();
        }
    }
}
