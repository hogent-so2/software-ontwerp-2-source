using System.Windows.Input;
using JokeApp.Dialogs;
using JokeApp.Domain.Interfaces;
using JokeApp.Domain.Models;
using JokeApp.Domain.Validation;
using JokeApp.Navigation;
using Microsoft.Maui.Controls;

namespace JokeApp.ViewModels;

/// <summary>
/// De tijdelijke invoer van één bewerkformulier. Dit is geen tweede Joke en
/// geen repository: alleen de service mag de voorgestelde wijziging bewaren.
/// </summary>
public sealed class JokeDetailViewModel : ViewModelBase
{
    private readonly IJokeService _jokeService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly Command _saveCommand;
    private readonly Command _deleteCommand;
    private readonly Command _cancelCommand;
    private readonly Command _restoreCommand;

    private Guid _jokeId;
    private string _originalText = string.Empty;
    private JokeCategory _originalCategory = JokeCategory.Programming;
    private string _text = string.Empty;
    private JokeCategory? _selectedCategory = JokeCategory.Programming;
    private string _errorMessage = string.Empty;
    private bool _isLoaded;
    private bool _isBusy;
    private bool _hasCommitted;

    public IReadOnlyList<JokeCategory> Categories { get; } =
        Array.AsReadOnly(Enum.GetValues<JokeCategory>());

    public string Text
    {
        get => _text;
        set
        {
            if (!SetProperty(ref _text, value ?? string.Empty))
                return;

            // Een invoerveld mag tijdelijk leeg zijn. We veranderen hier geen
            // model en voeren ook geen volledige validatie bij elke toets uit.
            ErrorMessage = string.Empty;
            OnPropertyChanged(nameof(CharacterCountText));
            NotifyChanges();
        }
    }

    public JokeCategory? SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (!SetProperty(ref _selectedCategory, value))
                return;

            ErrorMessage = string.Empty;
            NotifyChanges();
        }
    }

    // Must: vergelijk de FORMULIERWAARDEN met de bij het laden bewaarde waarden.
    // Ook een extra spatie is een wijziging in het formulier. De service trimt
    // pas bij Opslaan. Verschillend betekent dus niet automatisch geldig.
    public bool HasChanges => IsLoaded &&
        (!string.Equals(Text, _originalText, StringComparison.Ordinal) ||
         SelectedCategory != _originalCategory);

    public string CharacterCountText =>
        $"{Text.Trim().Length} / {JokeTextValidator.MaximumLength} tekens";

    public string TextRulesText =>
        $"Gebruik {JokeTextValidator.MinimumLength} tot {JokeTextValidator.MaximumLength} tekens. " +
        "Spaties aan het begin en einde tellen niet mee.";

    public string ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (SetProperty(ref _errorMessage, value))
                OnPropertyChanged(nameof(HasError));
        }
    }

    public bool HasError => ErrorMessage.Length > 0;

    public bool IsLoaded
    {
        get => _isLoaded;
        private set
        {
            if (!SetProperty(ref _isLoaded, value))
                return;

            NotifyChanges();
            NotifyAvailability();
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
                NotifyAvailability();
        }
    }

    // CanExecute is beschikbaarheid, geen tekstvalidatie. Ook zonder wijzigingen,
    // bij lege tekst of bij 281 tekens blijft Opslaan beschikbaar als er een grap is.
    public bool IsFormEnabled => IsLoaded && !IsBusy && !_hasCommitted;
    public ICommand SaveCommand => _saveCommand;
    public ICommand DeleteCommand => _deleteCommand;
    public ICommand CancelCommand => _cancelCommand;
    public ICommand RestoreCommand => _restoreCommand;

    public JokeDetailViewModel(
        IJokeService jokeService,
        INavigationService navigationService,
        IDialogService dialogService)
    {
        ArgumentNullException.ThrowIfNull(jokeService);
        ArgumentNullException.ThrowIfNull(navigationService);
        ArgumentNullException.ThrowIfNull(dialogService);
        _jokeService = jokeService;
        _navigationService = navigationService;
        _dialogService = dialogService;

        _saveCommand = new Command(async () => await SaveAsync(), () => IsFormEnabled);
        _deleteCommand = new Command(async () => await DeleteAsync(), () => IsFormEnabled);
        _cancelCommand = new Command(async () => await CancelAsync(), () => !IsBusy);
        _restoreCommand = new Command(Restore, () => IsFormEnabled && HasChanges);
    }

    public void Load(Guid jokeId)
    {
        // Alleen de navigatieparameter start het laden. OnAppearing zou een
        // onopgeslagen formulier onbedoeld opnieuw kunnen overschrijven.
        IsLoaded = false;
        _hasCommitted = false;
        _jokeId = jokeId;
        _originalText = string.Empty;
        _originalCategory = JokeCategory.Programming;
        Text = string.Empty;
        SelectedCategory = JokeCategory.Programming;
        ErrorMessage = string.Empty;

        var joke = _jokeService.GetById(jokeId);
        if (joke is null)
        {
            ErrorMessage = "Deze grap bestaat niet meer.";
            NotifyAvailability();
            return;
        }

        // Een snapshot van twee waarden volstaat. We bewaren geen modelreferentie
        // om later te muteren en maken evenmin een kopie van de hele bibliotheek.
        _originalText = joke.Text;
        _originalCategory = joke.Category;
        Text = joke.Text;
        SelectedCategory = joke.Category;
        IsLoaded = true;
    }

    private void Restore()
    {
        if (!IsFormEnabled || !HasChanges)
            return;

        Text = _originalText;
        SelectedCategory = _originalCategory;
        ErrorMessage = string.Empty;
        // Must: geen service-aanroep en geen bericht. Alleen de draft verandert.
        // Herstellen betekent de waarden van bij het laden, niet opnieuw ophalen.
    }

    internal async Task SaveAsync()
    {
        if (!IsFormEnabled)
            return;

        ErrorMessage = string.Empty;
        if (SelectedCategory is not JokeCategory category || !Enum.IsDefined(category))
        {
            ErrorMessage = "Kies een categorie.";
            return;
        }

        IsBusy = true;
        try
        {
            // Information Expert: de service kan bestaan, geldigheid en
            // duplicaten controleren. Het ViewModel beslist niet wat uniek is.
            var result = _jokeService.Update(_jokeId, Text, category);
            if (!result.IsValid)
            {
                ErrorMessage = result.ErrorMessage;
                return; // Invoer en oorspronkelijke waarden blijven beschikbaar.
            }

            // Een geslaagde opslag is nu het nieuwe referentiepunt. Dit voorkomt
            // ook twee schrijfacties wanneer de gebruiker snel dubbel klikt.
            _originalText = Text.Trim();
            _originalCategory = category;
            Text = _originalText;
            _hasCommitted = true;
            NotifyChanges();
            NotifyAvailability();
            await _navigationService.GoBackAsync();
            // Niet zelf publiceren: JokeService heeft na de opslag al gemeld.
        }
        finally
        {
            IsBusy = false;
        }
    }

    internal async Task CancelAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        try
        {
            // Should: het contract verbergt Shell en de concrete dialoog.
            // De gewone Shell-/systeemterugknop is een andere navigatieactie;
            // die wordt in deze oefening bewust niet onderschept.
            if (HasChanges && !await _dialogService.ConfirmAsync(
                "Wijzigingen weggooien?",
                "Je hebt niet-opgeslagen wijzigingen. Wil je die weggooien?",
                "Weggooien", "Verder bewerken"))
                return;

            await _navigationService.GoBackAsync();
            // Er hoeft niets teruggedraaid te worden: er is niets opgeslagen.
        }
        finally
        {
            IsBusy = false;
        }
    }

    internal async Task DeleteAsync()
    {
        if (!IsFormEnabled)
            return;

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            // Toon de opgeslagen tekst, niet een nog onopgeslagen vervanging.
            bool confirmed = await _dialogService.ConfirmAsync(
                "Grap verwijderen?",
                $"Wil je deze opgeslagen grap definitief verwijderen?\n\n{_originalText}",
                "Verwijderen", "Annuleren");
            if (!confirmed)
                return;

            var result = _jokeService.Remove(_jokeId);
            if (!result.IsValid)
            {
                ErrorMessage = result.ErrorMessage;
                return;
            }

            _hasCommitted = true;
            IsLoaded = false;
            await _navigationService.GoBackAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void NotifyChanges()
    {
        OnPropertyChanged(nameof(HasChanges));
        _restoreCommand.ChangeCanExecute();
    }

    private void NotifyAvailability()
    {
        OnPropertyChanged(nameof(IsFormEnabled));
        _saveCommand.ChangeCanExecute();
        _deleteCommand.ChangeCanExecute();
        _cancelCommand.ChangeCanExecute();
        _restoreCommand.ChangeCanExecute();
    }
}
