using CommunityToolkit.Mvvm.Messaging;
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

        // Composition root: alleen hier kiezen we concrete afhankelijkheden.
        builder.Services.AddSingleton<IJokeRepository>(_ =>
            new LiteDbJokeRepository(Path.Combine(FileSystem.AppDataDirectory, "jokes.db")));
        // Vervang voor de geheugenvariant de registratie hierboven (niet erbij):
        // builder.Services.AddSingleton<IJokeRepository, InMemoryJokeRepository>();

        // Sender en beide ontvangers moeten DEZELFDE messenger gebruiken.
        // Geen verborgen WeakReferenceMessenger.Default in services/ViewModels.
        builder.Services.AddSingleton<IMessenger, WeakReferenceMessenger>();
        builder.Services.AddTransient<IJokeService, JokeService>();
        builder.Services.AddTransient<INavigationService, ShellNavigationService>();
        builder.Services.AddTransient<IDialogService, ShellDialogService>();

        builder.Services.AddTransient<JokeListViewModel>();
        builder.Services.AddTransient<JokeStatisticsViewModel>();
        builder.Services.AddTransient<AddJokeViewModel>();
        builder.Services.AddTransient<JokeDetailViewModel>();

        // Zoals in de gepubliceerde Les06 staan de routes op het samenstelpunt.
        // Registreer dezelfde routes niet ook in AppShell.
        Routing.RegisterRoute(nameof(AddJokePage), typeof(AddJokePage));
        Routing.RegisterRoute(nameof(JokeDetailPage), typeof(JokeDetailPage));

        return builder.Build();
    }
}
