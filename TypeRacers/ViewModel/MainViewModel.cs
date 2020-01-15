using System;
using System.Windows.Navigation;

namespace TypeRacers.ViewModel
{
    internal class MainViewModel
    {
        public MainViewModel()
        {
            ContestCommand = new CommandHandler(() => NavigateContest(), () => true);
            PracticeCommand = new CommandHandler(() => NavigatePractice(), () => true);
        }

        public CommandHandler ContestCommand { get; }

        public CommandHandler PracticeCommand { get; }

        public NavigationService Navigation { get; set; }

        private void NavigateContest()
        {
            Navigation.Navigate(new Uri("View/VersusPage.xaml", UriKind.RelativeOrAbsolute));
        }

        private void NavigatePractice()
        {
            Navigation.Navigate(new Uri("View/PracticePage.xaml", UriKind.RelativeOrAbsolute));
        }
    }
}