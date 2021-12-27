namespace TocaTudoPlayer.Xamarim.Model.DB.Cosmos
{
    public class TbAppConfig : BaseCosmos
    {
        public TbAppConfig()
        {
        }
        public TbAppConfig(AppConfig item)
        {
            AdMob = new TbAppConfigAdMob(item.AdMob);
        }
        public TbAppConfigAdMob AdMob { get; set; }
    }
}
