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
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Sensus.MindTrailsBehind;
using Sensus.UI.MindTrailsBehind;
using Xamarin.Forms;
// try to do white frame layout w/ margin as 90 on the bottom

namespace Sensus.UI.MindTrails
{
    public class ScenarioTestPage : BannerFrameTool
    {
        private Button correctAnswer;
        private Button incorrectAnswer;
        private Button next;

        public ScenarioTestPage()
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
                Margin = new Thickness(0, 4, 0, 4)// CHANGED! added in 

                //Margin = new Thickness(20,10,0,0)// CHANGED! added in 

            };
            Image scenarioIcon = new Image
            {
                Source = "pencil.png",
                HeightRequest = 10,
                Margin = new Thickness(0, 1, -10, 1)
            };

            headerGrid.Children.Add(scenarioIcon, 0, 0);

            headerGrid.Children.Add(scenarioNum, 1, 0); // column, row

            _whiteframeLayout.Children.Add(headerGrid);



            Frame grayFrame = new Frame
            {
                BackgroundColor = Color.FromHex("F0ECEC"),
                HasShadow = false,
                Padding = 0,
                Margin = new Thickness(20, 30, 20, 0),
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                CornerRadius = 10
            };

            _whiteframeLayout.Children.Add(grayFrame);

            Label question = new Label
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                FontFamily = "Source Sans Pro",
                FontSize = 20,
                Margin = 20,
                TextColor = Color.Black
            };

            grayFrame.Content = question;



            Grid yesNo = new Grid
            {
                ColumnDefinitions =
            {
                new ColumnDefinition {Width = new GridLength(.5, GridUnitType.Star)},
                new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star)},
                new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star)},
                new ColumnDefinition {Width = new GridLength(.5, GridUnitType.Star)},

            },
                RowDefinitions =
            {
                new RowDefinition {Height = new GridLength(1, GridUnitType.Star)}

            },
                Margin = new Thickness(10, 40, 10, 0),
                IsClippedToBounds = true
            };

            _whiteframeLayout.Children.Add(yesNo);

            Grid bottomGrid = new Grid
            {
                RowSpacing = 0,
                ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(.5, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(.5, GridUnitType.Star) },

            },
                RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(.5, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(.5, GridUnitType.Star) }

            },
                Margin = new Thickness(10, 10, 10, 0), // change 8/3 from 10 to 0 
                IsClippedToBounds = true,
                VerticalOptions = LayoutOptions.EndAndExpand
            };

            _whiteframeLayout.Children.Add(bottomGrid);

            Button yes = new Button
            {
                Text = "Yes",
                FontFamily = "Source Sans Pro",
                BackgroundColor = Color.FromHex("B5E7FA"),
                TextColor = Color.Black,
                HeightRequest = 40,
                FontSize = 15,
                CornerRadius = 6,
            };

            Button no = new Button
            {
                Text = "No",
                FontFamily = "Source Sans Pro",
                BackgroundColor = Color.FromHex("B5E7FA"),
                TextColor = Color.Black,
                HeightRequest = 40,
                FontSize = 15,
                CornerRadius = 6,
            };




            Image correctIcon = new Image
            {
                Source = "CheckMark.png",
                HeightRequest = 40,
                Margin = 0
            };
            Image incorrectIcon = new Image
            {
                Source = "XMark.png",
                HeightRequest = 40,
                Margin = 0
            };

            Label blankLabel = new Label
            {
                Text = "    ",
                Margin = new Thickness(10, 20, 10, 10),
                BackgroundColor = Color.White, // CHANGE 166DA3 
                FontFamily = "Source Sans Pro",
                FontSize = 20,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.EndAndExpand,
                HeightRequest = 42, // keep trying smalled size CHANGED!! 48 
                WidthRequest = 150

            };
            Label blankLabel2 = new Label
            {
                Text = "    ",
                Margin = new Thickness(10, 20, 10, 10),
                BackgroundColor = Color.White, // CHANGE 166DA3 
                FontFamily = "Source Sans Pro",
                FontSize = 20,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.EndAndExpand,
                HeightRequest = 48,
                WidthRequest = 150

            };
            _whiteframeLayout.Children.Add(blankLabel);
            _whiteframeLayout.Children.Add(blankLabel2);

            //bottomGrid.Children.Add(blankLabel, 1, 1);

            ProgressBar progressBar = new ProgressBar
            {
                ProgressColor = Color.FromHex("233367")
            };

            Label whoops = new Label
            {
                TextColor = Color.Black,
                Text = "Whoops! That doesn’t look right. Please wait a moment and try again.",
                Margin = new Thickness(0, 0, 0, 43), //CHANGED!!
                FontSize = 15,
                VerticalOptions = LayoutOptions.EndAndExpand, // andexpand

            };

            yesNo.Children.Add(yes, 1, 0);
            yesNo.Children.Add(no, 2, 0);

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

            var assembly = typeof(ScenarioTestPage).GetTypeInfo().Assembly; // CHANGED
            Stream stream = assembly.GetManifestResourceStream(jsonFileName);
            //Stream stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.{jsonFileName}");

            using (var reader = new StreamReader(stream))
            {
                var json = reader.ReadToEnd();
                var data = JsonConvert.DeserializeObject<Root>(json);
                if (scenarioCounter < 39) // length of json 
                {
                    // SensusServiceHelper.Get().MindTrialsProtocol
                    //string input = MindTrailsProtocol.protocol.Session[scenarioCounter].question;
                    string input = data.firstSession[scenarioCounter].question;

                    question.Text = input;
                    // if the positive answer is yes, and the answer is the positive value --> yes is correct, no is incorrect
                    // if the negative answer is yes, and the answer is the negative value --> yes is correct, no is incorrect 
                    //if (MindTrailsProtocol.protocol.Session[scenarioCounter].positive.Equals("Yes") && MindTrailsProtocol.protocol.Session[scenarioCounter].answer.Equals("Positive")
                    //    || MindTrailsProtocol.protocol.Session[scenarioCounter].negative.Equals("Yes") && MindTrailsProtocol.protocol.Session[scenarioCounter].answer.Equals("Negative"))
                    //{
                    if (data.firstSession[scenarioCounter].positive.Equals("Yes") && data.firstSession[scenarioCounter].answer.Equals("Positive")
                        || data.firstSession[scenarioCounter].negative.Equals("Yes") && data.firstSession[scenarioCounter].answer.Equals("Negative"))
                    {
                            // yes is correct, no is incorrect
                            yes.Clicked += onCorrect;
                        no.Clicked += onIncorrect;
                        correctAnswer = yes;
                        incorrectAnswer = no;
                    }
                    else
                    {
                        // if positive answer is no, and the answer is the positive --> no is correct 
                        // if the negative answer is no, and the answer is the negative --> no is correct 
                        // no is correct, yes is incorrect
                        yes.Clicked += onIncorrect;
                        no.Clicked += onCorrect;
                        correctAnswer = no;
                        incorrectAnswer = yes;

                    }

                }
            }
            // red = BackgroundColor = Color.FromHex("FAB9B5"),
            // green = BackgroundColor = Color.FromHex("6662A74C"),
            next = new Button
            {
                Text = "Next",
                Margin = new Thickness(10, 20, 10, 10),
                TextColor = Color.Black, // CHANGE white
                BackgroundColor = Color.FromHex("48AADF"), // CHANGE 166DA3 
                FontFamily = "Source Sans Pro",
                FontSize = 20,
                FontAttributes = FontAttributes.Bold,
                CornerRadius = 8,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.EndAndExpand,
                WidthRequest = 150

            };
            next.Clicked += onNextClicked; 

            async void onNextClicked(object sender, EventArgs args)
            {
                // scenarioCounter = 9 when 10 scenarios have been completed
                scenarioCounter++;
                if(scenarioCounter % 5 == 0)
                {
                    await Navigation.PushModalAsync(new NavigationPage(new ReflectionsPage())); 

                }
                // scenarioCounter == 10 * roundCounter --> when RoundScore2 appears 
                else
                { 
                    await Navigation.PushModalAsync(new NavigationPage(new ScenarioPage())); 
                }
            };

            void onCorrect(object sender, EventArgs args)
            {
                correctAnswer.BackgroundColor = Color.FromHex("6662A74C");
                yesNo.Children.Add(correctIcon, 3, 0);
                yes.IsEnabled = false; // disable both buttons 
                no.IsEnabled = false;
                //_whiteframeLayout.Children.Remove(blankLabel);
                _whiteframeLayout.Children.Remove(blankLabel2);
                _whiteframeLayout.Children.Add(next);
            };

            void onIncorrect(object sender, EventArgs args)
            {
                //yesNo.Children.Add(noRed, 2, 0);
                if (roundCounter == 1)
                {
                    roundScore1 -= 0.5;
                    Console.WriteLine("Score after incorrect test");
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


                progressBar.Progress = 0;
                incorrectAnswer.BackgroundColor = Color.FromHex("FAB9B5");
                yesNo.Children.Add(incorrectIcon, 3, 0);
                yes.IsEnabled = false;
                no.IsEnabled = false;
                bottomGrid.Children.Add(progressBar, 1, 0);
                bottomGrid.Children.Add(whoops, 1, 1);
                _whiteframeLayout.Children.Remove(blankLabel2);
                _whiteframeLayout.Children.Remove(blankLabel); // CHANGED!!


                double step = .2; // 30
                                    // every 1 second, the progress bar increases by .2 
                Device.StartTimer(new TimeSpan(0, 0, 1), () =>
                {
                    if (progressBar.Progress < 100)
                    {
                        Device.BeginInvokeOnMainThread(() => progressBar.Progress += step);

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                });
                Device.StartTimer(new TimeSpan(0, 0, 5), () =>
                {
                    // after 5 seconds --> 
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        yes.IsEnabled = true;
                        no.IsEnabled = true;
                        bottomGrid.Children.Remove(progressBar);
                        bottomGrid.Children.Remove(whoops);
                        incorrectAnswer.BackgroundColor = Color.FromHex("B5E7FA");
                        yesNo.Children.Remove(incorrectIcon);
                        _whiteframeLayout.Children.Add(blankLabel);
                        _whiteframeLayout.Children.Add(blankLabel2);

                    });
                    return false;
                });

            }
        }
    }
}

