using Android.App;
using Android.Content;
using Android.Gms.Wearable;
using Android.OS;
using Android.Runtime;
using Android.Util;
using System;

namespace Stylist.Wear
{
    [Activity(Label = "Stylist", MainLauncher = true, Icon = "@drawable/icon")]
    public class WearSplashActivity : WearActivity
    {
        private static int SPLASH_TIMEOUT = 2000;

        private Handler handler = new Handler();

        protected override void BeforeInit()
        {
            tag = "WearSplashActivity";
            SetContentView(Resource.Layout.Splash);
        }

        protected override void AfterInit()
        {
            if (!StylistConstants.STRICT_CHECK)
            {
                Action action = () =>
                {
                    Intent intent = new Intent(Application.Context, typeof(WearListActivity));
                    StartActivity(intent);
                    Finish();
                };

                handler.PostDelayed(action, SPLASH_TIMEOUT);
            }
        }

        protected override void OnSwipeRight()
        {
            Finish();
        }

        public override async void OnConnected(Bundle connectionHint)
        {
            base.OnConnected(connectionHint);
            var res = await WearableClass.MessageApi.SendMessageAsync(client, "node", StylistConstants.StatusPath, new byte[0]);
            Log.Info(tag, res.Status.ToString());
        }

        public override void OnDataChanged(DataEventBuffer dataEvents)
        {
            base.OnDataChanged(dataEvents);
            foreach (var ev in dataEvents)
            {
                var e = ((Java.Lang.Object)ev).JavaCast<IDataEvent>();
                if (e.Type == DataEvent.TypeChanged)
                {
                    String path = e.DataItem.Uri.Path;
                    if (path.StartsWith(StylistConstants.StatusPath + StylistConstants.AnswerPath, StringComparison.CurrentCulture))
                    {
                        DataMapItem dataMapItem = DataMapItem.FromDataItem(e.DataItem);
                        var statusDataMap = dataMapItem.DataMap.GetDataMap(StylistConstants.StatusKey);

                        bool loggedIn = statusDataMap.GetBoolean(StylistConstants.LoggedInKey);
                        bool isPro = statusDataMap.GetBoolean(StylistConstants.IsProKey);

                        if (StylistConstants.STRICT_CHECK)
                        {
                            if (!loggedIn)
                            {
                                var notLoggedInIntent = new Intent(Application.Context, typeof(NotAllowedActivity));
                                StartActivity(notLoggedInIntent);
                                Finish();
                                return;
                            }
                            else if (!isPro)
                            {
                                var NotProIntent = new Intent(Application.Context, typeof(NotAllowedActivity));
                                StartActivity(NotProIntent);
                                Finish();
                                return;
                            }
                        }

                        Intent intent = new Intent(Application.Context, typeof(WearListActivity));
                        StartActivity(intent);
                        Finish();
                    }
                    else
                    {
                        Log.Info(tag, "Unrecognized path: " + path);
                    }
                }
                else if (e.Type == DataEvent.TypeDeleted)
                {
                    Log.Info("Unknown data event type", "Type = " + e.Type);
                }
                else
                {
                    Log.Info("Unknown data event type", "Type = " + e.Type);
                }
            }
        }
    }
}