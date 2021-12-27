using System;
using System.Windows.Input;

namespace TocaTudoPlayer.Xamarim
{
    public class SearchDownloadMusicVisibleCommand : ICommand
    {
        private SearchPlaylistViewModel _vm;
        public event EventHandler CanExecuteChanged;
        public SearchDownloadMusicVisibleCommand(SearchPlaylistViewModel vm)
        {
            _vm = vm;
        }
        public bool CanExecute(object parameter)
        {
            if (parameter == null)
                return false;

            return ((SearchMusicModel)parameter).IconMusicStatusEnabled;
        }
        public void Execute(object parameter)
        {
            _vm.DownloadMusicVisible((SearchMusicModel)parameter);
        }
    }
}
