using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TocaTudoPlayer.Xamarim
{
    public class CommonMusicPageViewModel : BaseViewModel, ICommonMusicPageViewModel
    {
        private readonly IPCLStorageDb _pclStorageDb;

        private const string USER_MUSIC_ALBUM_SELECT_KEY = "mas_select.json";

        private ObservableCollection<SelectModel> _albumMusicSavedSelectCollection;
        private List<UserMusicAlbumSelect> _musicAlbumPlaylistSelected;
        private bool _existsSavedAnyAlbum;
        public CommonMusicPageViewModel(IPCLStorageDb pclStorageDb)
        {
            _pclStorageDb = pclStorageDb;
            _albumMusicSavedSelectCollection = new ObservableCollection<SelectModel>();
        }
        public bool ExistsSavedAnyAlbum
        {
            get { return _existsSavedAnyAlbum; }
        }
        public UserMusicAlbumSelect[] MusicAlbumPlaylistSelected 
        {
            get => _musicAlbumPlaylistSelected?.ToArray();
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
        public void InitFormMusicUtils(SearchMusicModel music)
        {
            music.InitFormMusicUtils(_existsSavedAnyAlbum);
        }
        public async Task LoadMusicAlbumPlaylistSelected()
        {
            _musicAlbumPlaylistSelected = await _pclStorageDb.GetJson<List<UserMusicAlbumSelect>>(USER_MUSIC_ALBUM_SELECT_KEY);

            LoadAlbumMusicSavedSelect();
        }
        public void LoadAlbumMusicSavedSelect()
        {
            if (_musicAlbumPlaylistSelected == null)
                return;

            AlbumMusicSavedSelectCollection.Clear();

            foreach (UserMusicAlbumSelect musicAlbum in _musicAlbumPlaylistSelected)
            {
                AlbumMusicSavedSelectCollection.Add(new SelectModel(musicAlbum.Id, musicAlbum.AlbumName));
            }


            _existsSavedAnyAlbum = _musicAlbumPlaylistSelected?.Count > 0;
        }
        public async Task<bool> ExistsMusicAlbumPlaylist(string albumName, SearchMusicModel music)
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
        public async Task InsertMusicAlbumPlaylistSelected(string albumName, SearchMusicModel music)
        {
            List<UserMusicAlbumSelect> musicAlbumPlaylist = await _pclStorageDb.GetJson<List<UserMusicAlbumSelect>>(USER_MUSIC_ALBUM_SELECT_KEY);

            UserMusicAlbumSelect musicAlbumInsert = null;

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
                    VideoId = music.VideoId
                });
                musicAlbumPlaylist.Add(musicAlbumInsert);

                await _pclStorageDb.SaveFile(USER_MUSIC_ALBUM_SELECT_KEY, musicAlbumPlaylist);
            }
            else
            {
                musicAlbumInsert = musicAlbumPlaylist.Where(ma => string.Equals(ma.AlbumName, albumName, StringComparison.OrdinalIgnoreCase))
                                                     .FirstOrDefault();

                if (musicAlbumInsert != null)
                {
                    if (musicAlbumInsert.MusicsModel.Exists(ma => string.Equals(ma.VideoId, music.VideoId)))
                        return;

                    musicAlbumInsert.Id = (short)(musicAlbumPlaylist.Count + 1);

                    musicAlbumInsert.MusicsModel.Add(new UserMusicSelect()
                    {
                        Id = music.Id,
                        MusicName = music.MusicName,
                        VideoId = music.VideoId
                    });
                    musicAlbumPlaylist.Add(musicAlbumInsert);
                }
                else
                {
                    musicAlbumInsert = new UserMusicAlbumSelect()
                    {
                        Id = (short)(musicAlbumPlaylist.Count + 1),
                        AlbumName = AppHelper.FirstLetterToUpper(albumName),
                        TimestampIn = DateTimeOffset.Now.ToUnixTimeSeconds()
                    };

                    musicAlbumInsert.MusicsModel.Add(new UserMusicSelect()
                    {
                        Id = music.Id,
                        MusicName = music.MusicName,
                        VideoId = music.VideoId
                    });
                    musicAlbumPlaylist.Add(musicAlbumInsert);
                }

                await _pclStorageDb.SaveFile(USER_MUSIC_ALBUM_SELECT_KEY, musicAlbumPlaylist);
            }

            await LoadMusicAlbumPlaylistSelected();
            LoadAlbumMusicSavedSelect();

            music.SetAlbumMode(musicAlbumInsert);

            _existsSavedAnyAlbum = true;
        }
        public async Task UpdateMusicAlbumPlaylistSelected(int albumId, string albumName, SearchMusicModel musicModel)
        {
            if (_musicAlbumPlaylistSelected == null || _musicAlbumPlaylistSelected.Count == 0 || musicModel == null)
                return;

            SelectModel selectModel = null;

            _musicAlbumPlaylistSelected.ForEach(ma =>
            {
                var mModel = ma.MusicsModel.Where(m => string.Equals(m.VideoId, musicModel.VideoId))
                                           .FirstOrDefault();

                if (mModel != null)
                {
                    ma.MusicsModel.Remove(mModel);
                }
                if (ma.Id == albumId)
                {
                    ma.MusicsModel.Add(new UserMusicSelect()
                    {
                        Id = musicModel.Id,
                        MusicName = musicModel.MusicName,
                        VideoId = musicModel.VideoId
                    });

                    selectModel = new SelectModel(ma.Id, ma.AlbumName);
                }
            });

            await _pclStorageDb.SaveFile(USER_MUSIC_ALBUM_SELECT_KEY, _musicAlbumPlaylistSelected);

            musicModel.SelectMusicAlbum(selectModel);
            musicModel.SetAlbumMode();
        }
        public async Task DeleteMusicAlbumPlaylistSelected(SearchMusicModel musicModel)
        {
            if (_musicAlbumPlaylistSelected == null || _musicAlbumPlaylistSelected.Count == 0 || musicModel == null)
                return;

            _musicAlbumPlaylistSelected.ForEach(ma =>
            {
                var mModel = ma.MusicsModel.Where(m => string.Equals(m.VideoId, musicModel.VideoId))
                                           .FirstOrDefault();

                if (mModel != null)
                {
                    ma.MusicsModel.Remove(mModel);
                }
            });

            await _pclStorageDb.SaveFile(USER_MUSIC_ALBUM_SELECT_KEY, _musicAlbumPlaylistSelected);

            musicModel.SelectMusicAlbum(null);
            musicModel.SetNormalMode();
        }
    }
}
