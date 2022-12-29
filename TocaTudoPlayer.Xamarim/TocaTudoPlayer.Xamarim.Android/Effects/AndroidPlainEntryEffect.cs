using Android.Content.Res;
using System;
using System.ComponentModel;
using TocaTudo.Effects;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ResolutionGroupName("PlainEntryGroup")]
[assembly: ExportEffect(typeof(AndroidPlainEntryEffect), "PlainEntryEffect")]

namespace TocaTudo.Effects
{
    public class AndroidPlainEntryEffect : PlatformEffect
    {
        public AndroidPlainEntryEffect()
        {
        }
        protected override void OnAttached()
        {
            try
            {
                if (Control != null)
                {
                    Android.Graphics.Color entryLineColor = Android.Graphics.Color.White;
                    Control.BackgroundTintList = ColorStateList.ValueOf(entryLineColor);
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Error... Unable to set property on attached control", ex.Message);
            }
        }
        protected override void OnDetached()
        {
        }
        protected override void OnElementPropertyChanged(PropertyChangedEventArgs args)
        {
            base.OnElementPropertyChanged(args);
        }
    }
}