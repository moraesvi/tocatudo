using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TocaTudoPlayer.Xamarim
{
    public class CommonMusicPageViewModel : BaseViewModel
    {
        private readonly MusicPlayedHistoryViewModel _musicPlayedHistoryViewModel;
        private readonly IPCLUserMusicDb _pclUserMusicDb;
        private readonly IPCLStorageDb _pclStorageDb;

        private const string USER_MUSIC_ALBUM_SELECT_KEY = "mas_select.json";
        private ObservableCollection<SearchMusicModel> _musicPlaylist;
        private ObservableCollection<SelectModel> _albumMusicSavedSelectCollection;
        private ObservableCollection<SelectModel> _albumMusicSavedSelectFilteredCollection;
        private List<UserMusicAlbumSelect> _musicAlbumPlaylistSelected;
        public CommonMusicPageViewModel(MusicPlayedHistoryViewModel musicPlayedHistoryViewModel, IPCLUserMusicDb pclUserMusicDb, IPCLStorageDb pclStorageDb)
        {
            _musicPlayedHistoryViewModel = musicPlayedHistoryViewModel;
            _pclUserMusicDb = pclUserMusicDb;
            _pclStorageDb = pclStorageDb;
            _albumMusicSavedSelectCollection = new ObservableCollection<SelectModel>();
            _albumMusicSavedSelectFilteredCollection = new ObservableCollection<SelectModel>();
        }
        public ObservableCollection<SearchMusicModel> MusicPlaylist
        {
            get { return _musicPlaylist; }
            set
            {
                _musicPlaylist = value;
                OnPropertyChanged(nameof(MusicPlaylist));
            }
        }
        public List<UserMusicAlbumSelect> MusicAlbumPlaylistSelected
        {
            get => _musicAlbumPlaylistSelected;
        }
        public ObservableCollection<SelectModel> AlbumMusicSavedSelectCollection
        {
            get { return _albumMusicSavedSelectCollection; }
            set
            {
                _albumMusicSavedSelectCollection = value;
                OnPropertyChanged(nameof(AlbumMusicSavedSelectCollection));
            }
        }
        public ObservableCollection<SelectModel> AlbumMusicSavedSelectFilteredCollection
        {
            get { return _albumMusicSavedSelectFilteredCollection; }
            set
            {
                _albumMusicSavedSelectFilteredCollection = value;
                OnPropertyChanged(nameof(AlbumMusicSavedSelectFilteredCollection));
            }
        }
        public void InitFormMusicUtils(MusicAlbumDialogDataModel music)
        {
            music.ExistsAnyAlbumSaved = HasAnyAlbumSaved();
        }
        public bool HasAlbumSaved(string videoId)
        {
            if (_musicAlbumPlaylistSelected == null || _musicAlbumPlaylistSelected.Count == 0)
                return false;

            return _musicAlbumPlaylistSelected.Exists(ma =>
            {
                bool hasAlbum = ma.MusicsModel.Exists(mm => string.Equals(mm.VideoId, videoId));
                return hasAlbum;
            });
        }
        public UserMusicAlbumSelect[] GetAlbumSaved(string videoId)
        {
            if (_musicAlbumPlaylistSelected == null || _musicAlbumPlaylistSelected.Count == 0)
                return new UserMusicAlbumSelect[] { };

            return _musicAlbumPlaylistSelected.Where(ma =>
            {
                bool hasAlbum = ma.MusicsModel.Exists(mm => string.Equals(mm.VideoId, videoId));
                return hasAlbum;
            }).ToArray();
        }
        public async Task<UserMusicAlbumSelect[]> GetMusicAlbumPlaylistSelected()
        {
            return await _pclStorageDb.GetJson<UserMusicAlbumSelect[]>(USER_MUSIC_ALBUM_SELECT_KEY) ?? new UserMusicAlbumSelect[] { };
        }
        public async Task LoadMusicAlbumPlaylistSelected()
        {
            _musicAlbumPlaylistSelected = await _pclStorageDb.GetJson<List<UserMusicAlbumSelect>>(USER_MUSIC_ALBUM_SELECT_KEY) ?? new List<UserMusicAlbumSelect>() { };

            int countEqualsAlbumId = _musicAlbumPlaylistSelected.GroupBy(ma => ma.Id)
                                                                .Where(ma => ma.Count() > 1)
                                                                .Count();

            if (countEqualsAlbumId >= 1) //FIXING - In production the album id must be equals
            {
                _musicAlbumPlaylistSelected = _musicAlbumPlaylistSelected.Select((ma, index) =>
                                                                         {
                                                                             ma.Id = (short)(index + 1);
                                                                             return ma;
                                                                         }).ToList();
            }


            LoadAlbumMusicSavedSelect();
        }
        public void LoadAlbumMusicSavedSelect()
        {
            if (_musicAlbumPlaylistSelected == null)
                return;

            AlbumMusicSavedSelectCollection.Clear();
            AlbumMusicSavedSelectFilteredCollection.Clear();

            foreach (UserMusicAlbumSelect musicAlbum in _musicAlbumPlaylistSelected)
            {
                AlbumMusicSavedSelectCollection.Add(new SelectModel(musicAlbum.Id, musicAlbum.AlbumName));
                AlbumMusicSavedSelectFilteredCollection.Add(new SelectModel(musicAlbum.Id, musicAlbum.AlbumName));
            }
        }
        public void AddAlbumMusicSavedSelect(UserMusicAlbumSelect musicAlbum)
        {
            if (_musicAlbumPlaylistSelected == null)
                return;

            AlbumMusicSavedSelectCollection.Add(new SelectModel(musicAlbum.Id, musicAlbum.AlbumName));
            AlbumMusicSavedSelectFilteredCollection.Add(new SelectModel(musicAlbum.Id, musicAlbum.AlbumName));

            _musicAlbumPlaylistSelected.Add(musicAlbum);
        }
        public async Task<bool> ExistsMusicAlbumPlaylist(string albumName, ICommonMusicModel music)
        {
            List<UserMusicAlbumSelect> musicAlbumPlaylist = await _pclStorageDb.GetJson<List<UserMusicAlbumSelect>>(USER_MUSIC_ALBUM_SELECT_KEY);

            if (musicAlbumPlaylist == null)
                return false;

            UserMusicAlbumSelect musicAlbum = musicAlbumPlaylist.Where(ma => string.Equals(ma.AlbumName, albumName, StringComparison.OrdinalIgnoreCase))
                                                                .FirstOrDefault();
            if (musicAlbum == null)
                return false;

            return musicAlbum.MusicsModel.Exists(ma => string.Equals(ma.VideoId, music.VideoId));
        }
        public async Task InsertOrUpdateMusicAlbumPlaylistSelected(string albumName, UserMusic music, List<UserMusicAlbumSelect> musicAlbumPlaylist)
        {
            if (music == null || musicAlbumPlaylist == null)
                return;

            UserMusicAlbumSelect musicAlbumInsert = musicAlbumPlaylist.Where(ma => string.Equals(ma.AlbumName, albumName, StringComparison.OrdinalIgnoreCase))
                                                                      ?.FirstOrDefault();

            if (musicAlbumInsert != null)
            {
                UserMusicSelect userMusicUpdate = musicAlbumInsert.MusicsModel.Where(ma => string.Equals(ma.VideoId, music.VideoId))
                                                                              .FirstOrDefault();
                if (userMusicUpdate != null)
                {
                    userMusicUpdate.MusicName = music.MusicName;
                    userMusicUpdate.MusicTime = music.MusicTime;
                    userMusicUpdate.MusicImage = music.MusicImage;
                }

                await _pclStorageDb.SaveFile(USER_MUSIC_ALBUM_SELECT_KEY, musicAlbumPlaylist);
            }
        }
        public async Task InsertOrUpdateMusicAlbumPlaylistSelected(string albumName, ICommonMusicModel music)
        {
            if (music == null)
                return;

            List<UserMusicAlbumSelect> musicAlbumPlaylist = await _pclStorageDb.GetJson<List<UserMusicAlbumSelect>>(USER_MUSIC_ALBUM_SELECT_KEY);
            UserMusicAlbumSelect musicAlbumInsert = null;

            await SerializeMusicAlbumImage(music);

            if (musicAlbumPlaylist == null)
            {
                musicAlbumPlaylist = new List<UserMusicAlbumSelect>();
                musicAlbumInsert = new UserMusicAlbumSelect()
                {
                    Id = 1,
                    AlbumName = albumName,
                    TimestampIn = DateTimeOffset.Now.ToUnixTimeSeconds()
                };

                musicAlbumInsert.MusicsModel.Add(new UserMusicSelect()
                {
                    Id = music.Id,
                    MusicName = music.MusicName,
                    MusicTime = music.MusicTime,
                    MusicImage = music.ByteMusicImage,
                    VideoId = music.VideoId
                });
                musicAlbumPlaylist.Add(musicAlbumInsert);

                await _pclStorageDb.SaveFile(USER_MUSIC_ALBUM_SELECT_KEY, musicAlbumPlaylist);
            }
            else
            {
                musicAlbumInsert = new UserMusicAlbumSelect()
                {
                    Id = (short)(musicAlbumPlaylist.Count + 1),
                    AlbumName = AppHelper.ToTitleCase(albumName),
                    TimestampIn = DateTimeOffset.Now.ToUnixTimeSeconds()
                };

                musicAlbumInsert.MusicsModel.Add(new UserMusicSelect()
                {
                    Id = music.Id,
                    MusicName = music.MusicName,
                    MusicTime = music.MusicTime,
                    MusicImage = music.ByteMusicImage,
                    VideoId = music.VideoId
                });
                musicAlbumPlaylist.Add(musicAlbumInsert);

                await _pclStorageDb.SaveFile(USER_MUSIC_ALBUM_SELECT_KEY, musicAlbumPlaylist);
            }

            AddAlbumMusicSavedSelect(musicAlbumInsert);
            //music.MusicAlbumPopupModel.SetAlbumMode(musicAlbumInsert);
        }

        private static async Task SerializeMusicAlbumImage(ICommonMusicModel music)
        {
            if (music.ByteMusicImage == null && !string.IsNullOrEmpty(music.MusicImageUrl))
            {
                await music.MusicModel.LoadMusicImageInfo(music.MusicImageUrl);
                music.ByteMusicImage = music.MusicModel.MusicImage;
            }

            if (music.ByteMusicImage == null)
                throw new InvalidOperationException("Was not possible to define a valid music image");
        }
        public async Task<int> MusicAlbumTotalPlaylist(int albumId)
        {
            List<UserMusicAlbumSelect> musicAlbumPlaylist = await _pclStorageDb.GetJson<List<UserMusicAlbumSelect>>(USER_MUSIC_ALBUM_SELECT_KEY);

            UserMusicAlbumSelect album = musicAlbumPlaylist.Where(ma => ma.Id == albumId)
                                                           .FirstOrDefault();

            return album?.MusicsModel?.Count() ?? 0;
        }
        public async Task<bool> DeleteAlbum(int albumId)
        {
            List<UserMusicAlbumSelect> musicAlbumPlaylist = await _pclStorageDb.GetJson<List<UserMusicAlbumSelect>>(USER_MUSIC_ALBUM_SELECT_KEY);

            SelectModel albumSelected = _albumMusicSavedSelectCollection.Where(am => am.Id == albumId)
                                                                        .FirstOrDefault();

            SelectModel albumFilteredSelected = _albumMusicSavedSelectFilteredCollection.Where(am => am.Id == albumId)
                                                                                        .FirstOrDefault();

            UserMusicAlbumSelect album = musicAlbumPlaylist.Where(ma => ma.Id == albumId)
                                                           .FirstOrDefault();

            if (album != null)
            {
                AlbumMusicSavedSelectCollection?.Remove(albumSelected);
                AlbumMusicSavedSelectFilteredCollection?.Remove(albumFilteredSelected);

                musicAlbumPlaylist.Remove(album);
                await _pclStorageDb.SaveFile(USER_MUSIC_ALBUM_SELECT_KEY, musicAlbumPlaylist);

                return true;
            }

            return false;
        }
        public async Task DeleteDownloadedMusic(ICommonMusicModel musicModel)
        {
            await _pclUserMusicDb.RemoveMusicFromLocalDb(musicModel.VideoId, musicRemoved: async () =>
            {
                musicModel.IsSavedOnLocalDb = false;

                if (_musicPlayedHistoryViewModel.HistoryMusicPlayingNow != null)
                    _musicPlayedHistoryViewModel.HistoryMusicPlayingNow.IsSavedOnLocalDb = false;

                await _musicPlayedHistoryViewModel.LoadPlayedHistory();
            });
        }
        public async Task UpdateMusicAlbumPlaylistSelected(int albumId, ICommonMusicModel musicModel)
        {
            if (_musicAlbumPlaylistSelected == null || _musicAlbumPlaylistSelected.Count == 0 || musicModel == null)
                return;

            await SerializeMusicAlbumImage(musicModel);
            await DeleteMusicFromAlbumPlaylist(musicModel);

            foreach (UserMusicAlbumSelect ma in _musicAlbumPlaylistSelected)
            {
                var mModel = ma.MusicsModel.Where(m => string.Equals(m?.VideoId, musicModel.VideoId))
                                           .FirstOrDefault();

                if (ma.Id == albumId)
                {
                    ma.MusicsModel.Add(new UserMusicSelect()
                    {
                        Id = musicModel.Id,
                        MusicName = musicModel.MusicName,
                        MusicTime = musicModel.MusicTime,
                        MusicImage = musicModel.ByteMusicImage,
                        VideoId = musicModel.VideoId
                    });
                }
            }

            await _pclStorageDb.SaveFile(USER_MUSIC_ALBUM_SELECT_KEY, _musicAlbumPlaylistSelected);
        }
        public async Task DeleteMusicFromAlbumPlaylist(ICommonMusicModel musicModel)
        {
            if (_musicAlbumPlaylistSelected == null || _musicAlbumPlaylistSelected.Count == 0 || musicModel == null)
                return;

            foreach (UserMusicAlbumSelect ma in _musicAlbumPlaylistSelected)
            {
                var mModel = ma.MusicsModel?.Where(m => string.Equals(m?.VideoId, musicModel.VideoId))
                                           ?.FirstOrDefault();
                if (mModel != null)
                {
                    ma.MusicsModel.Remove(mModel);
                }
            }

            await _pclStorageDb.SaveFile(USER_MUSIC_ALBUM_SELECT_KEY, _musicAlbumPlaylistSelected);

            musicModel.MusicAlbumPopupModel.SelectMusicAlbum(null);
            musicModel.MusicAlbumPopupModel.SetNormalMode();
        }

        #region Private Methods
        private bool HasAnyAlbumSaved()
        {
            if (_musicAlbumPlaylistSelected == null || _musicAlbumPlaylistSelected.Count == 0)
                return false;

            return _musicAlbumPlaylistSelected.Count() > 0;
        }
        #endregion
    }
}
