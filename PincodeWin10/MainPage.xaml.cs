/**
* Copyright 2016 IBM Corp.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Newtonsoft.Json.Linq;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Worklight;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PincodeWin10
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static MainPage _this;
        PinCodeChallengeHandler pinCodeChallengeHandler;

        public MainPage()
        {
            this.InitializeComponent();
            _this = this;
            pinCodeChallengeHandler = new PinCodeChallengeHandler("PinCodeAttempts");
        }

        private async void GetBalance_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                pinCodeChallengeHandler = (PinCodeChallengeHandler)getChallengeHandler();
                pinCodeChallengeHandler.SetShouldSubmitChallenge(false);
                pinCodeChallengeHandler.SetSubmitFailure(false);

                IWorklightClient _newClient = WorklightClient.CreateInstance();
                
                _newClient.RegisterChallengeHandler(pinCodeChallengeHandler);

                StringBuilder uriBuilder = new StringBuilder().Append("/adapters").Append("/ResourceAdapter").Append("/balance");

                Debug.WriteLine(new Uri(uriBuilder.ToString(), UriKind.Relative));

                WorklightResourceRequest rr = _newClient.ResourceRequest(new Uri(uriBuilder.ToString(), UriKind.Relative), "GET", "accessRestricted");

                WorklightResponse resp = await rr.Send();

                System.Diagnostics.Debug.WriteLine(resp.ResponseText);

                AddTextToConsole(resp.ResponseText);

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }
        }

        public void AddTextToConsole(String consoleText)
        {
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                 async () =>
                 {
                     MainPage._this.Console.Text = consoleText;

                 });
        }

        private void ClearConsole(object sender, DoubleTappedRoutedEventArgs e)
        {
            Console.Text = "";
        }

        private void ShowConsole(object sender, TappedRoutedEventArgs e)
        {
            MainPage._this.ConsolePanel.Visibility = Visibility.Visible;
            MainPage._this.ConsoleTab.Foreground = new SolidColorBrush(Colors.DodgerBlue);
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            JObject pinJSON = new JObject();
            pinJSON.Add("pin", pintext.Text);
            _this.pintext.Text = "";
            pinCodeChallengeHandler.challengeAnswer = pinJSON;
            PinCodeChallengeHandler.waitForPincode.Set();
            hideChallenge();
        }

        public async void showChallenge(WorklightResponse challenge)
        {
            String errorMsg = "";

            if (challenge.ResponseJSON["errorMsg"] != null)
            {
                if (challenge.ResponseJSON["errorMsg"].Type == JTokenType.Null)
                    errorMsg = "This data requires a PIN Code.\n";
            }
            
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                 async () =>
                 {
                     _this.HintText.Text = "";
                     _this.LoginGrid.Visibility = Visibility.Visible;
                     if (errorMsg != "")
                     {
                         _this.HintText.Text = errorMsg + "Remaining Attempts: " + challenge.ResponseJSON["remainingAttempts"];
                     }
                     else
                     {
                         _this.HintText.Text = challenge.ResponseJSON["errorMsg"] + "\n" + "Remaining Attempts: " + challenge.ResponseJSON["remainingAttempts"];
                     }

                     _this.GetBalance.IsEnabled = false;
                 });
        }

        public void hideChallenge()
        {
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                async () =>
                {
                    MainPage._this.LoginGrid.Visibility = Visibility.Collapsed;
                    MainPage._this.GetBalance.IsEnabled = true;
                });
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            hideChallenge();
            pinCodeChallengeHandler.SetSubmitFailure(true);
            pinCodeChallengeHandler.SetShouldSubmitChallenge(false);
            PinCodeChallengeHandler.waitForPincode.Set();
        }

        public ChallengeHandler getChallengeHandler()
        {
            return pinCodeChallengeHandler;
        }

        private void ShowInfo(object sender, TappedRoutedEventArgs e)
        {
            MainPage._this.ConsolePanel.Visibility = Visibility.Collapsed;
            MainPage._this.ConsoleTab.Foreground = new SolidColorBrush(Colors.Gray);
        }
    }
}
