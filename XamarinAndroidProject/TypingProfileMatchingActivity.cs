using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Com.Wimika.Moneyguardcore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wimika.MoneyGuard.Core.Types;

namespace AndroidTestApp
{
    [Activity(Label = "TypingProfileMatchingActivity")]
    public class TypingProfileMatchingActivity : Activity
    {
        private EditText editTextFragment;
        private TextView editFragment;
        private ITypingProfileRecorder recorder;
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);


            SetContentView(Resource.Layout.typing_profile_check);

            // Create your application here
            var buttonCheckTypingProfile = FindViewById(Resource.Id.buttonCheckTypingProfile);
            buttonCheckTypingProfile.Click += CheckTypingProfileClick;

            var buttonTypingGoback = FindViewById(Resource.Id.buttonTypingGoback);
            buttonTypingGoback.Click += GoBackClick;

            editTextFragment = FindViewById<EditText>(Resource.Id.editTextFragment);
            editFragment = FindViewById<TextView>(Resource.Id.textFragment);

            recorder = SessionHolder.Session.SessionTypingProfileRecorder;

            editFragment.Text = recorder.TypingFragment;
            
            editTextFragment.KeyPress += EditTextFragment_KeyPress;
            editTextFragment.AfterTextChanged += EditTextFragment_AfterTextChanged;
        }

        private void EditTextFragment_AfterTextChanged(object sender, Android.Text.AfterTextChangedEventArgs e)
        {
            recorder.OnTextChanged(editTextFragment.Text);
        }

        private void EditTextFragment_KeyPress(object sender, View.KeyEventArgs e)
        {
            recorder.OnKeydown();
            e.Handled = false;
        }

        private async void GoBackClick(object sender, EventArgs eventArgs)
        {
            recorder.Reset();

            if (SessionHolder.LoginAfterTypingProfileCheck)
            {
                StartActivity(typeof(LogInActivity));
            }else
            {

                StartActivity(typeof(ChoosingActivity));
            }
        }

        private async void CheckTypingProfileClick(object sender, EventArgs eventArgs)
        {

            var typingProfileMatchingResult = await SessionHolder.Session.TypingProfileMatcher.MatchTypingProfile( recorder);
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
            else if(typingProfileMatchingResult.HasOtherEnrollments)
            {
                message = "User Account has enrollment on other devices";
            }

            Toast.MakeText(
                this,
               $"{message} {typingProfileMatchingResult.Message}",
                ToastLength.Long
                ).Show();

            if(SessionHolder.LoginAfterTypingProfileCheck)
            {

                SessionHolder.LoginAfterTypingProfileCheck = false;

                if (notMatched)
                {
                    StartActivity(typeof(LogInActivity));
                }
                else
                {
                    StartActivity(typeof(ChoosingActivity));
                }
            }



        }
    }


}