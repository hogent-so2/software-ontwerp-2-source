using JokeApp.ViewModels;

namespace JokeApp;

public partial class MainPage : ContentPage
{
    private readonly JokeListViewModel _viewModel;

    public MainPage(JokeListViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Dit is een lifecycle-doorgeefluik, geen scherm- of opslaglogica.
        // Ook bij terugkeer blijft het bestaande ViewModel met zijn filters bestaan.
        _viewModel.Reload();
    }
}
