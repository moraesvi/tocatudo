using System;
using System.Threading.Tasks;
using TocaTudoPlayer.Xamarim.Resources;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TocaTudoPlayer.Xamarim
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoadingControlPopup : Popup
    {
        private static BindableProperty StackLayoutBackgroundColorProperty = BindableProperty.Create(nameof(StackLayoutBackgroundColor), typeof(Color), typeof(Color), Color.Black);
        private static BindableProperty ActivityIndicatorColorProperty = BindableProperty.Create(nameof(ActivityIndicatorColor), typeof(Color), typeof(Color), Color.White);
        private static BindableProperty LabelColorProperty = BindableProperty.Create(nameof(LabelColor), typeof(Color), typeof(Color), Color.White);
        private static BindableProperty LabelTextProperty = BindableProperty.Create(nameof(LabelText), typeof(string), typeof(string), AppResource.PopupMerchanLoading);
        public LoadingControlPopup()
        {
            InitializeComponent();

            IsLightDismissEnabled = false;
            TaskMilissecondsDelay = 2000;

            BindingContext = this;
            Opened += MusicAlbumPopup_Opened;
        }
        public int TaskMilissecondsDelay { get; set; }
        public Func<Task> CloseWhen { get; set; }
        public Color StackLayoutBackgroundColor
        {
            get => (Color)GetValue(StackLayoutBackgroundColorProperty);
            set => SetValue(StackLayoutBackgroundColorProperty, value);
        }
        public Color ActivityIndicatorColor
        {
            get => (Color)GetValue(ActivityIndicatorColorProperty);
            set => SetValue(ActivityIndicatorColorProperty, value);
        }
        public Color LabelColor
        {
            get => (Color)GetValue(LabelColorProperty);
            set => SetValue(LabelColorProperty, value);
        }
        public string LabelText
        {
            get => (string)GetValue(LabelTextProperty);
            set => SetValue(LabelTextProperty, value);
        }
        private async void MusicAlbumPopup_Opened(object sender, PopupOpenedEventArgs e)
        {
            if (CloseWhen != null)
            {
                await Task.WhenAll(Task.Delay(800), DimissCloseWhen());
            }
            else
            {
                await Task.Delay(TaskMilissecondsDelay)
                          .ContinueWith(action =>
                          {
                              IsLightDismissEnabled = true;
                              Dismiss(this);
                          });
            }
        }
        private async Task DimissCloseWhen() 
        {
            await CloseWhen();
            Dismiss(this);
        }
    }
}