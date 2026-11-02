namespace JokeApp.Dialogs;

/// <summary>
/// De MAUI-specifieke uitvoering van een bevestigingsvraag.
/// In een test vervangen we deze klasse door een vooraf bepaald antwoord.
/// </summary>
public class ShellDialogService : IDialogService
{
    public Task<bool> ConfirmAsync(
        string title, string message, string accept, string cancel) =>
        Shell.Current.DisplayAlertAsync(title, message, accept, cancel);
}
