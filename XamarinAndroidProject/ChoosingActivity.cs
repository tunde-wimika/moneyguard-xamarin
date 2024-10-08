using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content;
using Java.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using Wimika.MoneyGuard.Application;
using Wimika.MoneyGuard.Application.Tools;
using Console = System.Console;
using Environment = System.Environment;
using File = System.IO.File;

namespace AndroidTestApp
{
    [Activity(Label = "ChoosingActivity")]
    public class ChoosingActivity : Activity
    {
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.choosing);



            var buttonCredentialCheck = FindViewById(Resource.Id.buttonCredentialCheck);
            buttonCredentialCheck.Click += CredentialCheckClick;
            //
            var buttonDebitCheck = FindViewById(Resource.Id.buttonDebitCheck);
            buttonDebitCheck.Click += DebitCheckClick;
            //
            var buttonGetRiskProfile = FindViewById(Resource.Id.buttonGetRiskProfile);
            buttonGetRiskProfile.Click += GetRiskProfileClick;

            var buttonTypingProfileMatching = FindViewById(Resource.Id.buttonTypingProfileMatching);
            buttonTypingProfileMatching.Click += TypingProfileMatchingClick;

            var installed = false;
            try
            {
                //installed = MoneyGuardApp.IsInstalled(this);
            }
            catch (Exception ex)
            {

            }

            var buttonInstallMoneyguard = (Button)FindViewById(Resource.Id.buttonInstallMoneyguard);
            buttonInstallMoneyguard.Visibility = !installed ? ViewStates.Visible : ViewStates.Invisible;
            buttonInstallMoneyguard.Click += InstallMoneyguardClick;
            // Create your application here
        }

        private async void InstallMoneyguardClick(object sender, EventArgs eventArgs)
        {
            //await Wimika.MoneyGuard.Application.MoneyGuardApp.Install();
        }

        private async void CredentialCheckClick(object sender, EventArgs eventArgs)
        {
            StartActivity(typeof(CredentialCheckActivity));
        }

        private async void GetRiskProfileClick(object sender, EventArgs eventArgs)
        {
            var result = await SessionHolder.Session.GetRiskProfile();
            Toast.MakeText(this, "Risk profile is ->" + SessionHolder.StatusAsString(result.Status), ToastLength.Long).Show();
        }

        private async void DebitCheckClick(object sender, EventArgs eventArgs)
        {
            StartActivity(typeof(DebitCheckActivity));
        }

        private async void TypingProfileMatchingClick(object sender, EventArgs eventArgs)
        {
            StartActivity(typeof (TypingProfileMatchingActivity));
        }
    }
}