using JokeApp.Dialogs;
using JokeApp.Domain.Interfaces;
using JokeApp.Domain.Services;
using JokeApp.Navigation;
using JokeApp.Persistence.Repositories;
using JokeApp.ViewModels;

namespace JokeApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        // Het samenstelpunt kiest de implementaties. Alleen hier kent de
        // presentatie de concrete repositories; ViewModels kennen IJokeService.
        builder.Services.AddSingleton<IJokeRepository>(_ =>
        {
            string databasePath = Path.Combine(FileSystem.AppDataDirectory, "jokes.db");
            return new LiteDbJokeRepository(databasePath);
        });

        // Voor de vergelijking uit de opdracht vervang je het HELE blok hierboven:
        // builder.Services.AddSingleton<IJokeRepository, InMemoryJokeRepository>();
        // Registreer nooit beide implementaties tegelijk voor hetzelfde contract.

        // Twee serviceobjecten delen dezelfde singleton repository. Een transient
        // service betekent dus niet automatisch dat elke pagina eigen gegevens heeft.
        builder.Services.AddTransient<IJokeService, JokeService>();
        builder.Services.AddTransient<INavigationService, ShellNavigationService>();
        builder.Services.AddTransient<IDialogService, ShellDialogService>();

        // Schermtoestand hoort bij het scherm. Een nieuw formulier krijgt een
        // nieuw ViewModel, zonder oude invoer of foutmeldingen van een vorige keer.
        builder.Services.AddTransient<JokeListViewModel>();
        builder.Services.AddTransient<AddJokeViewModel>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<AddJokePage>();

        // De navigatiestack kan een reeds gemaakte pagina bewaren. Teruggaan
        // maakt dus niet noodzakelijk een nieuw MainPage/ViewModel-object.
        return builder.Build();
    }
}
