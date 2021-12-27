using System;
using System.Windows.Input;

namespace TocaTudoPlayer.Xamarim
{
    public class SearchAlbumPlaylistCommand : ICommand
    {
        private readonly IAlbumPageViewModel _vm;
        public event EventHandler CanExecuteChanged;
        public SearchAlbumPlaylistCommand(IAlbumPageViewModel vm)
        {
            _vm = vm;
        }
        public bool CanExecute(object parameter)
        {
            if (string.IsNullOrWhiteSpace(_vm.AlbumSearchedName))
            {
                _vm.AlbumPlaylist.Clear();
                return false;
            }

            return _vm.AlbumSearchedName.Length > 2;
        }
        public async void Execute(object parameter)
        {
            await _vm.AlbumPlaylistSearch();
        }
    }
}
