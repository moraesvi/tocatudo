using System;
using System.IO;
using System.Text.Json.Serialization;
using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim
{
    public class UserAlbumPlayedHistory : BaseViewModel
    {
        private Color _musicSelectedColorPrimary;
        private Color _musicSelectedColorSecondary;
        private Color _musicTextColor;
        private FontAttributes _musicTextFontAttr;
        public UserAlbumPlayedHistory()
        {
            _musicSelectedColorPrimary = Color.FromHex("#F7F9FC");
            _musicSelectedColorSecondary = Color.FromHex("#F7F9FC");
            _musicTextColor = Color.Black;
            _musicTextFontAttr = FontAttributes.None;
        }
        private byte[] _byteImgAlbum;
        public string VideoId { get; set; }
        public string AlbumName { get; set; }
        [JsonIgnore]
        public ImageSource ImgAlbum { get; set; }
        public bool IsSavedOnLocalDb { get; set; }
        public byte[] ByteImgAlbum
        {
            get { return _byteImgAlbum; }
            set
            {
                _byteImgAlbum = value;

                if (_byteImgAlbum != null)
                    ImgAlbum = ImageSource.FromStream(() => new MemoryStream(_byteImgAlbum));
            }
        }
        public int ParseType { get; set; }
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
        public void UpdAlbumSelectedColor()
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
