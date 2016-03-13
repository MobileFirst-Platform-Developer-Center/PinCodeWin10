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
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Worklight;

namespace PincodeWin10
{
    public class PinCodeChallengeHandler : Worklight.ChallengeHandler
    {
        public JObject challengeAnswer { get; set; }
        private bool authSuccess = false;
        private bool shouldsubmitchallenge = false;
        private bool shouldsubmitfailure = false;
        private string Realm;

        public static ManualResetEvent waitForPincode = new ManualResetEvent(false);

        public PinCodeChallengeHandler(String securityCheck)
        {
            Realm = securityCheck;
        }

        public override JObject GetChallengeAnswer()
        {
            return this.challengeAnswer;
        }

        public override string GetRealm()
        {
            return Realm;
        }

        public override void HandleChallenge(WorklightResponse challenge)
        {

            waitForPincode.Reset();
            MainPage._this.showChallenge(challenge);
            shouldsubmitchallenge = true;
            waitForPincode.WaitOne();
        }

        public override bool ShouldSubmitFailure()
        {
            return shouldsubmitfailure;
        }

        public override void OnFailure(WorklightResponse response)
        {
            Debug.WriteLine(response.ResponseJSON);
        }

        public override void OnSuccess(WorklightResponse challenge)
        {
            Debug.WriteLine(challenge.ResponseJSON);
        }

        public override bool ShouldSubmitSuccess()
        {
            return authSuccess;
        }

        public override WorklightResponse GetSubmitFailureResponse()
        {
            JObject respJSON = new JObject();
            respJSON.Add("Respose", "Cancelled Request");

            WorklightResponse response = new WorklightResponse(false, "User cancelled the request", respJSON, "User cancelled the request", (int)HttpStatusCode.InternalServerError);
            return response;
        }

        public override bool IsCustomResponse(WorklightResponse response)
        {
            if (response == null || response.ResponseJSON == null || response.ResponseJSON["PinCodeAttempts"] == null)
            {
                return false;
            }

            return true;
        }

        public override bool ShouldSubmitChallengeAnswer()
        {
            return this.shouldsubmitchallenge;
        }


        public void SetShouldSubmitChallenge(bool shouldsubmitchallenge)
        {
            this.shouldsubmitchallenge = shouldsubmitchallenge;
        }

        public void SetSubmitFailure(bool shouldsubmitfailure)
        {
            this.shouldsubmitfailure = shouldsubmitfailure;
        }

    }
}