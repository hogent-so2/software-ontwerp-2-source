namespace JokeApp.Dialogs;

/// <summary>
/// Een klein contract voor een bevestigingsvraag. Het ViewModel ontvangt
/// alleen het antwoord en hoeft geen ContentPage of Shell op te zoeken.
/// </summary>
public interface IDialogService
{
    Task<bool> ConfirmAsync(
        string title, string message, string accept, string cancel);
}
