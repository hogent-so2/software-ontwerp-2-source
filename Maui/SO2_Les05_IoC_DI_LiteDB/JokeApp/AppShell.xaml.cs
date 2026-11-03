namespace JokeApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // De route koppelt een bestemming aan een pagina. Shell maakt de pagina
        // en ontvangt haar dependencies uit de DI-container.
        // MainPage staat al in AppShell.xaml en krijgt geen extra detailroute.
        Routing.RegisterRoute(nameof(AddJokePage), typeof(AddJokePage));
    }
}
