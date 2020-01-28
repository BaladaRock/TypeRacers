using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using TypeRacers.Model;

namespace TypeRacers.ViewModel
{
    public class PracticeViewModel : ITextToType, INotifyPropertyChanged
    {
        string textToType;
        InputCharacterValidation userInputValidator;
        bool isValid;
        int spaceIndex;
        int correctChars;
        int incorrectChars;
        int currentWordIndex;
        private bool alert;
        Model.Model model;
        private DateTime startTime;
        int numberOfCharactersTyped;

        public PracticeViewModel()
        {
            model = new Model.Model();
            TextToType = model.GetGeneratedTextToTypeLocally();
            userInputValidator = new InputCharacterValidation(TextToType);
            GetReadyAlert = true;
            TriggerPropertyChanged(nameof(GetReadyAlert));
            startTime = DateTime.UtcNow;
        }

        public IEnumerable<Inline> TextToTypeStyles
        {
            get => new[] { new Run() { Text = TextToType.Substring(0, spaceIndex) , Foreground = Brushes.Gold},
                new Run() { Text = TextToType.Substring(spaceIndex, correctChars), Foreground = Brushes.Gold, TextDecorations = TextDecorations.Underline},
                new Run() { Text = TextToType.Substring(correctChars + spaceIndex, incorrectChars), TextDecorations = TextDecorations.Underline, Background = Brushes.IndianRed},
                new Run() {Text = TextToType.Substring(spaceIndex + correctChars + incorrectChars, CurrentWordLength - correctChars - incorrectChars), TextDecorations = TextDecorations.Underline},
                new Run() {Text = TextToType.Substring(spaceIndex + CurrentWordLength) }
                };
        }

        public bool ValidateInput
        {
            get => isValid;

            set
            {
                if (isValid == value)
                    return;

                isValid = value;
                TriggerPropertyChanged(nameof(ValidateInput));
                TriggerPropertyChanged(nameof(InputBackgroundColor));
            }
        }

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

                return (numberOfCharactersTyped / 5) * 60/ ((int)(DateTime.UtcNow - startTime).TotalSeconds);
            }
        }
        public int CurrentWordLength
        {
            get => TextToType.Split()[currentWordIndex].Length;//length of current word
        }
        public bool AllTextTyped { get; set; }
        //determines if a popup alert should apear, bindedin open property of popup xaml
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
                ValidateInput = userInputValidator.ValidateWord(CurrentInputText, CurrentInputText.Length);

                CheckUserInput(textToType);

                TriggerPropertyChanged(nameof(CurrentWordLength));//moves to next word

                //determine number of characters that are valid/invalid to form substrings
                HighlightText();

                TriggerPropertyChanged(nameof(CurrentInputText));
            }
        }

        public bool CanUserType { get; internal set; }
        public string SecondsInGame { get; internal set; } = "90 seconds";
        public bool GetReadyAlert { get; internal set; }
        public string SecondsToGetReady { get; internal set; } = "3";

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
                TriggerPropertyChanged(nameof(WPMProgress));//recalculates progress 
            }
            //checks if current word is the last one
            if (ValidateInput && textToType.Length + spaceIndex == TextToType.Length)
            {
                AllTextTyped = true;
                TriggerPropertyChanged(nameof(AllTextTyped));
                TriggerPropertyChanged(nameof(WPMProgress));//recalculates progress 
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

        //INotifyPropertyChanged code - basic 
        public event PropertyChangedEventHandler PropertyChanged;

        public void TriggerPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
