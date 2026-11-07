using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using JokeApp.Domain.Messages;
using JokeApp.Dialogs;
using JokeApp.Domain.Interfaces;
using JokeApp.Domain.Models;
using JokeApp.Navigation;
using JokeApp.Presentation;
using Microsoft.Maui.Controls;

namespace JokeApp.ViewModels;

/// <summary>
/// Schermtoestand en acties van het overzicht. Kent geen controls of repositories.
/// De service blijft verantwoordelijk voor applicatiebewerkingen.
/// </summary>
public sealed class JokeListViewModel : ViewModelBase
{
    private readonly IJokeService _jokeService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;

    // Bewaar de concrete Commands om ChangeCanExecute te kunnen aanroepen.
    // De View gebruikt alleen de publieke ICommand-properties.
    private readonly Command _searchCommand;
    private readonly Command _showAllCommand;
    private readonly Command _addJokeCommand;
    private readonly Command _deleteJokeCommand;
    private readonly Command _openDetailsCommand;

    private readonly CategoryFilterOption _allCategoriesOption =
        new("Alle categorieën", null);

    private string _searchText = string.Empty;
    private CategoryFilterOption? _selectedCategoryFilter;
    private bool _sortAlphabetically;

    // De invoerproperties hierboven zijn VOORBEREIDE keuzes. Deze fields
    // veranderen pas bij Zoeken of Toon alles en vormen de TOEGEPASTE filters.
    private string _appliedSearchText = string.Empty;
    private JokeCategory? _appliedCategory;

    private JokeItemViewModel? _selectedJoke;
    private string _countText = "0 van 0 grappen";
    private string _resultCategoriesText = "0 categorieën in de resultaten";
    private string _emptyStateTitle = "Je bibliotheek is leeg";
    private string _emptyStateMessage = "Gebruik Nieuwe grap om je eerste grap toe te voegen.";
    private string _statusMessage = string.Empty;
    private bool _statusIsError;
    private bool _isBusy;

    // Dit is de zichtbare resultaatcollectie, niet een tweede database.
    // We behouden hetzelfde collectieobject en wijzigen zijn inhoud bij Reload.
    // ObservableCollection meldt Add/Clear; het meldt geen item-propertywijzigingen.
    public ObservableCollection<JokeItemViewModel> Jokes { get; } = new();

    public IReadOnlyList<CategoryFilterOption> CategoryFilters { get; }

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value ?? string.Empty);
    }

    public CategoryFilterOption? SelectedCategoryFilter
    {
        get => _selectedCategoryFilter;
        set => SetProperty(ref _selectedCategoryFilter, value ?? _allCategoriesOption);
    }

    public bool SortAlphabetically
    {
        get => _sortAlphabetically;
        set
        {
            if (SetProperty(ref _sortAlphabetically, value))
                Reload();

            // Net als in de vorige versie werkt sorteren onmiddellijk.
            // Reload leest echter geen voorbereide zoektekst of categorie in.
        }
    }

    public JokeItemViewModel? SelectedJoke
    {
        get => _selectedJoke;
        set
        {
            if (!SetProperty(ref _selectedJoke, value))
                return;

            // Afgeleide properties worden niet automatisch opnieuw berekend
            // door de binding. We melden beide zichtbare detailwaarden expliciet.
            OnPropertyChanged(nameof(SelectedJokeText));
            OnPropertyChanged(nameof(SelectedJokeCategoryText));

            // CanExecute wordt niet voortdurend opnieuw opgevraagd door MAUI.
            // De knop moet weten dat haar beschikbaarheid gewijzigd kan zijn.
            _deleteJokeCommand.ChangeCanExecute();
            _openDetailsCommand.ChangeCanExecute();
        }
    }

    public string SelectedJokeText => SelectedJoke?.Text ??
        "Selecteer een grap om de volledige tekst te lezen.";

    public string SelectedJokeCategoryText => SelectedJoke is null
        ? "Nog geen grap gekozen"
        : $"Categorie: {SelectedJoke.Category}";

    public string CountText
    {
        get => _countText;
        private set => SetProperty(ref _countText, value);
    }

    public string ResultCategoriesText
    {
        get => _resultCategoriesText;
        private set => SetProperty(ref _resultCategoriesText, value);
    }

    public string EmptyStateTitle
    {
        get => _emptyStateTitle;
        private set => SetProperty(ref _emptyStateTitle, value);
    }

    public string EmptyStateMessage
    {
        get => _emptyStateMessage;
        private set => SetProperty(ref _emptyStateMessage, value);
    }

    public string AppliedFiltersText
    {
        get
        {
            string textDescription = _appliedSearchText.Length == 0
                ? "alle teksten"
                : $"tekst bevat ‘{_appliedSearchText}’";
            string categoryDescription = _appliedCategory?.ToString() ?? "alle categorieën";
            return $"Toegepast: {textDescription} · {categoryDescription}";
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    // Het ViewModel biedt betekenis aan, geen kleuren of specifieke labels.
    // XAML kiest de bestaande normale stijl of foutstijl voor de boodschap.
    public bool HasStatusError => StatusMessage.Length > 0 && _statusIsError;
    public bool HasStatusInformation => StatusMessage.Length > 0 && !_statusIsError;

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (!SetProperty(ref _isBusy, value))
                return;

            OnPropertyChanged(nameof(IsNotBusy));
            _searchCommand.ChangeCanExecute();
            _showAllCommand.ChangeCanExecute();
            _addJokeCommand.ChangeCanExecute();
            _deleteJokeCommand.ChangeCanExecute();
            _openDetailsCommand.ChangeCanExecute();
        }
    }

    public bool IsNotBusy => !IsBusy;

    public ICommand OpenDetailsCommand => _openDetailsCommand;
    public ICommand SearchCommand => _searchCommand;
    public ICommand ShowAllCommand => _showAllCommand;
    public ICommand AddJokeCommand => _addJokeCommand;
    public ICommand DeleteJokeCommand => _deleteJokeCommand;

    public JokeListViewModel(
        IJokeService jokeService,
        INavigationService navigationService,
        IDialogService dialogService,
        IMessenger messenger)
    {
        ArgumentNullException.ThrowIfNull(jokeService);
        ArgumentNullException.ThrowIfNull(navigationService);
        ArgumentNullException.ThrowIfNull(dialogService);
        ArgumentNullException.ThrowIfNull(messenger);

        _jokeService = jokeService;
        _navigationService = navigationService;
        _dialogService = dialogService;

        var options = new List<CategoryFilterOption> { _allCategoriesOption };
        foreach (JokeCategory category in Enum.GetValues<JokeCategory>())
            options.Add(new CategoryFilterOption(category.ToString(), category));

        CategoryFilters = options.AsReadOnly();
        _selectedCategoryFilter = _allCategoriesOption;

        _searchCommand = new Command(ApplySearch, () => !IsBusy);
        _showAllCommand = new Command(ShowAll, () => !IsBusy);
        _addJokeCommand = new Command(async () => await OpenAddJokeAsync(), () => !IsBusy);
        _deleteJokeCommand = new Command(
            async () => await DeleteSelectedJokeAsync(),
            () => !IsBusy && SelectedJoke is not null);
        _openDetailsCommand = new Command(
            async () => await OpenDetailsAsync(),
            () => !IsBusy && SelectedJoke is not null);

        // Na constructorinitialisatie zijn alle commands beschikbaar voor setters.
        messenger.Register<JokeListViewModel, JokesChangedMessage>(
            this, static (recipient, message) => recipient.Receive(message));
        Reload();
    }

    private void Receive(JokesChangedMessage message)
    {
        // Herbereken de toegepaste query: een gewijzigde grap kan erin komen
        // of eruit verdwijnen. De ruwe invoer wordt hier NIET toegepast.
        Reload();
        SetStatus("De grappenverzameling is bijgewerkt.", isError: false);
    }

    public void Reload()
    {
        // Eerste lading, expliciete zoekactie of bericht; niet OnAppearing.
        // Lees hier bewust NIET SearchText of SelectedCategoryFilter.
        var matchingJokes = _jokeService.Search(
            _appliedSearchText, _appliedCategory, SortAlphabetically);
        int totalCount = _jokeService.Count();

        ClearStatus();
        SelectedJoke = null;
        Jokes.Clear();
        foreach (var joke in matchingJokes)
            Jokes.Add(new JokeItemViewModel(joke));

        // De collectie meldt haar inhoud, maar niet onze aparte tellerproperties.
        CountText = $"{Jokes.Count} van {totalCount} grappen";
        int categoryCount = Jokes.Select(joke => joke.Category).Distinct().Count();
        string noun = categoryCount == 1 ? "categorie" : "categorieën";
        ResultCategoriesText = $"{categoryCount} {noun} in de resultaten";

        // Geen resultaten betekent niet noodzakelijk dat de opslag leeg is.
        EmptyStateTitle = totalCount == 0
            ? "Je bibliotheek is leeg"
            : "Geen grappen gevonden";
        EmptyStateMessage = totalCount == 0
            ? "Gebruik Nieuwe grap om je eerste grap toe te voegen."
            : "Pas je zoektekst of categorie aan en klik op Zoeken, of kies Toon alles.";

        OnPropertyChanged(nameof(AppliedFiltersText));
    }

    private void ApplySearch()
    {
        if (IsBusy)
            return;

        _appliedSearchText = SearchText.Trim();
        _appliedCategory = SelectedCategoryFilter?.Category;
        Reload();
    }

    private void ShowAll()
    {
        if (IsBusy)
            return;

        SearchText = string.Empty;
        SelectedCategoryFilter = _allCategoriesOption;
        _appliedSearchText = string.Empty;
        _appliedCategory = null;
        Reload(); // De sorteervoorkeur blijft behouden, net als in de bijlage.
    }

    internal async Task OpenAddJokeAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        try
        {
            await _navigationService.ShowAddJokeAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    internal async Task DeleteSelectedJokeAsync()
    {
        // CanExecute stuurt de knop. De guard beschermt ook een rechtstreekse
        // command-aanroep en voorkomt twee gelijktijdige bevestigingsvragen.
        if (IsBusy || SelectedJoke is not JokeItemViewModel selectedJoke)
            return;

        IsBusy = true;
        ClearStatus();
        try
        {
            // selectedJoke is vastgelegd vóór await. We verwijderen de grap
            // waarover de vraag ging, niet een eventueel later gekozen object.
            bool confirmed = await _dialogService.ConfirmAsync(
                "Grap verwijderen?",
                $"Wil je deze grap definitief verwijderen?\n\n{selectedJoke.Text}",
                "Verwijderen",
                "Annuleren");

            if (!confirmed)
                return; // Geen opslagwijziging en geen verloren selectie.

            var result = _jokeService.Remove(selectedJoke.Id);

            // Bij succes heeft het servicebericht de lijst al vernieuwd.
            // Alleen bij een mislukking laden we hier zelf: een verouderde
            // selectie moet verdwijnen, zonder een vals wijzigingsbericht.
            if (!result.IsValid)
                Reload();
            SetStatus(result.IsValid ? "Grap verwijderd." : result.ErrorMessage,
                      isError: !result.IsValid);
        }
        finally
        {
            // Ook na annuleren wordt de bediening weer beschikbaar.
            // Verwachte fouten gebruiken ValidationResult; technische exceptions
            // worden hier niet stilzwijgend omgezet in een vals succesbericht.
            IsBusy = false;
        }
    }

    internal async Task OpenDetailsAsync()
    {
        if (IsBusy || SelectedJoke is not JokeItemViewModel selectedJoke)
            return;

        IsBusy = true;
        try
        {
            await _navigationService.ShowJokeDetailAsync(selectedJoke.Id);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void SetStatus(string message, bool isError)
    {
        _statusIsError = isError;
        StatusMessage = message;
        NotifyStatusVisibility();
    }

    private void ClearStatus() => SetStatus(string.Empty, isError: false);

    private void NotifyStatusVisibility()
    {
        OnPropertyChanged(nameof(HasStatusError));
        OnPropertyChanged(nameof(HasStatusInformation));
    }
}
