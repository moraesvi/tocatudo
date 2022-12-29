using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;

namespace TocaTudoPlayer.Xamarim
{
    public class SearchAlbumPlaylistCommand : IAsyncCommand
    {
        private readonly AlbumPageViewModel _vm;
        public bool IsExecuting => throw new NotImplementedException();
        public bool AllowsMultipleExecutions => throw new NotImplementedException();

        public event EventHandler CanExecuteChanged;
        public SearchAlbumPlaylistCommand(AlbumPageViewModel vm)
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
        public async Task ExecuteAsync()
        {
            await _vm.AlbumPlaylistSearch();
        }
        public void RaiseCanExecuteChanged()
        {
            throw new NotImplementedException();
        }
    }
}
