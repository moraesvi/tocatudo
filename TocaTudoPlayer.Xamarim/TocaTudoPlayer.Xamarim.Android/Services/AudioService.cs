using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Hardware.Lights;
using Android.Media;
using Android.Media.Session;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Media;
using Android.Support.V4.Media.Session;
using AndroidX.Core.App;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Audio;
using Com.Google.Android.Exoplayer2.Source;
using Com.Google.Android.Exoplayer2.Trackselection;
using Com.Google.Android.Exoplayer2.Upstream;
using Plugin.CurrentActivity;
using System;
using System.Linq;
using TocaTudo;
using TocaTudoPlayer.Xamarim;
using Xamarin.CommunityToolkit.Helpers;
using Xamarin.Forms;
using static Android.Media.AudioManager;
using static Com.Google.Android.Exoplayer2.IPlayer;

namespace TocaTudo
{
    [Service(Exported = true)]
    [IntentFilter(new[] { ActionSource, ActionPlay, ActionPause, ActionStop, ActionNext, ActionPrevious, ActionTogglePlayback, ActionSeek, ActionHeadphonesUnplugged, ActionClearSource, ActionHidePlayerControls })]
    public class AudioService : Service, IOnAudioFocusChangeListener, IEventListener, IDataSource.IFactory
    {
        private const string TAG = "TocaTudo";
        private const string CHANNEL_ID = "toca_tudo_channel_01";

        private const int PLAYER_TYPE_UNDEFINED = -1;
        private const int PLAYER_TYPE_ALBUM = 0;
        private const int PLAYER_TYPE_MUSIC = 1;
        private const int PLAYER_TYPE_ALBUM_MUSIC = 2;

        public const string ActionSource = "com.xamarin.action.SOURCE";
        public const string ActionPlay = "com.xamarin.action.PLAY";
        public const string ActionPause = "com.xamarin.action.PAUSE";
        public const string ActionStop = "com.xamarin.action.STOP";
        public const string ActionNext = "com.xamarin.action.NEXT";
        public const string ActionPrevious = "com.xamarin.action.PREVIOUS";
        public const string ActionTogglePlayback = "com.xamarin.action.TOGGLEPLAYBACK";
        public const string ActionSeek = "com.xamarin.action.SEEK";
        public const string ActionHeadphonesUnplugged = "com.xamarin.action.HEADPHONES_UNPLUGGED";
        public const string ActionClearSource = "com.xamarin.action.CLEAR_SOURCE";
        public const string ActionHidePlayerControls = "com.xamarin.action.HIDE_PLAYER_CONTROLS";

        private readonly WeakEventManager _playerInitializing;
        private readonly WeakEventManager _playerReady;
        private readonly WeakEventManager _playerReadyBuffering;
        private readonly WeakEventManager _playerSeekComplete;
        private readonly WeakEventManager<bool> _playingChanged;
        private readonly WeakEventManager<ItemServicePlayer> _playerPlaylistChanged;
        private readonly WeakEventManager _playerInvalidUri;
        private readonly WeakEventManager _playerLosedAudioFocus;
        private static WeakEventManager<Exception> _playerException;

        private static SimpleExoPlayer _exoPlayer;
        public NotificationManager _notificationManager;

        private MusicBroadcast _broadcastReceiver;

        private bool _isPlaying;
        private bool _stopMediaCompatPlayerNavigation;
        private bool _rebuildTimerPlayer;
        private bool _callPlayerActiveEvent;
        private bool _exceptionThrown;

        private Context _context;
        private IMediaSource _extractorMediaSource;
        private ByteArrayDataSource _dataSource;

        private AudioManager _audioManager;
        private MediaSessionManager _mediaSessionManager;
        private MediaSessionCompat _mediaSession;
        private MediaControllerCompat _mediaController;
        private MediaControllerCompat.TransportControls _transportControls;

        private (string AlbumName, string MusicName, byte[] Image) _tuppleAlbum;
        private (string AlbumName, string MusicName, long TotalMilliseconds, byte[] Image) _tuppleAlbumDetails;

        private MusicModelServicePlayer _musicModel;

        private ItemServicePlayer[] _albumPlaylist;
        private ItemServicePlayer _playlistItem;
        private static MainActivity _mainActivity;

        private int _playlistMusicActiveIndex;
        private int _lastPlaylistMusicActivedIndex;

        public AudioService()
        {
            _context = Android.App.Application.Context;
            _broadcastReceiver = new MusicBroadcast(this);
            _mainActivity = CrossCurrentActivity.Current.Activity as MainActivity;

            _playerInitializing = new WeakEventManager();
            _playerReady = new WeakEventManager();
            _playerReadyBuffering = new WeakEventManager();
            _playerSeekComplete = new WeakEventManager();
            _playingChanged = new WeakEventManager<bool>();
            _playerPlaylistChanged = new WeakEventManager<ItemServicePlayer>();
            _playerInvalidUri = new WeakEventManager();
            _playerLosedAudioFocus = new WeakEventManager();

            _lastPlaylistMusicActivedIndex = -1;
            _stopMediaCompatPlayerNavigation = false;
        }
        public static void Init()
        {
            _playerException = new WeakEventManager<Exception>();
            _mainActivity = CrossCurrentActivity.Current.Activity as MainActivity;
        }
        public static void Reset()
        {
            if (_exoPlayer != null)
            {
                _exoPlayer.Volume = 0.5f;
                _exoPlayer.Stop();
                _exoPlayer.Release();
                _exoPlayer = null;
            }
        }
        internal MediaSessionCompat MediaSessionCompat => _mediaSession;
        internal MediaControllerCompat.TransportControls TransportControls => _transportControls;
        /// <summary>
        internal bool AlbumMusicModelMode => _albumPlaylist?.Count() > 0;
        /// </summary>
        //internal AlbumMusicModelServicePlayer AlbumMusicModel => _albumMusicModel;
        internal MusicModelServicePlayer MusicModelService => _musicModel;
        internal ItemServicePlayer PlaylistItemService => _playlistItem;
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
                return _isPlaying || (_exoPlayer?.IsPlaying ?? false);
            }
            set
            {
                _isPlaying = value;
            }
        }
        public event EventHandler PlayerInitializing
        {
            add => _playerInitializing.AddEventHandler(value);
            remove => _playerInitializing.RemoveEventHandler(value);
        }
        public event EventHandler PlayerReady
        {
            add => _playerReady.AddEventHandler(value);
            remove => _playerReady.RemoveEventHandler(value);
        }
        public event EventHandler PlayerReadyBuffering
        {
            add => _playerReadyBuffering.AddEventHandler(value);
            remove => _playerReadyBuffering.RemoveEventHandler(value);
        }
        public event EventHandler PlayerSeekComplete
        {
            add => _playerSeekComplete.AddEventHandler(value);
            remove => _playerSeekComplete.RemoveEventHandler(value);
        }
        public event EventHandler<bool> PlayingChanged
        {
            add => _playingChanged.AddEventHandler(value);
            remove => _playingChanged.RemoveEventHandler(value);
        }
        public event EventHandler<ItemServicePlayer> PlayerPlaylistChanged
        {
            add => _playerPlaylistChanged.AddEventHandler(value);
            remove => _playerPlaylistChanged.RemoveEventHandler(value);
        }
        public event EventHandler PlayerInvalidUri
        {
            add => _playerInvalidUri.AddEventHandler(value);
            remove => _playerInvalidUri.RemoveEventHandler(value);
        }
        public event EventHandler PlayerLosedAudioFocus
        {
            add => _playerLosedAudioFocus.AddEventHandler(value);
            remove => _playerLosedAudioFocus.RemoveEventHandler(value);
        }
        public static event EventHandler<Exception> PlayerException
        {
            add => _playerException.AddEventHandler(value);
            remove => _playerException.RemoveEventHandler(value);
        }
        public override void OnCreate()
        {
            base.OnCreate();

            if (_exoPlayer != null)
            {
                _exoPlayer.Volume = 0.5f;
                _exoPlayer.Stop();
                _exoPlayer.Release();
                _exoPlayer = null;
            }

            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                CreateChannel();

                Notification notification = new NotificationCompat.Builder(this, CHANNEL_ID)
                                                                  .SetContentTitle("")
                                                                  .SetContentText("")
                                                                  .Build();

                StartForeground(1, notification);
            }
        }
        private void CreateChannel()
        {
            NotificationChannel channel = new NotificationChannel(CHANNEL_ID, TAG, NotificationImportance.Default);
            channel.SetSound(null, null);
            channel.EnableLights(true);

            ((NotificationManager)GetSystemService(NotificationService)).CreateNotificationChannel(channel);
        }
        private void InitMediaSession()
        {
            if (_mediaSessionManager != null)
                return;

            _mediaSessionManager = (MediaSessionManager)GetSystemService(MediaSessionService);
            _mediaSession = new MediaSessionCompat(ApplicationContext, TAG);
            _transportControls = _mediaSession.Controller.GetTransportControls();

            _mediaSession.Active = true;
            _mediaSession.SetFlags(MediaSessionCompat.FlagHandlesTransportControls);

            _mediaSession.SetCallback(new MediaSessionCallback(this));

            _mediaController = _mediaSession.Controller;
            _mediaController.RegisterCallback(new MediaControllerCallback(this));

            switch (GetPlayerType())
            {
                case PLAYER_TYPE_ALBUM_MUSIC:
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
            if (MainActivity.AlbumModelServicePlayerParameter != null)
                return PLAYER_TYPE_ALBUM;
            else if (MainActivity.MusicModelServicePlayerParameter != null)
                return PLAYER_TYPE_MUSIC;
            else if (MainActivity.AlbumMusicModelServicePlayerParameter != null)
                return PLAYER_TYPE_ALBUM_MUSIC;

            return PLAYER_TYPE_UNDEFINED;
        }
        private void StopNotification()
        {
            _notificationManager?.Cancel(1);
            _notificationManager?.CancelAll();
        }
        internal void BuildNotification(bool isPlaying, int indicePlaylist, (string AlbumName, string MusicName, byte[] Image) tuppleDetails, ItemServicePlayer[] playlist)
        {
            if (tuppleDetails.Image == null)
                return;
            if (playlist == null || playlist.Count() == 0)
                return;

            NotificationCompat.Builder builder = null;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                builder = new NotificationCompat.Builder(this, CHANNEL_ID);
            }
            else
            {
                builder = new NotificationCompat.Builder(this);
            }

            BuildNotificationVisible(builder, tuppleDetails);
            BuildNotificationVisibleActions(builder, isPlaying, indicePlaylist, playlist.Count() - 1);
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
        private void BuildNotificationVisible(NotificationCompat.Builder builder, (string AlbumName, string MusicName, byte[] Image) tuppleAlbum)
        {
            Bitmap albumArt = BitmapFactory.DecodeByteArray(tuppleAlbum.Image, 0, tuppleAlbum.Image?.Length ?? 0);

            builder.SetSmallIcon(Resource.Drawable.icon)
                   .SetContentIntent(_mediaSession.Controller.SessionActivity)
                   .SetVisibility(NotificationCompat.VisibilityPublic)
                   .SetLargeIcon(GetResizedBitmap(albumArt, 400))
                   .SetContentText(tuppleAlbum.AlbumName ?? string.Empty)
                   .SetContentTitle(tuppleAlbum.MusicName);
        }
        public Bitmap GetResizedBitmap(Bitmap bm, int width)
        {
            double aspectRatio = bm.Width / (float)bm.Height;
            double height = Math.Round(width / aspectRatio);

            Bitmap resizedBitmap = Bitmap.CreateScaledBitmap(bm, width, (int)height, false);

            return resizedBitmap;
        }
        private void BuildNotificationVisible(NotificationCompat.Builder builder, MusicModelServicePlayer musicModel)
        {
            if (musicModel == null || musicModel.Image == null)
                return;

            Bitmap albumArt = BitmapFactory.DecodeByteArray(musicModel?.Image, 0, musicModel?.Image?.Length ?? 0);

            builder.SetSmallIcon(Resource.Drawable.icon)
                   .SetContentIntent(_mediaSession.Controller.SessionActivity)
                   .SetVisibility(NotificationCompat.VisibilityPublic)
                   .SetLargeIcon(GetResizedBitmap(albumArt, 400))
                   .SetContentTitle(musicModel.Music);
        }
        private void BuildHideNotification((string AlbumName, string MusicName, byte[] Image) tuppleAlbum)
        {
            if (string.IsNullOrEmpty(tuppleAlbum.AlbumName))
                return;

            NotificationCompat.Builder builder = null;

            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                builder = new NotificationCompat.Builder(this, CHANNEL_ID);
            }
            else
            {
                builder = new NotificationCompat.Builder(this);
            }

            builder.SetSmallIcon(Resource.Drawable.icon)
                   .SetContentIntent(_mediaSession.Controller.SessionActivity)
                   .SetVisibility(NotificationCompat.VisibilityPublic);

            BuildHideNotificationActions(builder);
        }
        private void BuildNotificationVisibleActions(NotificationCompat.Builder builder, bool isPlaying, int indicePlaylist, int totalItens)
        {
            AndroidX.Media.App.NotificationCompat.MediaStyle mediaStyle = new AndroidX.Media.App.NotificationCompat
                                                                                                .MediaStyle()
                                                                                                .SetMediaSession(_mediaSession.SessionToken);
            Intent previousIntent = new Intent();
            previousIntent.SetAction(ActionPrevious);
            PendingIntent pPreviousIntent = PendingIntent.GetBroadcast(ApplicationContext, 0, previousIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            Intent nextIntent = new Intent();
            nextIntent.SetAction(ActionNext);
            PendingIntent pNextIntent = PendingIntent.GetBroadcast(ApplicationContext, 0, nextIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            IntentFilter intentFilter = new IntentFilter();

            if (_albumPlaylist.Count() == 1)
            {
                BuildNotificationActionPlayPause(builder, isPlaying);

                mediaStyle.SetShowActionsInCompactView(0);

                builder.SetStyle(mediaStyle);

                intentFilter.AddAction(ActionPlay);
                intentFilter.AddAction(ActionPause);
            }
            else
            {
                builder.AddAction(new NotificationCompat.Action.Builder(Resource.Drawable.exo_icon_previous, ActionPrevious, pPreviousIntent).Build());
                BuildNotificationActionPlayPause(builder, isPlaying);
                builder.AddAction(new NotificationCompat.Action.Builder(Resource.Drawable.exo_icon_next, ActionNext, pNextIntent).Build());

                if (indicePlaylist == totalItens)
                    mediaStyle.SetShowActionsInCompactView(0, 1);
                else if (indicePlaylist > 0)
                    mediaStyle.SetShowActionsInCompactView(0, 1, 2);
                else if (indicePlaylist < totalItens)
                    mediaStyle.SetShowActionsInCompactView(1, 2);

                builder.SetStyle(mediaStyle);

                intentFilter.AddAction(ActionPlay);
                intentFilter.AddAction(ActionPause);
                intentFilter.AddAction(ActionPrevious);
                intentFilter.AddAction(ActionNext);
            }

            ApplicationContext.RegisterReceiver(_broadcastReceiver, intentFilter);

            Notification notification = builder.Build();

            _notificationManager = (NotificationManager)GetSystemService(NotificationService);
            _notificationManager.Notify(1, notification);

            StartForeground(1, notification);
        }
        private void BuildNotificationVisibleActions(NotificationCompat.Builder builder, bool isPlaying, MusicModelServicePlayer musicModel)
        {
            if (musicModel == null)
                return;

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

            _notificationManager = (NotificationManager)GetSystemService(NotificationService);
            _notificationManager.Notify(1, notification);

            StartForeground(1, notification);
        }
        private void BuildNotificationActionPlayPause(NotificationCompat.Builder builder, bool isPlaying)
        {
            if (!isPlaying)
            {
                Intent playIntent = new Intent();
                playIntent.SetAction(ActionPlay);
                PendingIntent pPlayIntent = PendingIntent.GetBroadcast(ApplicationContext, 0, playIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

                builder.AddAction(new NotificationCompat.Action.Builder(Resource.Drawable.exo_icon_play, ActionPlay, pPlayIntent).Build());
            }
            else
            {
                Intent pauseIntent = new Intent();
                pauseIntent.SetAction(ActionPause);
                PendingIntent pPauseIntent = PendingIntent.GetBroadcast(ApplicationContext, 0, pauseIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

                builder.AddAction(new NotificationCompat.Action.Builder(Resource.Drawable.exo_icon_pause, ActionPause, pPauseIntent).Build());
            }
        }
        private void BuildHideNotificationActions(NotificationCompat.Builder builder)
        {
            AndroidX.Media.App.NotificationCompat.MediaStyle mediaStyle = new AndroidX.Media.App.NotificationCompat
                                                                                                .MediaStyle()
                                                                                                .SetMediaSession(_mediaSession.SessionToken);
            //mediaStyle.SetShowActionsInCompactView(0);
            builder.SetStyle(mediaStyle);

            Notification notification = builder.Build();

            _notificationManager = (NotificationManager)GetSystemService(NotificationService);
            _notificationManager.Notify(1, notification);

            StartForeground(1, notification);
        }
        internal void UpdateMetaData((string AlbumName, string MusicName, long TotalMilliseconds, byte[] Image) tuppleAlbum, ItemServicePlayer[] playlist, int trackNumber)
        {
            if (tuppleAlbum.Image == null)
                return;

            Bitmap albumArt = BitmapFactory.DecodeByteArray(tuppleAlbum.Image, 0, tuppleAlbum.Image?.Length ?? 0);

            _mediaSession.SetMetadata(new MediaMetadataCompat.Builder()
                         .PutBitmap(MediaMetadataCompat.MetadataKeyAlbumArt, albumArt)
                         .PutString(MediaMetadataCompat.MetadataKeyArtist, tuppleAlbum.AlbumName)
                         .PutString(MediaMetadataCompat.MetadataKeyTitle, tuppleAlbum.MusicName)
                         .PutLong(MediaMetadataCompat.MetadataKeyDuration, AppHelper.ExoplayerTimeToTocaTudo(tuppleAlbum.TotalMilliseconds))
                         .PutLong(MediaMetadataCompat.MetadataKeyTrackNumber, trackNumber + 1)
                         .PutLong(MediaMetadataCompat.MetadataKeyNumTracks, playlist.Count())
                         .Build());
        }
        internal void UpdateMetaData((string AlbumName, string MusicName, long TotalMilliseconds, byte[] Image) tuppleAlbum, PlaylistItemServicePlayer[] playlistItem, int trackNumber)
        {
            if (tuppleAlbum.Image == null)
                return;

            Bitmap albumArt = BitmapFactory.DecodeByteArray(tuppleAlbum.Image, 0, tuppleAlbum.Image.Length);

            _mediaSession.SetMetadata(new MediaMetadataCompat.Builder()
                         .PutBitmap(MediaMetadataCompat.MetadataKeyAlbumArt, albumArt)
                         .PutString(MediaMetadataCompat.MetadataKeyArtist, tuppleAlbum.AlbumName)
                         .PutString(MediaMetadataCompat.MetadataKeyTitle, tuppleAlbum.MusicName)
                         .PutLong(MediaMetadataCompat.MetadataKeyDuration, AppHelper.ExoplayerTimeToTocaTudo(tuppleAlbum.TotalMilliseconds))
                         .PutLong(MediaMetadataCompat.MetadataKeyTrackNumber, trackNumber + 1)
                         .PutLong(MediaMetadataCompat.MetadataKeyNumTracks, playlistItem.Count())
                         .Build());
        }
        internal void UpdateMetaData(MusicModelServicePlayer musicModel)
        {
            if (musicModel == null || musicModel.Image == null)
                return;

            Bitmap albumArt = BitmapFactory.DecodeByteArray(musicModel?.Image, 0, musicModel?.Image?.Length ?? 0);

            _mediaSession.SetMetadata(new MediaMetadataCompat.Builder()
                         .PutBitmap(MediaMetadataCompat.MetadataKeyAlbumArt, albumArt)
                         .PutString(MediaMetadataCompat.MetadataKeyTitle, musicModel.Music)
                         .PutLong(MediaMetadataCompat.MetadataKeyDuration, _exoPlayer.Duration < 0 ? AppHelper.ExoplayerTimeToTocaTudo(musicModel.MusicModel.MusicTimeTotalSeconds) : _exoPlayer.Duration)
                         .PutLong(MediaMetadataCompat.MetadataKeyNumTracks, musicModel.Number)
                         .Build());
        }
        internal void SetMediaPlaybackState(bool isPlaying, int indexPlaylist, ItemServicePlayer[] albumModel)
        {
            PlaybackStateCompat.Builder playbackstateBuilder = new PlaybackStateCompat.Builder();

            bool lastItemFromPlaylist = indexPlaylist >= albumModel.Count() - 1;
            bool onlyOneItem = albumModel.Count() == 1;

            if (onlyOneItem)
            {
                SetMediaPlaybackState(rebuildTimerPlayer: false);
                return;
            }

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
                    playbackstateBuilder.SetActions(PlaybackStateCompat.ActionPlayPause | PlaybackStateCompat.ActionPause | PlaybackStateCompat.ActionSkipToNext);
                }
                else if (indexPlaylist > 0)
                {
                    if (lastItemFromPlaylist)
                        playbackstateBuilder.SetActions(PlaybackStateCompat.ActionPlayPause | PlaybackStateCompat.ActionPause | PlaybackStateCompat.ActionSkipToPrevious);
                    else
                        playbackstateBuilder.SetActions(PlaybackStateCompat.ActionPlayPause | PlaybackStateCompat.ActionPause | PlaybackStateCompat.ActionSkipToNext | PlaybackStateCompat.ActionSkipToPrevious);
                }
            }

            if (IsPlaying)
            {
                playbackstateBuilder.SetState(PlaybackStateCompat.StatePlaying, _exoPlayer.CurrentPosition, 1F);
            }
            else
            {
                playbackstateBuilder.SetState(PlaybackStateCompat.StatePaused, _exoPlayer.CurrentPosition, 1F);
            }

            _mediaSession.SetPlaybackState(playbackstateBuilder.Build());
        }
        internal void SetMediaPlaybackState(bool rebuildTimerPlayer)
        {
            PlaybackStateCompat.Builder playbackstateBuilder = new PlaybackStateCompat.Builder();

            playbackstateBuilder.SetActions(PlaybackStateCompat.ActionPlayPause | PlaybackStateCompat.ActionPause);

            if (rebuildTimerPlayer && IsPlaying)
            {
                playbackstateBuilder.SetState(PlaybackStateCompat.StatePlaying, 0, 1F);
                _rebuildTimerPlayer = false;
            }
            else if (IsPlaying)
            {
                playbackstateBuilder.SetState(PlaybackStateCompat.StatePlaying, _exoPlayer.CurrentPosition, 1F);
            }
            else
            {
                playbackstateBuilder.SetState(PlaybackStateCompat.StatePaused, _exoPlayer.CurrentPosition, 1F);
            }

            _mediaSession.SetPlaybackState(playbackstateBuilder.Build());
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            try
            {
                _exceptionThrown = false;

                if (GetPlayerType() == PLAYER_TYPE_UNDEFINED)
                {
                    return StartCommandResult.Sticky;
                }

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
                        long iSource = intent.GetLongExtra("seek", 0);

                        switch (GetPlayerType())
                        {
                            case PLAYER_TYPE_ALBUM_MUSIC:
                            case PLAYER_TYPE_ALBUM:

                                if (MainActivity.PlaylistItemServicePlayerParameter != null)
                                    Seek(iSource, MainActivity.PlaylistItemServicePlayerParameter);
                                break;

                            case PLAYER_TYPE_MUSIC:
                                Seek(iSource);
                                break;
                            default:
                                Seek(iSource);
                                break;
                        }
                        break;
                    case ActionNext:
                        switch (GetPlayerType())
                        {
                            case PLAYER_TYPE_ALBUM_MUSIC:
                            case PLAYER_TYPE_ALBUM:

                                _playlistItem = MainActivity.PlaylistItemServicePlayerParameter;
                                NextMusicMetaData();
                                break;
                        }
                        break;
                    case ActionClearSource:
                        ClearSource();
                        break;
                    case ActionHidePlayerControls:
                        BuildHideNotification(_tuppleAlbum);
                        break;
                }

                //Set sticky as we are a long running operation
                return StartCommandResult.Sticky;
            }
            catch (Exception ex)
            {
                _exceptionThrown = true;
                _playerException.HandleEvent(this, ex, nameof(PlayerException));
                return StartCommandResult.RedeliverIntent;
            }
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

            ClearSource();
            StopSelf();
            return base.OnUnbind(intent);
        }
        public override void OnDestroy()
        {
            ClearSource();
            StopNotification();
            StopForeground(StopForegroundFlags.Detach);

            if (_exoPlayer != null)
            {
                _exoPlayer.Volume = 0.5f;
                _exoPlayer.Stop();
                _exoPlayer.Release();
                _exoPlayer = null;
            }

            _broadcastReceiver?.Dispose();

            base.OnDestroy();
        }
        internal void ClearSource()
        {
            if (AlbumMusicModelMode)
            {
                _musicModel = null;
                _albumPlaylist = null;
                _playlistItem = null;
                _tuppleAlbum = (null, null, null);
                _tuppleAlbumDetails = (null, null, 0, null);
            }
        }
        internal void Source(AlbumMusicModelServicePlayer albumMusicModel)
        {
            if (albumMusicModel == null || albumMusicModel?.Playlist?.Count() == 0)
                return;

            MusicModelItemServicePlayer musicModelItem = albumMusicModel.Playlist.FirstOrDefault();

            _tuppleAlbum = (albumMusicModel.AlbumName, musicModelItem.Music, musicModelItem.Image);
            _tuppleAlbumDetails = (albumMusicModel.AlbumName, musicModelItem?.Music, musicModelItem.TotalMilliseconds, musicModelItem.Image);
            _albumPlaylist = albumMusicModel.Playlist;
            _playlistItem = albumMusicModel.Playlist?.FirstOrDefault();
        }
        internal void Source(string url, AlbumModelServicePlayer albumModel)
        {
            if (string.IsNullOrWhiteSpace(url) || albumModel == null || albumModel.Playlist?.Count() == 0)
                return;

            MusicSource(url);

            PlaylistItemServicePlayer playlistItem = albumModel.Playlist.FirstOrDefault();

            _tuppleAlbum = (albumModel.AlbumName, playlistItem?.Music, albumModel.Image);
            _tuppleAlbumDetails = (albumModel.AlbumName, playlistItem?.Music, playlistItem.TotalMilliseconds, albumModel.Image);
            _albumPlaylist = albumModel.Playlist;
            _playlistItem = albumModel.Playlist?.FirstOrDefault();

            BuildNotification(false, 0, _tuppleAlbum, albumModel.Playlist);
        }
        internal void Source(byte[] music, AlbumModelServicePlayer albumModel)
        {
            if (music == null || albumModel == null || albumModel.Playlist == null)
                return;

            MusicSource(music);

            PlaylistItemServicePlayer playlistItem = albumModel.Playlist.FirstOrDefault();

            _tuppleAlbum = (albumModel.AlbumName, albumModel.Playlist?.FirstOrDefault()?.Music, albumModel.Image);
            _tuppleAlbumDetails = (albumModel.AlbumName, playlistItem?.Music, playlistItem.TotalMilliseconds, albumModel.Image);
            _albumPlaylist = albumModel.Playlist;
            _playlistItem = albumModel.Playlist?.FirstOrDefault();

            BuildNotification(false, 0, _tuppleAlbum, albumModel.Playlist);
        }
        internal void Source(string url, MusicModelServicePlayer musicModel)
        {
            if (string.IsNullOrWhiteSpace(url) || musicModel == null)
                return;

            MusicSource(url);

            _musicModel = musicModel;
            _rebuildTimerPlayer = true;

            if (AlbumMusicModelMode)
            {
                _playlistMusicActiveIndex = _albumPlaylist.ToList()
                                                          .FindIndex(item => string.Equals(item.VideoId, musicModel.MusicModel.VideoId));
                _playlistItem = _albumPlaylist.Where(item => string.Equals(item.VideoId, musicModel.MusicModel.VideoId))
                                              .FirstOrDefault();

                _lastPlaylistMusicActivedIndex = _playlistMusicActiveIndex;

                _tuppleAlbum.MusicName = musicModel.Music;
                _tuppleAlbum.Image = musicModel.Image;
                _tuppleAlbumDetails.MusicName = musicModel.Music;
                _tuppleAlbumDetails.TotalMilliseconds = musicModel.MusicModel.MusicTimeTotalSeconds;
                _tuppleAlbumDetails.Image = musicModel.Image;

                BuildNotification(false, _playlistMusicActiveIndex, _tuppleAlbum, _albumPlaylist);
            }
            else
            {
                BuildNotification(false, musicModel);
            }
        }
        internal void Source(byte[] music, MusicModelServicePlayer musicModel)
        {
            if (music == null || musicModel == null)
                return;

            MusicSource(music);

            _musicModel = musicModel;
            _rebuildTimerPlayer = true;

            if (AlbumMusicModelMode)
            {
                _playlistMusicActiveIndex = _albumPlaylist.ToList()
                                                          .FindIndex(item => string.Equals(item.VideoId, musicModel.MusicModel.VideoId));
                _playlistItem = _albumPlaylist.Where(item => string.Equals(item.VideoId, musicModel.MusicModel.VideoId))
                                              .FirstOrDefault();

                _lastPlaylistMusicActivedIndex = _playlistMusicActiveIndex;

                _tuppleAlbum.MusicName = musicModel.Music;
                _tuppleAlbum.Image = musicModel.Image;
                _tuppleAlbumDetails.MusicName = musicModel.Music;
                _tuppleAlbumDetails.TotalMilliseconds = musicModel.MusicModel.MusicTimeTotalSeconds;
                _tuppleAlbumDetails.Image = musicModel.Image;

                BuildNotification(false, _playlistMusicActiveIndex, _tuppleAlbum, _albumPlaylist);
            }
            else
            {
                BuildNotification(false, musicModel);
            }
        }
        public bool Play()
        {
            try
            {
                if (_exoPlayer != null)
                {
                    RequestAudioFocus();

                    _exoPlayer.PlayWhenReady = true;
                    IsPlaying = _exoPlayer.IsPlaying;

                    SetPlayerAlbumPlaybackState(true);
                }

                return true;
            }
            catch (Exception ex)
            {
                _playerException.HandleEvent(this, ex, nameof(PlayerException));
                return false;
            }
        }
        public void Pause()
        {
            if (_exoPlayer != null)
            {
                _exoPlayer.PlayWhenReady = false;
                IsPlaying = _exoPlayer.IsPlaying;

                SetPlayerAlbumPlaybackState(false);
                SetPlayerPlaybackState();
            }
        }
        public void PlayPause()
        {
            if (_exoPlayer != null)
            {
                if (IsPlaying)
                    Pause();
                else
                    Play();
            }
        }
        public void Seek(long milliseconds)
        {
            if (_exoPlayer != null)
            {
                IsPlaying = false;
                _exoPlayer.SeekTo(milliseconds);
            }
        }
        public void Seek(long milliseconds, ItemServicePlayer playlistItem)
        {
            if (_exoPlayer != null)
            {
                IsPlaying = false;

                _playlistItem = playlistItem;
                _tuppleAlbum.MusicName = playlistItem.Music;
                _tuppleAlbumDetails.MusicName = playlistItem.Music;
                _tuppleAlbumDetails.TotalMilliseconds = playlistItem.TotalMilliseconds;

                _exoPlayer.SeekTo(milliseconds);
            }
        }
        public void Next(ItemServicePlayer playlistItem)
        {
            if (_exoPlayer != null)
            {
                IsPlaying = false;

                _playlistItem = playlistItem;
                _tuppleAlbum.MusicName = playlistItem.Music;
                _tuppleAlbumDetails.MusicName = playlistItem.Music;
                _tuppleAlbumDetails.TotalMilliseconds = playlistItem.TotalMilliseconds;
            }
        }
        public long Max()
        {
            return _exoPlayer?.Duration ?? 0;
        }
        public long CurrentPosition()
        {
            return _exoPlayer?.CurrentPosition ?? 0;
        }
        public bool Stop()
        {
            if (_exoPlayer == null)
                return false;

            if (_exoPlayer.IsPlaying)
            {
                _exoPlayer.Volume = 0.5f;
                _exoPlayer.Stop();

                SetPlayerPlaybackState();
            }

            return true;
        }
        public void OnMediaItemTransition(MediaItem mediaItem, int reason)
        {

        }
        public void OnVolumeChanged(float volume)
        {

        }
        public void OnLoadingChanged(bool p0)
        {

        }
        public void OnIsLoadingChanged(bool isLoading)
        {

        }
        public void OnPositionDiscontinuity(PositionInfo oldPosition, PositionInfo newPosition, int reason)
        {
        }
        public void OnAvailableCommandsChanged(Commands availableCommands)
        {

        }
        public void OnPlayerError(ExoPlaybackException error)
        {
            switch (error.Type)
            {
                case ExoPlaybackException.TypeSource:
                    _playerInvalidUri.HandleEvent(this, null, nameof(PlayerInvalidUri));
                    break;
            }
        }
        public void OnPlayerErrorChanged(ExoPlaybackException error)
        {
            switch (error.Type)
            {
                case ExoPlaybackException.TypeSource:
                    _playerInvalidUri.HandleEvent(this, null, nameof(PlayerInvalidUri));
                    break;
            }
        }
        public void OnPlayWhenReadyChanged(bool playWhenReady, int playbackState)
        {
            if (_exceptionThrown)
                return;

            if (playbackState == StateIdle)
            {
                IsPlaying = _exoPlayer.IsPlaying;
            }
            else if (playbackState == StateBuffering)
            {
                if (_playerReadyBuffering != null)
                {
                    IsPlaying = _exoPlayer.IsPlaying;

                    if (_playerReady != null && _callPlayerActiveEvent)
                    {
                        _callPlayerActiveEvent = false;
                        _playerReady.RaiseEvent(this, null, nameof(PlayerReady));
                    }

                    if (_exoPlayer.BufferedPercentage > 0)
                        _playerReadyBuffering.HandleEvent(this, null, nameof(PlayerReadyBuffering));
                }
            }
            else if (playbackState == StateReady)
            {
                if (_playerReady != null && _callPlayerActiveEvent)
                {
                    _callPlayerActiveEvent = false;
                    _playerReady.RaiseEvent(this, null, nameof(PlayerReady));
                }
            }
            else if (playbackState == 4)//Player.StateEnded
            {
                IsPlaying = _exoPlayer.IsPlaying;
            }
        }
        public void OnPlayerStateChanged(bool playWhenReady, int playbackState)
        {
            if (_exceptionThrown)
                return;

            if (playbackState == StateReady && !AppHelper.HasMusicTotalTime)
            {
                if (_playerReady != null && _callPlayerActiveEvent)
                {
                    _callPlayerActiveEvent = false;

                    switch (GetPlayerType())
                    {
                        case PLAYER_TYPE_MUSIC:
                            if (AlbumMusicModelMode)
                            {
                                UpdateMetaData(_tuppleAlbumDetails, _albumPlaylist, _playlistMusicActiveIndex);
                                SetMediaPlaybackState(IsPlaying, _playlistMusicActiveIndex, _albumPlaylist);
                            }
                            else
                            {
                                UpdateMetaData(_musicModel);
                                SetMediaPlaybackState(_rebuildTimerPlayer);
                            }
                            break;
                    }

                    _playerReady.RaiseEvent(this, null, nameof(PlayerReady));
                }
            }
        }
        public void OnSeekProcessed()
        {
            if (_exceptionThrown)
                return;

            if (_playerSeekComplete != null)
            {
                NextMusicMetaData();
                _playerSeekComplete.HandleEvent(this, null, nameof(PlayerSeekComplete));
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
        public void OnPlaybackStateChanged(int param)
        {

        }
        public void OnEvents(IPlayer player, Events events)
        {
            try
            {
                if (_exceptionThrown)
                    return;

                if ((events.Contains(EventPlaybackStateChanged) || events.Contains(EventPlayWhenReadyChanged)) && AppHelper.HasMusicTotalTime)
                {
                    if (_playerReadyBuffering != null)
                    {
                        IsPlaying = _exoPlayer.IsPlaying;

                        if (_playerReady != null && _callPlayerActiveEvent)
                        {
                            _callPlayerActiveEvent = false;

                            switch (GetPlayerType())
                            {
                                case PLAYER_TYPE_MUSIC:
                                    if (AlbumMusicModelMode)
                                    {
                                        UpdateMetaData(_tuppleAlbumDetails, _albumPlaylist, _playlistMusicActiveIndex);
                                        SetMediaPlaybackState(IsPlaying, _playlistMusicActiveIndex, _albumPlaylist);
                                    }
                                    else
                                    {
                                        UpdateMetaData(_musicModel);
                                        SetMediaPlaybackState(_rebuildTimerPlayer);
                                    }
                                    break;
                            }

                            _playerReady.RaiseEvent(this, null, nameof(PlayerReady));
                        }

                        if (_exoPlayer.BufferedPercentage > 0)
                            _playerReadyBuffering.HandleEvent(this, null, nameof(PlayerReadyBuffering));
                    }
                }
            }
            catch (Exception ex)
            {
                _playerException.HandleEvent(this, ex, nameof(PlayerException));
            }
        }
        public void OnIsPlayingChanged(bool playing)
        {
            _playingChanged.RaiseEvent(this, playing, nameof(PlayingChanged));

            if (playing)
            {
                switch (GetPlayerType())
                {
                    case PLAYER_TYPE_MUSIC:
                        SetPlayerPlaybackState();
                        break;
                    case PLAYER_TYPE_ALBUM_MUSIC:
                    case PLAYER_TYPE_ALBUM:
                        SetMediaPlaybackState(IsPlaying, _playlistMusicActiveIndex, _albumPlaylist);
                        break;
                }
            }
        }
        private void SetMusicSource(Intent intent)
        {
            if (intent == null)
                return;

            if (intent.HasExtra("source-url"))
            {
                string source = intent.GetStringExtra("source-url");
                switch (GetPlayerType())
                {
                    case PLAYER_TYPE_ALBUM:
                        Source(source, MainActivity.AlbumModelServicePlayerParameter);
                        break;
                    case PLAYER_TYPE_MUSIC:
                        Source(source, MainActivity.MusicModelServicePlayerParameter);
                        break;
                }
            }
            else if (intent.HasExtra("source-byte"))
            {
                bool source = intent.GetBooleanExtra("source-byte", false);
                if (source)
                {
                    switch (GetPlayerType())
                    {
                        case PLAYER_TYPE_ALBUM:
                            Source(_mainActivity.AudioServiceMusicParameter, MainActivity.AlbumModelServicePlayerParameter);
                            break;
                        case PLAYER_TYPE_MUSIC:
                            Source(_mainActivity.AudioServiceMusicParameter, MainActivity.MusicModelServicePlayerParameter);
                            break;
                    }
                }
            }
            else
            {
                switch (GetPlayerType())
                {
                    case PLAYER_TYPE_ALBUM_MUSIC:
                        Source(MainActivity.AlbumMusicModelServicePlayerParameter);
                        break;
                }
            }
        }
        private void MusicSource(string url)
        {
            try
            {
                if (_exoPlayer != null)
                {
                    _exoPlayer.Release();
                    _exoPlayer.Dispose();
                }

                if (_playerInitializing != null)
                    _playerInitializing.RaiseEvent(this, null, nameof(PlayerInitializing));

                _exoPlayer = new SimpleExoPlayer.Builder(this).Build();
                _exoPlayer.AddListener(this);

                _extractorMediaSource = new ProgressiveMediaSource.Factory(new DefaultHttpDataSource.Factory()).CreateMediaSource(MediaItem.FromUri(Android.Net.Uri.Parse(url)));
                _exoPlayer.SetMediaSource(_extractorMediaSource);
                _exoPlayer.Prepare();

                _callPlayerActiveEvent = true;
            }
            catch (Exception ex)
            {
                _playerException.HandleEvent(this, ex, nameof(PlayerException));
            }
        }
        private void MusicSource(byte[] music)
        {
            try
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
                    _playerInitializing.HandleEvent(this, null, nameof(PlayerInitializing));


                _exoPlayer = new SimpleExoPlayer.Builder(this).Build();
                _exoPlayer.AddListener(this);

                _dataSource = new ByteArrayDataSource(music);
                Android.Net.Uri audioByteUri = new UriByteDataHelper().GetUri(music);

                DataSpec dataSpec = new DataSpec(audioByteUri);
                _dataSource.Open(dataSpec);

                IDataSource.IFactory dataSourceFactory = new DefaultDataSourceFactory(_context, this);

                _extractorMediaSource = new ProgressiveMediaSource.Factory(dataSourceFactory).CreateMediaSource(audioByteUri);
                _exoPlayer.Prepare(_extractorMediaSource);

                _callPlayerActiveEvent = true;
            }
            catch (Exception ex)
            {
                _playerException.HandleEvent(this, ex, nameof(PlayerException));
            }
        }
        private void SetPlayerPlaybackState()
        {
            switch (GetPlayerType())
            {
                case PLAYER_TYPE_ALBUM_MUSIC:
                case PLAYER_TYPE_ALBUM:

                    if (_stopMediaCompatPlayerNavigation || _albumPlaylist == null)
                        return;

                    _playlistMusicActiveIndex = _albumPlaylist.ToList()
                                                              .FindIndex(item => (_playlistItem.Number > -1 && item.Number == _playlistItem.Number) || (string.Equals(item.VideoId, _playlistItem.VideoId) && _playlistItem.Number < 0));

                    BuildNotification(IsPlaying, _playlistMusicActiveIndex, _tuppleAlbum, _albumPlaylist);
                    SetMediaPlaybackState(IsPlaying, _playlistMusicActiveIndex, _albumPlaylist);

                    _lastPlaylistMusicActivedIndex = _playlistMusicActiveIndex;

                    break;
                case PLAYER_TYPE_MUSIC:

                    if (_albumPlaylist?.Count() > 0)
                    {
                        BuildNotification(IsPlaying, _playlistMusicActiveIndex, _tuppleAlbum, _albumPlaylist);
                        SetMediaPlaybackState(IsPlaying, _playlistMusicActiveIndex, _albumPlaylist);
                    }
                    else
                    {
                        BuildNotification(IsPlaying, _musicModel);
                        SetMediaPlaybackState(false);
                    }

                    break;
            }
        }
        private void NextMusicMetaData()
        {
            if (_albumPlaylist == null || _albumPlaylist?.Count() == 0)
                return;

            switch (GetPlayerType())
            {
                case PLAYER_TYPE_ALBUM_MUSIC:
                case PLAYER_TYPE_ALBUM:

                    if (_stopMediaCompatPlayerNavigation)
                        return;

                    _playlistMusicActiveIndex = _albumPlaylist.ToList()
                                                              .FindIndex(item => (_playlistItem.Number > -1 && item.Number == _playlistItem.Number) || (string.Equals(item.VideoId, _playlistItem.VideoId) && _playlistItem.Number < 0));
                    _lastPlaylistMusicActivedIndex = _playlistMusicActiveIndex;

                    _tuppleAlbum.MusicName = _playlistItem.Music;
                    _tuppleAlbumDetails.MusicName = _playlistItem.Music;
                    _tuppleAlbumDetails.TotalMilliseconds = _playlistItem.TotalMilliseconds;

                    _rebuildTimerPlayer = true;

                    UpdateMetaData(_tuppleAlbumDetails, _albumPlaylist, _playlistMusicActiveIndex);
                    BuildNotification(IsPlaying, _playlistMusicActiveIndex, _tuppleAlbum, _albumPlaylist);
                    SetMediaPlaybackState(IsPlaying, _playlistMusicActiveIndex, _albumPlaylist);

                    _lastPlaylistMusicActivedIndex = _playlistMusicActiveIndex;

                    break;
            }
        }
        private void SetPlayerAlbumPlaybackState(bool isPlaying)
        {
            switch (GetPlayerType())
            {
                case PLAYER_TYPE_ALBUM_MUSIC:
                case PLAYER_TYPE_ALBUM:

                    if (_stopMediaCompatPlayerNavigation)
                        return;

                    BuildNotification(isPlaying: true, _playlistMusicActiveIndex, _tuppleAlbum, _albumPlaylist);

                    break;
            }
        }
        internal bool RequestAudioFocus()
        {
            AudioFocusRequest? audioFocus = null;

            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                _audioManager = (AudioManager)GetSystemService(AudioService);
                audioFocus = _audioManager?.RequestAudioFocus(new AudioFocusRequestClass.Builder(AudioFocus.Gain).SetOnAudioFocusChangeListener(this).SetAcceptsDelayedFocusGain(true).Build());
            }
            else
            {
                _audioManager = (AudioManager)GetSystemService(AudioService);
                audioFocus = _audioManager?.RequestAudioFocus(this, Stream.Music, AudioFocus.Gain);
            }

            return audioFocus == AudioFocusRequest.Granted;
        }
        public IDataSource CreateDataSource()
        {
            return _dataSource;
        }
        public void OnAudioFocusChange([GeneratedEnum] AudioFocus focusChange)
        {
            if (_exoPlayer == null)
                return;

            if (AppHelper.MusicPlayerInterstitialIsLoadded)
            {
                Pause();
                return;
            }

            switch (focusChange)
            {
                case AudioFocus.Gain:
                    _exoPlayer.Volume = 1.0f;
                    _playerLosedAudioFocus.HandleEvent(this, null, nameof(PlayerLosedAudioFocus));
                    Play();
                    break;
                case AudioFocus.LossTransient:
                    _playerLosedAudioFocus.HandleEvent(this, null, nameof(PlayerLosedAudioFocus));
                    break;
                case AudioFocus.Loss:
                    Pause();
                    _playerLosedAudioFocus.HandleEvent(this, null, nameof(PlayerLosedAudioFocus));
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
                case PLAYER_TYPE_ALBUM_MUSIC:
                case PLAYER_TYPE_ALBUM:
                    HandleIncomingPlayerAlbumActions(action);
                    break;
                case PLAYER_TYPE_MUSIC:
                    if (_albumPlaylist?.Count() > 0)
                        HandleIncomingPlayerAlbumActions(action);
                    else
                        HandleIncomingPlayerMusicActions(action);
                    break;
            }
        }
        internal void HandleIncomingPlayerAlbumActions(string action)
        {
            if (string.IsNullOrWhiteSpace(action) || AppHelper.HasInterstitialToShow || AppHelper.MusicPlayerInterstitialIsLoadded)
                return;

            _stopMediaCompatPlayerNavigation = false;

            int musicPlayingIndex = _albumPlaylist.ToList()
                                                  .FindIndex(item => (_playlistItem.Number > -1 && item.Number == _playlistItem.Number) || (string.Equals(item.VideoId, _playlistItem.VideoId) && _playlistItem.Number < 0));

            switch (action)
            {
                case ActionPlay:

                    _playlistMusicActiveIndex = musicPlayingIndex;
                    BuildNotification(true, musicPlayingIndex, _tuppleAlbum, _albumPlaylist);

                    break;
                case ActionStop:

                    _playerPlaylistChanged.RaiseEvent(this, _playlistItem, nameof(PlayerPlaylistChanged));

                    UpdateMetaData(_tuppleAlbumDetails, _albumPlaylist, musicPlayingIndex);
                    BuildNotification(false, musicPlayingIndex, _tuppleAlbum, _albumPlaylist);

                    _rebuildTimerPlayer = true;

                    break;
                case ActionPause:
                    UpdateMetaData(_tuppleAlbumDetails, _albumPlaylist, musicPlayingIndex);
                    BuildNotification(false, musicPlayingIndex, _tuppleAlbum, _albumPlaylist);

                    _rebuildTimerPlayer = false;

                    break;
                case ActionPrevious:

                    if (IsFirstMusicPreviousSelected())
                    {
                        _stopMediaCompatPlayerNavigation = true;
                        return;
                    }

                    if (musicPlayingIndex == 0)
                        _playlistItem = _albumPlaylist[musicPlayingIndex];
                    else
                    {
                        musicPlayingIndex -= 1;
                        _playlistItem = _albumPlaylist[musicPlayingIndex];
                    }

                    UpdateMetaData(_tuppleAlbumDetails, _albumPlaylist, musicPlayingIndex);
                    BuildNotification(false, musicPlayingIndex, _tuppleAlbum, _albumPlaylist);

                    _playlistMusicActiveIndex = musicPlayingIndex;
                    _playerPlaylistChanged.RaiseEvent(this, _playlistItem, nameof(PlayerPlaylistChanged));

                    _rebuildTimerPlayer = true;

                    break;
                case ActionNext:

                    if (IsLastMusicPreviousSelected())
                    {
                        _stopMediaCompatPlayerNavigation = true;
                        return;
                    }

                    if (musicPlayingIndex == _albumPlaylist.Count() - 1)
                        _playlistItem = _albumPlaylist[musicPlayingIndex];
                    else
                    {
                        musicPlayingIndex += 1;
                        _playlistItem = _albumPlaylist[musicPlayingIndex];
                    }

                    UpdateMetaData(_tuppleAlbumDetails, _albumPlaylist, musicPlayingIndex);
                    BuildNotification(false, musicPlayingIndex, _tuppleAlbum, _albumPlaylist);

                    _playlistMusicActiveIndex = musicPlayingIndex;
                    _playerPlaylistChanged.RaiseEvent(this, _playlistItem, nameof(PlayerPlaylistChanged));

                    _rebuildTimerPlayer = true;

                    break;
                case ActionTogglePlayback:
                    if (_exoPlayer.IsPlaying)
                        Pause();
                    else
                        Play();

                    break;
                case ActionSeek:
                    //int iSource = intent.GetIntExtra("seek", 0);
                    //Seek(iSource);
                    break;
            }
        }
        internal void HandleIncomingPlayerMusicActions(string action)
        {
            if (string.IsNullOrWhiteSpace(action) || AppHelper.HasInterstitialToShow)
                return;

            switch (action)
            {
                case ActionPlay:

                    BuildNotification(true, _musicModel);

                    break;
                case ActionStop:

                    BuildNotification(false, _musicModel);
                    SetMediaPlaybackState(true);

                    break;
                case ActionPause:

                    BuildNotification(false, _musicModel);

                    break;
                case ActionTogglePlayback:

                    break;
                case ActionSeek:

                    break;
            }
        }
        private bool IsFirstMusicPreviousSelected() => _lastPlaylistMusicActivedIndex == 0;
        private bool IsLastMusicPreviousSelected() => _lastPlaylistMusicActivedIndex == _albumPlaylist.Count() - 1;
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
            if (intent == null || context == null)
                return;

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
            if (AppHelper.HasInterstitialToShow)
                return;

            _audioService.MediaSessionCompat.Active = true;
            _audioService.Play();
            _audioService.HandleIncomingActions(AudioService.ActionPlay);

            _audioService.RebuildTimerPlayer = false;

            if (_audioService.MusicModelService != null)
                _audioService.MusicModelService.MusicModel.IsActiveMusic = false;
            if (_audioService.PlaylistItemService != null)
                _audioService.PlaylistItemService.PlaylistItem.IsActiveMusic = true;
        }
        public override void OnPause()
        {
            if (AppHelper.HasInterstitialToShow)
                return;

            _audioService.Pause();
            _audioService.HandleIncomingActions(AudioService.ActionPause);

            _audioService.RebuildTimerPlayer = false;

            if (_audioService.MusicModelService?.MusicModel != null)
                _audioService.MusicModelService.MusicModel.IsActiveMusic = false;
            if (_audioService.PlaylistItemService?.PlaylistItem != null)
                _audioService.PlaylistItemService.PlaylistItem.IsActiveMusic = false;
        }
        public override void OnSkipToNext()
        {
            _audioService.HandleIncomingActions(AudioService.ActionNext);
            _audioService.RebuildTimerPlayer = true;
        }
        public override void OnSkipToPrevious()
        {
            _audioService.HandleIncomingActions(AudioService.ActionPrevious);
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
        }
    }
    public class UriByteDataHelper
    {
        public Android.Net.Uri GetUri(byte[] data)
        {
            Java.Net.URL url = new Java.Net.URL(null, "bytes:///" + "audio", new BytesHandler(data));
            return Android.Net.Uri.Parse(url.ToURI().ToString());
        }
        public class BytesHandler : Java.Net.URLStreamHandler
        {
            byte[] mData;
            public BytesHandler(byte[] data)
            {
                mData = data;
            }
            protected override Java.Net.URLConnection OpenConnection(Java.Net.URL u)
            {
                return new ByteUrlConnection(u, mData);
            }
        }
    }
    public class ByteUrlConnection : Java.Net.URLConnection
    {
        private byte[] mData;
        public ByteUrlConnection(Java.Net.URL url, byte[] data)
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
}