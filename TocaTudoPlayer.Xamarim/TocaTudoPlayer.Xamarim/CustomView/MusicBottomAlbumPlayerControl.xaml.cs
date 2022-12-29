using MarcTron.Plugin;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TocaTudoPlayer.Xamarim
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MusicBottomAlbumPlayerControl : StackLayout
    {
        public static readonly BindableProperty ViewModelProperty = BindableProperty.Create(nameof(ViewModel), typeof(MusicBottomAlbumPlayerViewModel), typeof(MusicBottomAlbumPlayerViewModel));

        private LetterUpdate _letterUpdate;
        private bool _lettersInRigthProgress;
        private bool _runMusicStatusLabelTimer;
        private bool _okMusicLabelLengh;
        public MusicBottomAlbumPlayerControl()
        {
            InitializeComponent();

            _runMusicStatusLabelTimer = false;
            _okMusicLabelLengh = false;

            CrossMTAdmob.Current.OnInterstitialClosed += async (s, e) => await MusicStatusLabelTimer();
        }
        public MusicBottomAlbumPlayerViewModel ViewModel
        {
            get
            {
                return (MusicBottomAlbumPlayerViewModel)GetValue(ViewModelProperty);
            }
            set
            {
                SetValue(ViewModelProperty, value);

                BindingContext = ViewModel;

                ViewModel.MusicPlayerLoadedEvent += ViewModel_MusicPlayerLoadedEvent;
                ViewModel.MusicStreamProgessEvent += ViewModel_MusicStreamProgessEvent;
                ViewModel.ActivePlayer += ViewModel_ActivePlayer;
                ViewModel.StopPlayer += ViewModel_StopPlayer;
            }
        }
        protected override void OnParentSet()
        {
            base.OnParentSet();
        }
        private void ViewModel_MusicPlayerLoadedEvent(object sender, float musicMaxDuration)
        {
            if (musicMaxDuration == 0)
                return;

            _runMusicStatusLabelTimer = true;

            progressBar.Value = 0;
            progressBar.Minimum = 0;
            progressBar.Maximum = musicMaxDuration;

            if (_letterUpdate == null)
                Task.Run(async () => await MusicStatusLabelTimer());
        }
        private void ViewModel_MusicStreamProgessEvent(object sender, float progress)
        {
            progressBar.Value = progress;
        }
        private async void ViewModel_ActivePlayer(object sender, EventArgs e)
        {
            await MusicStatusLabelTimer();
        }
        private void ViewModel_StopPlayer(object sender, EventArgs e)
        {
            _letterUpdate?.Dispose();
            _letterUpdate = null;
        }
        private void ProgressBar_DragStarted(object sender, EventArgs e)
        {
            ViewModel.ProgressBarDragStartedCommand.Execute(null);
        }
        private void ProgressBar_DragCompleted(object sender, EventArgs e)
        {
            Slider slider = (Slider)sender;

            ViewModel.ProgressBarDragCompletedCommand.Execute((long)slider.Value);
        }
        private void ProgressBar_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            ViewModel.UpdateMusicPartTimeDesc((int)e.NewValue);
        }
        private async Task MusicStatusLabelTimer()
        {
            if (_letterUpdate != null)
            {
                _letterUpdate.Dispose();
                _letterUpdate = null;
            }

            _letterUpdate = new LetterUpdate();
            await _letterUpdate.Translate(lblNomeAbum, stlAlbum);
        }
    }
}