namespace JokeApp.Domain.Messages;

/// <summary>
/// Een feit NA geslaagde opslag, geen verzoek om iets te bewaren.
/// De id is voldoende: ontvangers halen hun eigen actuele gegevens op.
/// </summary>
public sealed class JokesChangedMessage
{
    public Guid JokeId { get; }

    public JokesChangedMessage(Guid jokeId)
    {
        JokeId = jokeId;
    }
}
