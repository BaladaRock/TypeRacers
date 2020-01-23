using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace TypeRacers.View
{
    /// <summary>
    /// Interaction logic for IOUserControl.xaml
    /// </summary>
    public partial class ReadAndTypeUserControl : UserControl
    {
        public ReadAndTypeUserControl()
        {
            InitializeComponent();
        }

        //dp for typed text properties
        public readonly static DependencyProperty RATUCInlinesProperty = DependencyProperty.Register("RATUCInlines", typeof(IEnumerable<Inline>), typeof(ReadAndTypeUserControl));

        public IEnumerable<Inline> RATUCInlines
        {
            get { return (IEnumerable<Inline>)GetValue(RATUCInlinesProperty); }
            set
            {
                SetValue(RATUCInlinesProperty, value);
            }
        }

        //dp for textbox text
        public readonly static DependencyProperty RATUCCurrentInputTextProperty = DependencyProperty.Register("RATUCCurrentInputText", typeof(string), typeof(ReadAndTypeUserControl));

        public string RATUCCurrentInputText
        {
            get { return (string)GetValue(RATUCCurrentInputTextProperty); }
            set
            {
                SetValue(RATUCCurrentInputTextProperty, value);
            }
        }

        //dp for all text typed
        public readonly static DependencyProperty RATUCAllTextTypedProperty = DependencyProperty.Register("RATUCAllTextTyped", typeof(bool), typeof(ReadAndTypeUserControl));

        public bool RATUCAllTextTyped
        {
            get { return (bool)GetValue(RATUCAllTextTypedProperty); }
            set
            {
                SetValue(RATUCAllTextTypedProperty, value);
            }
        }

        //dp for background color in text box
        public readonly static DependencyProperty RATUCBackgroundColorProperty = DependencyProperty.Register("RATUCBackgroundColor", typeof(string), typeof(ReadAndTypeUserControl));

        public string RATUCBackgroundColor
        {
            get { return (string)GetValue(RATUCBackgroundColorProperty); }
            set
            {
                SetValue(RATUCBackgroundColorProperty, value);
            }
        }


        public readonly static DependencyProperty RATUCTypingAlertProperty = DependencyProperty.Register("RATUCTypingAlert", typeof(string), typeof(ReadAndTypeUserControl));

        public string RATUCTypingAlert
        {
            get { return (string)GetValue(RATUCTypingAlertProperty); }
            set
            {
                SetValue(RATUCTypingAlertProperty, value);
            }
        }

        public readonly static DependencyProperty RATUCSecondsToStartProperty = DependencyProperty.Register("RATUCSecondsToStart", typeof(string), typeof(ReadAndTypeUserControl));

        public string RATUCSecondsToStart
        {
            get { return (string)GetValue(RATUCSecondsToStartProperty); }
            set
            {
                SetValue(RATUCSecondsToStartProperty, value);
            }
        }

        public readonly static DependencyProperty RATUCGetReadyAlertProperty = DependencyProperty.Register("RATUCGetReadyAlert", typeof(bool), typeof(ReadAndTypeUserControl));

        public bool RATUCGetReadyAlert
        {
            get { return (bool)GetValue(RATUCGetReadyAlertProperty); }
            set
            {
                SetValue(RATUCGetReadyAlertProperty, value);
            }
        }

        public readonly static DependencyProperty RATUCCanTypeProperty = DependencyProperty.Register("RATUCCanType", typeof(bool), typeof(ReadAndTypeUserControl));

        public bool RATUCCanType
        {
            get { return (bool)GetValue(RATUCCanTypeProperty); }
            set
            {
                SetValue(RATUCCanTypeProperty, value);
            }
        }


        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
        }

    }
}
