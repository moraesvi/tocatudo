using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim
{
    public class AlbumPlayedHistoryViewModel : HistoryBaseViewModel
    {
        private const string USER_LOCAL_ALBUM_PLAYED_HISTORY_KEY = "ap_history.json";

        private readonly IPCLStorageDb _pclStorageDb;
        private readonly IPCLUserAlbumLogic _pclUserAlbumLogic;
        private readonly string _dbSearchedKey;
        private readonly string _dbPlayedKey;
        private bool _recentlyPlayedFormIsVisible;
        private HistoryAlbumModel _recentlyPlayedSelected;
        private UserSearchHistoryModel _userSearchHistory;
        private ObservableCollection<UserAlbumPlayedHistory> _playedHistory;
        private List<UserAlbumPlayedHistory> _lstUserHistory;
        public AlbumPlayedHistoryViewModel(IPCLStorageDb pclStorageDb, IPCLUserAlbumLogic pclUserAlbumLogic)
            : base(pclStorageDb)
        {
            _dbSearchedKey = UserAlbumLocalSearchHistoryKey;
            _dbPlayedKey = USER_LOCAL_ALBUM_PLAYED_HISTORY_KEY;
            _pclStorageDb = pclStorageDb;
            _pclUserAlbumLogic = pclUserAlbumLogic;
            _playedHistory = new ObservableCollection<UserAlbumPlayedHistory>();
            _recentlyPlayedFormIsVisible = false;
        }
        public AlbumPlayedHistoryViewModel(string dbSearchedKey, string dbPlayedKey, IPCLStorageDb pclStorageDb)
            : base(pclStorageDb)
        {
            _dbSearchedKey = dbSearchedKey;
            _dbPlayedKey = dbPlayedKey;
            _pclStorageDb = pclStorageDb;
            _playedHistory = new ObservableCollection<UserAlbumPlayedHistory>();
            _recentlyPlayedFormIsVisible = false;
        }
        public bool RecentlyPlayedFormIsVisible
        {
            get { return _recentlyPlayedFormIsVisible; }
            set
            {
                _recentlyPlayedFormIsVisible = value;
                OnPropertyChanged(nameof(RecentlyPlayedFormIsVisible));
            }
        }
        public HistoryAlbumModel RecentlyPlayedSelected
        {
            get { return _recentlyPlayedSelected; }
            set
            {
                _recentlyPlayedSelected = value;
                OnPropertyChanged(nameof(RecentlyPlayedSelected));
            }
        }
        public UserSearchHistoryModel UserSearchHistory
        {
            get { return _userSearchHistory; }
            set
            {
                _userSearchHistory = value;
            }
        }
        public ObservableCollection<UserAlbumPlayedHistory> PlayedHistory
        {
            get { return _playedHistory; }
            set
            {
                _playedHistory = value;
                OnPropertyChanged(nameof(PlayedHistory));
            }
        }
        public async Task SerializarPlayedHistory(List<UserAlbumPlayedHistory> lstUserAlbumHistory, string videoId)
        {
            if (lstUserAlbumHistory == null)
                return;

            _lstUserHistory = lstUserAlbumHistory;

            PlayedHistory.Clear();

            foreach (UserAlbumPlayedHistory userAlbumHist in _lstUserHistory)
            {
                await Task.Delay(200);
                PlayedHistory.Add(new UserAlbumPlayedHistory()
                {
                    VideoId = userAlbumHist.VideoId,
                    AlbumName = userAlbumHist.AlbumName,
                    ImgAlbum = userAlbumHist.ImgAlbum,
                    ParseType = userAlbumHist.ParseType,
                    DateTimeIn = userAlbumHist.DateTimeIn
                });
            }
        }
        public async Task LoadUserSearchHistory()
        {
            _userSearchHistory = await _pclStorageDb.GetJson<UserSearchHistoryModel>(_dbSearchedKey);
        }
        public async Task LoadPlayedHistory(UserAlbumPlayedHistory userAlbumSelected = null)
        {
            if (IsBindingData)
                return;

            IsBindingData = true;

            await _pclUserAlbumLogic.LoadDb();            

            _lstUserHistory = (await _pclStorageDb.GetJson<List<UserAlbumPlayedHistory>>(_dbPlayedKey))
                                                  ?.GroupBy(uh => new
                                                  {
                                                      VideoId = uh.VideoId,
                                                      AlbumName = uh.AlbumName
                                                  })
                                                  ?.Select(uh =>
                                                  {
                                                      UserAlbumPlayedHistory uHist = uh.Where(u => string.Equals(u.VideoId, uh.Key.VideoId) && string.Equals(u.AlbumName, uh.Key.AlbumName))
                                                                                       .FirstOrDefault();

                                                      bool existsOnLocalDb = _pclUserAlbumLogic.ExistsOnLocalDb(uh.Key.VideoId);

                                                      return new UserAlbumPlayedHistory()
                                                      {
                                                          VideoId = uh.Key.VideoId,
                                                          AlbumName = uh.Key.AlbumName,
                                                          ByteImgAlbum = uHist.ByteImgAlbum,
                                                          ImgAlbum = uHist.ImgAlbum,
                                                          MusicSelectedColorPrimary = uHist.MusicSelectedColorPrimary,
                                                          MusicSelectedColorSecondary = uHist.MusicSelectedColorSecondary,
                                                          IsSavedOnLocalDb = existsOnLocalDb,
                                                          DateTimeIn = uHist.DateTimeIn,
                                                          ParseType = uHist.ParseType,
                                                      };
                                                  })
                                                  ?.OrderByDescending(uh => uh.DtIn)
                                                  ?.ToList() ?? new List<UserAlbumPlayedHistory>();

            if (_lstUserHistory == null)
                return;

            PlayedHistory.Clear();
            PlayedHistoryIsVisible = _lstUserHistory.Count > 0;

            if (_lstUserHistory.Count == 1)
            {
                PlayedHistoryCollectionSize = 140;
                PlayedHistoryFrameSize = PlayedHistoryCollectionSize + 30;
                PlayedHistoryCollectionTotalItens = 1;
            }
            else if (_lstUserHistory.Count > 1)
            {
                PlayedHistoryCollectionSize = 220;
                PlayedHistoryFrameSize = PlayedHistoryCollectionSize + 30;
                PlayedHistoryCollectionTotalItens = 2;
            }
            else
            {
                PlayedHistoryFrameSize = 0;
                PlayedHistoryCollectionSize = 0;
                PlayedHistoryCollectionTotalItens = 1;
            }

            if (userAlbumSelected != null)
            {
                foreach (UserAlbumPlayedHistory userMusicHist in _lstUserHistory)
                {
                    if (string.Equals(userMusicHist.AlbumName, userAlbumSelected.AlbumName) && string.Equals(userMusicHist.VideoId, userAlbumSelected.VideoId))
                    {
                        userMusicHist.UpdAlbumSelectedColor();
                    }

                    await Task.Delay(200);
                    PlayedHistory.Add(userMusicHist);
                }
            }
            else 
            {
                foreach (UserAlbumPlayedHistory userMusicHist in _lstUserHistory)
                {
                    await Task.Delay(200);
                    PlayedHistory.Add(userMusicHist);
                }
            }

            _pclUserAlbumLogic.UnLoadDb();

            IsBindingData = false;
        }
        public string[] FilterUserSearchHistory(string term)
        {
            return FilterUserSearchHistory(_userSearchHistory, term);
        }
        public string[] FilterUserSearchHistory(List<string> lstFilters)
        {
            return FilterUserSearchHistory(_userSearchHistory, lstFilters);
        }
        public async Task SaveLocalSearchHistory(string albumName)
        {
            await SaveLocalSearchHistory(_userSearchHistory, albumName, _dbSearchedKey);
        }
        public async Task SaveLocalHistory(AlbumModel album, AlbumParseType parseType, byte[] byteMusicImage)
        {
            await Task.Run(async () =>
            {
                UserAlbumPlayedHistory musicModel = new UserAlbumPlayedHistory()
                {
                    VideoId = album.VideoId,
                    ByteImgAlbum = byteMusicImage,
                    AlbumName = album.Album,
                    ParseType = (int)parseType,
                    DateTimeIn = DateTimeOffset.UtcNow.ToString(),
                };

                UserAlbumPlayedHistory userAlbumPlayed = _lstUserHistory.ToList()
                                                                        .Where(mp => string.Equals(mp.AlbumName, album.Album))
                                                                        .FirstOrDefault();

                if (_lstUserHistory == null || _lstUserHistory.Count == 0)
                {
                    _lstUserHistory = new List<UserAlbumPlayedHistory>();
                    _lstUserHistory.Add(musicModel);

                    await _pclStorageDb.SaveFile(_dbPlayedKey, _lstUserHistory);

                    _lstUserHistory.Clear();
                    await LoadPlayedHistory();
                }
                else if (userAlbumPlayed == null)
                {
                    _lstUserHistory.Add(musicModel);
                    _lstUserHistory = _lstUserHistory.OrderByDescending(uh => uh.DtIn)
                                                     .Take(UserPlayedHistoryTotalReg)
                                                     .ToList();

                    await _pclStorageDb.SaveFile(_dbPlayedKey, _lstUserHistory);

                    _lstUserHistory.Clear();
                    await LoadPlayedHistory();
                }

                if (userAlbumPlayed != null)
                {
                    userAlbumPlayed.DateTimeIn = DateTimeOffset.UtcNow.ToString();
                    _lstUserHistory = _lstUserHistory.OrderByDescending(uh => uh.DtIn)
                                                     .Take(UserPlayedHistoryTotalReg)
                                                     .ToList();

                    await _pclStorageDb.SaveFile(_dbPlayedKey, _lstUserHistory);

                    _lstUserHistory.Clear();
                    await LoadPlayedHistory();
                }
            }).ConfigureAwait(false);
        }
    }
}
