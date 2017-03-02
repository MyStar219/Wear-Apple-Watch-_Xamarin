using Android.Content;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Support.Wearable.Views;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using static Android.Support.V7.Widget.RecyclerView;

namespace Stylist.Wear
{
    public class WearableAdapter : WearableListView.Adapter
    {
        private LayoutInflater inflater;
        private int color, transparent;

        public List<ServiceItem> Items { get; set; }

        public WearableAdapter(Context context, List<ServiceItem> items)
        {
            inflater = LayoutInflater.From(context);
            Items = items;
            color = new Color(ContextCompat.GetColor(context, Resource.Color.wl_green));
            transparent = Color.Transparent;
        }

        public override ViewHolder OnCreateViewHolder(ViewGroup viewGroup, int i)
        {
            return new ItemViewHolder(inflater.Inflate(Resource.Layout.ListItem, null));
        }

        public override void OnBindViewHolder(ViewHolder viewHolder, int position)
        {
            ServiceItem item = Items[position];
            ItemViewHolder itemViewHolder = (ItemViewHolder)viewHolder;
            itemViewHolder.CircledImageView.SetCircleColor(item.Status == ServiceStatuses.CHAIR ? color : transparent);
            itemViewHolder.ListTypeTextView.Text = item.ListTypeChar;
            itemViewHolder.DetailTextView.Text =
                string.Format("{0}\n{1}", item.ScheduleDate.ToShortTimeString(), item.FullName);
        }

        public override int ItemCount { get { return Items.Count(); } }

        private class ItemViewHolder : WearableListView.ViewHolder
        {
            public CircledImageView CircledImageView;
            public TextView ListTypeTextView;
            public TextView DetailTextView;

            public ItemViewHolder(View itemView) : base(itemView)
            {
                CircledImageView = itemView.FindViewById<CircledImageView>(Resource.Id.circle);
                ListTypeTextView = itemView.FindViewById<TextView>(Resource.Id.txtListItemListType);
                DetailTextView = itemView.FindViewById<TextView>(Resource.Id.txtListItemDetail);
            }
        }
    }
}