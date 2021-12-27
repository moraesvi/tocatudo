using System;
using System.Windows.Input;

namespace TocaTudoPlayer.Xamarim
{
    public class MusicSavedActionCommand : ICommand
    {
        private readonly SearchPlaylistViewModel _vm;
        public event EventHandler CanExecuteChanged;
        public MusicSavedActionCommand(SearchPlaylistViewModel vm)
        {
            _vm = vm;
        }
        public bool CanExecute(object parameter)
        {
            return _vm.MenuActionsEnabled;
        }
        public async void Execute(object parameter)
        {
            await _vm.MusicPlaylistSearchFromDb();
        }
    }
}
