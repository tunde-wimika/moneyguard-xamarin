using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.AppCompat.App;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Snackbar;
using Wimika.MoneyGuard.Core.Android;
using System.Threading.Tasks;
using Android.Widget;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Wimika.MoneyGuard.Core.Types;
using static System.Net.Mime.MediaTypeNames;
using Wimika.MoneyGuard.Application;
using System.Linq;
using System.Collections.Generic;
using Xamarin.Essentials;

namespace AndroidTestApp
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private TextView text; 

        private const int WIMIKA_XAMARIN_BANK = 101;

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            //await RequestLocationPermission();

            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            text = (TextView)FindViewById(Resource.Id.textViewWarning);

            var button = FindViewById(Resource.Id.buttonProceed);
            button.Click += ProceedClick;
             
            var startupRisk = await MoneyGuardSdk.Startup(this); 
            if(startupRisk.MoneyGuardActive)
            {
               var issues = startupRisk.Risks.Where(r => r.Status != RiskStatus.RISK_STATUS_SAFE);
                
                //Assess prelaunch risk
                switch (startupRisk.PreLaunchVerdict.Decision)
                {
                    case PreLaunchDecision.Launch:
                        text.Text = "proceed to launch app";
                        break;
                    case PreLaunchDecision.DoNotLaunch:
                        DisplayDoNotLaunchMessages(issues);
                        break;
                    case PreLaunchDecision.LaunchWithWarning:
                        DisplayWarningMessages(issues);
                        break;
                    case PreLaunchDecision.LaunchWith2FA:
                        break;
                }
            }
            else
            {
                text.Text = "Moneyguard not active";
            }
        }

        private void DisplayWarningMessages(IEnumerable<SpecificRisk> issues)
        {
            foreach (var issue in issues)
            {
                if (issue.Name == SpecificRisk.SPECIFIC_RISK_DEVICE_SECURITY_MISCONFIGURATION_USB_DEBUGGING_NAME)
                {
                    ShowAlertDialog("Launch Warning",
                    "USB debugging is enabled on your device. Your login credentials may be compromised.",
                    "Disable", "Proceed to launch anyway");
                }

                if (issue.Name == SpecificRisk.SPECIFIC_RISK_NETWORK_WIFI_ENCRYPTION_NAME ||
                    issue.Name == SpecificRisk.SPECIFIC_RISK_NETWORK_WIFI_PASSWORD_PROTECTION_NAME)
                {
                    ShowAlertDialog("Launch Warning",
                    "Unsecured WIFI detected. Your digital banking activities may be compromised.",
                    "Disconnect", "Proceed");
                }
            }
        }

        private void DisplayDoNotLaunchMessages(IEnumerable<SpecificRisk> issues)
        {
            foreach (var issue in issues)
            {
                if (issue.Name == SpecificRisk.SPECIFIC_RISK_DEVICE_ROOT_OR_JAILBREAK_NAME)
                {
                    ShowAlertDialog("Launch Warning",
                        "Device security is compromised. We strongly advise you not to log into your bank app.",
                        "Ok", "Continue to login");
                }

                if (issue.Name == SpecificRisk.SPECIFIC_RISK_NETWORK_DNS_SPOOFING_NAME)
                {
                    ShowAlertDialog("Launch Warning",
                        "Spoofing Detected. Your banking activities are at risk if you continue.",
                        "Ok", "Proceed to login");
                }

                if (issue.Name == SpecificRisk.SPECIFIC_RISK_DEVICE_SECURITY_MISCONFIGURATION_LOW_QUALITY_DEVICE_PASSWORD_NAME)
                {
                    ShowAlertDialog("Launch Warning",
                        "Your device doesn't have a password set. We recommend setting one for better security before logging into your bank app.",
                        "Ok", "Continue to login");
                }
            }
        }

        private async void ProceedClick(object sender, EventArgs eventArgs)
        {
            StartActivity(typeof(LogInActivity));
        }

        private void ShowAlertDialog(string title, string message, string btn1Text = "Ok", string btn2Text = null)
        {
            Android.App.AlertDialog.Builder dialog = new Android.App.AlertDialog.Builder(this);
            Android.App.AlertDialog alert = dialog.Create();
            alert.SetTitle(title);
            alert.SetMessage(message);
            alert.SetButton(btn1Text, (c, ev) => {
                alert.Dismiss();
            });

            if (btn2Text != null)
            {
                alert.SetButton2(btn2Text, (c, ev) =>
                {
                    alert.Dismiss();
                });
            }

            alert.Show();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }


        public async Task RequestLocationPermission()
        {
            var locationPermissionStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (locationPermissionStatus == PermissionStatus.Granted)
            {
                // Permission granted, proceed with location operations
            }
            else if (locationPermissionStatus == PermissionStatus.Denied)
            {
                // Permission denied, handle accordingly
               
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
	}
}
