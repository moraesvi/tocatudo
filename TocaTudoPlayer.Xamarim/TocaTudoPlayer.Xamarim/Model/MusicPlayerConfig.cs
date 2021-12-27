namespace TocaTudoPlayer.Xamarim
{
    public class MusicPlayerConfig
    {
        public bool PlayFirstMusic { get; set; } = true;
        public int TotalMusicsWillPlay { get; set; }
        public int CountMusicsPlayed { get; private set; } = 0;
        public bool CheckIfMusicPlayedCountAchieveTotal(bool autoRebuild = false)
        {
            if (PlayFirstMusic)
            {
                PlayFirstMusic = false;
                return true;
            }

            if (CountMusicsPlayed < TotalMusicsWillPlay)
            {
                AddCountMusicsPlayed();
                return false;
            }

            if (autoRebuild)
                RebuildCountMusicsPlayed();

            return true;
        }
        public void AddCountMusicsPlayed()
        {
            CountMusicsPlayed += 1;
        }
        public void RebuildCountMusicsPlayed()
        {
            CountMusicsPlayed = 0;
        }
    }
}
