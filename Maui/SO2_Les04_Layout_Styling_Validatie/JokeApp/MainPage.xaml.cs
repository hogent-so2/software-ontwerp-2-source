using JokeApp.Models;
using JokeApp.Presentation;
using JokeApp.Services;

namespace JokeApp;

public partial class MainPage : ContentPage
{
    // Eén gedeelde bibliotheek. AddJokePage ontvangt straks DEZE instantie.
    private readonly JokeService _jokeService = new();

    // Het invoerveld en de picker bevatten wat de gebruiker aan het voorbereiden is.
    // Deze fields bewaren uitsluitend wat met Zoeken werkelijk is toegepast.
    private string _appliedSearchText = string.Empty;
    private JokeCategory? _appliedCategory;
    private bool _sortAlphabetically;
    private bool _isOpeningAddPage;

    private readonly CategoryFilterOption _allCategoriesOption =
        new("Alle categorieën", null);

    public MainPage()
    {
        InitializeComponent();
        InitializeCategoryFilter();

        // Nog geen RefreshJokes: OnAppearing zorgt zowel voor de eerste weergave
        // als voor vernieuwen wanneer we van het toevoegformulier terugkeren.
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

        // De filterpicker bevat presentatiekeuzes. De picker op het
        // toevoegformulier bevat wél alleen echte JokeCategory-waarden.
        FilterCategoryPicker.ItemsSource = options;
        FilterCategoryPicker.SelectedItem = _allCategoriesOption;
    }

    private void Search_Clicked(object? sender, EventArgs e)
    {
        _appliedSearchText = SearchTextEntry.Text?.Trim() ?? string.Empty;
        _appliedCategory = (FilterCategoryPicker.SelectedItem as CategoryFilterOption)?.Category;

        SearchTextEntry.Unfocus();
        RefreshJokes();
    }

    private void ShowAll_Clicked(object? sender, EventArgs e)
    {
        // Zowel de zichtbare invoer als de toegepaste filters worden gewist.
        // Alleen Entry.Text wissen zou de oude zoekopdracht actief laten.
        SearchTextEntry.Text = string.Empty;
        FilterCategoryPicker.SelectedItem = _allCategoriesOption;
        _appliedSearchText = string.Empty;
        _appliedCategory = null;

        // De sorteervoorkeur blijft behouden: Toon alles verwijdert filters,
        // maar verandert niet ongevraagd de gekozen presentatievolgorde.
        SearchTextEntry.Unfocus();
        RefreshJokes();
    }

    private void Alphabetical_CheckedChanged(object? sender, CheckedChangedEventArgs e)
    {
        _sortAlphabetically = e.Value;

        // Sorteren werkt meteen op de al toegepaste resultaten. Het mag géén
        // nieuwe tekst of categorie toepassen die nog op Zoeken wacht.
        RefreshJokes();
    }

    private void RefreshJokes()
    {
        var previousSelection = JokesCollectionView.SelectedItem as Joke;

        // Lees hier NIET SearchTextEntry.Text of FilterCategoryPicker uit.
        // Bij terugkeer kan daarin nog niet toegepaste invoer staan.
        IReadOnlyList<Joke> results = _jokeService.Search(_appliedSearchText, _appliedCategory, _sortAlphabetically);
        int totalCount = _jokeService.GetAll().Count;

        JokesCollectionView.ItemsSource = results;

        // Bewaar de selectie alleen als datzelfde object nog in het resultaat zit.
        // Een verdwenen selectie (ook bij 0 resultaten) mag geen oude details tonen.
        Joke? selectionToKeep = previousSelection is null
            ? null
            : results.FirstOrDefault(joke => joke.Id == previousSelection.Id);

        JokesCollectionView.SelectedItem = selectionToKeep;

        // Bij dezelfde SelectedItem hoeft het event niet opnieuw te ontstaan.
        // Vernieuwen zorgt daarom zelf voor een volledig consistente weergave.
        UpdateDetails(selectionToKeep);
        UpdateStatus(results, totalCount);
        UpdateEmptyState(totalCount);
        UpdateAppliedFiltersLabel();
    }

    private void Joke_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        UpdateDetails(e.CurrentSelection.FirstOrDefault() as Joke);
    }

    private void UpdateDetails(Joke? joke)
    {
        if (joke is null)
        {
            CurrentJokeCategoryLabel.Text = "Nog geen grap gekozen";
            CurrentJokeLabel.Text = "Selecteer een grap om de volledige tekst te lezen.";
            return;
        }

        ShowJoke(joke);
    }

    private void ShowJoke(Joke joke)
    {
        // Behouden uit de startoplossing: tekst en categorie horen samen.
        CurrentJokeLabel.Text = joke.Text;
        CurrentJokeCategoryLabel.Text = $"Categorie: {joke.Category}";
    }

    private void UpdateStatus(IReadOnlyList<Joke> results, int totalCount)
    {
        StatusLabel.Text = $"{results.Count} van {totalCount} grappen";

        // Could: afgeleide informatie voor het scherm. Geen extra property
        // op Joke en geen afzonderlijke teller die we handmatig moeten bijhouden.
        int categoryCount = results.Select(joke => joke.Category).Distinct().Count();
        string noun = categoryCount == 1 ? "categorie" : "categorieën";
        ResultCategoriesLabel.Text = $"{categoryCount} {noun} in de resultaten";
    }

    private void UpdateEmptyState(int totalCount)
    {
        // EmptyView wordt alleen zichtbaar wanneer de resultaten leeg zijn.
        // De volledige bibliotheek kan op dat moment wél nog grappen bevatten.
        if (totalCount == 0)
        {
            EmptyStateTitleLabel.Text = "Je bibliotheek is nog leeg";
            EmptyStateMessageLabel.Text = "Gebruik Nieuwe grap om je eerste grap toe te voegen.";
        }
        else
        {
            EmptyStateTitleLabel.Text = "Geen grappen gevonden";
            EmptyStateMessageLabel.Text = "Pas je zoektekst of categorie aan en klik op Zoeken, of kies Toon alles.";
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
        // Een snelle tweede klik mag tijdens de navigatie geen tweede formulier openen.
        if (_isOpeningAddPage)
            return;

        _isOpeningAddPage = true;
        OpenAddJokeButton.IsEnabled = false;

        try
        {
            await Navigation.PushAsync(new AddJokePage(_jokeService));
        }
        finally
        {
            // finally herstelt de knop; we verbergen geen navigatiefout met catch.
            _isOpeningAddPage = false;
            OpenAddJokeButton.IsEnabled = true;
        }
    }
}
