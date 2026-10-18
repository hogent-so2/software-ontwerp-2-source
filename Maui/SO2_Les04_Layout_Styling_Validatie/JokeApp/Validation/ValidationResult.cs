namespace JokeApp.Validation;

/// <summary>
/// Het resultaat van een verwachte controle, zonder afhankelijkheid van MAUI.
/// De aanroeper beslist wat hij met de eventuele foutmelding doet.
/// </summary>
public sealed class ValidationResult
{
    public bool IsValid 
        => ErrorMessage.Length == 0;
    
    public string ErrorMessage { get; }

    // Via de private constructor voorkomen we dat een aanroeper een
    // tegenstrijdig resultaat kan maken, zoals geldig mét een foutmelding.
    private ValidationResult(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }

    public static ValidationResult Success()
    {
        return new ValidationResult(string.Empty);
    }

    public static ValidationResult Failure(string errorMessage)
    {
        // Een ontbrekende foutmelding is een programmeerfout van de aanroeper,
        // geen verwachte invoerfout van een gebruiker.
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);

        return new ValidationResult(errorMessage);
    }
}
