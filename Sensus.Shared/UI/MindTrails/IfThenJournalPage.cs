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

namespace Sensus.UI.MindTrails
{
    public class IfThenJournalPage : BannerFrameTool
    {
        public IfThenJournalPage()
        {
            Content = _contentLayout;

            Label copyMessage = new Label
            {
                Text = "Copy message below " +
                "to add to your journal",
                Margin = new Thickness(15, 5, 15, 10),
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                TextColor = Color.Black,
                FontSize = 24
            };

            _whiteframeLayout.Children.Add(copyMessage);

            Frame grayFrame1 = new Frame
            {
                BackgroundColor = Color.FromHex("F0ECEC"),
                CornerRadius = 10,
                Margin = new Thickness(15, 2, 15, 10),

            };

            Frame grayFrame2 = new Frame
            {
                BackgroundColor = Color.FromHex("F0ECEC"),
                CornerRadius = 10,
                Margin = new Thickness(15, 0, 15, 10),

            };
            Label ifMessage = new Label
            {
                BackgroundColor = Color.FromHex("F0ECEC"),
                TextColor = Color.Black,
                HorizontalTextAlignment = TextAlignment.Start,
                FontSize = 22,
                Margin = new Thickness(5, 0, 5, 0), //CHANGED!! to 5 instead of 10
                HeightRequest = 60
            };

            var ifMessageString = new FormattedString();
            Span ifSpan = new Span
            {
                FontSize =22
            };
            ifMessageString.Spans.Add(new Span { Text = "If ", FontSize = 22, FontAttributes = FontAttributes.Bold });
            ifMessageString.Spans.Add(ifSpan);
            ifMessage.FormattedText = ifMessageString;


            Label thenMessage = new Label
            {
                BackgroundColor = Color.FromHex("F0ECEC"),
                TextColor = Color.Black,
                HorizontalTextAlignment = TextAlignment.End,
                FontSize = 22,
                Margin = new Thickness(5, 0, 5, 0), //CHANGED!! to 5 instead of 10
                HeightRequest = 80 // CHANGED!! to 80 from 60 
            };
            var thenMessageString = new FormattedString();
            thenMessageString.Spans.Add(new Span { Text = "then ", FontSize = 22, FontAttributes = FontAttributes.Bold });
            Span thenSpan = new Span
            {
                FontSize = 22
            };
            thenMessageString.Spans.Add(thenSpan);
            thenMessage.FormattedText = thenMessageString;

            grayFrame1.Content = ifMessage;
            grayFrame2.Content = thenMessage;
            _whiteframeLayout.Children.Add(grayFrame1);
            _whiteframeLayout.Children.Add(grayFrame2);

            Frame journalFrame = new Frame
            {
                CornerRadius = 10,
                BackgroundColor = Color.Transparent,
                Margin = new Thickness(20, -30, 20, -20), // second from -30
                IsClippedToBounds = true,
            };

            Editor journalEditor = new Editor
            {
                HeightRequest = 100,
                WidthRequest = 400,
                HorizontalOptions = LayoutOptions.Center,
                TextColor = Color.Black,
                BackgroundColor = Color.Transparent,
            };

            journalFrame.Content = journalEditor;

            Grid journalGrid = new Grid
            {
                Margin = new Thickness(20, 0, 20, 0),
                Padding = 0,
                IsClippedToBounds = true
            };
            // check and see if JournalPage.png is being found
            Image journalStripes = new Image
            {
                Source = "JournalPage.png",
                //Aspect = Aspect.Fill,
                Aspect = Aspect.Fill, // CHANGED 
                HeightRequest = 100,
                //WidthRequest = 500,
                Margin = 0
            };
            journalGrid.Children.Add(journalStripes);
            journalGrid.Children.Add(journalFrame);




            Button add = new Button
            {
                // make it so once text is written, then you can add
                // gray and not enabled before text 
                Text = "Add",
                HorizontalOptions = LayoutOptions.Center,
                Margin = -30,
                CornerRadius = 10

            };

            add.Clicked += on_add;

            async void on_add(object sender, EventArgs args)
            {
                if (scenarioCounter % 10 == 0)
                {
                    await Navigation.PushModalAsync(new NavigationPage(new RoundScore2()));

                }
                else
                {
                    await Navigation.PushAsync(new ScenarioPage());
                }

            }

            _contentStack.Children.Add(journalGrid);
            _contentStack.Children.Add(add);

            var assembly = typeof(IfThenJournalPage).GetTypeInfo().Assembly;
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

            using (var reader = new StreamReader(stream))
            {
                var json = reader.ReadToEnd();
                var data = JsonConvert.DeserializeObject<Root>(json);

                //string input = MindTrailsProtocol.protocol.Session[scenarioCounter - 1].ifthen; //  - 1
                string input = data.firstSession[scenarioCounter - 1].ifthen; //  - 1


                string ifString = input.Substring(3, input.LastIndexOf(", then", input.Length) - 3);
                ifString += "...";
                ifSpan.Text = ifString;
                string thenString = input.Substring(input.LastIndexOf(", then", input.Length) + 7);
                thenSpan.Text = thenString;

            }
        }
    }
}
