using System;
using System.Windows.Input;

namespace TocaTudoPlayer.Xamarim
{
    public class SearchMusicPlaylistCommand : ICommand
    {
        private readonly IMusicPageViewModel _vm;
        public event EventHandler CanExecuteChanged;
        public SearchMusicPlaylistCommand(IMusicPageViewModel vm)
        {
            _vm = vm;
        }
        public bool CanExecute(object parameter)
        {
            if (string.IsNullOrWhiteSpace(_vm.MusicSearchedName))
            {
                _vm.MusicPlaylist.Clear();
                return false;
            }

            return _vm.MusicSearchedName.Length > 2;
        }
        public async void Execute(object parameter)
        {
            await _vm.MusicPlaylistSearch();
        }
    }
}
