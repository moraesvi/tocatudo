using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TocaTudoPlayer.Xamarim
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MusicBottomPlayerControl : StackLayout
    {       
        public static readonly BindableProperty ViewModelProperty = BindableProperty.Create(nameof(ViewModel), typeof(IMusicBottomPlayerViewModel), typeof(IMusicBottomPlayerViewModel));

        private bool _lettersInRigthProgress;
        private bool _runMusicStatusLabelTimer;
        public MusicBottomPlayerControl()
        {
            InitializeComponent();

            _lettersInRigthProgress = false;
            _runMusicStatusLabelTimer = false;
        }
        public IMusicBottomPlayerViewModel ViewModel
        {
            get 
            {
                return (IMusicBottomPlayerViewModel)GetValue(ViewModelProperty);
            }
            set 
            { 
                SetValue(ViewModelProperty, value);

                BindingContext = ViewModel;

                ViewModel.MusicPlayerLoadedEvent += ViewModel_MusicPlayerLoadedEvent;
                ViewModel.MusicStreamProgessEvent += ViewModel_MusicStreamProgessEvent;
                ViewModel.ActivePlayer += ViewModel_ActivePlayer;
                ViewModel.StopPlayer += ViewModel_StopPlayer;


                MusicStatusLabelTimer();
            }
        }
        protected override void OnParentSet()
        {
            _runMusicStatusLabelTimer = false;
            base.OnParentSet();
        }
        private void ViewModel_MusicPlayerLoadedEvent(float musicMaxDuration)
        {
            progressBar.Value = 0;
            progressBar.Minimum = 0;
            progressBar.Maximum = musicMaxDuration;

            _runMusicStatusLabelTimer = true;
        }
        private void ViewModel_MusicStreamProgessEvent(float progress)
        {
            progressBar.Value = progress;
        }
        private void ViewModel_ActivePlayer()
        {
            _runMusicStatusLabelTimer = true;
        }
        private void ViewModel_StopPlayer()
        {
            _runMusicStatusLabelTimer = false;
        }
        private void ProgressBar_DragStarted(object sender, EventArgs e)
        {
            ViewModel.ProgressBarDragStartedCommand.Execute(null);
        }
        private void ProgressBar_DragCompleted(object sender, EventArgs e)
        {
            Slider slider = (Slider)sender;

            ViewModel.ProgressBarDragCompletedCommand.Execute((int)slider.Value);
        }
        private void ProgressBar_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            ViewModel.UpdateMusicPartTimeDesc((int)e.NewValue);
        }
        private void MusicStatusLabelTimer()
        {
            bool deviceTimerAppearing = true;
            Device.StartTimer(TimeSpan.FromMilliseconds(80), () =>
            {
                if (!_runMusicStatusLabelTimer)
                {
                    lblNomeAbum.TranslationX = 0;
                    return true;
                }

                if (lblNomeAbum.Width == stlAlbum.Width || lblNomeAbum.Text == null)
                {
                    lblNomeAbum.TranslationX = 0;
                    return true;
                }

                int letterExtraSpaceRigthProgress = ((int)(lblNomeAbum.Width - stlAlbum.Width) - lblNomeAbum.Text.Length) + 2;
                if (Width > 0 && lblNomeAbum.TranslationX <= -(lblNomeAbum.Text.Length + letterExtraSpaceRigthProgress))
                {
                    _lettersInRigthProgress = true;
                }

                if (Width > 0 && lblNomeAbum.TranslationX >= 0 && deviceTimerAppearing)
                {
                    _lettersInRigthProgress = false;
                    deviceTimerAppearing = false;
                }

                if (Width > 0 && lblNomeAbum.TranslationX >= 2)
                {
                    _lettersInRigthProgress = false;
                }

                if (_lettersInRigthProgress)
                {
                    lblNomeAbum.TranslationX += 2f;
                }

                if (!_lettersInRigthProgress)
                {
                    lblNomeAbum.TranslationX -= 2f;
                }

                Device.BeginInvokeOnMainThread(() =>
                {
                    MusicStatusLabelTimer();
                });

                return false;
            });
        }
    }
}