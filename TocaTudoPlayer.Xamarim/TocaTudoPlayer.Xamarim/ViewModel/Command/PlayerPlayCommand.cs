using System;
using System.Windows.Input;

namespace TocaTudoPlayer.Xamarim.ViewModel
{
    public class PlayerPlayCommand : ICommand
    {
        private readonly AlbumPlayerViewModel _vm;
        public event EventHandler CanExecuteChanged;
        public PlayerPlayCommand(AlbumPlayerViewModel vm)
        {
            _vm = vm;
        }
        public bool CanExecute(object parameter)
        {
            return _vm.PlayerLoaded;
        }
        public async void Execute(object parameter)
        {
            await _vm.PlayMusic((PlaylistItem)parameter);
        }
    }
}
