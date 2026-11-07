using JokeApp.ViewModels;

namespace JokeApp;

public partial class MainPage : ContentPage
{
    public MainPage(JokeListViewModel viewModel, JokeStatisticsViewModel statisticsViewModel)
    {
        InitializeComponent();
        // De View stelt twee presentaties samen. Het ene ViewModel heeft
        // geen property of methode nodig om het andere te kunnen aanspreken.
        statisticsPanel.BindingContext = statisticsViewModel;
        BindingContext = viewModel;
    }

    // Geen OnAppearing-refresh: eerste lading en wijzigingsberichten volstaan
    // zolang alle mutaties tijdens deze sessie via onze service gebeuren.
}
