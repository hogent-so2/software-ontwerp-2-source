using CommunityToolkit.Mvvm.Messaging;
using JokeApp.Domain.Interfaces;
using JokeApp.Domain.Messages;
using JokeApp.Domain.Models;

namespace JokeApp.ViewModels;

/// <summary>
/// Tweede onafhankelijke ontvanger: telt de HELE bibliotheek, niet zoekresultaten.
/// Kent het lijst- en detail-ViewModel niet en heeft geen eigen opslag.
/// </summary>
public sealed class JokeStatisticsViewModel : ViewModelBase
{
    private readonly IJokeService _jokeService;
    private int _programmingCount;
    private int _dadJokeCount;
    private int _otherCount;

    public int ProgrammingCount
    {
        get => _programmingCount;
        private set => SetProperty(ref _programmingCount, value);
    }

    public int DadJokeCount
    {
        get => _dadJokeCount;
        private set => SetProperty(ref _dadJokeCount, value);
    }

    public int OtherCount
    {
        get => _otherCount;
        private set => SetProperty(ref _otherCount, value);
    }

    public JokeStatisticsViewModel(IJokeService jokeService, IMessenger messenger)
    {
        ArgumentNullException.ThrowIfNull(jokeService);
        ArgumentNullException.ThrowIfNull(messenger);
        _jokeService = jokeService;

        // Eén registratie per instantie. static houdt geen verborgen this vast.
        // Dit paneel blijft bewust luisteren terwijl een detailpagina openstaat.
        messenger.Register<JokeStatisticsViewModel, JokesChangedMessage>(
            this, static (recipient, message) => recipient.Receive(message));
        Reload();
    }

    private void Receive(JokesChangedMessage message) => Reload();

    private void Reload()
    {
        var jokes = _jokeService.GetAll();
        ProgrammingCount = jokes.Count(joke => joke.Category == JokeCategory.Programming);
        DadJokeCount = jokes.Count(joke => joke.Category == JokeCategory.DadJoke);
        OtherCount = jokes.Count(joke => joke.Category == JokeCategory.Other);
    }
}
