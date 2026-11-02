namespace JokeApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Een route koppelt een bestemming aan een pagina. De DI-registraties
        // in MauiProgram bepalen hoe die pagina en haar dependencies ontstaan.
        // MainPage staat al in AppShell.xaml en krijgt geen extra detailroute.
        Routing.RegisterRoute(nameof(AddJokePage), typeof(AddJokePage));
    }
}
