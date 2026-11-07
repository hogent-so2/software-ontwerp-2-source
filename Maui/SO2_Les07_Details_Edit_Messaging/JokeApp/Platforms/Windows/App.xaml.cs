namespace JokeApp.WinUI;

// Dit is de Windows-startklasse, niet de gedeelde JokeApp.App-klasse.
public partial class App : MauiWinUIApplication
{
    public App()
    {
        InitializeComponent();
    }

    protected override MauiApp CreateMauiApp()
    {
        return MauiProgram.CreateMauiApp();
    }
}
