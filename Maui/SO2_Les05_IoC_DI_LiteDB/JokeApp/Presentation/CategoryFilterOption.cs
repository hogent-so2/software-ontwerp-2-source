using JokeApp.Models;

namespace JokeApp.Presentation;

/// <summary>
/// Eén keuze in de zoekpicker. Dit is een hulpmiddel voor de presentatie,
/// geen extra categorie die een Joke kan krijgen.
/// </summary>
public class CategoryFilterOption
{
    public string Label { get; }
    public JokeCategory? Category { get; }

    public CategoryFilterOption(string label, JokeCategory? category)
    {
        Label = label;
        Category = category;
    }
}
