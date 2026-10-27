using JokeApp.Domain.Interfaces;
using JokeApp.Domain.Services;
using JokeApp.Navigation;
using JokeApp.Persistence.Repositories;

namespace JokeApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        // Het samenstelpunt (composition root) kiest de concrete implementaties.
        // Alleen hier in de presentatie kennen we de persistentieklassen;
        // pagina's gebruiken IJokeService. Domain kent geen MAUI of LiteDB.
        // Het platformafhankelijke bestandspad wordt van buitenaf doorgegeven.
        builder.Services.AddSingleton<IJokeRepository>(_ =>
        {
            string databasePath = Path.Combine(FileSystem.AppDataDirectory, "jokes.db");

            return new LiteDbJokeRepository(databasePath);
        });

        // Om in-memory te controleren: vervang het HELE blok hierboven door:
        // builder.Services.AddSingleton<IJokeRepository, InMemoryJokeRepository>();
        // Registreer niet beide implementaties tegelijk voor IJokeRepository.

        // Elke aanvraag krijgt een nieuwe service. Toch zien ze dezelfde data,
        // want de container geeft hun dezelfde singleton repository door.
        builder.Services.AddTransient<IJokeService, JokeService>();

        // Navigatie hoort bij de presentatie. Ook een concrete klasse zonder
        // interface kan via constructor injection worden doorgegeven.
        builder.Services.AddTransient<NavigationService>();

        return builder.Build();
    }
}
