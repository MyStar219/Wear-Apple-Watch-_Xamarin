using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.Wearable.Views;
using Android.Util;
using Android.Support.Wearable.Activity;

namespace Stylist.Wear
{
    [Activity(Label = "Stylist")]
    public class WearConfirmationActivity : WearActivity, DelayedConfirmationView.IDelayedConfirmationListener
    {
        private DelayedConfirmationView delayedConfirmation;
        private TextView delayedConfirmationTextView;

        protected override void BeforeInit()
        {
            tag = "WearConfirmationActivity";
            SetContentView(Resource.Layout.Confirmation);
        }

        protected override void AfterInit()
        {
            string message = Intent.GetStringExtra("message");

            if (!string.IsNullOrWhiteSpace(message))
            {
                // Set confirmation text
                delayedConfirmationTextView = FindViewById<TextView>(Resource.Id.delayed_confirm_textview);
                delayedConfirmationTextView.Text = message;
            }

            delayedConfirmation = FindViewById<DelayedConfirmationView>(Resource.Id.delayed_confirm);
            delayedConfirmation.SetListener(this);
            delayedConfirmation.SetTotalTimeMs(5000);
            delayedConfirmation.Start();
        }

        protected void StartDelayedConfirmation()
        {
            if (delayedConfirmation == null)
                return;

            Log.Info(tag, "Starting Confirmation Timer");
            delayedConfirmation.Start();
        }

        public virtual void OnTimerFinished(View view)
        {
            Log.Info(tag, "Confirmation Timer Finished");
            Intent intent = new Intent(this, typeof(ConfirmationActivity));
            intent.PutExtra(ConfirmationActivity.ExtraAnimationType, ConfirmationActivity.SuccessAnimation);
            intent.PutExtra(ConfirmationActivity.ExtraMessage, "Success");
            StartActivity(intent);

            SetResult(Result.Ok);
            Finish();
        }

        public virtual void OnTimerSelected(View view)
        {
            Log.Info(tag, "Confirmation Timer Aborted");
            delayedConfirmation.Reset();
            SetResult(Result.Canceled);
            Finish();
        }
    }
}