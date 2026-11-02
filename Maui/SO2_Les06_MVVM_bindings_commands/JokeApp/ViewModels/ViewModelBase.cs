using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace JokeApp.ViewModels;

/// <summary>
/// Gemeenschappelijke ondersteuning voor wijzigingsmeldingen, zoals in de les.
/// Geen opslag, validatie of schermspecifiek gedrag in deze basisklasse.
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        // Een binding blijft niet voortdurend een property opvragen.
        // Dit event vertelt de luisteraar welke waarde opnieuw gelezen moet worden.
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(
        ref T field, T value,
        [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    // CallerMemberName vult de naam van de aanroepende property in.
    // Een ANDERE, afgeleide property melden we expliciet met nameof(...).
    // De bool van SetProperty helpt daarbij: alleen bij een echte wijziging.
}
