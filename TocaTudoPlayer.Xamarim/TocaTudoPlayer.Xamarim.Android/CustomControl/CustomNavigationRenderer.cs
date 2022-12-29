using Android.Content;
using System.Threading.Tasks;
using TocaTudo.CustomControl;
using TocaTudoPlayer.Xamarim;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CustomNavigationPage), typeof(CustomNavigationRenderer))]
namespace TocaTudo.CustomControl
{
    public class CustomNavigationRenderer : NavigationRenderer
    {
        public CustomNavigationRenderer(Context context)
           : base(context)
        {
        }
        protected override Task<bool> OnPopViewAsync(Page page, bool animated)
        {
            return base.OnPopViewAsync(page, (Element as CustomNavigationPage).Animated);
        }
        protected override Task<bool> OnPushAsync(Page page, bool animated)
        {
            return base.OnPushAsync(page, (Element as CustomNavigationPage).Animated);
        }
    }
}