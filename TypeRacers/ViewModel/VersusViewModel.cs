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
        static int elapsedTime = 0; // Elapsed time in ms


        public VersusViewModel()
        {
            TextToType = Model.Model.GetGeneratedTextToTypeFromServer();
            userInputValidator = new InputCharacterValidation(TextToType);

            // first time getting opponents
            Opponents = Model.Model.GetOpponents();
            //start searching for 30 seconds
            Model.Model.StartSearchingOpponents();
            Model.Model.SubscribeToSearchingOpponents(UpdateOpponents);
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

        public IEnumerable<Tuple<string, string>> Opponents { get; private set; }

        public int OpponentsCount { get; set; }

        public int ElapsedTime { get; set; }
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
        public int Progress
        {
            get
            {
                if (AllTextTyped)
                {
                    return 100;
                }

                return (spaceIndex * 100 / TextToType.Length);
            }
        }
        public int CurrentWordLength
        {
            get => TextToType.Split()[currentWordIndex].Length;//length of current word
        }
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
            Model.Model.ReportProgress(Progress);
            Opponents = Model.Model.GetOpponents();
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
                textToType = string.Empty;
                TriggerPropertyChanged(nameof(Progress));//recalculates progress 
                ReportProgress();
            }
            //checks if current word is the last one
            if (IsValid && textToType.Length + spaceIndex == TextToType.Length)
            {
                AllTextTyped = true;
                TriggerPropertyChanged(nameof(AllTextTyped));
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
        public void UpdateOpponents(List<Tuple<string, string>> updatedOpponents)
        {
            Opponents = updatedOpponents;
            OpponentsCount = Opponents.Count() + 1;
            TriggerPropertyChanged(nameof(OpponentsCount));
            if (OpponentsCount == 3)
            {
                //enabling input
                CanUserType = true;
                TriggerPropertyChanged(nameof(CanUserType));
                //we stop the timer after 30 seconds
                return;
            }
            TriggerPropertyChanged(nameof(Opponents));

        }
        //INotifyPropertyChanged code - basic 
        public event PropertyChangedEventHandler PropertyChanged;

        public void TriggerPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
