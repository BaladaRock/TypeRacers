using System.Windows;
using System.Windows.Controls;

namespace TypeRacers.View
{
    /// <summary>
    /// Interaction logic for CarAndProgressUserControl.xaml
    /// </summary>
    public partial class CarAndProgressUserControl : UserControl
    {
        public CarAndProgressUserControl()
        {
            InitializeComponent();
        }

        public static DependencyProperty CAPUCProgressProperty = DependencyProperty.Register("CAPUCProgress", typeof(int), typeof(CarAndProgressUserControl));

        public int CAPUCProgress
        {
            get { return (int)GetValue(CAPUCProgressProperty); }
            set
            {
                SetValue(CAPUCProgressProperty, value);
            }
        }

        public static DependencyProperty CAPUCNameProperty = DependencyProperty.Register("CAPUCName", typeof(string), typeof(CarAndProgressUserControl));

        public string CAPUCName
        {
            get { return (string)GetValue(CAPUCProgressProperty); }
            set
            {
                SetValue(CAPUCProgressProperty, value);
            }
        }
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
        }

        public static DependencyProperty CAPUCSliderStyleProperty = DependencyProperty.Register("CAPUCSliderStyle", typeof(Style), typeof(CarAndProgressUserControl));

        public Style CAPUCSliderStyle
        {
            get { return (Style)GetValue(CAPUCSliderStyleProperty); }
            set
            {
                SetValue(CAPUCSliderStyleProperty, value);
            }
        }
    }
}
