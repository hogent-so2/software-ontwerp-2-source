using System;
using System.Collections.Generic;
using System.Text;

namespace JokeApp.Services
{
    public class NavigationService
    {
        public async Task GoBack()
        {
            await GoToAsync("..");
        }

        public async Task GoToAddJokePage()
        {
            await GoToAsync(nameof(AddJokePage));
        }

        private async Task GoToAsync(string name)
        {
            await Shell.Current.GoToAsync(name);
        }

    }
}
