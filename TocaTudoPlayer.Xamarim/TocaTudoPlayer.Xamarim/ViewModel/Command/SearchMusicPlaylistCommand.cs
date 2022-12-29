using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;

namespace TocaTudoPlayer.Xamarim
{
    public class SearchMusicPlaylistCommand : IAsyncCommand
    {
        private readonly MusicPageViewModel _vm;
        public bool IsExecuting => throw new NotImplementedException();
        public bool AllowsMultipleExecutions => throw new NotImplementedException();
        public event EventHandler CanExecuteChanged;
        public SearchMusicPlaylistCommand(MusicPageViewModel vm)
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
        public async Task ExecuteAsync()
        {
            await _vm.MusicPlaylistSearch();
        }
        public void RaiseCanExecuteChanged()
        {
            throw new NotImplementedException();
        }
        public void Execute(object parameter)
        {
            throw new NotImplementedException();
        }
    }
}
