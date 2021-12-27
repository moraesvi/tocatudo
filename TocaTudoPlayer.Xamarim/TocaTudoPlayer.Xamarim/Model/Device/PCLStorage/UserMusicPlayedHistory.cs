using System;
using System.IO;
using System.Text.Json.Serialization;
using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim
{
    public class UserMusicPlayedHistory : BaseViewModel
    {
        private byte[] _byteImgMusic;
        private Color _musicSelectedColorPrimary;
        private Color _musicSelectedColorSecondary;
        private Color _musicTextColor;
        private Color _recentPlayedMusicTextColor;
        private FontAttributes _musicTextFontAttr;
        public UserMusicPlayedHistory()
        {
            _musicSelectedColorPrimary = Color.FromHex("#F7F9FC");
            _musicSelectedColorSecondary = Color.FromHex("#F7F9FC");
            _musicTextColor = Color.Black;
            _musicTextFontAttr = FontAttributes.None;
        }
        public string VideoId { get; set; }
        public string MusicName { get; set; }
        public bool IsSavedOnLocalDb { get; set; }

        [JsonIgnore]
        public ImageSource ImgMusic { get; set; }

        [JsonIgnore]
        public MusicSearchType SearchType { get; set; }
        public byte[] ByteImgMusic
        {
            get { return _byteImgMusic; }
            set
            {
                _byteImgMusic = value;
                ImgMusic = ImageSource.FromStream(() => new MemoryStream(_byteImgMusic));
            }
        }
        [JsonIgnore]
        public DateTimeOffset DtIn
        {
            get
            {
                return Convert.ToDateTime(DateTimeIn);
            }
        }
        public string DateTimeIn { get; set; }
        public Color MusicSelectedColorPrimary
        {
            get { return _musicSelectedColorPrimary; }
            set
            {
                _musicSelectedColorPrimary = value;
                OnPropertyChanged(nameof(MusicSelectedColorPrimary));
            }
        }
        public Color MusicSelectedColorSecondary
        {
            get { return _musicSelectedColorSecondary; }
            set
            {
                _musicSelectedColorSecondary = value;
                OnPropertyChanged(nameof(MusicSelectedColorSecondary));
            }
        }
        public Color MusicTextColor
        {
            get { return _musicTextColor; }
            set
            {
                _musicTextColor = value;
                OnPropertyChanged(nameof(MusicTextColor));
            }
        }
        public FontAttributes MusicTextFontAttr
        {
            get { return _musicTextFontAttr; }
            set
            {
                _musicTextFontAttr = value;
                OnPropertyChanged(nameof(MusicTextFontAttr));
            }
        }
        public void UpdMusicSelectedColor()
        {
            if (_musicSelectedColorPrimary == Color.FromHex("#D4420C") && _musicSelectedColorSecondary == Color.FromHex("#F7F9FC"))
            {
                MusicSelectedColorPrimary = Color.FromHex("#F7F9FC");
                MusicSelectedColorSecondary = Color.FromHex("#F7F9FC");
                MusicTextColor = Color.Black;
                MusicTextFontAttr = FontAttributes.None;
            }
            else
            {
                MusicSelectedColorPrimary = Color.FromHex("#D4420C");
                MusicSelectedColorSecondary = Color.FromHex("#F7F9FC");
                MusicTextColor = Color.White;
                MusicTextFontAttr = FontAttributes.None;
            }
        }
    }
}
