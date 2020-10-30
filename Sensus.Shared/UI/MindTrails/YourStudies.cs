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
using Rg.Plugins.Popup.Animations;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace Sensus.UI.MindTrails
{
    public class YourStudies : BannerFrameTool
    {
        public YourStudies()
        {
            Content = _contentLayout;

            sessionNum.Text = "Your Studies";

            Button addStudy = new Button
            {
                Text = "Add a study!",
                Margin = new Thickness(10, 20, 10, 35),
                TextColor = Color.Black,
                BackgroundColor = Color.FromHex("48AADF"),
                FontFamily = "Source Sans Pro",
                FontSize = 20,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                CornerRadius = 8,
                WidthRequest = 150,

            };
            Button next = new Button
            {
                Text = "Next",
                Margin = new Thickness(10, 20, 10, 35),
                TextColor = Color.Black,
                BackgroundColor = Color.FromHex("48AADF"),
                FontFamily = "Source Sans Pro",
                FontSize = 20,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                CornerRadius = 8,
                WidthRequest = 150,

            };

            addStudy.Clicked += AddStudy_Clicked;
            next.Clicked += Next_Clicked;


            _contentStack.Children.Add(addStudy);
            _contentStack.Children.Add(next);


            async void AddStudy_Clicked(object sender, EventArgs args)
            {
                // animation for popup
                var popupProperties = new PopupPage();
                var scaleAnimation = new ScaleAnimation
                {
                    PositionIn = Rg.Plugins.Popup.Enums.MoveAnimationOptions.Right,
                    PositionOut = Rg.Plugins.Popup.Enums.MoveAnimationOptions.Left
                };
                await PopupNavigation.Instance.PushAsync(new AddStudyPopUp());
            };

            async void Next_Clicked(object sender, EventArgs args)
            {

                await Navigation.PushModalAsync(new NavigationPage(new HomePage()));

            }

        }
    }
}
