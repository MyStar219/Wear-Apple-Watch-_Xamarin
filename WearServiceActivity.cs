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
using System.Timers;
using Android.Gms.Wearable;
using Newtonsoft.Json;
using Android.Util;

namespace Stylist.Wear
{
    [Activity(Label = "Stylist", LaunchMode = Android.Content.PM.LaunchMode.SingleInstance)]
    public class WearServiceActivity : WearActivity
    {
        private ServiceItem item;
        private Timer timer;


        protected override void BeforeInit()
        {
            tag = "WearServiceActivity";
            SetContentView(Resource.Layout.Service);

            item = JsonConvert.DeserializeObject<ServiceItem>(Intent.GetStringExtra("data"));

            if (item.StartServiceDate == null)
                item.StartServiceDate = DateTime.Now;

            timer = new Timer(1000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            TextView listTypeTextView = FindViewById<TextView>(Resource.Id.txtServiceListType);
            listTypeTextView.Text = item.ListTypeChar;

            TextView fullNameTextView = FindViewById<TextView>(Resource.Id.txtServiceFullName);
            fullNameTextView.Text = item.FullName;

            TextView statusTextView = FindViewById<TextView>(Resource.Id.txtServiceStatus);
            statusTextView.Text = item.Status.ToString();

            Button completeButton = FindViewById<Button>(Resource.Id.btnServiceComplete);
            completeButton.Click += CompleteButton_Click;

            Button resetButton = FindViewById<Button>(Resource.Id.btnServiceReset);
            resetButton.Click += ResetButton_Click;

            Button cancelButton = FindViewById<Button>(Resource.Id.btnServiceCancel);
            cancelButton.Click += CancelButton_Click;
        }

        protected override void OnSwipeRight()
        {
            FinishActivity();
        }

        private void CompleteButton_Click(object sender, EventArgs e)
        {
            CloseService(ServiceStatuses.COMPLETE);
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            CloseService(ServiceStatuses.CANCEL);
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            if (timer == null || !timer.Enabled)
                return;

            item.StartServiceDate = DateTime.Now;
            UpdateItem();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            RunOnUiThread(() =>
            {
                TimeSpan elapsed = DateTime.Now - (DateTime)item.StartServiceDate;
                TextView serviceTimerTextView = FindViewById<TextView>(Resource.Id.txtServiceTimer);
                serviceTimerTextView.Text = elapsed.ToString(@"hh\:mm\:ss");
            });
        }
        
        private void CloseService(ServiceStatuses status)
        {
            if (timer == null || !timer.Enabled)
                return;

            timer.Stop();
            item.EndServiceDate = DateTime.Now;
            item.Status = status;
            UpdateItem();
            FinishActivity();
        }

        private void FinishActivity()
        {
            Intent intent = new Intent();
            intent.PutExtra("data", JsonConvert.SerializeObject(item));
            SetResult(Result.Ok, intent);
            Finish();
        }

        private void UpdateItem()
        {
            Log.Info(tag, "Update Item: ");
            DataMap itemMap = new DataMap();
            itemMap.PutString(StylistConstants.AppIdKey, item.AppId);
            itemMap.PutString(StylistConstants.FirstNameKey, item.FirstName);
            itemMap.PutString(StylistConstants.LastNameKey, item.LastName);
            itemMap.PutString(StylistConstants.ScheduleDateKey, item.ScheduleDate.ToString());
            itemMap.PutString(StylistConstants.StartServiceDateKey, item.StartServiceDate.ToString());
            itemMap.PutString(StylistConstants.EndServiceDateKey, item.EndServiceDate.ToString());
            itemMap.PutString(StylistConstants.ListStatusKey, item.Status.ToString());
            itemMap.PutString(StylistConstants.ListTypeKey, item.ListType.ToString());
            itemMap.PutBoolean(StylistConstants.IsDirtyKey, true);

            var serviceMap = new List<DataMap>();
            serviceMap.Add(itemMap);

            var request = PutDataMapRequest.Create(StylistConstants.GetDataPath + StylistConstants.UpdatePath).SetUrgent();

            Log.Info(tag, "Send data");
            request.DataMap.PutDataMapArrayList(StylistConstants.ListKey, serviceMap);
            WearableClass.DataApi.PutDataItem(client, request.AsPutDataRequest());
        }
    }
}