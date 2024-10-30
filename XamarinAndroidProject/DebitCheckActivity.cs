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