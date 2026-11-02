namespace JokeApp.Domain.Validation;

/// <summary>
/// De tekstregels staan op één plaats. De UI gebruikt ze voor feedback,
/// het model gebruikt ze om zichzelf te beschermen.
/// Deze klasse kent geen invoervelden, labels, collectie of navigatie.
/// </summary>
public static class JokeTextValidator
{
    public const int MinimumLength = 10;
    public const int MaximumLength = 280;

    public static ValidationResult Validate(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return ValidationResult.Failure("Een grap mag niet leeg zijn.");

        // Alleen witruimte aan het begin en einde telt niet mee.
        // Spaties en regeleinden binnen de tekst blijven gewoon behouden.
        string normalizedText = text.Trim();

        if (normalizedText.Length < MinimumLength)
            return ValidationResult.Failure($"Een grap bevat minstens {MinimumLength} tekens.");

        if (normalizedText.Length > MaximumLength)
            return ValidationResult.Failure($"Een grap bevat maximaal {MaximumLength} tekens.");

        // Valideren wijzigt het argument niet. Het model bewaart straks
        // de getrimde tekst; de UI behoudt bij een fout de oorspronkelijke invoer.
        return ValidationResult.Success();
    }
}
