using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace TocaTudoPlayer.Xamarim.ViewModel.CustomView
{
    public abstract class HistoryBaseViewModel : BaseViewModel
    {
        private readonly IPCLStorageDb _pclStorageDb;
        private readonly string _dbKey;
        private bool _playedHistoryIsVisible;
        private int _playedHistoryCollectionSize;
        private int _playedHistoryPlayerFormSize;
        private int _playedHistoryCollectionTotalItens;
        private int _userPlayedHistoryTotalReg;
        public HistoryBaseViewModel(IPCLStorageDb pclStorageDb)
        {
            _pclStorageDb = pclStorageDb;
            _playedHistoryIsVisible = false;
            _playedHistoryCollectionSize = 0;
            _playedHistoryCollectionTotalItens = 1;
            _userPlayedHistoryTotalReg = 30;
        }
        public bool PlayedHistoryIsVisible
        {
            get { return _playedHistoryIsVisible; }
            set
            {
                _playedHistoryIsVisible = value;
                OnPropertyChanged(nameof(PlayedHistoryIsVisible));
            }
        }
        public int PlayedHistoryCollectionSize
        {
            get { return _playedHistoryCollectionSize; }
            set
            {
                _playedHistoryCollectionSize = value;
                OnPropertyChanged(nameof(PlayedHistoryCollectionSize));
            }
        }
        public int PlayedHistoryPlayerFormSize
        {
            get { return _playedHistoryPlayerFormSize; }
            set
            {
                _playedHistoryPlayerFormSize = value;
                OnPropertyChanged(nameof(PlayedHistoryPlayerFormSize));
            }
        }
        public int PlayedHistoryCollectionTotalItens
        {
            get { return _playedHistoryCollectionTotalItens; }
            set
            {
                _playedHistoryCollectionTotalItens = value;
                OnPropertyChanged(nameof(PlayedHistoryCollectionTotalItens));
            }
        }
        public int UserPlayedHistoryTotalReg => _userPlayedHistoryTotalReg;
        protected string[] FilterUserSearchHistory(UserSearchHistoryModel searchHistory, string term)
        {
            if (searchHistory != null)
                return searchHistory.Terms
                                    .Where(t => t?.Split(' ')?.Where(t => t.StartsWith(term, StringComparison.OrdinalIgnoreCase))?.FirstOrDefault() != null)
                                    .Take(10)
                                    .ToArray();
            return null;
        }
        protected string[] FilterUserSearchHistory(UserSearchHistoryModel searchHistory, List<string> lstFilters)
        {
            if (searchHistory != null)
                return searchHistory.Terms
                                    .Where(term =>
                                    {
                                        return lstFilters.Exists(f => term?.Split(' ')?.Where(t => f.StartsWith(t, StringComparison.OrdinalIgnoreCase))?.FirstOrDefault() != null);
                                    })
                                    .Take(10)
                                    .ToArray();
            return null;
        }
        protected async Task SaveLocalSearchHistory(UserSearchHistoryModel searchHistory, string name)
        {
            await SaveLocalSearchHistory(searchHistory, name, _dbKey);
        }
        protected async Task SaveLocalSearchHistory(UserSearchHistoryModel searchHistory, string name, string dbKey)
        {
            if (searchHistory == null)
            {
                searchHistory = new UserSearchHistoryModel();
                searchHistory.Terms.Add(name);
            }
            else if (!searchHistory.Terms.Exists(term => string.Equals(name, term)))
            {
                searchHistory.Terms.Add(name);
            }

            await _pclStorageDb.SaveFile(dbKey, searchHistory);
        }
    }
}
