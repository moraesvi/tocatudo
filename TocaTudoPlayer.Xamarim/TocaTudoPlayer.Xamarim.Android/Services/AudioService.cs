using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.Media.Session;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Media;
using Android.Support.V4.Media.Session;
using AndroidX.Core.App;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Source;
using Com.Google.Android.Exoplayer2.Trackselection;
using Com.Google.Android.Exoplayer2.Upstream;
using Com.Google.Android.Exoplayer2.Util;
using Java.Net;
using Plugin.CurrentActivity;
using System;
using System.Collections.Generic;
using System.Linq;
using TocaTudo;
using TocaTudoPlayer.Xamarim;
using Xamarin.Forms;
using static Android.Media.AudioManager;

[assembly: Dependency(typeof(AudioService))]
namespace TocaTudo
{
    [Service]
    [IntentFilter(new[] { ActionSource, ActionPlay, ActionPause, ActionStop, ActionNext, ActionPrevious, ActionTogglePlayback, ActionSeek, ActionHeadphonesUnplugged })]
    public class AudioService : Service, IOnAudioFocusChangeListener, IPlayerEventListener, IDataSourceFactory
    {
        private const string TAG = "TocaTudo";
        private const string CHANNEL_ID = "toca_tudo_channel_01";

        private const int PLAYER_TYPE_ALBUM = 0;
        private const int PLAYER_TYPE_MUSIC = 1;

        public const string ActionSource = "com.xamarin.action.SOURCE";
        public const string ActionPlay = "com.xamarin.action.PLAY";
        public const string ActionPause = "com.xamarin.action.PAUSE";
        public const string ActionStop = "com.xamarin.action.STOP";
        public const string ActionNext = "com.xamarin.action.NEXT";
        public const string ActionPrevious = "com.xamarin.action.PREVIOUS";
        public const string ActionTogglePlayback = "com.xamarin.action.TOGGLEPLAYBACK";
        public const string ActionSeek = "com.xamarin.action.SEEK";
        public const string ActionHeadphonesUnplugged = "com.xamarin.action.HEADPHONES_UNPLUGGED";

        private event Action _playerInitializing;
        private event Action _playerReady;
        private event Action _playerReadyBuffering;
        private event Action _playerSeekComplete;
        private event Action<PlaylistItemServicePlayer> _playerPlaylistChanged;
        private event Action _playerInvalidUri;
        private event Action _playerLosedAudioFocus;

        private SimpleExoPlayer _exoPlayer;
        public NotificationManager _notificationManager;
        //private AudioManager _audioManager;

        private MusicBroadcast _broadcastReceiver;

        private bool _isPlaying;
        private bool _rebuildTimerPlayer;

        private string _url;

        private Context _context;
        private IMediaSource _extractorMediaSource;
        private ByteArrayDataSource _dataSource;

        private AudioManager _audioManager;
        private MediaSessionManager _mediaSessionManager;
        private MediaSessionCompat _mediaSession;
        private MediaControllerCompat _mediaController;
        private Android.Support.V4.Media.Session.MediaControllerCompat.TransportControls _transportControls;

        private AlbumModelServicePlayer _albumModel;
        private MusicModelServicePlayer _musicModel;
        private PlaylistItemServicePlayer _playlistItem;
        private MainActivity _mainActivity;

        private int _playlistMusicActiveIndex;

        public AudioService()
        {
            _context = Android.App.Application.Context;
            _broadcastReceiver = new MusicBroadcast(this);
            _mainActivity = CrossCurrentActivity.Current.Activity as MainActivity;
        }
        internal MediaSessionCompat MediaSessionCompat => _mediaSession;
        internal Android.Support.V4.Media.Session.MediaControllerCompat.TransportControls TransportControls => _transportControls;
        internal AlbumModelServicePlayer AlbumModel => _albumModel;
        internal bool RebuildTimerPlayer
        {
            get => _rebuildTimerPlayer;
            set => _rebuildTimerPlayer = value;
        }
        internal int PlaylistMusicActiveIndex => _playlistMusicActiveIndex;
        public bool IsPlaying
        {
            get
            {
                return _isPlaying;
            }
            set
            {
                _isPlaying = value;
            }
        }
        public event Action PlayerInitializing
        {
            add
            {
                _playerInitializing += value;
            }
            remove
            {
                _playerInitializing -= value;
            }
        }
        public event Action PlayerReady
        {
            add
            {
                _playerReady += value;
            }
            remove
            {
                _playerReady -= value;
            }
        }
        public event Action PlayerReadyBuffering
        {
            add
            {
                _playerReadyBuffering += value;
            }
            remove
            {
                _playerReadyBuffering -= value;
            }
        }
        public event Action PlayerSeekComplete
        {
            add
            {
                _playerSeekComplete += value;
            }
            remove
            {
                _playerSeekComplete -= value;
            }
        }
        public event Action<PlaylistItemServicePlayer> PlayerPlaylistChanged
        {
            add => _playerPlaylistChanged += value;
            remove => _playerPlaylistChanged -= value;
        }
        public event Action PlayerInvalidUri
        {
            add => _playerInvalidUri += value;
            remove => _playerInvalidUri -= value;
        }
        public event Action PlayerLosedAudioFocus
        {
            add => _playerLosedAudioFocus += value;
            remove => _playerLosedAudioFocus -= value;
        }
        public override void OnCreate()
        {
            base.OnCreate();

            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                CreateChannel();
            }
        }
        private void CreateChannel()
        {
            NotificationChannel channel = new NotificationChannel(CHANNEL_ID, "TocaTudo", NotificationImportance.Default);
            channel.SetSound(null, null);
            channel.EnableLights(true);

            ((NotificationManager)GetSystemService(Context.NotificationService)).CreateNotificationChannel(channel);
        }
        private void InitMediaSession()
        {
            if (_mediaSessionManager != null)
                return;

            _mediaSessionManager = (MediaSessionManager)GetSystemService(Context.MediaSessionService);
            _mediaSession = new MediaSessionCompat(ApplicationContext, "TocaTudo");
            _transportControls = _mediaSession.Controller.GetTransportControls();

            _mediaSession.Active = true;
            _mediaSession.SetFlags(MediaSessionCompat.FlagHandlesTransportControls);

            _mediaSession.SetCallback(new MediaSessionCallback(this));

            _mediaController = _mediaSession.Controller;
            _mediaController.RegisterCallback(new MediaControllerCallback(this));

            switch (GetPlayerType())
            {
                case PLAYER_TYPE_ALBUM:
                    _mediaSession.SetPlaybackState(new PlaybackStateCompat.Builder()
                                                      .SetActions(PlaybackState.ActionPlayPause | PlaybackState.ActionPlay | PlaybackState.ActionSkipToNext)
                                                      .Build());
                    break;
                case PLAYER_TYPE_MUSIC:
                    _mediaSession.SetPlaybackState(new PlaybackStateCompat.Builder()
                                                      .SetActions(PlaybackState.ActionPlayPause | PlaybackState.ActionPlay)
                                                      .Build());
                    break;
            }
        }
        private int GetPlayerType()
        {
            if (_mainActivity.AlbumModelServicePlayerParameter != null)
                return PLAYER_TYPE_ALBUM;
            else if (_mainActivity.MusicModelServicePlayerParameter != null)
                return PLAYER_TYPE_MUSIC;

            return PLAYER_TYPE_ALBUM;
        }
        private void StopNotification()
        {
            _notificationManager?.CancelAll();
        }
        internal void BuildNotification(bool isPlaying, int indicePlaylist, AlbumModelServicePlayer albumModel, PlaylistItemServicePlayer playlistItem)
        {
            NotificationCompat.Builder builder = null;

            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                builder = new NotificationCompat.Builder(this, CHANNEL_ID);
            }
            else
            {
                builder = new NotificationCompat.Builder(this);
            }

            BuildNotificationVisible(builder, albumModel, playlistItem);
            BuildNotificationVisibleActions(builder, isPlaying, indicePlaylist, albumModel, playlistItem);
        }
        internal void BuildNotification(bool isPlaying, MusicModelServicePlayer musicModel)
        {
            NotificationCompat.Builder builder = null;

            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                builder = new NotificationCompat.Builder(this, CHANNEL_ID);
            }
            else
            {
                builder = new NotificationCompat.Builder(this);
            }

            BuildNotificationVisible(builder, musicModel);
            BuildNotificationVisibleActions(builder, isPlaying, musicModel);
        }
        private void BuildNotificationVisible(NotificationCompat.Builder builder, AlbumModelServicePlayer albumModel, PlaylistItemServicePlayer playlistItem)
        {
            Bitmap albumArt = BitmapFactory.DecodeByteArray(albumModel.Image, 0, albumModel.Image.Length);

            builder.SetSmallIcon(Resource.Drawable.icon)
                   .SetContentIntent(_mediaSession.Controller.SessionActivity)
                   .SetVisibility(NotificationCompat.VisibilityPublic)
                   .SetLargeIcon(albumArt)
                   .SetContentText(albumModel.AlbumName)
                   .SetContentTitle(playlistItem.Music);
        }
        private void BuildNotificationVisible(NotificationCompat.Builder builder, MusicModelServicePlayer musicModel)
        {
            Bitmap albumArt = BitmapFactory.DecodeByteArray(musicModel.Image, 0, musicModel.Image.Length);

            builder.SetSmallIcon(Resource.Drawable.icon)
                   .SetContentIntent(_mediaSession.Controller.SessionActivity)
                   .SetVisibility(NotificationCompat.VisibilityPublic)
                   .SetLargeIcon(albumArt)
                   .SetContentTitle(musicModel.Music);
        }
        private void BuildNotificationVisibleActions(NotificationCompat.Builder builder, bool isPlaying, int indicePlaylist, AlbumModelServicePlayer albumModel, PlaylistItemServicePlayer playlistItem)
        {
            AndroidX.Media.App.NotificationCompat.MediaStyle mediaStyle = new AndroidX.Media.App.NotificationCompat
                                                                                                .MediaStyle()
                                                                                                .SetMediaSession(_mediaSession.SessionToken);
            Intent previousIntent = new Intent();
            previousIntent.SetAction(ActionPrevious);
            PendingIntent pPreviousIntent = PendingIntent.GetBroadcast(ApplicationContext, 0, previousIntent, PendingIntentFlags.UpdateCurrent);

            Intent nextIntent = new Intent();
            nextIntent.SetAction(ActionNext);
            PendingIntent pNextIntent = PendingIntent.GetBroadcast(ApplicationContext, 0, nextIntent, PendingIntentFlags.UpdateCurrent);

            builder.AddAction(new NotificationCompat.Action.Builder(Resource.Drawable.exo_icon_previous, ActionPrevious, pPreviousIntent).Build());
            BuildNotificationActionPlayPause(builder, isPlaying);
            builder.AddAction(new NotificationCompat.Action.Builder(Resource.Drawable.exo_icon_next, ActionNext, pNextIntent).Build());

            if (indicePlaylist > 0)
                mediaStyle.SetShowActionsInCompactView(0, 1, 2);
            else if (indicePlaylist < albumModel.Playlist.Count() - 1)
                mediaStyle.SetShowActionsInCompactView(1, 2);

            builder.SetStyle(mediaStyle);

            IntentFilter intentFilter = new IntentFilter();
            intentFilter.AddAction(ActionPlay);
            intentFilter.AddAction(ActionPause);
            intentFilter.AddAction(ActionPrevious);
            intentFilter.AddAction(ActionNext);

            ApplicationContext.RegisterReceiver(_broadcastReceiver, intentFilter);

            Notification notification = builder.Build();

            _notificationManager = (NotificationManager)GetSystemService(Context.NotificationService);
            _notificationManager.Notify(1, notification);

            StartForeground(1, notification);
        }
        private void BuildNotificationVisibleActions(NotificationCompat.Builder builder, bool isPlaying, MusicModelServicePlayer musicModel)
        {
            AndroidX.Media.App.NotificationCompat.MediaStyle mediaStyle = new AndroidX.Media.App.NotificationCompat
                                                                                                .MediaStyle()
                                                                                                .SetMediaSession(_mediaSession.SessionToken);

            BuildNotificationActionPlayPause(builder, isPlaying);

            mediaStyle.SetShowActionsInCompactView(0);

            builder.SetStyle(mediaStyle);

            IntentFilter intentFilter = new IntentFilter();
            intentFilter.AddAction(ActionPlay);
            intentFilter.AddAction(ActionPause);

            ApplicationContext.RegisterReceiver(_broadcastReceiver, intentFilter);

            Notification notification = builder.Build();

            _notificationManager = (NotificationManager)GetSystemService(Context.NotificationService);
            _notificationManager.Notify(1, notification);

            StartForeground(1, notification);
        }
        private void BuildNotificationActionPlayPause(NotificationCompat.Builder builder, bool isPlaying)
        {
            if (!isPlaying)
            {
                Intent playIntent = new Intent();
                playIntent.SetAction(ActionPlay);
                PendingIntent pPlayIntent = PendingIntent.GetBroadcast(ApplicationContext, 0, playIntent, PendingIntentFlags.UpdateCurrent);

                builder.AddAction(new NotificationCompat.Action.Builder(Resource.Drawable.exo_icon_play, ActionPlay, pPlayIntent).Build());
            }
            else
            {
                Intent pauseIntent = new Intent();
                pauseIntent.SetAction(ActionPause);
                PendingIntent pPauseIntent = PendingIntent.GetBroadcast(ApplicationContext, 0, pauseIntent, PendingIntentFlags.UpdateCurrent);

                builder.AddAction(new NotificationCompat.Action.Builder(Resource.Drawable.exo_icon_pause, ActionPause, pPauseIntent).Build());
            }
        }
        internal void UpdateMetaData(AlbumModelServicePlayer albumModel)
        {
            PlaylistItemServicePlayer startPlaylistItem = albumModel.Playlist[0];
            Bitmap albumArt = BitmapFactory.DecodeByteArray(albumModel.Image, 0, albumModel.Image.Length);

            _mediaSession.SetMetadata(new MediaMetadataCompat.Builder()
                         .PutBitmap(MediaMetadataCompat.MetadataKeyAlbumArt, albumArt)
                         .PutString(MediaMetadataCompat.MetadataKeyArtist, albumModel.AlbumName)
                         .PutString(MediaMetadataCompat.MetadataKeyTitle, startPlaylistItem.Music)
                         .PutLong(MediaMetadataCompat.MetadataKeyDuration, startPlaylistItem.TotalMilliseconds * 1000)
                         .PutLong(MediaMetadataCompat.MetadataKeyTrackNumber, 1)
                         .PutLong(MediaMetadataCompat.MetadataKeyNumTracks, albumModel.Playlist.Count())
                         .Build());
        }
        internal void UpdateMetaData(AlbumModelServicePlayer albumModel, PlaylistItemServicePlayer playlistItem)
        {
            Bitmap albumArt = BitmapFactory.DecodeByteArray(albumModel.Image, 0, albumModel.Image.Length);

            _mediaSession.SetMetadata(new MediaMetadataCompat.Builder()
                         .PutBitmap(MediaMetadataCompat.MetadataKeyAlbumArt, albumArt)
                         .PutString(MediaMetadataCompat.MetadataKeyArtist, albumModel.AlbumName)
                         .PutString(MediaMetadataCompat.MetadataKeyTitle, playlistItem.Music)
                         .PutLong(MediaMetadataCompat.MetadataKeyDuration, playlistItem.TotalMilliseconds * 1000)
                         .PutLong(MediaMetadataCompat.MetadataKeyNumTracks, albumModel.Playlist.Count())
                         .Build());
        }
        internal void UpdateMetaData(MusicModelServicePlayer musicModel)
        {
            Bitmap albumArt = BitmapFactory.DecodeByteArray(musicModel.Image, 0, musicModel.Image.Length);

            _mediaSession.SetMetadata(new MediaMetadataCompat.Builder()
                         .PutBitmap(MediaMetadataCompat.MetadataKeyAlbumArt, albumArt)
                         .PutString(MediaMetadataCompat.MetadataKeyTitle, musicModel.Music)
                         .PutLong(MediaMetadataCompat.MetadataKeyDuration, _exoPlayer.Duration)
                         .PutLong(MediaMetadataCompat.MetadataKeyNumTracks, musicModel.Number)
                         .Build());
        }
        internal void StartMediaPlaybackState(bool isPlaying)
        {
            if (isPlaying)
            {
                PlaybackStateCompat.Builder playbackstateBuilder = new PlaybackStateCompat.Builder()
                                                                                          .SetState(PlaybackStateCompat.StatePlaying, 0, 1F);
                _mediaSession.SetPlaybackState(playbackstateBuilder.Build());
            }
        }
        internal void SetMediaPlaybackState(bool isPlaying, bool rebuildTimerPlayer, int indexPlaylist, AlbumModelServicePlayer albumModel)
        {
            PlaybackStateCompat.Builder playbackstateBuilder = new PlaybackStateCompat.Builder();

            bool lastItemFromPlaylist = indexPlaylist >= albumModel.Playlist.Count() - 1;

            if (isPlaying)
            {
                if (indexPlaylist == 0)
                {
                    playbackstateBuilder.SetActions(PlaybackStateCompat.ActionPlayPause | PlaybackStateCompat.ActionPause | PlaybackStateCompat.ActionSkipToNext);
                }
                else if (indexPlaylist > 0)
                {
                    if (lastItemFromPlaylist)
                    {
                        playbackstateBuilder.SetActions(PlaybackStateCompat.ActionPlayPause | PlaybackStateCompat.ActionPause | PlaybackStateCompat.ActionSkipToPrevious);
                    }
                    else
                    {
                        playbackstateBuilder.SetActions(PlaybackStateCompat.ActionPlayPause | PlaybackStateCompat.ActionPause | PlaybackStateCompat.ActionSkipToNext | PlaybackStateCompat.ActionSkipToPrevious);
                    }
                }
            }
            if (!isPlaying)
            {
                if (indexPlaylist == 0)
                {
                    playbackstateBuilder.SetActions(PlaybackStateCompat.ActionPlayPause | PlaybackStateCompat.ActionPlay | PlaybackStateCompat.ActionSkipToNext);
                }
                else if (indexPlaylist > 0)
                {
                    if (lastItemFromPlaylist)
                        playbackstateBuilder.SetActions(PlaybackStateCompat.ActionPlayPause | PlaybackStateCompat.ActionPause | PlaybackStateCompat.ActionSkipToPrevious);
                    else
                        playbackstateBuilder.SetActions(PlaybackStateCompat.ActionPlayPause | PlaybackStateCompat.ActionPause | PlaybackStateCompat.ActionSkipToNext | PlaybackStateCompat.ActionSkipToPrevious);
                }
            }

            if (rebuildTimerPlayer && IsPlaying)
                playbackstateBuilder.SetState(PlaybackStateCompat.StatePlaying, 0, 1F);

            _mediaSession.SetPlaybackState(playbackstateBuilder.Build());
        }
        internal void SetMediaPlaybackState(bool isPlaying, bool rebuildTimerPlayer)
        {
            PlaybackStateCompat.Builder playbackstateBuilder = new PlaybackStateCompat.Builder();

            playbackstateBuilder.SetActions(PlaybackStateCompat.ActionPlayPause | PlaybackStateCompat.ActionPause);

            if (rebuildTimerPlayer && IsPlaying)
            {
                playbackstateBuilder.SetState(PlaybackStateCompat.StatePlaying, 0, 1F);
                _rebuildTimerPlayer = false;
            }

            _mediaSession.SetPlaybackState(playbackstateBuilder.Build());
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            RequestAudioFocus();

            if (_mediaSessionManager == null)
            {
                InitMediaSession();
            }

            switch (intent.Action)
            {
                case ActionSource:
                    SetMusicSource(intent);
                    break;
                case ActionPlay:
                    Play();
                    break;
                case ActionStop:
                    Stop();
                    break;
                case ActionPause:
                    Pause();
                    break;
                case ActionTogglePlayback:
                    if (_exoPlayer == null)
                        return StartCommandResult.Sticky;

                    if (_exoPlayer.IsPlaying)
                        Pause();
                    else
                        Play();

                    break;
                case ActionSeek:
                    int iSource = intent.GetIntExtra("seek", 0);

                    switch (GetPlayerType())
                    {
                        case PLAYER_TYPE_ALBUM:

                            if (_mainActivity.PlaylistItemServicePlayerParameter != null)
                                Seek(iSource, _mainActivity.PlaylistItemServicePlayerParameter);
                            else if (_mainActivity.MusicModelServicePlayerParameter != null)
                                Seek(iSource, _mainActivity.PlaylistItemServicePlayerParameter);
                            break;

                        case PLAYER_TYPE_MUSIC:
                            Seek(iSource);
                            break;
                        default:
                            Seek(iSource);
                            break;
                    }
                    break;
            }

            //Set sticky as we are a long running operation
            return StartCommandResult.Sticky;
        }
        public override IBinder OnBind(Intent intent)
        {
            AudioServiceBinder binder = new AudioServiceBinder(this);
            return binder;
        }
        public override bool OnUnbind(Intent intent)
        {
            _exoPlayer?.Stop();
            _exoPlayer?.Release();
            _exoPlayer = null;

            StopSelf();
            return base.OnUnbind(intent);
        }
        public override void OnDestroy()
        {
            StopNotification();
            base.OnDestroy();
        }
        internal void Source(string url, AlbumModelServicePlayer albumModel)
        {
            if (albumModel == null)
                return;

            MusicSource(url);

            _albumModel = albumModel;
            _playlistItem = _albumModel.Playlist.FirstOrDefault();

            UpdateMetaData(_albumModel);
            BuildNotification(false, 0, _albumModel, _playlistItem);
        }
        internal void Source(byte[] music, AlbumModelServicePlayer albumModel)
        {
            if (albumModel == null)
                return;

            MusicSource(music);

            _albumModel = albumModel;
            _playlistItem = _albumModel.Playlist.FirstOrDefault();

            UpdateMetaData(_albumModel);
            BuildNotification(false, 0, _albumModel, _playlistItem);
        }
        internal void Source(string url, MusicModelServicePlayer musicModel)
        {
            if (musicModel == null)
                return;

            MusicSource(url);

            _musicModel = musicModel;
            _rebuildTimerPlayer = true;

            UpdateMetaData(_musicModel);
            BuildNotification(false, musicModel);
        }
        internal void Source(byte[] music, MusicModelServicePlayer musicModel)
        {
            if (musicModel == null)
                return;

            MusicSource(music);

            _musicModel = musicModel;
            _rebuildTimerPlayer = true;

            UpdateMetaData(_musicModel);
            BuildNotification(false, musicModel);
        }
        public bool Play()
        {
            if (_exoPlayer != null)
            {
                _exoPlayer.PlayWhenReady = true;
            }

            return true;
        }
        public void Pause()
        {
            if (_exoPlayer != null)
            {
                _exoPlayer.PlayWhenReady = false;
            }
        }
        public void Seek(int milliseconds)
        {
            if (_exoPlayer != null)
            {
                IsPlaying = false;
                _exoPlayer.SeekTo(milliseconds);
            }
        }
        public void Seek(int milliseconds, PlaylistItemServicePlayer playlistItem)
        {
            if (_exoPlayer != null)
            {
                IsPlaying = false;

                if (!string.Equals(playlistItem.AlbumId, _albumModel.AlbumId))
                    throw new InvalidOperationException("Id de álbum inválido");

                _exoPlayer.SeekTo(milliseconds);

                _playlistItem = playlistItem;
                _playlistMusicActiveIndex = _albumModel.Playlist.Select((Item, Index) => (Item, Index))
                                                                .Where(item => item.Item.Number == _playlistItem.Number)
                                                                .FirstOrDefault()
                                                                .Index;

                //StartMediaPlaybackState(IsPlaying);
            }
        }
        public long Max()
        {
            return _exoPlayer.Duration;
        }
        public long CurrentPosition()
        {
            return _exoPlayer.CurrentPosition;
        }
        public bool Stop()
        {
            if (_exoPlayer == null)
                return false;

            if (_exoPlayer.IsPlaying)
            {
                _exoPlayer.Volume = 0.5f;
                _exoPlayer.Stop();
            }

            return true;
        }
        public void OnLoadingChanged(bool p0)
        {

        }
        public void OnPlayerError(ExoPlaybackException error)
        {
            switch (error.Type)
            {
                case ExoPlaybackException.TypeSource:
                    _playerInvalidUri();
                    break;
            }
        }
        public void OnPlayerStateChanged(bool playWhenReady, int playbackState)
        {
            if (playbackState == 1)//Player.StateIdle
            {
                IsPlaying = _exoPlayer.IsPlaying;
            }
            else if (playbackState == 2)//Player.StateBuffering
            {
                if (_playerReadyBuffering != null)
                {
                    IsPlaying = _exoPlayer.IsPlaying;

                    if (_exoPlayer.BufferedPercentage > 0)
                        _playerReadyBuffering();
                }
            }
            else if (playbackState == 3)//Player.StateReady
            {
                if (_playerReady != null)
                {
                    IsPlaying = _exoPlayer.IsPlaying;

                    switch (GetPlayerType())
                    {
                        case PLAYER_TYPE_ALBUM:

                            if (IsPlaying)
                            {
                                _playlistMusicActiveIndex = _albumModel.Playlist.Select((Item, Index) => (Item, Index))
                                                                                .Where(item => item.Item.Number == _playlistItem.Number)
                                                                                .FirstOrDefault()
                                                                                .Index;

                                UpdateMetaData(_albumModel, _playlistItem);
                                BuildNotification(true, _playlistMusicActiveIndex, _albumModel, _playlistItem);
                            }

                            SetMediaPlaybackState(IsPlaying, _rebuildTimerPlayer, _playlistMusicActiveIndex, _albumModel);

                            break;
                        case PLAYER_TYPE_MUSIC:

                            if (IsPlaying)
                            {
                                UpdateMetaData(_musicModel);
                                BuildNotification(IsPlaying, _musicModel);
                            }

                            SetMediaPlaybackState(IsPlaying, _rebuildTimerPlayer);

                            break;
                    }

                    _playerReady();
                }
            }
            else if (playbackState == 4)//Player.StateEnded
            {
                IsPlaying = _exoPlayer.IsPlaying;
            }
        }
        public void OnSeekProcessed()
        {
            if (_playerSeekComplete != null)
            {
                IsPlaying = _exoPlayer.IsPlaying;

                switch (GetPlayerType())
                {
                    case PLAYER_TYPE_ALBUM:

                        _playlistMusicActiveIndex = _albumModel.Playlist.Select((Item, Index) => (Item, Index))
                                                                        .Where(item => item.Item.Number == _playlistItem.Number)
                                                                        .FirstOrDefault()
                                                                        .Index;
                        _rebuildTimerPlayer = true;

                        UpdateMetaData(_albumModel, _playlistItem);
                        BuildNotification(IsPlaying, _playlistMusicActiveIndex, _albumModel, _playlistItem);
                        SetMediaPlaybackState(IsPlaying, _rebuildTimerPlayer, _playlistMusicActiveIndex, _albumModel);

                        break;
                    case PLAYER_TYPE_MUSIC:

                        UpdateMetaData(_musicModel);
                        BuildNotification(IsPlaying, _musicModel);
                        SetMediaPlaybackState(IsPlaying, _rebuildTimerPlayer);

                        break;
                }

                _playerSeekComplete();
            }
        }
        public void OnPositionDiscontinuity(int p1)
        {

        }
        public void OnTimelineChanged(Timeline p0, int p1)
        {

        }
        public void OnTracksChanged(TrackGroupArray p0, TrackSelectionArray p1)
        {

        }
        public void OnIsPlayingChanged(bool p)
        {

        }
        private void SetMusicSource(Intent intent)
        {
            if (intent.HasExtra("source-url"))
            {
                string source = intent.GetStringExtra("source-url");
                switch (GetPlayerType())
                {
                    case PLAYER_TYPE_ALBUM:
                        Source(source, _mainActivity.AlbumModelServicePlayerParameter);
                        break;
                    case PLAYER_TYPE_MUSIC:
                        Source(source, _mainActivity.MusicModelServicePlayerParameter);
                        break;
                }
            }
            if (intent.HasExtra("source-byte"))
            {
                bool source = intent.GetBooleanExtra("source-byte", false);
                if (source)
                {
                    switch (GetPlayerType())
                    {
                        case PLAYER_TYPE_ALBUM:
                            Source(_mainActivity.AudioServiceMusicParameter, _mainActivity.AlbumModelServicePlayerParameter);
                            break;
                        case PLAYER_TYPE_MUSIC:
                            Source(_mainActivity.AudioServiceMusicParameter, _mainActivity.MusicModelServicePlayerParameter);
                            break;
                    }
                }
            }
        }
        private void MusicSource(string url)
        {
            if (_exoPlayer != null)
            {
                _exoPlayer.Release();
                _exoPlayer.Dispose();
            }

            if (_playerInitializing != null)
                _playerInitializing();

            _url = url;

            _exoPlayer = new SimpleExoPlayer.Builder(this).Build();
            _exoPlayer.AddListener(this);

            Android.Net.Uri sourceUri = Android.Net.Uri.Parse(url);

            var userAgent = Util.GetUserAgent(_context, "TocaTudo");
            var defaultHttpDataSourceFactory = new DefaultHttpDataSourceFactory(userAgent);
            var defaultDataSourceFactory = new DefaultDataSourceFactory(_context, null, defaultHttpDataSourceFactory);

            _extractorMediaSource = new ProgressiveMediaSource.Factory(defaultDataSourceFactory).CreateMediaSource(sourceUri);
            _exoPlayer.Prepare(_extractorMediaSource);

            _exoPlayer.VolumeChanged += (sender, e) =>
            {
                if (IsPlaying)
                    Pause();
                else
                    Play();
            };
        }
        private void MusicSource(byte[] music)
        {
            if (_exoPlayer != null)
            {
                _exoPlayer.Volume = 0.5f;
                _exoPlayer.Stop();
                _exoPlayer.Release();
                _exoPlayer = null;
            }
            if (_dataSource != null)
            {
                _dataSource.Close();
                _dataSource.Dispose();
                _dataSource = null;
            }
            if (_extractorMediaSource != null)
            {
                _extractorMediaSource.UnregisterFromRuntime();
                _extractorMediaSource.Dispose();
                _extractorMediaSource = null;
            }

            if (_playerInitializing != null)
                _playerInitializing();

            _exoPlayer = new SimpleExoPlayer.Builder(this).Build();
            _exoPlayer.AddListener(this);

            _dataSource = new ByteArrayDataSource(music);
            Android.Net.Uri audioByteUri = new UriByteDataHelper().GetUri(music);

            DataSpec dataSpec = new DataSpec(audioByteUri);

            _dataSource.Open(dataSpec);

            IDataSourceFactory dataSourceFactory = new DefaultDataSourceFactory(_context, this);

            _extractorMediaSource = new ProgressiveMediaSource.Factory(dataSourceFactory).CreateMediaSource(audioByteUri);
            _exoPlayer.Prepare(_extractorMediaSource);

            _exoPlayer.VolumeChanged += (sender, e) =>
            {
                if (IsPlaying)
                    Pause();
                else
                    Play();
            };
        }
        internal bool RequestAudioFocus()
        {
            AudioFocusRequest? audioFocus = null;

            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                _audioManager = (AudioManager)GetSystemService(Context.AudioService);
                audioFocus = _audioManager.RequestAudioFocus(new AudioFocusRequestClass.Builder(AudioFocus.Gain).SetOnAudioFocusChangeListener(this).SetAcceptsDelayedFocusGain(true).Build());
            }
            else
            {
                _audioManager = (AudioManager)GetSystemService(Context.AudioService);
                audioFocus = _audioManager.RequestAudioFocus(this, Stream.Music, AudioFocus.Gain);
            }

            if (audioFocus == AudioFocusRequest.Granted)
            {
                return true;
            }

            return false;
        }
        private bool RemoveAudioFocus()
        {
            return AudioFocusRequest.Granted == _audioManager.AbandonAudioFocus(this);
        }

        private void Player_Prepared(object sender, EventArgs e)
        {
            _playerReady();
        }
        private void Player_SeekComplete(object sender, EventArgs e)
        {
            IsPlaying = _exoPlayer.IsPlaying;
            //_playerSeekComplete();
        }
        public IDataSource CreateDataSource()
        {
            return _dataSource;
        }
        public void OnAudioFocusChange([GeneratedEnum] AudioFocus focusChange)
        {
            if (_exoPlayer == null)
                return;

            switch (focusChange)
            {
                case AudioFocus.Gain:
                    _exoPlayer.Volume = 1.0f;
                    _playerLosedAudioFocus();
                    break;
                case AudioFocus.LossTransient:
                    _exoPlayer.Volume = 0.0f;
                    break;
                case AudioFocus.Loss:
                    Pause();
                    _exoPlayer.Volume = 1.0f;
                    _playerLosedAudioFocus();
                    break;
                case AudioFocus.LossTransientCanDuck:
                    _exoPlayer.Volume = 0.5f;
                    break;
            }
        }
        internal void HandleIncomingActions(string action)
        {
            switch (GetPlayerType())
            {
                case PLAYER_TYPE_ALBUM:
                    HandleIncomingPlayerAlbumActions(action);
                    break;
                case PLAYER_TYPE_MUSIC:
                    HandleIncomingPlayerMusicActions(action);
                    break;
            }
        }
        internal void HandleIncomingPlayerAlbumActions(string action)
        {
            if (string.IsNullOrWhiteSpace(action))
                return;

            int musicPlayingIndex = _albumModel.Playlist.Select((Item, Index) => (Item, Index))
                                                        .Where(item => item.Item.Number == _playlistItem.Number)
                                                        .FirstOrDefault()
                                                        .Index;

            switch (action)
            {
                case ActionPlay:

                    _playlistMusicActiveIndex = musicPlayingIndex;
                    _playerPlaylistChanged(_playlistItem);

                    UpdateMetaData(_albumModel, _playlistItem);
                    BuildNotification(true, musicPlayingIndex, _albumModel, _playlistItem);

                    _rebuildTimerPlayer = false;

                    break;
                case ActionStop:

                    _playerPlaylistChanged(_playlistItem);

                    UpdateMetaData(_albumModel, _playlistItem);
                    BuildNotification(false, musicPlayingIndex, _albumModel, _playlistItem);

                    _rebuildTimerPlayer = true;

                    break;
                case ActionPause:

                    _playerPlaylistChanged(_playlistItem);

                    UpdateMetaData(_albumModel, _playlistItem);
                    BuildNotification(false, musicPlayingIndex, _albumModel, _playlistItem);

                    _rebuildTimerPlayer = false;

                    break;
                case ActionPrevious:

                    if (musicPlayingIndex == 0)
                        _playlistItem = _albumModel.Playlist[musicPlayingIndex];
                    else
                    {
                        musicPlayingIndex -= 1;
                        _playlistItem = _albumModel.Playlist[musicPlayingIndex];
                    }

                    UpdateMetaData(_albumModel, _playlistItem);
                    BuildNotification(false, musicPlayingIndex, _albumModel, _playlistItem);

                    _playlistMusicActiveIndex = musicPlayingIndex;
                    _playerPlaylistChanged(_playlistItem);

                    _rebuildTimerPlayer = true;

                    break;
                case ActionNext:

                    if (musicPlayingIndex == _albumModel.Playlist.Count() - 1)
                        _playlistItem = _albumModel.Playlist[musicPlayingIndex];
                    else
                    {
                        musicPlayingIndex += 1;
                        _playlistItem = _albumModel.Playlist[musicPlayingIndex];
                    }

                    UpdateMetaData(_albumModel, _playlistItem);
                    BuildNotification(false, musicPlayingIndex, _albumModel, _playlistItem);

                    _playlistMusicActiveIndex = musicPlayingIndex;
                    _playerPlaylistChanged(_playlistItem);

                    _rebuildTimerPlayer = true;

                    break;
                case ActionTogglePlayback:
                    //if (_exoPlayer == null)
                    //    return StartCommandResult.Sticky;
                    //
                    //if (_exoPlayer.IsPlaying)
                    //    Pause();
                    //else
                    //    Play();

                    break;
                case ActionSeek:
                    //int iSource = intent.GetIntExtra("seek", 0);
                    //Seek(iSource);
                    break;
            }
        }
        internal void HandleIncomingPlayerMusicActions(string action)
        {
            if (string.IsNullOrWhiteSpace(action))
                return;

            switch (action)
            {
                case ActionPlay:

                    UpdateMetaData(_musicModel);
                    BuildNotification(true, _musicModel);

                    _rebuildTimerPlayer = false;

                    break;
                case ActionStop:

                    //_playerPlaylistChanged(_playlistItem);

                    UpdateMetaData(_musicModel);
                    BuildNotification(true, _musicModel);

                    _rebuildTimerPlayer = true;

                    break;
                case ActionPause:

                    //_playerPlaylistChanged(_playlistItem);

                    UpdateMetaData(_musicModel);
                    BuildNotification(false, _musicModel);

                    _rebuildTimerPlayer = false;

                    break;
                case ActionTogglePlayback:
                    //if (_exoPlayer == null)
                    //    return StartCommandResult.Sticky;
                    //
                    //if (_exoPlayer.IsPlaying)
                    //    Pause();
                    //else
                    //    Play();

                    break;
                case ActionSeek:
                    //int iSource = intent.GetIntExtra("seek", 0);
                    //Seek(iSource);
                    break;
            }
        }
    }
    public class MusicBroadcast : BroadcastReceiver
    {
        private AudioService _audioService;
        public MusicBroadcast(AudioService audioService)
        {
            _audioService = audioService;
        }
        public override void OnReceive(Context context, Intent intent)
        {
            switch (intent.Action)
            {
                case AudioService.ActionPlay:
                    _audioService.TransportControls.Play();
                    break;
                case AudioService.ActionStop:
                    _audioService.TransportControls.Stop();
                    break;
                case AudioService.ActionPause:
                    _audioService.TransportControls.Pause();
                    break;
                case AudioService.ActionPrevious:
                    _audioService.TransportControls.SkipToPrevious();
                    break;
                case AudioService.ActionNext:
                    _audioService.TransportControls.SkipToNext();
                    break;
                case AudioService.ActionTogglePlayback:
                    break;
                case AudioService.ActionSeek:
                    break;
            }
        }
    }
    public class MediaSessionCallback : MediaSessionCompat.Callback
    {
        private readonly AudioService _audioService;
        public Action OnPlayImpl { get; set; }
        public MediaSessionCallback(AudioService audioService)
        {
            _audioService = audioService;
        }
        public override void OnPlay()
        {
            if (!_audioService.RequestAudioFocus())
                return;

            _audioService.MediaSessionCompat.Active = true;
            _audioService.Play();
            _audioService.HandleIncomingActions(AudioService.ActionPlay);
            //_audioService.SetMediaPlaybackState(_audioService.IsPlaying, _audioService.RebuildTimerPlayer, _audioService.PlaylistMusicActiveIndex, _audioService.AlbumModel);

            _audioService.RebuildTimerPlayer = false;
        }
        public override void OnPause()
        {
            _audioService.Pause();
            _audioService.HandleIncomingActions(AudioService.ActionPause);
            //_audioService.SetMediaPlaybackState(_audioService.IsPlaying, _audioService.RebuildTimerPlayer, _audioService.PlaylistMusicActiveIndex, _audioService.AlbumModel);

            _audioService.RebuildTimerPlayer = false;
        }
        public override void OnSkipToNext()
        {
            _audioService.HandleIncomingActions(AudioService.ActionNext);

            _audioService.RebuildTimerPlayer = true;
        }
        public override void OnSkipToPrevious()
        {
            _audioService.HandleIncomingActions(AudioService.ActionPrevious);
            //_audioService.SetMediaPlaybackState(_audioService.IsPlaying, _audioService.RebuildTimerPlayer, _audioService.PlaylistMusicActiveIndex, _audioService.AlbumModel);

            _audioService.RebuildTimerPlayer = true;
        }
    }
    public class AudioBecommingNoisy : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {

        }
    }
    public class MediaControllerCallback : MediaControllerCompat.Callback
    {
        private AudioService _audioService;
        public MediaControllerCallback(AudioService audioService)
        {
            _audioService = audioService;
        }
        public override void OnPlaybackStateChanged(PlaybackStateCompat state)
        {
            switch (state.State)
            {
                case PlaybackStateCompat.StatePlaying:
                    //_audioService.SetMediaPlaybackState(_audioService.IsPlaying, 0, _audioService.AlbumModel);
                    break;
                case PlaybackStateCompat.StatePaused:
                    //_audioService.SetMediaPlaybackState(_audioService.IsPlaying, 0, _audioService.AlbumModel);
                    break;
                case PlaybackStateCompat.StateStopped:
                    //_audioService.HandleIncomingActions(AudioService.ActionStop);
                    break;
                case PlaybackStateCompat.StateSkippingToNext:
                    //_audioService.HandleIncomingActions(AudioService.ActionNext);
                    break;
                case PlaybackStateCompat.StateSkippingToPrevious:
                    //_audioService.HandleIncomingActions(AudioService.ActionPrevious);
                    break;
            }
        }
        public override void OnSessionDestroyed()
        {
            base.OnSessionDestroyed();
            //updateSessionToken();
        }
    }
    public class UriByteDataHelper
    {
        public Android.Net.Uri GetUri(byte[] data)
        {
            URL url = new URL(null, "bytes:///" + "audio", new BytesHandler(data));
            return Android.Net.Uri.Parse(url.ToURI().ToString());
        }
        public class BytesHandler : URLStreamHandler
        {
            byte[] mData;
            public BytesHandler(byte[] data)
            {
                mData = data;
            }
            protected override URLConnection OpenConnection(URL u)
            {
                return new ByteUrlConnection(u, mData);
            }
        }
    }

    public class ByteUrlConnection : URLConnection
    {
        private byte[] mData;
        public ByteUrlConnection(URL url, byte[] data)
         : base(url)
        {
            mData = data;
        }
        public override void Connect()
        {
        }
        public Java.IO.InputStream GetInputStream()
        {
            return new Java.IO.ByteArrayInputStream(mData);
        }
    }
    public class AudioFocusChangeListener : Java.Lang.Object, IOnAudioFocusChangeListener
    {
        public void OnAudioFocusChange([GeneratedEnum] AudioFocus focusChange)
        {
            switch (focusChange)
            {
                case AudioFocus.Loss:
                    break;
                case AudioFocus.LossTransient:
                    break;
                case AudioFocus.LossTransientCanDuck:
                    //if (mMediaPlayer != null)
                    //{
                    //mMediaPlayer.setVolume(0.3f, 0.3f);
                    //}
                    break;
                case AudioFocus.Gain:
                    //if (mMediaPlayer != null)
                    //{
                    //    if (!mMediaPlayer.isPlaying())
                    //    {
                    //        mMediaPlayer.start();
                    //    }
                    //    mMediaPlayer.setVolume(1.0f, 1.0f);
                    //}
                    break;
            }
        }
    }
}