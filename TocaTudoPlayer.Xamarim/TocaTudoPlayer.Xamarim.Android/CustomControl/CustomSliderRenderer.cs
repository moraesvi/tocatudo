using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Android.Views;
using TocaTudo;
using TocaTudoPlayer.Xamarim;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CustomSlider), typeof(CustomSliderRenderer))]
namespace TocaTudo
{
    public class CustomSliderRenderer : SliderRenderer
    {
        public CustomSliderRenderer(Context context)
            : base(context)
        {

        }
        protected override void OnElementChanged(ElementChangedEventArgs<Slider> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                Control.ProgressDrawable.SetColorFilter(new PorterDuffColorFilter(Xamarin.Forms.Color.FromHex("#ff0066").ToAndroid(), PorterDuff.Mode.SrcIn));

                //ShapeDrawable th = new ShapeDrawable(new OvalShape());
                //th.SetIntrinsicWidth(100);
                //th.SetIntrinsicHeight(100);
                //th.SetColorFilter(Android.Graphics.Color.Red, Android.Graphics.PorterDuff.Mode.SrcOver);

                ////Control.ProgressDrawable.SetColorFilter(new PorterDuffColorFilter(Xamarin.Forms.Color.FromHex("#ff0066").ToAndroid(), PorterDuff.Mode.SrcIn));
                //// Set custom drawable resource
                //Control.SetProgressDrawableTiled(Resources.GetDrawable(Resource.Drawable.custom_progressbar_style, (this.Context).Theme));

                //// Hide thumb to make it look cool lol
                ////Control.SetThumb(new ColorDrawable(Android.Graphics.Color.Red));
                //Control.SetThumb(th);
                //Control.SetPadding(5, 0, 0, 0);
            }
        }
    }
}