using JokeApp.Domain.Models;

namespace JokeApp.ViewModels;

/// <summary>
/// De gegevens van één grap voor de lijst. Reload maakt nieuwe items na een wijziging.
/// </summary>
public sealed class JokeItemViewModel
{
    public Guid Id { get; }
    public string Text { get; }
    public JokeCategory Category { get; }

    public JokeItemViewModel(Joke joke)
    {
        ArgumentNullException.ThrowIfNull(joke);

        Id = joke.Id;
        Text = joke.Text;
        Category = joke.Category;
    }
}
