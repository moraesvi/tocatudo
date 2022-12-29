namespace TocaTudoPlayer.Xamarim
{
    public class MusicModelServicePlayer : IModelServicePlayer
    {
        private ICommonMusicModel _musicModel;
        public MusicModelServicePlayer(ICommonMusicModel musicModel) 
        {
            _musicModel = musicModel;
        }
        public ICommonMusicModel MusicModel => _musicModel;
        public short Id { get; set; }
        public short Number { get; set; }
        public string Music { get; set; }
        public byte[] Image { get; set; }
    }
}
