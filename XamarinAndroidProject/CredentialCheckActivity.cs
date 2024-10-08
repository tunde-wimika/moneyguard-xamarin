using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Wimika.MoneyGuard.Core.Types;

namespace AndroidTestApp
{
    [Activity(Label = "CredentialCheckActivity")]
    public class CredentialCheckActivity : Activity
    {
        private EditText usernameEditText;
        private EditText passwordEditText;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.credential_check);



            var buttonCredentialCheck = FindViewById(Resource.Id.buttonCheckCredential);
            buttonCredentialCheck.Click += CredentialCheckClick;

            var buttonCredentialGoback = FindViewById(Resource.Id.buttonCredentialGoback);
            buttonCredentialGoback.Click += GoBackClick;

            usernameEditText = FindViewById<EditText>(Resource.Id.editTextUsername);
            passwordEditText = FindViewById<EditText>(Resource.Id.editTextPassword);
        }

        public static string ComputeSha256Hash(string value)
        {
            var sb = new StringBuilder();

            using (var hash = SHA256Managed.Create())
            {
                var enc = Encoding.UTF8;
                var result = hash.ComputeHash(enc.GetBytes(value));

                foreach (var b in result)
                    sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }
        private async void CredentialCheckClick(object sender, EventArgs eventArgs)
        {
            if(passwordEditText.Text.Length < 4)
            {
                Toast.MakeText(this,"Password must be at least 4 Characters long", ToastLength.Long).Show();
                return;
            }
            var scanResult = await SessionHolder.Session.CheckCredential(
                new Wimika.MoneyGuard.Core.Types.Credential
                {
                    Username = usernameEditText.Text, 
                     LastThreePasswordCharactersHash = ComputeSha256Hash(passwordEditText.Text.Substring(passwordEditText.Text.Length - 3)),
                    //PasswordFragmentLength = StartingCharactersLength.FOUR,
                    Domain = "wimika.ng",
                    HashAlgorithm = Wimika.MoneyGuard.Core.Types.HashAlgorithm.SHA256,
                    //PasswordStartingCharactersHash = ComputeSha256Hash(passwordEditText.Text.Substring(0, StartingCharactersLength.FOUR.Length))
                }); 

         
            Toast.MakeText(this, "Credential is " + SessionHolder.StatusAsString(scanResult.Status), ToastLength.Long).Show();      
        }

        private async void GoBackClick(object sender, EventArgs eventArgs)
        {
            StartActivity(typeof(ChoosingActivity));
        }
    }
}