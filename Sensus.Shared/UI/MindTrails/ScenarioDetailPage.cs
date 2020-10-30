// Copyright 2014 The Rector & Visitors of the University of Virginia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sensus.MindTrailsBehind;
using Sensus.UI.MindTrailsBehind;
using Xamarin.Forms;

namespace Sensus.UI.MindTrails
{

    public class ScenarioDetailPage : BannerFrameTool
    {
        protected Label scenarioDescription;
        protected Grid wordGrid;
        protected Grid lettersGrid;
        protected Button correctLetterGreen;
        protected int missingLetterIndex;
        public char missingLetter;

        public ScenarioDetailPage()
        {

            Content = _contentLayout;

            Grid headerGrid = new Grid
            {
                ColumnSpacing = 0,
                RowSpacing = 0,
                Padding = 0,
                Margin = new Thickness(10, 10, 10, 0), // , 20 
                ColumnDefinitions = {
                new ColumnDefinition {
                    Width = new GridLength(1, GridUnitType.Star)
                },
                new ColumnDefinition
                {
                    Width = new GridLength(3, GridUnitType.Star)
                }
            },

            };
            Label scenarioNum = new Label
            {
                Text = "Scenario " + (scenarioCounter + 1).ToString(),
                TextColor = Color.FromHex("166DA3"),
                FontSize = 22,
                FontFamily = "Source Sans Pro",
                HorizontalTextAlignment = TextAlignment.Start,
                Margin = new Thickness(0, 4, 0, 4)


            };
            Image scenarioIcon = new Image
            {
                Source = "pencil.png",
                HeightRequest = 10,
                Margin = new Thickness(0, 1, -10, 1)
            };

            headerGrid.Children.Add(scenarioIcon, 0, 0);

            headerGrid.Children.Add(scenarioNum, 1, 0); 

            _whiteframeLayout.Children.Add(headerGrid);


            Frame grayFrame = new Frame
            {
                BackgroundColor = Color.FromHex("F0ECEC"),
                HasShadow = false,
                Padding = 0,
                Margin = new Thickness(15, 0, 15, 0),
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                CornerRadius = 10
            };

            _whiteframeLayout.Children.Add(grayFrame);

            scenarioDescription = new Label
            {
                TextColor = Color.Black,
                FontFamily = "Source Sans Pro",
                Margin = new Thickness(10),
                FontSize = 20
            };

            grayFrame.Content = scenarioDescription;

            wordGrid = new Grid
            {

                Margin = new Thickness(10, 5, 10, 0),
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                ColumnSpacing = 5
            };

            _whiteframeLayout.Children.Add(wordGrid);



            Label selectTile = new Label
            {
                Text = "SELECT A TILE:",
                FontSize = 14,
                TextColor = Color.Black,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Margin = 0

            };

            _whiteframeLayout.Children.Add(selectTile);


            // read json

            var assembly = typeof(ScenarioDetailPage).GetTypeInfo().Assembly;
            string jsonFileName = "";
            if (sessionNumber == 1)
            {
                jsonFileName = "Sensus.Android.Resources.firstSession.json";
            }
            if (sessionNumber == 2)
            {
                jsonFileName = "Sensus.Android.Resources.secondSession.json";
            }
            if (sessionNumber == 3)
            {
                jsonFileName = "Sensus.Android.Resources.thirdSession.json";
            }
            if (sessionNumber == 4)
            {
                jsonFileName = "Sensus.Android.Resources.fourthSession.json";
            }
            if (sessionNumber == 5)
            {
                jsonFileName = "Sensus.Android.Resources.fifthSession.json";
            }
            Stream stream = assembly.GetManifestResourceStream(jsonFileName);

            // should be protocol.

            using (var reader = new StreamReader(stream))
            {
                var json = reader.ReadToEnd();
                var data = JsonConvert.DeserializeObject<Root>(json);
                // instead of data, protocol.Session 
                if (scenarioCounter < 40) // length of json 
                {
                    //string input = MindTrailsProtocol.protocol.Session[scenarioCounter].statement1;
                    string input = data.firstSession[scenarioCounter].statement1;

                    // protocol.Session[scenarioCounter].statement1;

                    // from the data, and then from the firstSession (list of all scenarios from SessionModel.cs)
                    // get the # scenario that corresponds to scenarioCounter, then get the first statement

                    string[] sentences = Regex.Split(input, @"(?<=[\.!\?])\s+"); // split by sentences

                    string description = "";
                    if (sentences.Length < 3) // if there are 3 sentences, separate each sentence with two enters
                    {
                        foreach (string sentence in sentences)
                        {
                            description += sentence + "\n\n";
                        }
                    }
                    else // if not, the sentences after the third one should be combined without \n 
                    {
                        for (int s = 0; s < 3; s++)
                        {
                            description += sentences[s] + "\n\n";
                        }
                        for (int f = 3; f < sentences.Length; f++)
                        {
                            description = description.Substring(0, description.Length - 2) + " " + sentences[f];
                        }
                    }
                    description = description.Substring(0, description.LastIndexOf(" ", description.Length)); // delete last space 
                    description += "..."; // add ... to the scenario description

                    scenarioDescription.Text = description; // text of the label scenarioDescription is description

                    //string word = MindTrailsProtocol.protocol.Session[scenarioCounter].word1.ToUpper(); // the word that should be taken out, to upper case 
                    string word = data.firstSession[scenarioCounter].word1.ToUpper(); // the word that should be taken out, to upper case 
                    int columnNum = word.Length; // # of columns in the word tiles is equal to the word length

                    ColumnDefinitionCollection columnCollection = new ColumnDefinitionCollection(); 

                    // dictionary of letters
                    var letters = new Dictionary<string, Button>(); 
                    string letterVariable;
                    for (int i = 0; i < columnNum; i++) // in a loop from 0 to columnNum 
                    {
                        // add a new column 
                        ColumnDefinition column = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
                        columnCollection.Add(column);

                        // now we are adding the letters in the word to the letters dictionary:


                        // usually, string letterVariable = "letter" + word[i].ToString();
                        // ex. lettere
                        // if letter's key already exists in the dictionary:
                        if (letters.ContainsKey("letter" + word[i].ToString()) && letters.ContainsKey("letter" + word[i].ToString() + word[i].ToString()))
                        {
                            // make the key something like letteree
                            letterVariable = "letter" + word[i].ToString() + word[i].ToString() + word[i].ToString();

                        }
                        // otherwise, just make it "lettere" 
                        else if (letters.ContainsKey("letter" + word[i].ToString()))
                        {
                            letterVariable = "letter" + word[i].ToString() + word[i].ToString();

                        }
                        else { letterVariable = "letter" + word[i].ToString(); }


                        // for each letter, make it into a button
                        letters[letterVariable] = new Button
                        {
                            Text = word[i].ToString(),
                            FontFamily = "Source Sans Pro",
                            FontAttributes = FontAttributes.Bold,
                            BackgroundColor = Color.FromHex("B5E7FA"),
                            TextColor = Color.Black,
                            HeightRequest = 40,
                            FontSize = 15,
                            CornerRadius = 6,
                            Padding = 0,
                            Margin = 0
                        };
                    }

                    // add letters to each column
                    wordGrid.ColumnDefinitions = columnCollection;
                    int a = 0;
                    foreach (KeyValuePair<string, Button> letter in letters)
                    {
                        // letter.Key is lettere, lettera, letterb, etc.
                        // letter.Value is the actual button
                        wordGrid.Children.Add(letter.Value, a, 0);
                        Console.WriteLine(letter.Key);
                        Console.WriteLine(a);
                        a++;
                    }

                    Button missingLetterButton = new Button
                    {
                        BackgroundColor = Color.FromHex("DADADA"),
                        HeightRequest = 40,
                        FontSize = 15,
                        CornerRadius = 6
                    };
                    correctLetterGreen = new Button
                    {
                        FontFamily = "Source Sans Pro",
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.Black,
                        BackgroundColor = Color.FromHex("B5E7FA"),
                        HeightRequest = 40,
                        FontSize = 15,
                        CornerRadius = 6,
                        Padding = 0,
                        Margin = 0
                    };
                    // choose random letter to be missing 
                    Random rand = new Random();
                    missingLetterIndex = rand.Next(0, letters.Count);
                    string missingRandStr = letters.ElementAt(missingLetterIndex).Key; // missing letter string - "lettera"
                    Button missingRandButton = letters[missingRandStr]; // missing letter button 
                    wordGrid.Children.Remove(missingRandButton); // delete button from grid

                    missingLetter = missingRandStr.Last(); // missing letter 
                    correctLetterGreen.Text = missingLetter.ToString();
                    // get index of missing letter in word
                    // gray button to index of word in grid
                    wordGrid.Children.Add(missingLetterButton, missingLetterIndex, 0);


                }

            }

            lettersGrid = new Grid
            {
                ColumnDefinitions = {
                new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star)},
                new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star)},
                new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star)},
                new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star)},
                new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star)},
                new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star)},

            },
                Padding = new Thickness(30, 0, 30, 5),
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.EndAndExpand // CHANGE? 

            };

            _whiteframeLayout.Children.Add(lettersGrid);

            var letterOptions = new Dictionary<string, Button>();

            letterOptions["correct"] = correctLetterGreen;

            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            int randIndex;
            string randomLetter;

            for (int j = 0; j < 3; j++)
            {
                Random rng = new Random();
                randIndex = rng.Next(chars.Length);
                randomLetter = chars[randIndex].ToString();
                if (randomLetter.Equals(missingLetter.ToString().ToUpper())) 
                {
                    randIndex += 1;
                    randomLetter = chars[randIndex].ToString();

                }
                string key;

                if (letterOptions.ContainsKey("option" + randomLetter))
                {
                    randIndex += 1;
                    randomLetter = chars[randIndex].ToString();
                    key = "option" + randomLetter;

                }
                else { key = "option" + randomLetter; }


                Button letterOption = new Button
                {
                    Text = randomLetter, // random letter 
                    FontFamily = "Source Sans Pro",
                    FontAttributes = FontAttributes.Bold,
                    BackgroundColor = Color.FromHex("B5E7FA"),
                    TextColor = Color.Black,
                    HeightRequest = 40,
                    FontSize = 15,
                    CornerRadius = 6,
                    // ADD: Clicked = X 
                };
                // if key is not correct 
                if (!letterOption.Text.Equals(missingLetter.ToString().ToUpper()))
                {
                    // missingRandStr.Last()
                    letterOption.Clicked += onIncorrect;
                    void onIncorrect(object sender, EventArgs args)
                    {
                        if (roundCounter == 1)
                        {
                            roundScore1 -= 0.5;
                            Console.WriteLine("Score after incorrect letter");
                            Console.WriteLine(roundScore1);
                        }
                        else if (roundCounter == 2)
                        {
                            roundScore2 -= 0.5;
                        }
                        else if (roundCounter == 3)
                        {
                            roundScore3 -= 0.5;
                        }
                        else
                        {
                            roundScore4 -= 0.50;
                        }
                    };
                }

                letterOptions[key] = letterOption;

            }
            foreach (int b in Enumerable.Range(1, 4))
            {
                string randOptionStr;
                int random;
                // random value in letterOptions
                // but delete pair when done
                Random rand = new Random();
                // CHANGE
                if (letterOptions.Count <= 0)
                {
                    random = 0;
                }
                else
                {
                    random = rand.Next(0, letterOptions.Count);
                };
                randOptionStr = letterOptions.ElementAt(random).Key; // missing letter string - "lettera"
                Button randOptionButton = letterOptions[randOptionStr]; // missing letter button 
                lettersGrid.Children.Add(randOptionButton, b, 0);
                letterOptions.Remove(randOptionStr);



            }

            Button whiteButton = new Button
            {
                BackgroundColor = Color.White,
                HeightRequest = 40,
                CornerRadius = 6
            };

            Image correctIcon = new Image
            {
                Source = "CheckMark.png",
                HeightRequest = 40,
                Margin = 0
            };


            correctLetterGreen.Clicked += onCorrectLetter;

            async void onCorrectLetter(object sender, EventArgs args)
            {
                correctLetterGreen.BackgroundColor = Color.FromHex("6662A74C");
                wordGrid.Children.Add(correctLetterGreen, missingLetterIndex, 0);
                lettersGrid.Children.Add(correctIcon, 5, 0);
                //// timer to next page
                await Task.Delay(500); 
                await Navigation.PushAsync(new ScenarioTestPage());

            };

            Label blankLabel = new Label
            {
                Text = " ",
                BackgroundColor = Color.White,
                VerticalOptions = LayoutOptions.EndAndExpand,
            };

            _whiteframeLayout.Children.Add(blankLabel);

        }
    }
}
