using Android.App;
using Android.Content;
using Android.Gms.Wearable;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Stylist.Wear
{
    [Activity(Label = "Stylist", LaunchMode = Android.Content.PM.LaunchMode.SingleInstance)]
    public class WearDetailActivity : WearActivity
    {
        private ServiceItem item;

        protected override void BeforeInit()
        {
            tag = "WearDetailActivity";
            SetContentView(Resource.Layout.Detail);

            item = JsonConvert.DeserializeObject<ServiceItem>(Intent.GetStringExtra("data"));

            TextView listTypeTextView = FindViewById<TextView>(Resource.Id.txtDetailListType);
            listTypeTextView.Text = item.ListTypeChar;

            TextView fullNameTextView = FindViewById<TextView>(Resource.Id.txtDetailFullName);
            fullNameTextView.Text = item.FullName;

            TextView statusTextView = FindViewById<TextView>(Resource.Id.txtDetailStatus);
            statusTextView.Text = item.Status.ToString();

            TextView scheduleDateTextView = FindViewById<TextView>(Resource.Id.txtDetailScheduleDate);
            scheduleDateTextView.Text = item.ScheduleDate.ToShortTimeString();

            Button startButton = FindViewById<Button>(Resource.Id.btnDetailStart);
            startButton.Click += StartButton_Click;

            Button cancelButton = FindViewById<Button>(Resource.Id.btnDetailCancel);
            cancelButton.Click += CancelButton_Click;
        }

        protected override void OnSwipeRight()
        {
            FinishActivity();
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            item.Status = ServiceStatuses.CHAIR;
            item.StartServiceDate = DateTime.Now;
            UpdateItem();

            Intent intent = new Intent(this, typeof(WearServiceActivity));
            intent.PutExtra("data", JsonConvert.SerializeObject(item));
            StartActivityForResult(intent, 1);
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            item.Status = ServiceStatuses.CANCEL;
            UpdateItem();
            FinishActivity();
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == 1)
            {
                string json = data.GetStringExtra("data");
                item = JsonConvert.DeserializeObject<ServiceItem>(json);

                // finish and go back to list
                FinishActivity();
            }
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