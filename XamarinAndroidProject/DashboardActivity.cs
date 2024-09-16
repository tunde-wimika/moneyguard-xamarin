using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using System;
using Wimika.MoneyGuard.Application.REST.ResponseModels;
using Wimika.MoneyGuard.Application.Tools;

namespace AndroidTestApp
{
    [Activity(Label = "Dashboard", Theme = "@style/AppTheme.NoActionBar")]
    public class DashboardActivity : Activity
    {
        MoneyGuardAppStatus moneyGuardAppStatus;
        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.dashboard_layout);

            var btnEnableMoneyguard = FindViewById<Button>(Resource.Id.btn_enable_moneyguard);
            btnEnableMoneyguard.Click += EnableMoneyGuard; 
            var btnExploreMoneyguard = FindViewById<Button>(Resource.Id.btn_explore_moneyguard);
            btnExploreMoneyguard.Click += ExploreMoneyGuard;

            string moneyGuardStatusStr = Intent.GetStringExtra("moneyGuardStatus");
            moneyGuardAppStatus = (MoneyGuardAppStatus)Enum.Parse(typeof(MoneyGuardAppStatus), moneyGuardStatusStr);

            btnEnableMoneyguard.Visibility = moneyGuardAppStatus == MoneyGuardAppStatus.Active ? ViewStates.Invisible : ViewStates.Visible;
            btnExploreMoneyguard.Visibility = moneyGuardAppStatus == MoneyGuardAppStatus.Active ? ViewStates.Visible : ViewStates.Invisible;
        }

        private async void EnableMoneyGuard(object sender, EventArgs eventArgs)
        {
            Intent intent = new Intent(this, typeof(EnableMoneyguardActivity));
            intent.PutExtra("moneyguardStatus", moneyGuardAppStatus.ToString());
            StartActivity(intent);

            //await Wimika.MoneyGuard.Application.MoneyGuardApp.Install();
        }

        private async void ExploreMoneyGuard(object sender, EventArgs eventArgs)
        {
            StartActivity(typeof(ChoosingActivity));
        }
    }
}