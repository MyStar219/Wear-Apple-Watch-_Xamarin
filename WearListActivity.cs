using Android.App;
using Android.Content;
using Android.Gms.Wearable;
using Android.Runtime;
using Android.Support.Wearable.Views;
using Android.Util;
using Android.Widget;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Android.OS;
using Android.Views;
using Android.Support.Wearable.Activity;

namespace Stylist.Wear
{
    [Activity(Label = "Stylist", LaunchMode = Android.Content.PM.LaunchMode.SingleInstance)]
    public class WearListActivity : WearActivity
    {
        private TextView header;
        private WearableListView wearableListView;
        private WearableAdapter wearableAdapter;
		//private IList<DataMap> dataMapList = new List<DataMap>();

        protected override void BeforeInit()
        {
            tag = "WearListActivity";
            SetContentView(Resource.Layout.Main);
        }

        protected override void AfterInit()
        {
            header = FindViewById<TextView>(Resource.Id.header);
            wearableListView = FindViewById<WearableListView>(Resource.Id.wearable_List);
            wearableAdapter = new WearableAdapter(this, new List<ServiceItem>());
            wearableListView.SetAdapter(wearableAdapter);
            wearableListView.Click += WearableListView_Click;
            wearableListView.TopEmptyRegionClick += WearableListView_TopEmptyRegionClick;
            wearableListView.AbsoluteScrollChange += WearableListView_AbsoluteScrollChange;

            // Request data from mobile
            RequestDataFromMobile();
        }

        private void WearableListView_Click(object sender, WearableListView.ClickEventArgs e)
        {
            ServiceItem selected = wearableAdapter.Items[e.P0.AdapterPosition];
            Type activityType = selected.Status == ServiceStatuses.CHAIR ? typeof(WearServiceActivity) : typeof(WearDetailActivity);
            Intent intent = new Intent(this, activityType);
            intent.PutExtra("data", JsonConvert.SerializeObject(selected));
            StartActivityForResult(intent, 1);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            switch (requestCode)
            {
                case 1:
                    string json = data.GetStringExtra("data");
                    ServiceItem resultItem = JsonConvert.DeserializeObject<ServiceItem>(json);
                    //UpdateDisplay(resultItem);
                    RequestDataFromMobile();
                    break;

                case 2: // Confirmation Add
                    if (resultCode == Result.Ok)
                        AddNewItemToList();
                    break;
            }
        }

        private void UpdateDisplay(ServiceItem changedItem)
        {
            Log.Info(tag, "UpdateDisplay(): change to Item " + changedItem.AppId);

            if (changedItem == null)
                return;

            Log.Info(tag, "ChangeItem: fullname-" + changedItem.FullName +
                                  " status" + changedItem.Status.ToString() +
                                  " start" + changedItem.StartServiceDate.ToString() +
                                  " end" + changedItem.EndServiceDate.ToString() +
                                  " start" + changedItem.IsDirty.ToString());

            int index = wearableAdapter.Items.FindIndex(i => i.AppId == changedItem.AppId);
            if (index == -1)
                return;

            Log.Info(tag, "Process ChangeItem: " + changedItem.AppId);

            RunOnUiThread(() =>
            {
                if (changedItem.Status == ServiceStatuses.CANCEL || changedItem.Status == ServiceStatuses.COMPLETE)
                {
                    wearableAdapter.Items.RemoveAt(index);
                }
                else
                {
                    wearableAdapter.Items[index] = changedItem;
                }

                wearableListView.GetAdapter().NotifyDataSetChanged();
            });

            Log.Info(tag, "UpdateDisplay(): exit");
        }

        private void WearableListView_TopEmptyRegionClick(object sender, EventArgs e) { }

        private void WearableListView_AbsoluteScrollChange(object sender, WearableListView.AbsoluteScrollChangeEventArgs e)
        {
            if (e.P0 > 0)
                header.SetY(-e.P0);
        }

        protected override void OnSwipeRight()
        {
            Finish();
        }

        protected override void OnSwipeLeft()
        {
            Log.Info(tag, "Swipe Left: Add new Item...");
            Intent intent = new Intent(this, typeof(WearConfirmationActivity));
            intent.PutExtra("message", "Adding new service...");
            StartActivityForResult(intent, 2);
        }

        public override void OnConnected(Bundle connectionHint)
        {
            base.OnConnected(connectionHint);
            RequestDataFromMobile();
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
                    Log.Info(tag, "DataEvent path: " + path);
                    if (path.StartsWith(StylistConstants.GetDataPath + StylistConstants.AnswerPath, StringComparison.CurrentCulture))
                    {
						CleanUpList();
                        DataMapItem dataMapItem = DataMapItem.FromDataItem(e.DataItem);
                        var dataMapList = dataMapItem.DataMap.GetDataMapArrayList(StylistConstants.ListKey);

                        Log.Info(tag, "dataMapList path: " + dataMapList.Count);

                        foreach (var d in dataMapList)
                        {
                            var changedItem = new ServiceItem
                            {
                                AppId = d.GetString(StylistConstants.AppIdKey),
                                FirstName = d.GetString(StylistConstants.FirstNameKey),
                                LastName = d.GetString(StylistConstants.LastNameKey),
                                ListType = (ServiceTypes)Enum.Parse(typeof(ServiceTypes), d.GetString(StylistConstants.ListTypeKey)),
                                Status = (ServiceStatuses)Enum.Parse(typeof(ServiceStatuses), d.GetString(StylistConstants.ListStatusKey)),
                                ScheduleDate = DateTime.Parse(d.GetString(StylistConstants.ScheduleDateKey)),
                                StartServiceDate = DateTime.Parse(d.GetString(StylistConstants.StartServiceDateKey)),
                                EndServiceDate = DateTime.Parse(d.GetString(StylistConstants.EndServiceDateKey)),
                                IsDirty = false,
                            };

                            Log.Info(tag, "add changeItem : " + changedItem.AppId);

                            RunOnUiThread(() =>
                            {
                                if (changedItem.Status == ServiceStatuses.READY || changedItem.Status == ServiceStatuses.CHAIR)
                                {
                                //    int index = wearableAdapter.Items.FindIndex(i => i.AppId == changedItem.AppId);

                                //    if (index == -1)
                                //    {
                                //        index = wearableAdapter.Items.FindIndex(i => i.AppId == d.GetString(StylistConstants.TempAppIdKey));
                                      wearableAdapter.Items.Add(changedItem);
                                    //}
                                    //else
                                    //{
                                    //    wearableAdapter.Items[index] = changedItem;
                                    //}
                                }
                            });
                        }
                    }
                    else if (path.StartsWith(StylistConstants.UpdateService, StringComparison.CurrentCulture))
                    {
                        DataMapItem dataMapItem = DataMapItem.FromDataItem(e.DataItem);
                        var data = dataMapItem.DataMap.GetDataMap(StylistConstants.ItemKey);

						Log.Info(tag, "data : " + data.GetString(StylistConstants.AppIdKey));
						var changedItem = new ServiceItem
						{
							AppId = data.GetString(StylistConstants.AppIdKey),
							FirstName = data.GetString(StylistConstants.FirstNameKey),
							LastName = data.GetString(StylistConstants.LastNameKey),
							ListType = (ServiceTypes)Enum.Parse(typeof(ServiceTypes), data.GetString(StylistConstants.ListTypeKey)),
							Status = (ServiceStatuses)Enum.Parse(typeof(ServiceStatuses), data.GetString(StylistConstants.ListStatusKey)),
							ScheduleDate = DateTime.Parse(data.GetString(StylistConstants.ScheduleDateKey)),
							StartServiceDate = DateTime.Parse(data.GetString(StylistConstants.StartServiceDateKey)),
							EndServiceDate = DateTime.Parse(data.GetString(StylistConstants.EndServiceDateKey)),
							IsDirty = false,
						};

						bool removeFromList = (changedItem.Status != ServiceStatuses.READY && changedItem.Status != ServiceStatuses.CHAIR);
						Log.Info(tag, "data remove from list: " + removeFromList);

                        RunOnUiThread(() =>
                        {
                            int index = wearableAdapter.Items.FindIndex(i => i.AppId == changedItem.AppId);

                            if (index == -1)
                            {
                                index = wearableAdapter.Items.FindIndex(i => i.AppId == data.GetString(StylistConstants.TempAppIdKey));
                            }

							if (index == -1 )
							{
								if (!removeFromList)
								{
									wearableAdapter.Items.Add(changedItem);
								}
							}
                            else
                            {
								if (removeFromList) {
									wearableAdapter.Items.RemoveAt(index);
								}
								else {
									wearableAdapter.Items[index] = changedItem;
								}
                            }
                        });
                    }
                }
            }

            wearableListView.GetAdapter().NotifyDataSetChanged();
        }

		private void CleanUpList()
		{
			foreach (var ev in wearableAdapter.Items)
			{
				if (ev.IsDirty)
				{
					SendDataToCompanion(ev);
				}
			}

			wearableAdapter.Items.Clear();
		}

        public void SendDataToCompanion(ServiceItem changedItem)
        {
            var request = PutDataMapRequest.Create(StylistConstants.GetDataPath + StylistConstants.UpdatePath).SetUrgent();
            DataMap itemMap = null;

            Log.Info(tag, "Create new itemMap");
            itemMap = new DataMap();

            itemMap.PutString(StylistConstants.AppIdKey, changedItem.AppId);
            itemMap.PutString(StylistConstants.FirstNameKey, changedItem.FirstName);
            itemMap.PutString(StylistConstants.LastNameKey, changedItem.LastName);
            itemMap.PutString(StylistConstants.ScheduleDateKey, changedItem.ScheduleDate.ToString());
            itemMap.PutString(StylistConstants.StartServiceDateKey, changedItem.StartServiceDate.ToString());
            itemMap.PutString(StylistConstants.EndServiceDateKey, changedItem.EndServiceDate.ToString());
            itemMap.PutString(StylistConstants.ListStatusKey, changedItem.Status.ToString());
            itemMap.PutString(StylistConstants.ListTypeKey, changedItem.ListType.ToString());
            itemMap.PutBoolean(StylistConstants.IsDirtyKey, changedItem.IsDirty);

            Log.Info(tag, "Send data ");

			var serviceMap = new List<DataMap>();
			serviceMap.Add(itemMap);

            request.DataMap.PutDataMapArrayList(StylistConstants.ListKey, serviceMap);
            WearableClass.DataApi.PutDataItem(client, request.AsPutDataRequest());
        }

        private void AddNewItemToList()
        {
            Log.Info(tag, "AddNewItemToList: add new item ");

            var addItem = new ServiceItem()
            {
                FirstName = "Customer-" + RandomGenerator.RandomString(10),
                ScheduleDate = DateTime.Now,
                EndServiceDate = DateTime.Now,
                IsDirty = true,
                LastName = "",
                ListType = ServiceTypes.WalkIn,
                Status = ServiceStatuses.READY
            };

            Log.Info(tag, "AddNewItemToList: item:  " + addItem.FirstName);

            wearableAdapter.Items.Insert(0, addItem);
            wearableListView.GetAdapter().NotifyDataSetChanged();
            SendDataToCompanion(addItem);

            RequestDataFromMobile();
        }

        private void RequestDataFromMobile()
        {
            Log.Info(tag, "RequestNewList");
            WearableClass.MessageApi.SendMessageAsync(client, "node",
                StylistConstants.GetDataPath, new byte[0]);
        }
    }
}