using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim
{
    public class CustomScrollView : ScrollView
    {
        private ItemsView _scrollableView;
        private double previousScrollViewPosition = 0;
        private double previousScrollableViewPosition = 0;

        public CustomScrollView()
        {
            this.Scrolled += CustomScrollView_Scrolled;
        }

        [TypeConverter(typeof(ReferenceTypeConverter))]
        public ItemsView ScrollableView
        {
            set
            {
                _scrollableView = value;
                _scrollableView.Scrolled += _scrollableView_Scrolled; ;
            }
        }
        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            if (_scrollableView != null)
                _scrollableView.HeightRequest = height;

            base.LayoutChildren(x, y, width, height);
        }
        private void CustomScrollView_Scrolled(object sender, ScrolledEventArgs e)
        {
            double scrollingSpace = this.ContentSize.Height - this.Height;

            if (previousScrollViewPosition < e.ScrollY)
            {
                //scrolled down

            }
            else if (scrollingSpace <= e.ScrollY)
            {
                // Touched bottom view
            }
            else
            {
                //scrolled up

            }

            previousScrollViewPosition = e.ScrollY;
        }
        private void _scrollableView_Scrolled(object sender, ItemsViewScrolledEventArgs e)
        {
            double scrollingSpace = this.ContentSize.Height - this.Height;

            if (previousScrollableViewPosition < e.VerticalOffset)
            {
                //scrolled down

            }
            else if (scrollingSpace <= e.VerticalOffset)
            {
                // Touched bottom view
            }
            else
            {
                //scrolled up

            }

            previousScrollableViewPosition = e.VerticalOffset;
        }
    }
}
