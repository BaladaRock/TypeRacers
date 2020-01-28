using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using TypeRacers.ViewModel;

namespace TypeRacers.View
{
    /// <summary>
    /// Interaction logic for IOUserControl.xaml
    /// </summary>
    public partial class ReadAndTypeUserControl : UserControl, INotifyPropertyChanged
    {
        DispatcherTimer timer;
        private int secondsToStart;
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

        public readonly static DependencyProperty RATUCSecondsInGameProperty = DependencyProperty.Register("RATUCSecondsInGame", typeof(string), typeof(ReadAndTypeUserControl));

        public string RATUCSecondsInGame
        {
            get { return (string)GetValue(RATUCSecondsInGameProperty); }
            set
            {
                SetValue(RATUCSecondsInGameProperty, value);
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

        public readonly static DependencyProperty RATUCStartTimeProperty = DependencyProperty.Register("RATUCStartTime", typeof(DateTime), typeof(ReadAndTypeUserControl));

        public DateTime RATUCStartTime
        {
            get { return (DateTime)GetValue(RATUCStartTimeProperty); }
            set
            {
                SetValue(RATUCStartTimeProperty, value);
            }
        }
  
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
        }

        private void GetReadyPopUp_Opened(object sender, EventArgs e)
        {
            timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            timer.Tick += Timer_Tick;

            timer.Start();
            
            secondsToStart = 3;
            
            //if (RATUCStartTime != null)
            //{
            //    secondsToStart = (DateTime.UtcNow - RATUCStartTime).Seconds;
            //}
        }

       
        private void Timer_Tick(object sender, EventArgs e)
        {
           secondsToStart--;

            if (secondsToStart < 0)
            {
                timer.Stop();
                RATUCGetReadyAlert = false;
                TriggerPropertyChanged(nameof(RATUCGetReadyAlert));
                RATUCCanType = true;
                StartGameTimer();
                TriggerPropertyChanged(nameof(RATUCCanType));
            }

            RATUCSecondsToStart = secondsToStart.ToString();
            TriggerPropertyChanged(nameof(RATUCSecondsToStart));

            if (secondsToStart == 0)
            {
                RATUCSecondsToStart = "START!";
                TriggerPropertyChanged(nameof(RATUCSecondsToStart));
            }
        }

        private void StartGameTimer()
        {
            timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            timer.Tick += GameTimer_Tick;

            timer.Start();
        }

        private int secondsInGame = 90;

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            secondsInGame--;

            if (secondsInGame < 0)
            {
                timer.Stop();
                RATUCCanType = false;
                TriggerPropertyChanged(nameof(RATUCCanType));
            }

            if (secondsInGame > 0)
            {
                RATUCSecondsInGame = secondsInGame.ToString() + " seconds left";
                TriggerPropertyChanged(nameof(RATUCSecondsInGame));
            }

            if (secondsInGame == 0)
            {
                RATUCSecondsInGame = "Time is up!";
                TriggerPropertyChanged(nameof(RATUCSecondsInGame));
            }
        }
       
        public event PropertyChangedEventHandler PropertyChanged;

        public void TriggerPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
