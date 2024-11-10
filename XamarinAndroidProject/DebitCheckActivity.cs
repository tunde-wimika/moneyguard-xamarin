using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Wimika.MoneyGuard.Core.Types;

namespace AndroidTestApp
{
    [Activity(Label = "DebitCheckActivity")]
    public class DebitCheckActivity : Activity, ILocationListener
    {
        private EditText sourceAccount;
        private EditText destinationBank;
        private EditText destinationAccount;
        private EditText memo;
        private EditText amount;
        LocationManager locationManager;
        string locationProvider;
        Location _location = null;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.debit_check);

            var buttonCheckDebit = FindViewById(Resource.Id.buttonCheckDebit);
            buttonCheckDebit.Click += DebitCheckClick;

            var buttonDebitGoback = FindViewById(Resource.Id.buttonDebitGoback);
            buttonDebitGoback.Click += GoBackClick;

            amount = FindViewById<EditText>(Resource.Id.editTextAmount);
            memo = FindViewById<EditText>(Resource.Id.editTextMemo);
            destinationAccount = FindViewById<EditText>(Resource.Id.editTextDestinationAccount);
            destinationBank = FindViewById<EditText>(Resource.Id.editTextDestinationBank);
            sourceAccount = FindViewById<EditText>(Resource.Id.editTextSource);

            // Get the location manager
            locationManager = (LocationManager)GetSystemService(Context.LocationService);

            // Get the best location provider
            Criteria criteria = new Criteria();
            criteria.Accuracy = Accuracy.Coarse;
            locationProvider = locationManager.GetBestProvider(criteria, true);

            // Register the listener
            locationManager.RequestLocationUpdates(locationProvider, 0, 0, this);
        }

        public void OnLocationChanged(Location location)
        {
            if (location != null)
            {
                _location = location;
            }
        }

        public void OnProviderDisabled(string provider)
        {
        }

        public void OnProviderEnabled(string provider)
        {
        }

        public void OnStatusChanged(string provider, Availability status, Bundle extras)
        {
        }

        private async void DebitCheckClick(object sender, EventArgs eventArgs)
        {
            try
            {
                var location = GetLocation();
                var result = await SessionHolder.Session.CheckDebitTransaction(
                    new DebitTransaction
                    {
                        Amount = double.Parse(amount.Text),
                        SourceAccountNumber = sourceAccount.Text,
                        DestinationAccountNumber = destinationAccount.Text,
                        DestinationBank = destinationBank.Text,
                        Memo = memo.Text,
                        Location = new LatLng() { Latitude = location.Latitude, Longitude = location.Longitude }
                    }
                    );

                Toast.MakeText(this, "Debit Transaction status is " + SessionHolder.StatusAsString(result.Status), ToastLength.Long).Show();

                if (result.Status == RiskStatus.RISK_STATUS_WARN)
                {
                    var commaSeparatedRisks = 
                    string.Join(", ", result.Risks
                                      .Where(r => r.Status == RiskStatus.RISK_STATUS_WARN)
                                      .Select(r => r.StatusSummary));

                    ShowAlertDialog("Warning",
                        $"We have detected some threats that may put your transaction at risk, " +
                        $"please review and proceed with caution - {commaSeparatedRisks}",
                        "Proceed", null);
                }
                else if (result.Status == RiskStatus.RISK_STATUS_UNSAFE_CREDENTIALS)
                {
                    ShowAlertDialog("2FA Required",
                        $"We have detected that you logged in with compromised credentials, " +
                        $"a 2FA is required to proceed",
                        "Proceed", null);
                }
                else if (result.Status == RiskStatus.RISK_STATUS_UNSAFE_LOCATION)
                {
                    ShowAlertDialog("2FA Required",
                        $"We have detected that this transaction is happening in a suspicious location," +
                        $"a 2FA is required to proceed",
                        "Proceed", null);
                }
                else if (result.Status == RiskStatus.RISK_STATUS_UNSAFE)
                {
                    var commaSeparatedRisks =
                   string.Join(", ", result.Risks
                                     .Where(r => r.Status == RiskStatus.RISK_STATUS_UNSAFE)
                                     .Select(r => r.StatusSummary));

                    ShowAlertDialog("2FA Required",
                        $"We have detected some threats that may put your transaction at risk, " +
                        $"a 2FA is required to proceed - {commaSeparatedRisks}",
                        "Proceed", null);
                }
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, "ERROR: " + ex.ToString(), ToastLength.Long).Show();
            }
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


        private async void GoBackClick(object sender, EventArgs eventArgs)
        {
            StartActivity(typeof(ChoosingActivity));
        }

        private Location GetLocation()
        {
            while (_location == null)            
                Thread.Sleep(1000);
            
            return _location;
        }
    }
}