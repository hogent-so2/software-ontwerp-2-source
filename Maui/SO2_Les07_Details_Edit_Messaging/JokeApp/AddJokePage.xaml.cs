using JokeApp.ViewModels;

namespace JokeApp;

public partial class AddJokePage : ContentPage
{
    public AddJokePage(AddJokeViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        InitializeComponent();
        BindingContext = viewModel;

        // De pagina leest geen controls uit, valideert niet en slaat niets op.
        // TwoWay-bindings geven invoer door; bindings tonen de feedback terug.
    }
}
