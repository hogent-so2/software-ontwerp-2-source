using Android.App;
using Android.Content.PM;

namespace JokeApp;

// Standaard MAUI-opstartcode voor Android; de schermlogica staat in MainPage.
[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize
                           | ConfigChanges.Orientation
                           | ConfigChanges.UiMode
                           | ConfigChanges.ScreenLayout
                           | ConfigChanges.SmallestScreenSize
                           | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
}
