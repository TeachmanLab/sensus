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
using Xamarin.Forms;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup;
using Rg.Plugins.Popup.Animations;
using Rg.Plugins.Popup.Services;
using System.Reflection;
using Sensus.UI.Inputs;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Sensus.Context;
using System.Threading.Tasks;
using Sensus.Authentication;
using System.Net;
using Sensus.Notifications;
using System.Text;
using Sensus.Probes.User.Scripts;

namespace Sensus.UI.MindTrails
{
    public class AddStudyPopUp : PopupPage
    {
        StackLayout _frameLayout;

        public AddStudyPopUp()
        {
            BackgroundColor = Color.FromHex("D9FFFFFF");


            _frameLayout = new StackLayout
            {
                VerticalOptions = LayoutOptions.Center
            };


            Frame blueFrame = new Frame
            {
                BackgroundColor = Color.FromHex("166DA3"),
                Margin = 20,
                CornerRadius = 10,
                IsClippedToBounds = true,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            _frameLayout.Children.Add(blueFrame);

            Content = blueFrame;

            StackLayout blueLayout = new StackLayout
            {
                Spacing = 0
            };

            blueFrame.Content = blueLayout;

            Button fromQR = new Button
            {
                Text = "From QR Code",
                Margin = new Thickness(10, 25, 10, 10),
                TextColor = Color.Black, // CHANGE white
                BackgroundColor = Color.FromHex("B5E7FA"), // CHANGE 166DA3 
                FontFamily = "Source Sans Pro",
                FontSize = 20,
                FontAttributes = FontAttributes.Bold,
                CornerRadius = 8,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.End, // Start and Expand 
                WidthRequest = 180
            };

            Button fromURL = new Button
            {
                Text = "From URL",
                Margin = new Thickness(10, 25, 10, 10),
                TextColor = Color.Black, // CHANGE white
                BackgroundColor = Color.FromHex("B5E7FA"), // CHANGE 166DA3 
                FontFamily = "Source Sans Pro",
                FontSize = 20,
                FontAttributes = FontAttributes.Bold,
                CornerRadius = 8,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.End, // Start and Expand 
                WidthRequest = 180
            };

            fromURL.Clicked += FromURL_Clicked; 



            async void FromURL_Clicked(object sender, EventArgs e) // is this not being called? 
            {
                await PopupNavigation.Instance.PopAsync(true);

                string url = null;
                Input input = await SensusServiceHelper.Get().PromptForInputAsync("Download Study", new SingleLineTextInput("Study URL:", Keyboard.Url), null, true, null, null, null, null, false);
                url = input?.Value?.ToString();
                Console.WriteLine("URL IS: ");
                Console.WriteLine(url);

                // GOT TO HERE! 


                if (url != null)
                {
                    Protocol protocol = null;
                    Exception loadException = null;

                    // handle managed studies...handshake with authentication service.
                    if (url.StartsWith(Protocol.MANAGED_URL_STRING)) // prefix defines how it should be loaded 
                    {
                        Console.WriteLine("Managed");
                        ProgressPage loadProgressPage = null;

                        try
                        {
                            Tuple<string, string> baseUrlParticipantId = ParseManagedProtocolURL(url);

                            AuthenticationService authenticationService = new AuthenticationService(baseUrlParticipantId.Item1);

                            // get account and credentials. this can take a while, so show the user something fun to look at.
                            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                            loadProgressPage = new ProgressPage("Configuring study. Please wait...", cancellationTokenSource);
                            await loadProgressPage.DisplayAsync(Navigation);

                            await loadProgressPage.SetProgressAsync(0, "creating account");
                            Account account = await authenticationService.CreateAccountAsync(baseUrlParticipantId.Item2);
                            cancellationTokenSource.Token.ThrowIfCancellationRequested();

                            await loadProgressPage.SetProgressAsync(0.3, "getting credentials");
                            AmazonS3Credentials credentials = await authenticationService.GetCredentialsAsync();
                            cancellationTokenSource.Token.ThrowIfCancellationRequested();

                            await loadProgressPage.SetProgressAsync(0.6, "downloading study");
                            protocol = await Protocol.DeserializeAsync(new Uri(credentials.ProtocolURL), true, credentials);
                            await loadProgressPage.SetProgressAsync(1, null);

                            // don't throw for cancellation here as doing so will leave the protocol partially configured. if 
                            // the download succeeds, ensure that the properties get set below before throwing any exceptions.
                            protocol.ParticipantId = account.ParticipantId;
                            protocol.AuthenticationService = authenticationService;

                            // make sure protocol has the id that we expect
                            if (protocol.Id != credentials.ProtocolId)
                            {
                                throw new Exception("The identifier of the study does not match that of the credentials.");
                            }

                            // deserializing file + adding to Protocol 
                        }
                        catch (Exception ex)
                        {
                            loadException = ex;
                        }
                        finally
                        {
                            // ensure the progress page is closed
                            await (loadProgressPage?.CloseAsync() ?? Task.CompletedTask);
                        }
                    }
                    // handle unmanaged studies...direct download from URL.
                    else
                    {
                        Console.WriteLine("unmanaged");

                        try
                        {
                            protocol = await MindTrailsProtocol.DeserializeAsync(new Uri(url), true); // problem here 
                            Console.WriteLine("protocol tried");
                            Console.WriteLine(protocol == null);
                            // create a new function to deserialize JSON file --> using similar method 
                        }
                        catch (Exception ex)
                        {
                            loadException = ex;
                            Console.WriteLine("exception!");

                        }
                    }

                    // show load exception to user
                    if (loadException != null)
                    {
                        await SensusServiceHelper.Get().FlashNotificationAsync("Failed to get study:  " + loadException.Message);
                        protocol = null;
                    }

                    // start protocol if we have one
                    if (protocol != null)
                    {
                        // save app state to hang on to protocol, authentication information, etc.
                        await SensusServiceHelper.Get().SaveAsync();

                        // show the protocol to the user and start
                        await Protocol.DisplayAndStartAsync(protocol);
                    }
                }
            }


            Button back = new Button
            {
                Text = "Back",
                TextColor = Color.White,
                BackgroundColor = Color.Transparent
            };

            back.Clicked += back_Clicked;

            async void back_Clicked(object sender, EventArgs args)
            {
                await PopupNavigation.Instance.PopAsync(true);
            };

            async void onModuleSelected(object sender, EventArgs args)
            {
                //await Navigation.PushModalAsync(new NavigationPage(new RateDomain(domain))); // worked

                await PopupNavigation.Instance.PopAsync(true);

            }

            blueLayout.Children.Add(fromQR);
            blueLayout.Children.Add(fromURL);
            blueLayout.Children.Add(back);


        }

        private Tuple<string, string> ParseManagedProtocolURL(string url)
        {
            Console.WriteLine("GOT TO PARSE MANAGED"); 

            // should have the following parts (participant is optional but the last colon is still required):  managed:BASEURL:PARTICIPANT_ID
            int firstColon = url.IndexOf(':');
            int lastColon = url.LastIndexOf(':');

            Console.WriteLine("GOT TO PARSE MANAGED 2");

            // managed:BASEURL:PARTICIPANT_ID

            if (firstColon == lastColon)
            {
                Console.WriteLine("GOT TO PARSE MANAGED 3");
                // HERE IS WHERE IT IS STOPPING. THERE ARE NO COLONS
                // but the same thing is happening for other one 
                
                throw new Exception("Invalid study URL format.");
            }
            Console.WriteLine("GOT TO PARSE MANAGED 4");

            string baseUrl = url.Substring(firstColon + 1, lastColon - firstColon - 1);

            Console.WriteLine("GOT TO PARSE MANAGED 5");

            // get participant id if one follows the last colon
            string participantId = null;
            if (lastColon < url.Length - 1)
            {
                participantId = url.Substring(lastColon + 1);
                Console.WriteLine("PARTICIPANT ID: ");
                Console.WriteLine(participantId);


            }
            Console.WriteLine("GOT TO PARSE MANAGED 6");

            return new Tuple<string, string>(baseUrl, participantId);
        }
    }
}
