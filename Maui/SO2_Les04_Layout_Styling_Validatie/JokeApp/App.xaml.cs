namespace JokeApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // De standaard opstartroute: App maakt het venster; AppShell bevat MainPage.
        // We stellen daarnaast niet de verouderde App.MainPage-property in.
        return new Window(new AppShell());
    }
}
