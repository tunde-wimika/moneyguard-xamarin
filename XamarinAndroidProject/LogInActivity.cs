﻿using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Com.Wimika.Moneyguardcore.Biometrics.Typing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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


                var response = await new LoginService().Session(usernameEditText.Text, passwordEditText.Text);

                try
                {
                    var session = await MoneyGuardSdk.Register(this, WIMIKA_XAMARIN_BANK, response.Data.SessionId);
                    Console.WriteLine("InstallationId -> " + session.InstallationId);
                    Console.WriteLine("SessionId -> " + session.SessionId);
                    SessionHolder.Session = session;
                }
                catch (Exception ex)
                {
                    Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
                    Console.WriteLine(ex.ToString());
                    return;
                }



            var typingProfileMatchingResult = await SessionHolder.Session.TypingProfileMatcher.MatchTypingProfile(recorder);
            var message = "Not Enrolled";
            bool notMatched = false;

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

            Toast.MakeText(
                this,
                message,
                ToastLength.Long
                ).Show();


            if (notMatched)
            {
                return;
            }

            StartActivity(typeof(ChoosingActivity));
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