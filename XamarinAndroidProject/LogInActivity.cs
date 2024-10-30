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
using Wimika.MoneyGuard.Application.REST.ResponseModels;
using Wimika.MoneyGuard.Application.Tools;
using Wimika.MoneyGuard.Core.Android;
using Wimika.MoneyGuard.Core.Types;

namespace AndroidTestApp
{
    [Activity(Label = "LogInActivity")]
    public class LogInActivity : Activity
    {
        private EditText usernameEditText;
        private EditText passwordEditText;

        private const int WIMIKA_XAMARIN_BANK = 101;

        Wimika.MoneyGuard.Core.Types.ITypingProfileRecorder recorder;

        protected async override void OnCreate(Bundle savedInstanceState)
        {

            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.login);

            var button = FindViewById(Resource.Id.buttonLogin);
            button.Click += LoginClick;

            usernameEditText = FindViewById<EditText>(Resource.Id.editTextUsername);
            passwordEditText = FindViewById<EditText>(Resource.Id.editTextPassword);

            recorder = MoneyGuardSdk.CreateTypingProfileRecorder(this, TypingProfileParameters.CreateForFixeduserText("email"));

            usernameEditText.KeyPress += UsernameEditText_KeyPress; ;
            usernameEditText.AfterTextChanged += UsernameEditText_AfterTextChanged; ;

        }

        private void UsernameEditText_AfterTextChanged(object sender, Android.Text.AfterTextChangedEventArgs e)
        {
            recorder.OnTextChanged(usernameEditText.Text);
        }

        private void UsernameEditText_KeyPress(object sender, View.KeyEventArgs e)
        {
            recorder.OnKeydown();
            e.Handled = false;
        }

        private async void LoginClick(object sender, EventArgs eventArgs)
        {
            var response = await new LoginService().Session(usernameEditText.Text, passwordEditText.Text); // partner bank login

            if (response.IsError)
            {
                Toast.MakeText(
                            this,
                            "Invalid email",
                            ToastLength.Long
                            ).Show();
                return;
            }

            var moneyGuardStatus = await MoneyGuardApp.Status(this, response.Data.SessionId);

            try
            {
                if (moneyGuardStatus == MoneyGuardAppStatus.Active)
                {
                    var session = await MoneyGuardSdk.Register(this, WIMIKA_XAMARIN_BANK, response.Data.SessionId);
                    Console.WriteLine("InstallationId -> " + session.InstallationId);
                    Console.WriteLine("SessionId -> " + session.SessionId);
                    SessionHolder.Session = session;

                    if (session != null) //if we get session, go ahead with typing profile check
                    {
                        var scanResult = await SessionHolder.Session.CheckCredential(
                           new Wimika.MoneyGuard.Core.Types.Credential
                           {
                               Username = usernameEditText.Text,
                               LastThreePasswordCharactersHash = ComputeSha256Hash(passwordEditText.Text.Substring(passwordEditText.Text.Length - 3)),
                               Domain = "wimika.ng",
                               HashAlgorithm = Wimika.MoneyGuard.Core.Types.HashAlgorithm.SHA256,
                           });

                        Toast.MakeText(this, "Credential is " + SessionHolder.StatusAsString(scanResult.Status), ToastLength.Long).Show();

                        //Toast.MakeText(this, "Credential is " + SessionHolder.StatusAsString(scanResult.Status), ToastLength.Long).Show();

                        var typingProfileMatchingResult = await SessionHolder.Session.TypingProfileMatcher.MatchTypingProfile(recorder);
                        var message = "Not Enrolled";
                        bool notMatched = false;

                        if (typingProfileMatchingResult != null)
                        {
                            if (typingProfileMatchingResult.IsEnrolledOnThisDevice)
                            {
                                if (typingProfileMatchingResult.Matched)
                                {
                                    if (typingProfileMatchingResult.HighConfidence)
                                    {
                                        message = "Enrolled and Matched With High Confidence";
                                    }
                                    else
                                    {
                                        message = "Enrolled and Matched With Low Confidence";
                                    }
                                }
                                else
                                {
                                    //typing profile did not match, do not proceed
                                    message = "Not matched";
                                    notMatched = true;
                                }
                            }
                            else if (typingProfileMatchingResult.HasOtherEnrollments)
                            {
                                message = "User Account has enrollment on other devices";
                            }
                        }
                        else
                        {
                            Toast.MakeText(
                           this,
                           "No typingProfileMatchingResult",
                           ToastLength.Long
                           ).Show();
                        }

                        Toast.MakeText(
                            this,
                            message,
                            ToastLength.Long
                            ).Show();

                        if (notMatched)
                        {
                            //return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, ex.ToString(), ToastLength.Long).Show();
                Console.WriteLine(ex.ToString());
                return;
            }

            //StartActivity(typeof(ChoosingActivity));
            Intent intent = new Intent(this, typeof(DashboardActivity));
            intent.PutExtra("moneyguardStatus", moneyGuardStatus.ToString());
            StartActivity(intent);
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
    }


}