using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace TypeRacers.ViewModel
{
    class VersusViewModel : ITextToType, INotifyPropertyChanged
    {
        string textToType;
        InputCharacterValidation userInputValidator;
        bool isValid;
        int spaceIndex;
        int correctChars;
        int incorrectChars;
        int currentWordIndex;
        private bool alert;
        readonly Model.Model model;
        private DateTime startTime;
        private int numberOfCharactersTyped;

        public VersusViewModel()
        {
            model = new Model.Model();
            TextToType = model.GetGeneratedTextToTypeFromServer();
            userInputValidator = new InputCharacterValidation(TextToType);
            startTime = DateTime.UtcNow;
            // first time getting opponents
            Opponents = model.GetOpponents();
            //check how many players can we display on the screen
            UpdateShownPlayers();
            //start searching for 30 seconds and subscribe to timer
            model.StartSearchingOpponents();
            model.SubscribeToSearchingOpponents(UpdateOpponents);
            CanUserType = false;
        }
       
        public IEnumerable<Inline> Inlines
        {
            get => new[] { new Run() { Text = TextToType.Substring(0, spaceIndex) , Foreground = Brushes.Gold},
                new Run() { Text = TextToType.Substring(spaceIndex, correctChars), Foreground = Brushes.Gold, TextDecorations = TextDecorations.Underline},
                new Run() { Text = TextToType.Substring(correctChars + spaceIndex, incorrectChars), TextDecorations = TextDecorations.Underline, Background = Brushes.IndianRed},
                new Run() {Text = TextToType.Substring(spaceIndex + correctChars + incorrectChars, CurrentWordLength - correctChars - incorrectChars), TextDecorations = TextDecorations.Underline},
                new Run() {Text = TextToType.Substring(spaceIndex + CurrentWordLength) }
                };
        }

        public IEnumerable<Tuple<string, Tuple<string, string>>> Opponents { get; private set; }

        public Visibility ShowFirstOpponent { get; set; }

        public Visibility ShowSecondOpponent { get; set; }

        public int OpponentsCount { get; set; }

        public string SecondsToStart { get; set; }

        public int ElapsedTimeFrom30SecondsTimer { get; set; }

        public int LoadingPercent
        {
            get => ElapsedTimeFrom30SecondsTimer;

            set
            {
                if(ElapsedTimeFrom30SecondsTimer == value)
                {
                    return;
                }

                LoadingPercent = value;
                TriggerPropertyChanged(nameof(LoadingPercent));
            }
        }

        public bool IsValid
        {
            get => isValid;

            set
            {
                if (isValid == value)
                    return;

                isValid = value;
                TriggerPropertyChanged(nameof(IsValid));
                TriggerPropertyChanged(nameof(InputBackgroundColor));
            }
        }

        public bool CanUserType { get; set; }

        public int SliderProgress
        {
            get
            {
                if (AllTextTyped)
                {
                    return 100;
                }

                return spaceIndex * 100 / TextToType.Length;
            }
        }

        public int Progress
        {
            get
            {
                if (currentWordIndex == 0)
                {
                    return 0;
                }

                return (numberOfCharactersTyped / 5) * 60 / ((int)(DateTime.UtcNow - startTime).TotalSeconds);
            }
        }

        public int CurrentWordLength
        {
            get => TextToType.Split()[currentWordIndex].Length;//length of current word
        }

        public bool GetReadyAlert { get; set; }

        public bool AllTextTyped { get; set; }

        //determines if a popup alert should apear, binded in open property of popup xaml
        public bool TypingAlert
        {
            get => alert;

            set
            {
                if (alert == value)
                {
                    return;
                }

                alert = value;
                TriggerPropertyChanged(nameof(TypingAlert));
            }
        }

        public string InputBackgroundColor
        {
            get
            {
                if (string.IsNullOrEmpty(CurrentInputText))
                {
                    return default;
                }
                if (!isValid)
                {
                    return "IndianRed";
                }

                return default;
            }
        }

        public string TextToType { get; }

        public string CurrentInputText
        {
            get => textToType;
            set
            {
                // return because we dont need to execute logic if the input text has not changed
                if (textToType == value)
                    return;

                textToType = value;

                //validate current word
                IsValid = userInputValidator.ValidateWord(CurrentInputText, CurrentInputText.Length);

                CheckUserInput(textToType);

                TriggerPropertyChanged(nameof(CurrentWordLength));//moves to next word

                //determine number of characters that are valid/invalid to form substrings
                HighlightText();

                TriggerPropertyChanged(nameof(CurrentInputText));
            }
        }

        public void ReportProgress()
        {
            model.ReportProgress(Progress, SliderProgress);
            Opponents = model.GetOpponents();
            TriggerPropertyChanged(nameof(Opponents));
        }

        public void CheckUserInput(string value)
        {
            //checks if current word is typed, clears textbox, reintializes remaining text to the validation, sends progress 
            if (isValid && value.EndsWith(" "))
            {
                spaceIndex += textToType.Length;

                if (currentWordIndex < TextToType.Split().Length - 1)
                {
                    currentWordIndex++;
                }

                userInputValidator = new InputCharacterValidation(TextToType.Substring(spaceIndex));
                numberOfCharactersTyped += CurrentInputText.Length;
                textToType = string.Empty;
                TriggerPropertyChanged(nameof(SliderProgress));
                TriggerPropertyChanged(nameof(Progress));
                //recalculates progress 
                ReportProgress();
            }
            //checks if current word is the last one
            if (IsValid && textToType.Length + spaceIndex == TextToType.Length)
            {
                AllTextTyped = true;
                TriggerPropertyChanged(nameof(AllTextTyped));
                TriggerPropertyChanged(nameof(SliderProgress));
                TriggerPropertyChanged(nameof(Progress));//recalculates progress 
                ReportProgress();
            }
        }

        public void HighlightText()
        {
            if (!Keyboard.IsKeyDown(Key.Back))
            {
                if (isValid)
                {
                    TypingAlert = false;
                    correctChars = textToType.Length;
                    incorrectChars = 0;
                }

                if (!isValid)
                {
                    incorrectChars++;
                    if (CurrentWordLength - correctChars - incorrectChars < 0)
                    {
                        TypingAlert = true;
                        textToType = textToType.Substring(0, correctChars);
                        incorrectChars = 0;
                    }
                }
            }
            else
            {
                if (!isValid && !string.IsNullOrEmpty(textToType))
                {
                    incorrectChars--;
                }

                else
                {
                    TypingAlert = false;
                    correctChars = textToType.Length;
                    incorrectChars = 0;
                }
            }

            TriggerPropertyChanged(nameof(Inlines)); //new Inlines formed at each char in input
        }

        public void UpdateOpponents(Tuple<List<Tuple<string, Tuple<string, string>>>, int> updatedOpponentsAndElapsedTime)
        {
            Opponents = updatedOpponentsAndElapsedTime.Item1;
            ElapsedTimeFrom30SecondsTimer = updatedOpponentsAndElapsedTime.Item2;
            OpponentsCount = Opponents.Count() + 1;
            TriggerPropertyChanged(nameof(ElapsedTimeFrom30SecondsTimer));
            TriggerPropertyChanged(nameof(OpponentsCount));
            UpdateShownPlayers();
            if (OpponentsCount == 3)
            {
                TriggerPropertyChanged(nameof(Opponents));
                //enabling input
                GetReadyAlert = true;
                TriggerPropertyChanged(nameof(GetReadyAlert));
                startTime = startTime.AddSeconds(ElapsedTimeFrom30SecondsTimer / 1000 + 5);
                var now = DateTime.UtcNow;
                while ((startTime - now).Seconds < 5)
                {
                    SecondsToStart = (startTime - now).Seconds.ToString();
                    now = DateTime.UtcNow;
                    if ((startTime - now).Seconds == 0)
                    {
                        SecondsToStart = "START!";
                        TriggerPropertyChanged(nameof(SecondsToStart));
                        GetReadyAlert = false;
                        TriggerPropertyChanged(nameof(GetReadyAlert));
                        CanUserType = true;
                        TriggerPropertyChanged(nameof(CanUserType));
                        break;
                    }

                    TriggerPropertyChanged(nameof(SecondsToStart));
                }
                //we stop the timer after 30 seconds
                return;
            }

            TriggerPropertyChanged(nameof(Opponents));
        }

        public void UpdateShownPlayers()
        {
            if (Opponents.Count() == 0)
            {
                ShowFirstOpponent = Visibility.Hidden;
                ShowSecondOpponent = Visibility.Hidden;

            }
            if (Opponents.Count() == 1)
            {
                ShowFirstOpponent = Visibility.Visible;
                ShowSecondOpponent = Visibility.Hidden;
            }
            
            if(Opponents.Count() == 2)
            {
                ShowFirstOpponent = Visibility.Visible;
                ShowSecondOpponent = Visibility.Visible;
            }

            TriggerPropertyChanged(nameof(ShowFirstOpponent));
            TriggerPropertyChanged(nameof(ShowSecondOpponent));
        }

        //INotifyPropertyChanged code - basic 
        public event PropertyChangedEventHandler PropertyChanged;

        public void TriggerPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
