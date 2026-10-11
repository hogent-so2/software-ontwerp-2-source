namespace JokeApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        // Geen eigen DI-registraties of extra libraries in deze les.
        // We gebruiken de standaardlettertypes van het platform.
        return builder.Build();
    }
}
