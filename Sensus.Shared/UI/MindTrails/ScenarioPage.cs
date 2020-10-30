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
using CsvHelper;
using Newtonsoft.Json;
using Sensus.MindTrailsBehind;
using Sensus.Notifications;
using Sensus.UI.MindTrailsBehind;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Sensus.UI.MindTrails
{
    public class ScenarioPage : BannerFrameTool
    {
        private Label scenarioName;

        public ScenarioPage()
        { 

            Content = _contentLayout;


            Grid headerGrid = new Grid // header with scenario number
            {
                ColumnSpacing = 0,
                RowSpacing = 0,
                Padding = 0,
                Margin = new Thickness(10, 10, 10, 0),
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

            headerGrid.Children.Add(scenarioNum, 1, 0); // column, row

            _whiteframeLayout.Children.Add(headerGrid);

            Frame grayFrame = new Frame
            {
                BackgroundColor = Color.FromHex("F0ECEC"),
                HasShadow = false,
                Padding = 0,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,  
                WidthRequest = 400,
                Margin = new Thickness(40,10,40,14),
                CornerRadius = 10
            };

            scenarioName = new Label
            {
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                FontSize = 25,
                TextColor = Color.Black,
                Margin = new Thickness(5,0,5,0),
                HeightRequest = 80, 
            };

            Image scenarioImage = new Image // image for the scenario 
            {
                //Source = "dentist.png",
                HeightRequest = 200,
                Margin = new Thickness(0, 0, 0, 0)
            };

            // READ json file for scenarioPage to get
            var assembly = typeof(ScenarioPage).GetTypeInfo().Assembly;
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
                Root data = JsonConvert.DeserializeObject<Root>(json); // Root is from SessionModel.cs

                if (scenarioCounter < 40) // max number of scenarios 
                {
                    //scenarioName.Text = MindTrailsProtocol.protocol.Session[scenarioCounter].title;
                    scenarioName.Text = data.firstSession[scenarioCounter].title;
                    //scenarioImage.Source = MindTrailsProtocol.protocol.Session[scenarioCounter].image;
                    scenarioImage.Source = data.firstSession[scenarioCounter].image;


                }

            }



            grayFrame.Content = scenarioName;
            _whiteframeLayout.Children.Add(grayFrame);
            _whiteframeLayout.Children.Add(scenarioImage);

            Button next = new Button
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
                await Navigation.PushAsync(new ScenarioDetailPage());
            };
            _whiteframeLayout.Children.Add(next);

            ProgressBar progress = new ProgressBar
            {
                ProgressColor = Color.FromHex("166DA3"),
                Progress = scenarioCounter/40.0, // progress bar is the scenario # out of 40 
                Margin = new Thickness(20, 0, 20, 10),
                VerticalOptions = LayoutOptions.EndAndExpand
            };

            _whiteframeLayout.Children.Add(progress);
        }


    }
}
