using System.Windows.Input;
using JokeApp.Domain.Interfaces;
using JokeApp.Domain.Models;
using JokeApp.Domain.Validation;
using JokeApp.Navigation;
using Microsoft.Maui.Controls;

namespace JokeApp.ViewModels;

/// <summary>
/// Bewaart formulierinvoer en biedt opslaan/wissen aan. Een lege of te lange
/// invoer mag hier bestaan: pas bij Opslaan proberen we een geldig model te maken.
/// </summary>
public sealed class AddJokeViewModel : ViewModelBase
{
    private const JokeCategory DefaultCategory = JokeCategory.Programming;
    private readonly IJokeService _jokeService;
    private readonly INavigationService _navigationService;
    private readonly Command _saveCommand;
    private readonly Command _clearInputCommand;

    private string _text = string.Empty;
    private JokeCategory? _selectedCategory = DefaultCategory;
    private string _errorMessage = string.Empty;
    private string _feedbackMessage = string.Empty;
    private bool _isBusy;
    private bool _hasSaved;

    public IReadOnlyList<JokeCategory> Categories { get; } =
        Array.AsReadOnly(Enum.GetValues<JokeCategory>());

    public string Text
    {
        get => _text;
        set
        {
            if (!SetProperty(ref _text, value ?? string.Empty))
                return;

            // Een afgeleide property heeft geen eigen field nodig, maar wel
            // een melding zodra haar bron verandert. Dit is GEEN validatie.
            OnPropertyChanged(nameof(CharacterCountText));
            ClearFeedback();
        }
    }

    public JokeCategory? SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (!SetProperty(ref _selectedCategory, value))
                return;

            OnPropertyChanged(nameof(SelectedCategoryText));
            ClearFeedback();
        }
    }

    public string SelectedCategoryText => SelectedCategory is JokeCategory category
        ? $"Gekozen categorie voor de nieuwe grap: {category}"
        : "Nog geen categorie geselecteerd";

    public string TextRulesText =>
        $"Gebruik {JokeTextValidator.MinimumLength} tot {JokeTextValidator.MaximumLength} tekens. " +
        "Spaties aan het begin en einde tellen niet mee.";

    // De validator en het model werken op Trim(). De teller toont daarom de
    // lengte die bij Opslaan gecontroleerd wordt, niet de omliggende witruimte.
    // Ook 281 / 280 is zichtbaar: geen MaxLength dat invoer stil afkapt.
    public string CharacterCountText =>
        $"{Text.Trim().Length} / {JokeTextValidator.MaximumLength} tekens";

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

    public string FeedbackMessage
    {
        get => _feedbackMessage;
        private set => SetProperty(ref _feedbackMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
                NotifyFormAvailability();
        }
    }

    // Invoergeldigheid komt NIET voor in deze beschikbaarheidsregel.
    // Opslaan blijft dus klikbaar bij leeg, te kort en te lang. Alleen een
    // lopende of reeds geslaagde opslag mag een tweede uitvoering blokkeren.
    public bool IsFormEnabled => !IsBusy && !_hasSaved;

    public ICommand SaveCommand => _saveCommand;
    public ICommand ClearInputCommand => _clearInputCommand;

    public AddJokeViewModel(IJokeService jokeService, INavigationService navigationService)
    {
        ArgumentNullException.ThrowIfNull(jokeService);
        ArgumentNullException.ThrowIfNull(navigationService);

        _jokeService = jokeService;
        _navigationService = navigationService;
        _saveCommand = new Command(async () => await SaveAsync(), () => IsFormEnabled);
        _clearInputCommand = new Command(ClearInput, () => IsFormEnabled);
    }

    internal async Task SaveAsync()
    {
        if (!IsFormEnabled)
            return;

        ClearFeedback();
        var validation = JokeTextValidator.Validate(Text);
        if (!validation.IsValid)
        {
            ErrorMessage = validation.ErrorMessage;
            return; // De oorspronkelijke invoer blijft beschikbaar om te verbeteren.
        }

        if (SelectedCategory is not JokeCategory category || !Enum.IsDefined(category))
        {
            ErrorMessage = "Kies een categorie.";
            return;
        }

        IsBusy = true;
        try
        {
            // Creator: dit ViewModel heeft de gevalideerde gegevens om een Joke
            // te maken. Het model bewaakt nog steeds zijn eigen geldigheid.
            var joke = new Joke(Text, category);
            var result = _jokeService.Add(joke);
            if (!result.IsValid)
            {
                // Uniciteit blijft een serviceregel, niet een ViewModel-regel.
                ErrorMessage = result.ErrorMessage;
                return;
            }

            _hasSaved = true;
            NotifyFormAvailability();
            await _navigationService.GoBackAsync();

            // Add publiceerde al een bericht na geslaagde opslag.
            // Lijst en statistieken reageren zelfstandig; hier geen Reload/Send.
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ClearInput()
    {
        if (!IsFormEnabled)
            return;

        Text = string.Empty;
        SelectedCategory = DefaultCategory;

        // Ook als Text of SelectedCategory al hun beginwaarde hadden, wissen
        // we de oude fout. Een setter hoeft dan immers geen melding te geven.
        ClearFeedback();
        FeedbackMessage = "Invoer gewist.";
        // Geen service-aanroep: een formulier wissen is niet hetzelfde als bewaren.
    }

    private void ClearFeedback()
    {
        ErrorMessage = string.Empty;
        FeedbackMessage = string.Empty;
    }

    private void NotifyFormAvailability()
    {
        OnPropertyChanged(nameof(IsFormEnabled));
        _saveCommand.ChangeCanExecute();
        _clearInputCommand.ChangeCanExecute();
    }
}
