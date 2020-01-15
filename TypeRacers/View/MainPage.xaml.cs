using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using TypeRacers.ViewModel;

namespace TypeRacers.View
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private MainViewModel ViewModel
            => (MainViewModel)Resources["MainVM"];

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Navigation = NavigationService.GetNavigationService(this);
        }
    }
}