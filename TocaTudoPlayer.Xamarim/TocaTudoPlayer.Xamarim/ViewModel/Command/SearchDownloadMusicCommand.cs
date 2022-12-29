using MarcTron.Plugin;
using System;
using System.Windows.Input;

namespace TocaTudoPlayer.Xamarim
{
    public class SearchDownloadMusicCommand : ICommand
    {
        private MusicPageViewModel _vm;
        public event EventHandler CanExecuteChanged;

        public SearchDownloadMusicCommand(MusicPageViewModel vm)
        {
            _vm = vm;
        }
        public bool CanExecute(object parameter)
        {
            if (parameter == null)
                return false;

            return ((SearchMusicModel)parameter).IconMusicStatusEnabled;
        }
        public async void Execute(object parameter)
        {
            await _vm.MusicPlayerViewModel.StartDownloadMusic((SearchMusicModel)parameter);
        }        
    }
}
