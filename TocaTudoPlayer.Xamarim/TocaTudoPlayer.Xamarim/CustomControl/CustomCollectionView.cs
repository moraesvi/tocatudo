using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim
{
    public class CustomCollectionView : CollectionView
    {
        private ScrollView _scrollView;
        private int _rowHeigt, _columns;
        private double _previousScrollViewPosition = 0;
        private int _rowCount;

        [TypeConverter(typeof(ReferenceTypeConverter))]
        public ScrollView ScrollView
        {
            set
            {
                _scrollView = value;
                _scrollView.Scrolled += _scrollView_Scrolled;
            }
        }
        public int RowHeigt
        {
            set => _rowHeigt = Convert.ToInt32(value);
        }
        private void UpdateHeight()
        {
            if (_columns == 0)
            {
                if (ItemsLayout is GridItemsLayout layout)
                    _columns = layout.Span;
                else
                    _columns = 1;
            }

            if (_rowHeigt > 0)
                HeightRequest = (_rowHeigt * _rowCount) / _columns;
        }
        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            UpdateHeight();
        }
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            UpdateHeight();
        }
        private void _scrollView_Scrolled(object sender, ScrolledEventArgs e)
        {

            double scrollingSpace = _scrollView.ContentSize.Height - _scrollView.Height;
            if (scrollingSpace <= e.ScrollY)
            {
                // Touched bottom view
                RemainingItemsThresholdReachedCommand?.Execute(RemainingItemsThresholdReachedCommandParameter);
            }

            _previousScrollViewPosition = e.ScrollY;
        }
        protected override async void OnChildAdded(Element child)
        {
            if (child is View)
            {
                SetHeight();

                View view = (View)child;
                ICommonMusicModel musicModel = view.BindingContext as ICommonMusicModel;

                if (musicModel != null && !musicModel.IsAnimated)
                {
                    await view.FadeTo(0, 0);
                    await Task.Delay(20);
                    await view.FadeTo(1);

                    musicModel.IsAnimated = true;
                }
            }

            base.OnChildAdded(child);
            UpdateHeight();
        }
        private async void Grid_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Grid grid = (Grid)sender;
            await grid.TranslateTo(70, 0);
            await Task.Delay(2000);
            await grid.TranslateTo(0, 0);
        }
        protected override void OnChildRemoved(Element child, int oldLogicalIndex)
        {
            base.OnChildRemoved(child, oldLogicalIndex);
            UpdateHeight();
        }
        private void SetHeight()
        {
            try
            {
                _rowCount = Convert.ToInt32(ItemsSource?.Cast<object>()?.ToList()?.Count);
            }
            catch { }
        }
    }
}
