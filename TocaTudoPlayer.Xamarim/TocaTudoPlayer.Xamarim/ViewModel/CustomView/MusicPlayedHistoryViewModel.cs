using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim.ViewModel.CustomView
{
    public class MusicPlayedHistoryViewModel : HistoryBaseViewModel, IMusicPlayedHistoryViewModel
    {
        private const string USER_LOCAL_MUSIC_PLAYED_HISTORY_KEY = "mp_history.json";

        private readonly IDbLogic _albumDbLogic;
        private readonly IPCLStorageDb _pclStorageDb;
        private readonly IPCLUserMusicLogic _pclUserMusicLogic;
        private readonly string _dbSearchedKey;
        private readonly string _dbPlayedKey;
        private int _playedHistoryPlayerFormDownloadSize;
        private bool _musicHistoryFormIsEnabled;
        private HttpDownload _download;
        private UserSearchHistoryModel _userSearchHistory;
        private ObservableCollection<UserMusicPlayedHistory> _musicPlayedHistory;
        private HistoryMusicModel _historyMusicPlayingNow;
        private List<UserMusicPlayedHistory> _lstUserHistory;
        public MusicPlayedHistoryViewModel(IPCLStorageDb pclStorageDb, IPCLUserMusicLogic pclUserMusicLogic)
            : base(pclStorageDb)
        {
            _dbSearchedKey = UserMusicLocalSearchHistoryKey;
            _dbPlayedKey = USER_LOCAL_MUSIC_PLAYED_HISTORY_KEY;
            _pclStorageDb = pclStorageDb;
            _pclUserMusicLogic = pclUserMusicLogic;
            _playedHistoryPlayerFormDownloadSize = 0;
            _musicHistoryFormIsEnabled = true;
            _musicPlayedHistory = new ObservableCollection<UserMusicPlayedHistory>();

            _download = new HttpDownload();
        }
        public MusicPlayedHistoryViewModel(string dbSearchedKey, string dbPlayedKey, IPCLStorageDb pclStorageDb, IPCLUserMusicLogic pclUserMusicLogic)
            : base(pclStorageDb)
        {
            _dbSearchedKey = dbSearchedKey;
            _dbPlayedKey = dbPlayedKey;
            _pclStorageDb = pclStorageDb;
            _pclUserMusicLogic = pclUserMusicLogic;
            _playedHistoryPlayerFormDownloadSize = 0;
            _musicHistoryFormIsEnabled = true;
            _musicPlayedHistory = new ObservableCollection<UserMusicPlayedHistory>();

            _download = new HttpDownload();
        }
        public HistoryMusicModel HistoryMusicPlayingNow
        {
            get { return _historyMusicPlayingNow; }
            set
            {
                _historyMusicPlayingNow = value;
                OnPropertyChanged(nameof(HistoryMusicPlayingNow));
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
        public HttpDownload Download
        {
            get { return _download; }
            set
            {
                _download = value;
            }
        }
        public UserMusicPlayedHistory ActiveMusicNow { get; set; }
        public bool MusicHistoryFormIsEnabled
        {
            get { return _musicHistoryFormIsEnabled; }
            set
            {
                _musicHistoryFormIsEnabled = value;
                OnPropertyChanged(nameof(MusicHistoryFormIsEnabled));
            }
        }
        public ObservableCollection<UserMusicPlayedHistory> PlayedHistory
        {
            get { return _musicPlayedHistory; }
            set
            {
                _musicPlayedHistory = value;
                OnPropertyChanged(nameof(PlayedHistory));
            }
        }
        public Command MusicHistoryDownloadFormCommand => MusicHistoryDownloadFormEventCommand();
        public void SerializarPlayedHistory(List<UserMusicPlayedHistory> lstUserAHistory, string videoId)
        {
            if (lstUserAHistory == null)
                return;

            _lstUserHistory = lstUserAHistory;

            PlayedHistory.Clear();

            foreach (UserMusicPlayedHistory userAlbumHist in lstUserAHistory)
            {
                if (!string.Equals(userAlbumHist.VideoId, videoId))
                    PlayedHistory.Add(new UserMusicPlayedHistory()
                    {
                        VideoId = userAlbumHist.VideoId,
                        MusicName = userAlbumHist.MusicName,
                        ByteImgMusic = userAlbumHist.ByteImgMusic,
                        DateTimeIn = userAlbumHist.DateTimeIn
                    });
            }
        }
        public async Task LoadUserSearchHistory()
        {
            _userSearchHistory = await _pclStorageDb.GetJson<UserSearchHistoryModel>(_dbSearchedKey);
        }
        public async Task LoadPlayedHistory(UserMusicPlayedHistory userMusicSelected = null)
        {
            await _pclUserMusicLogic.LoadDb();

            _lstUserHistory = (await _pclStorageDb.GetJson<List<UserMusicPlayedHistory>>(_dbPlayedKey))
                                                  ?.GroupBy(uh => new
                                                  {
                                                      VideoId = uh.VideoId,
                                                  })
                                                  ?.Select(uh =>
                                                  {
                                                      UserMusicPlayedHistory uHist = uh.Where(u => string.Equals(u.VideoId, uh.Key.VideoId))
                                                                                       .FirstOrDefault();


                                                      bool existsOnLocalDb = _pclUserMusicLogic.ExistsOnLocalDb(uh.Key.VideoId);

                                                      return new UserMusicPlayedHistory()
                                                      {
                                                          VideoId = uh.Key.VideoId,
                                                          MusicName = uHist.MusicName,
                                                          ByteImgMusic = uHist.ByteImgMusic,
                                                          ImgMusic = uHist.ImgMusic,
                                                          MusicSelectedColorPrimary = uHist.MusicSelectedColorPrimary,
                                                          MusicSelectedColorSecondary = uHist.MusicSelectedColorSecondary,
                                                          IsSavedOnLocalDb = existsOnLocalDb,
                                                          DateTimeIn = uHist.DateTimeIn
                                                      };
                                                  })
                                                  ?.OrderByDescending(uh => uh.DtIn)
                                                  ?.ToList() ?? new List<UserMusicPlayedHistory>() { };

            if (_lstUserHistory == null)
                return;

            PlayedHistory.Clear();

            if (userMusicSelected != null && userMusicSelected?.SearchType == MusicSearchType.SearchMusicHistory)
            {
                foreach (UserMusicPlayedHistory userMusicHist in _lstUserHistory)
                {
                    if (string.Equals(userMusicHist.VideoId, userMusicSelected.VideoId))
                    {
                        userMusicHist.UpdMusicSelectedColor();
                        ActiveMusicNow = userMusicHist;
                    }
                    else
                    {
                        userMusicHist.MusicTextColor = Color.Black;
                    }

                    PlayedHistory.Add(userMusicHist);
                }
            }
            else
            {
                foreach (UserMusicPlayedHistory userMusicHist in _lstUserHistory)
                {
                    userMusicHist.MusicTextColor = Color.Black;
                    PlayedHistory.Add(userMusicHist);
                }
            }

            PlayedHistoryIsVisible = PlayedHistory.Count > 0;

            if (_musicPlayedHistory.Count == 1)
            {
                PlayedHistoryCollectionSize = 46;
                PlayedHistoryCollectionTotalItens = 1;
            }
            else if (_musicPlayedHistory.Count > 1)
            {
                PlayedHistoryCollectionSize = 100;
                PlayedHistoryCollectionTotalItens = 2;
            }
            else
            {
                PlayedHistoryCollectionSize = 0;
                PlayedHistoryCollectionTotalItens = 1;
            }
        }
        public string[] FilterUserSearchHistory(string term)
        {
            return FilterUserSearchHistory(_userSearchHistory, term);
        }
        public string[] FilterUserSearchHistory(List<string> lstFilters)
        {
            return FilterUserSearchHistory(_userSearchHistory, lstFilters);
        }
        public async Task SaveLocalSearchHistory(string musicName)
        {
            await SaveLocalSearchHistory(_userSearchHistory, musicName, _dbSearchedKey);
        }
        public async Task SaveLocalHistory(ICommonMusicModel musicPlayer, byte[] byteMusicImage)
        {
            UserMusicPlayedHistory userMusicHistory = new UserMusicPlayedHistory()
            {
                VideoId = musicPlayer.VideoId,
                ByteImgMusic = byteMusicImage,
                MusicName = musicPlayer.MusicName,
                SearchType = musicPlayer.SearchType,
                DateTimeIn = DateTimeOffset.UtcNow.ToString()
            };

            UserMusicPlayedHistory userMusicPlayed = _lstUserHistory.ToList()
                                                                    .Where(mp => string.Equals(mp.VideoId, musicPlayer.VideoId))
                                                                    .FirstOrDefault();

            if (_lstUserHistory == null)
            {
                _lstUserHistory = new List<UserMusicPlayedHistory>();
                _lstUserHistory.Add(userMusicHistory);

                if (musicPlayer.IsActiveMusic)
                {
                    musicPlayer.IsHistoryPlayedSavedOnLocalDb = true;
                    await _pclStorageDb.SaveFile(_dbPlayedKey, _lstUserHistory);
                }

                _lstUserHistory.Clear();
                await LoadPlayedHistory();
            }
            else if (userMusicPlayed == null)
            {
                _lstUserHistory.Add(userMusicHistory);
                _lstUserHistory = _lstUserHistory.OrderByDescending(uh => uh.DtIn)
                                                 .Take(UserPlayedHistoryTotalReg)
                                                 .ToList();

                if (musicPlayer.IsActiveMusic)
                {
                    musicPlayer.IsHistoryPlayedSavedOnLocalDb = true;
                    await _pclStorageDb.SaveFile(_dbPlayedKey, _lstUserHistory);
                }

                _lstUserHistory.Clear();
                await LoadPlayedHistory();
            }

            if (userMusicPlayed != null)
            {
                userMusicPlayed.DateTimeIn = DateTimeOffset.UtcNow.ToString();
                userMusicPlayed.SearchType = musicPlayer.SearchType;

                _lstUserHistory = _lstUserHistory.OrderByDescending(uh => uh.DtIn)
                                                 .Take(UserPlayedHistoryTotalReg)
                                                 .ToList();

                if (musicPlayer.IsActiveMusic)
                {
                    musicPlayer.IsHistoryPlayedSavedOnLocalDb = true;
                    await _pclStorageDb.SaveFile(_dbPlayedKey, _lstUserHistory);
                }

                _lstUserHistory.Clear();
                await LoadPlayedHistory(userMusicPlayed);
            }
        }

        #region Private Methods
        private Command<HistoryMusicModel> MusicHistoryDownloadFormEventCommand()
        {
            return new Command<HistoryMusicModel>(
                execute: (userMusic) =>
                {
                    if (userMusic.FormDownloadSize > 0)
                    {
                        PlayedHistoryPlayerFormSize -= userMusic.FormDownloadSize;
                        userMusic.FormDownloadSize = 0;
                    }
                    else
                    {
                        userMusic.FormDownloadSize = 38;
                        PlayedHistoryPlayerFormSize += userMusic.FormDownloadSize;
                    }
                }
            );
        }
        #endregion
    }
}
