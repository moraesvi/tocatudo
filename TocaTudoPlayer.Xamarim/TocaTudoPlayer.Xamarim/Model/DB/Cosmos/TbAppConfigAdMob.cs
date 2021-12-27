namespace TocaTudoPlayer.Xamarim.Model.DB.Cosmos
{
    public class TbAppConfigAdMob
    {
        public TbAppConfigAdMob() 
        {        
        }
        public TbAppConfigAdMob(AppConfigAdMob item)
        {
            AdsMusicBanner = item.AdsMusicBanner;
            AdsIntersticialAlbum = item.AdsIntersticialAlbum;
            AdsActiveProdMode = item.AdsActiveProdMode;
        }
        public string AdsMusicBanner { get; set; }
        public string AdsIntersticialAlbum { get; set; }
        public bool AdsActiveProdMode { get; set; }
    }
}
