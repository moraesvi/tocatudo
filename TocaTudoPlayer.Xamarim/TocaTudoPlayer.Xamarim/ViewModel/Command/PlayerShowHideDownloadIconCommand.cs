using System;
using System.Windows.Input;

namespace TocaTudoPlayer.Xamarim.ViewModel
{
    public class PlayerShowHideDownloadIconCommand : ICommand
    {
        private readonly AlbumPlayerViewModel _vm;
        public event EventHandler CanExecuteChanged;
        public PlayerShowHideDownloadIconCommand(AlbumPlayerViewModel vm)
        {
            _vm = vm;
        }
        public bool CanExecute(object parameter)
        {
            return _vm.PlayerLoaded;
        }
        public async void Execute(object parameter)
        {
            _vm.ShowHideDownloadIcon();
        }
    }
}
