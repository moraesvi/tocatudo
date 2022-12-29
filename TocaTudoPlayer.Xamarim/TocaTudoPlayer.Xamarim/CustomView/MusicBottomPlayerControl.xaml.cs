using MarcTron.Plugin;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TocaTudoPlayer.Xamarim
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MusicBottomPlayerControl : StackLayout
    {
        public static readonly BindableProperty ViewModelProperty = BindableProperty.Create(nameof(ViewModel), typeof(MusicBottomPlayerViewModel), typeof(MusicBottomPlayerViewModel));

        private LetterUpdate _letterUpdate;
        public MusicBottomPlayerControl()
        {
            InitializeComponent();
        }
        public MusicBottomPlayerViewModel ViewModel
        {
            get
            {
                return (MusicBottomPlayerViewModel)GetValue(ViewModelProperty);
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
            progressBar.Value = 0;
            progressBar.Minimum = 0;
            progressBar.Maximum = musicMaxDuration;

            Task.Run(async () => await MusicStatusLabelTimer());
        }
        private void ViewModel_MusicStreamProgessEvent(object sender, float progress)
        {
            progressBar.Value = progress;
        }
        private void ViewModel_ActivePlayer(object sender, EventArgs e)
        {
        }
        private void ViewModel_StopPlayer(object sender, EventArgs e)
        {
        }
        private void ProgressBar_DragStarted(object sender, EventArgs e)
        {
            ViewModel.ProgressBarDragStartedCommand.Execute(null);
        }
        private void ProgressBar_DragCompleted(object sender, EventArgs e)
        {
            Slider slider = (Slider)sender;

            ViewModel.ProgressBarDragCompletedCommand.Execute(AppHelper.ExoplayerTimeToTocaTudo((int)slider.Value));
        }
        private void ProgressBar_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            ViewModel.UpdateMusicPartTimeDesc(AppHelper.ExoplayerTimeToTocaTudo((long)e.NewValue));
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