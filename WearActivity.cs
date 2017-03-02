using System;
using Android.App;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Wearable;
using Android.OS;
using Android.Support.Wearable.Views;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Support.Wearable.Activity;
using Android.Content;

namespace Stylist.Wear
{
    public abstract class WearActivity : Activity,
        GestureDetector.IOnGestureListener,
        GoogleApiClient.IConnectionCallbacks,
        GoogleApiClient.IOnConnectionFailedListener,
        IDataApiDataListener,
        IMessageApiMessageListener
    {
        public string tag = "WearActivity";
        public GoogleApiClient client;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            BeforeInit();
            client = new GoogleApiClient.Builder(this)
                .AddApi(WearableClass.API)
                .AddConnectionCallbacks(this)
                .AddOnConnectionFailedListener(this)
                .Build();

            gestureDetector = new GestureDetector(this);

            AfterInit();
        }

        protected void ShowToastMessage(string message, ToastLength duration = ToastLength.Short)
        {
            Toast toast = Toast.MakeText(ApplicationContext, message, duration);
            toast.Show();
        }

        protected virtual void BeforeInit()
        {
            // Set tag Here
            // SetContentView Here
            // SetContentView(Resource.Layout.Main);
        }

        protected virtual void AfterInit()
        {
            // Set View Values Here
        }

        #region Swipe Events
        private GestureDetector gestureDetector;

        protected virtual void OnSwipeRight()
        {
            // Implement On Swipe Right Here
        }

        protected virtual void OnSwipeLeft()
        {
            // Implement On Swipe Left Here
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            gestureDetector.OnTouchEvent(e);
            return false;
        }

        public bool OnDown(MotionEvent e) { return true; }

        public bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {
            if (e2.GetX() > e1.GetX())
                OnSwipeRight();

            if (e2.GetX() < e1.GetX())
                OnSwipeLeft();

            return true;
        }

        public void OnLongPress(MotionEvent e) { }

        public bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY) { return true; }

        public void OnShowPress(MotionEvent e) { }

        public bool OnSingleTapUp(MotionEvent e) { return true; }
        #endregion

        #region Activity Events
        protected override void OnStart()
        {
            Log.Info(tag, "OnStart(): Google API client started");
            base.OnStart();
            if (!client.IsConnected)
                client.Connect();
        }

        protected override void OnResume()
        {
            Log.Info(tag, "OnResume(): Google API client resumed");
            base.OnResume();
            if (!client.IsConnected)
                client.Connect();
        }

        protected override void OnPause()
        {
            Log.Info(tag, "OnPause(): Google API client paused");
            DisconnectClientAsync();
            base.OnPause();
        }

        protected override void OnStop()
        {
            Log.Info(tag, "OnStop(): Google API client stopped");
            DisconnectClientAsync();
            base.OnStop();
        }
        #endregion

        #region Connection Events
        private void DisconnectClientAsync()
        {
            if (client != null && client.IsConnected)
            {
                WearableClass.DataApi.RemoveListenerAsync(client, this);
                WearableClass.MessageApi.RemoveListenerAsync(client, this);
                client.Disconnect();
            }
        }

        public virtual void OnConnected(Bundle connectionHint)
        {
            Log.Info(tag, "OnConnected(): Successfully connected to Google API client");
            WearableClass.DataApi.AddListenerAsync(client, this);
            WearableClass.MessageApi.AddListenerAsync(client, this);
        }

        public virtual void OnConnectionSuspended(int cause)
        {
            Log.Info(tag, "OnConnectionSuspended(): Connection to Google API clinet was suspended");
        }

        public virtual void OnConnectionFailed(ConnectionResult result)
        {
            Log.Info(tag, "OnConnectionFailed(): Failed to connect, with result: " + result);
        }

        public virtual void OnDataChanged(DataEventBuffer dataEvents)
        {
            Log.Info(tag, "Data changed: " + dataEvents);
            //ShowToastMessage("Updating...");
        }

        public virtual void OnMessageReceived(IMessageEvent messageEvent)
        {
            Log.Info(tag, "OnMessageReceived(): Message received: " + messageEvent);
        }
        #endregion
    }
}