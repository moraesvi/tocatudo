using System.Collections.ObjectModel;

namespace TocaTudoPlayer.Xamarim
{
    public class MusicAlbumDialogModel
    {    
        public ObservableCollection<SelectModel> AlbumMusicSavedCollection { get; set; }         
        public MusicModelBase MusicModel { get; set; }
    }
}
