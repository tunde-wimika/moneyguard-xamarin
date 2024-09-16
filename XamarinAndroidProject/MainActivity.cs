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

            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            text = (TextView)FindViewById(Resource.Id.textViewWarning);

            var button = FindViewById(Resource.Id.buttonProceed);
            button.Click += ProceedClick;
             
            var startupRisk = await MoneyGuardSdk.Startup(this); 
            if(startupRisk.MoneyGuardActive)
            {
                var issueList = startupRisk.Risks.Where(r => r.Status != RiskStatus.RISK_STATUS_SAFE).Select(x => x.StatusSummary).ToList();
                //Assess prelaunch risk
                switch (startupRisk.PreLaunchVerdict.Decision)
                {
                    case PreLaunchDecision.Launch:
                        text.Text = "proceed to launch app";
                        break;
                    case PreLaunchDecision.DoNotLaunch:
                        text.Text = $"do not launch app";
                        break;
                    case PreLaunchDecision.LaunchWithWarning:
                        text.Text = $"launch app with warning";
                        break;
                    case PreLaunchDecision.LaunchWith2FA:
                        text.Text = $"Request 2FA before launching app";
                        break;
                }
                text.Text += System.Environment.NewLine + string.Join(",", issueList);
            }
            else
            {
                text.Text = "Moneyguard not active";
            }
        }

        private async void ProceedClick(object sender, EventArgs eventArgs)
        {
            StartActivity(typeof(LogInActivity));
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


        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
	}
}
