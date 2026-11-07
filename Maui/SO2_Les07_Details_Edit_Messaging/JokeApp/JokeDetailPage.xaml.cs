using JokeApp.ViewModels;

namespace JokeApp;

public partial class JokeDetailPage : ContentPage, IQueryAttributable
{
    private readonly JokeDetailViewModel _viewModel;

    public JokeDetailPage(JokeDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        // Alleen deze verbindingscode kent Shells dictionary en parametersleutel.
        // Het ViewModel krijgt een gewone Guid en blijft los van Shell.
        Guid jokeId = query.TryGetValue("JokeId", out var value) && value is Guid id
            ? id : Guid.Empty;
        _viewModel.Load(jokeId);
    }
}
