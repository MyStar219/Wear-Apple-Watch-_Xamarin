using Android.Content;
using Android.Graphics;
using Android.Support.Wearable.Views;
using Android.Util;
using Android.Widget;

namespace Stylist.Wear
{
    public class WearableListItemLayout : LinearLayout, WearableListView.IOnCenterProximityListener
    {
        private CircledImageView circle;
        private TextView txtListItemListType;
        private TextView txtListItemDetail;
        private float bigCircleRadius;
        private float smallCircleRadius;

        public WearableListItemLayout(Context context) : this(context, null) { }

        public WearableListItemLayout(Context context, IAttributeSet attrs) : this(context, attrs, 0) { }

        public WearableListItemLayout(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
            smallCircleRadius = Resources.GetDimensionPixelSize(Resource.Dimension.small_circle_radius);
            bigCircleRadius = Resources.GetDimensionPixelSize(Resource.Dimension.big_circle_radius);
        }

        protected override void OnFinishInflate()
        {
            base.OnFinishInflate();
            circle = FindViewById<CircledImageView>(Resource.Id.circle);
            txtListItemListType = FindViewById<TextView>(Resource.Id.txtListItemListType);
            txtListItemDetail = FindViewById<TextView>(Resource.Id.txtListItemDetail);
        }

        public void OnCenterPosition(bool animate)
        {
            circle.CircleRadius = bigCircleRadius;
        }

        public void OnNonCenterPosition(bool animate)
        {
            circle.CircleRadius = smallCircleRadius;
        }
    }
}