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
    [Activity(Label = "Enable Moneyguard", Theme = "@style/AppTheme.NoActionBar")]
    public class EnableMoneyguardActivity : Activity
	{
        MoneyGuardAppStatus moneyGuardAppStatus;
        protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.enable_moneyguard_activity);

            var btnPurchasePolicy = FindViewById<Button>(Resource.Id.btn_purchase_policy);
            btnPurchasePolicy.Click += PurchasePolicy;
            var btnInstallMoneyguard = FindViewById<Button>(Resource.Id.btn_install_moneyguard);
            btnInstallMoneyguard.Click += InstallMoneyGuard;

            string moneyGuardStatusStr = Intent.GetStringExtra("moneyGuardStatus");
            moneyGuardAppStatus = (MoneyGuardAppStatus)Enum.Parse(typeof(MoneyGuardAppStatus), moneyGuardStatusStr);
            switch (moneyGuardAppStatus)
            {
                case MoneyGuardAppStatus.ValidPolicyAppNotInstalled:
                    btnInstallMoneyguard.Visibility = ViewStates.Invisible;
                    break;
                case MoneyGuardAppStatus.NoPolicyAppInstalled:
                    btnPurchasePolicy.Visibility = ViewStates.Visible;
                    break;
            }
        }

        private void PurchasePolicy(object sender, EventArgs e)
        {
            //invoke existing purchase policy flow and then install moneyguard app
        }

        private async void InstallMoneyGuard(object sender, EventArgs e)
        {
            await MoneyGuardApp.Install();
        }
    }
}