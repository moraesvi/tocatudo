using System;
using System.Windows.Input;

namespace TocaTudoPlayer.Xamarim.ViewModel
{
    public class ShowHideDownloadMusicOptionsCommand : ICommand
    {
        private readonly AlbumPlayerViewModel _vm;
        public event EventHandler CanExecuteChanged;
        public ShowHideDownloadMusicOptionsCommand(AlbumPlayerViewModel vm)
        {
            _vm = vm;
        }
        public bool CanExecute(object parameter)
        {
            return _vm.Download.IsDownloadEventEnabled;
        }
        public void Execute(object parameter)
        {
            _vm.ShowPlayingOfflineInfo = false;
            _vm.ShowHideDownloadMusicOptions = !_vm.ShowHideDownloadMusicOptions;
        }
    }
}
