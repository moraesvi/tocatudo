using System;
using System.Windows.Input;

namespace TocaTudoPlayer.Xamarim.ViewModel
{
    public class SearchTermCommand : ICommand
    {
        private readonly SearchPlaylistViewModel _vm;
        public event EventHandler CanExecuteChanged;
        public SearchTermCommand(SearchPlaylistViewModel vm) 
        {
            _vm = vm;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public async void Execute(object parameter)
        {
            //await _vm.SearchTerm(parameter.ToString());
        }
    }
}
