namespace TocaTudoPlayer.Xamarim
{
    public interface ItemServicePlayer
    {
        short Id { get; }
        short Number { get; }
        string Music { get; }
        string VideoId { get; }
        long TotalMilliseconds { get; }
        ICommonMusicServiceModel PlaylistItem { get; }
    }
}
