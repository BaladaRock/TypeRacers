using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;


namespace TypeRacers.ViewModel
{
    public class VersusViewModel : ITextToType, INotifyPropertyChanged
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
        private int numberOfCharactersTyped;

        public VersusViewModel()
        {
            model = new Model.Model();
            TextToType = model.GetGeneratedTextToTypeFromServer();
            userInputValidator = new InputCharacterValidation(TextToType);
            StartTime = DateTime.UtcNow;
            // first time getting opponents
            Opponents = model.GetOpponents();
            StartingTime = model.GetStartingTime();
            SecondsToGetReady = ((int)(DateTime.Parse(StartingTime) - DateTime.Now).Seconds / 1000).ToString();
            //check how many players can we display on the screen
            UpdateShownPlayers();

            ExitProgramCommand = new CommandHandler(() => ExitProgram(), () => true);
            RestartSearchingOpponentsCommand = new CommandHandler(() => RestartSearchingOpponents(), () => true);
            //start searching for 30 seconds and subscribe to timer
            model.StartSearchingOpponents();
            model.SubscribeToSearchingOpponents(UpdateOpponents);

            CanUserType = false;
        }

        public CommandHandler RestartSearchingOpponentsCommand { get; }

        public CommandHandler ExitProgramCommand { get; }

        public IEnumerable<Inline> TextToTypeStyles
        {
            get => new[] { new Run() { Text = TextToType.Substring(0, spaceIndex) , Foreground = Brushes.Gold},
                new Run() { Text = TextToType.Substring(spaceIndex, correctChars), Foreground = Brushes.Gold, TextDecorations = TextDecorations.Underline},
                new Run() { Text = TextToType.Substring(correctChars + spaceIndex, incorrectChars), TextDecorations = TextDecorations.Underline, Background = Brushes.IndianRed},
                new Run() {Text = TextToType.Substring(spaceIndex + correctChars + incorrectChars, CurrentWordLength - correctChars - incorrectChars), TextDecorations = TextDecorations.Underline},
                new Run() {Text = TextToType.Substring(spaceIndex + CurrentWordLength) }
                };
        }

        public IEnumerable<Tuple<string, Tuple<string, string, int>>> Opponents { get; private set; }

        public Visibility ShowFirstOpponent { get; set; }

        public Visibility ShowSecondOpponent { get; set; }

        public int OpponentsCount { get; set; }


        public string StartingTime { get; set; }

        public int ElapsedTimeFrom30SecondsTimer { get; set; }

        public int LoadingPercent
        {
            get => (90 - ElapsedTimeFrom30SecondsTimer) * 10;

            set
            {
                if (ElapsedTimeFrom30SecondsTimer == value)
                {
                    return;
                }

                LoadingPercent = value;
                TriggerPropertyChanged(nameof(LoadingPercent));
            }
        }

        public bool InputValidation
        {
            get => isValid;

            set
            {
                if (isValid == value)
                    return;

                isValid = value;
                TriggerPropertyChanged(nameof(InputValidation));
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

        public int WPMProgress
        {
            get
            {
                if (currentWordIndex == 0)
                {
                    return 0;
                }

                return (numberOfCharactersTyped / 5) * 60 / ((int)(DateTime.Now - DateTime.Parse(StartingTime)).TotalSeconds / 1000);
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
                InputValidation = userInputValidator.ValidateWord(CurrentInputText, CurrentInputText.Length);

                CheckUserInput(textToType);

                TriggerPropertyChanged(nameof(CurrentWordLength));//moves to next word

                //determine number of characters that are valid/invalid to form substrings
                HighlightText();

                TriggerPropertyChanged(nameof(CurrentInputText));
            }
        }

        public bool EnableGetReadyAlert { get; set; }

        public bool EnableRestartOrExitAlert { get; set; }

        public string SecondsToGetReady { get; set; }

        public string SecondsInGame { get; internal set; } = "90 seconds";

        public DateTime StartTime { get; }

        public void ReportProgress()
        {
            model.ReportProgress(WPMProgress, SliderProgress);
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
                TriggerPropertyChanged(nameof(WPMProgress));
                //recalculates progress 
                ReportProgress();
            }
            //checks if current word is the last one
            if (InputValidation && textToType.Length + spaceIndex == TextToType.Length)
            {
                AllTextTyped = true;
                TriggerPropertyChanged(nameof(AllTextTyped));
                TriggerPropertyChanged(nameof(SliderProgress));
                TriggerPropertyChanged(nameof(WPMProgress));//recalculates progress 
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

            TriggerPropertyChanged(nameof(TextToTypeStyles)); //new Inlines formed at each char in input
        }

        private void RestartSearchingOpponents()
        {
            EnableRestartOrExitAlert = false;
            TriggerPropertyChanged(nameof(EnableRestartOrExitAlert));
            model.StartSearchingOpponents();
            model.SubscribeToSearchingOpponents(UpdateOpponents);
        }

        private void ExitProgram()
        {
            Application.Current.Shutdown();
        }

        public void UpdateOpponents(Tuple<List<Tuple<string, Tuple<string, string, int>>>, int> updatedOpponentsAndElapsedTime)

        {
            Opponents = updatedOpponentsAndElapsedTime.Item1;
            ElapsedTimeFrom30SecondsTimer = updatedOpponentsAndElapsedTime.Item2;
            OpponentsCount = Opponents.Count() + 1;
            TriggerPropertyChanged(nameof(ElapsedTimeFrom30SecondsTimer));
            TriggerPropertyChanged(nameof(OpponentsCount));
            UpdateShownPlayers();

            if (DateTime.Now.ToString("h:mm:ss") == StartingTime && OpponentsCount < 2)
            {
                EnableRestartOrExitAlert = true;
                TriggerPropertyChanged(nameof(EnableRestartOrExitAlert));
            }
            if (OpponentsCount == 3 || DateTime.Now.ToString("h:mm:ss") == StartingTime && OpponentsCount == 2)
            {
                TriggerPropertyChanged(nameof(Opponents));
                //enabling input

                EnableGetReadyAlert = true;

                int.TryParse(SecondsToGetReady, out int seconds);

                if (seconds < 0)
                {
                    EnableGetReadyAlert = false;
                }

                TriggerPropertyChanged(nameof(EnableGetReadyAlert));

                //we stop the timer after 30 seconds
                return;
            }

            TriggerPropertyChanged(nameof(Opponents));
        }

        public void CheckIfRaceCanStart()
        {

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

            if (Opponents.Count() == 2)
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
