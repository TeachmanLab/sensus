﻿// Copyright 2014 The Rector & Visitors of the University of Virginia
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
using Sensus.Notifications;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Sensus.UI
{
    public class ScenarioPage : ContentPage
    {
        protected RelativeLayout _contentLayout;

        public ScenarioPage()
        {
            Title = "Scenario 1";

            NavigationPage.SetHasNavigationBar(this, false);

            _contentLayout = new RelativeLayout
            {
                BackgroundColor = Color.FromHex("E5E7ED"),
                Padding = new Thickness(20, 15),
            };

            Content = _contentLayout;

            Frame bannerFrame = new Frame
            {
                BackgroundColor = Color.FromHex("233367"),
                HeightRequest = 90,
                CornerRadius = 0
                // eventually add MTlogo + "Session 2" label 
            };

            RelativeLayout bannerLayout = new RelativeLayout();

            Image logoImage = new Image { Source = "Logo.png" };
            Label sessionNum = new Label
            {
                Text = "Session 2",
                TextColor = Color.White,
                FontFamily = "Source Sans Pro",
                FontAttributes = FontAttributes.Bold,
                FontSize = 30
            };
            bannerLayout.Children.Add(logoImage,
                xConstraint: Constraint.RelativeToParent((parent) =>
                    { return parent.Width * .01; }),
                yConstraint: Constraint.RelativeToParent((parent) =>
                    { return parent.Height * .3; }),
                widthConstraint: Constraint.RelativeToParent((parent) =>
                    { return parent.Width *.2; }));

            bannerLayout.Children.Add(sessionNum,
                xConstraint: Constraint.RelativeToView(logoImage,
                    (parent, sibling) => { return sibling.Width * 1.2; }),
                yConstraint: Constraint.RelativeToParent((parent) =>
                    { return parent.Height - 40; }));

            bannerFrame.Content = bannerLayout;

            Frame whiteFrame = new Frame
            {
                BackgroundColor = Color.White,
                HasShadow = false,
                BorderColor = Color.Gray,
                CornerRadius = 10,
                Padding = 0,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
            };


            _contentLayout.Children.Add(bannerFrame,
                heightConstraint: Constraint.RelativeToParent((parent) =>
                { return parent.Height * 0.16; }),
                widthConstraint: Constraint.RelativeToParent((parent) =>
                { return parent.Width; }));

            _contentLayout.Children.Add(whiteFrame,
                heightConstraint: Constraint.RelativeToParent(
                    (parent) => { return parent.Height * .75; }),
                widthConstraint: Constraint.RelativeToView(bannerFrame,
                    (parent, sibling) => { return sibling.Width *.8; }),
                xConstraint: Constraint.RelativeToParent(
                    (parent) => { return parent.Width * .1; }),
                yConstraint: Constraint.RelativeToView(bannerFrame,
                    (parent, sibling) => { return sibling.Height + 30; }));

            StackLayout frameLayout = new StackLayout
            {
                Padding = new Thickness(20, 15)
            };

            whiteFrame.Content = frameLayout; // set the content of a frame

            Grid headerGrid = new Grid
            {
                ColumnSpacing = 0,
                RowSpacing = 0,
                Padding = 0,
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
            Label scenarioNum = new Label {
                Text = "Scenario 1",
                Margin = new Thickness(10),
                TextColor = Color.FromHex("166DA3"),
                FontSize = 22,
                FontFamily = "Source Sans Pro",
                HorizontalTextAlignment = TextAlignment.Start
                // WidthRequest =1000

            };
            Image scenarioIcon = new Image {Source = "pencil.png", HeightRequest = 10 };

            headerGrid.Children.Add(scenarioIcon, 0, 0);

            headerGrid.Children.Add(scenarioNum, 1, 0); // column, row 
            
            frameLayout.Children.Add(headerGrid);

            Frame grayFrame = new Frame
            {
                BackgroundColor = Color.FromHex("F0ECEC"),
                HasShadow = false,
                Padding =0,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.Center,
                WidthRequest = 250,
                CornerRadius = 10
            };

            Label scenarioName = new Label
            {
                Text = "Writing a report",
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                FontSize = 25,
                TextColor = Color.Black,
                HeightRequest = 100
            };
            Image scenarioImage = new Image {
                Source = "Report.png",
                HeightRequest=200,
                Margin = new Thickness(0,20,0,0)};

            grayFrame.Content = scenarioName;
            frameLayout.Children.Add(grayFrame);
            frameLayout.Children.Add(scenarioImage);

            Button nextButton = new Button
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.EndAndExpand,
                Text = "Next",
                TextColor = Color.White,
                BackgroundColor = Color.FromHex("166DA3"),
                FontAttributes = FontAttributes.Bold,
                FontFamily = "Source Sans Pro",
                Margin = new Thickness(10),
                WidthRequest = 150,
                CornerRadius = 8,
                FontSize = 20
            };


            frameLayout.Children.Add(nextButton);

            ProgressBar sessionProgress = new ProgressBar
            {
                Progress = .2,
                ProgressColor = Color.FromHex("166DA3")
            };

            frameLayout.Children.Add(sessionProgress);
        }

    }
}
