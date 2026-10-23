using JokeApp.Interfaces;
using JokeApp.Repositories;
using JokeApp.Services;

namespace JokeApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>()
               .ConfigureFonts(fonts =>
               {
                   fonts.AddFont("OpenSans-Regular.ttf", "CollectionRegular");
                   fonts.AddFont("OpenSans-Semibold.ttf", "CollectionSemibold");
               });

        // Must: LiteDB is de actieve registratie. De factory ontvangt geen
        // pagina; alleen hier kennen we het platformafhankelijke bestandspad.
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

        // Alleen de detailpagina krijgt een route. MainPage staat al in XAML.
        // Bij navigatie laat Shell de container AddJokePage met IJokeService maken.
        Routing.RegisterRoute(nameof(AddJokePage), typeof(AddJokePage));

        return builder.Build();
    }
}
