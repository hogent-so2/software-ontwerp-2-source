using CommunityToolkit.Mvvm.Messaging;
using JokeApp.Domain.Interfaces;
using JokeApp.Domain.Messages;
using JokeApp.Domain.Models;
using JokeApp.Domain.Validation;

namespace JokeApp.Domain.Services;

/// <summary>
/// GRASP Controller: coördineert applicatiebewerkingen en bewaakt uniciteit.
/// Kent contracten voor opslag en berichten, maar geen schermen.
/// </summary>
public sealed class JokeService : IJokeService
{
    private readonly IJokeRepository _jokeRepository;
    private readonly IMessenger _messenger;

    public JokeService(
        IJokeRepository jokeRepository,
        IMessenger messenger)
    {
        ArgumentNullException.ThrowIfNull(jokeRepository);
        ArgumentNullException.ThrowIfNull(messenger);
        _jokeRepository = jokeRepository;
        _messenger = messenger;
    }

    public IReadOnlyList<Joke> GetAll() => _jokeRepository.GetAll();
    public int Count() => _jokeRepository.Count();
    public Joke? GetById(Guid id) => _jokeRepository.GetById(id);

    public IReadOnlyList<Joke> Search(string? searchText) =>
        Search(searchText, category: null, sortAlphabetically: false);

    public IReadOnlyList<Joke> Search(
        string? searchText, JokeCategory? category, bool sortAlphabetically) =>
        _jokeRepository.Search(searchText, category, sortAlphabetically);

    public ValidationResult Add(Joke joke)
    {
        ArgumentNullException.ThrowIfNull(joke);

        // Eén Joke kan niet weten welke andere grappen er zijn. Deze regel
        // hoort daarom bij de service, niet in de setter of in een pagina.
        if (HasDuplicateText(joke.Text))
            return ValidationResult.Failure("Deze grap bestaat al.");

        _jokeRepository.Add(joke);
        _messenger.Send(new JokesChangedMessage(joke.Id));
        return ValidationResult.Success();
    }

    public ValidationResult Update(Guid id, string text, JokeCategory category)
    {
        var existingJoke = _jokeRepository.GetById(id);
        if (existingJoke is null)
            return ValidationResult.Failure("Deze grap bestaat niet meer.");

        var validation = JokeTextValidator.Validate(text);
        if (!validation.IsValid)
            return validation;

        if (!Enum.IsDefined(category))
            return ValidationResult.Failure("Kies een geldige categorie.");

        string normalizedText = text.Trim();
        // Een ongewijzigde grap is geen duplicaat van zichzelf.
        if (HasDuplicateText(normalizedText, excludedId: id))
            return ValidationResult.Failure("Deze grap bestaat al.");

        // Pas het bestaande object pas aan nadat alle controles geslaagd zijn.
        existingJoke.Text = normalizedText;
        existingJoke.Category = category;

        if (!_jokeRepository.Update(existingJoke))
            return ValidationResult.Failure("Deze grap bestaat niet meer.");

        // Eén bericht na succes, ongeacht welk scherm Update heeft aangeroepen.
        // Een geweigerd resultaat of opslag-exception bereikt deze regel niet.
        _messenger.Send(new JokesChangedMessage(id));
        return ValidationResult.Success();
    }

    public ValidationResult Remove(Guid id)
    {
        if (!_jokeRepository.Remove(id))
            return ValidationResult.Failure("Deze grap bestaat niet meer.");

        _messenger.Send(new JokesChangedMessage(id));
        return ValidationResult.Success();
    }

    private bool HasDuplicateText(string text, Guid? excludedId = null) =>
        _jokeRepository.GetAll().Any(other =>
            other.Id != excludedId &&
            string.Equals(other.Text, text, StringComparison.OrdinalIgnoreCase));
}
