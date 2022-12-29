using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TocaTudoPlayer.Xamarim.Helper;
using TocaTudoPlayer.Xamarim.Pages;
using Xamarin.CommunityToolkit.Helpers;
using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim
{
    public class AppHelper
    {
        private static CultureInfo _ci = CultureInfo.CurrentCulture;
        private static WeakEventManager<Exception> _playerException;
        public static event EventHandler<Exception> PlayerException
        {
            add => _playerException.AddEventHandler(value);

            remove => _playerException.RemoveEventHandler(value);
        }
        public static void Init() 
        {
            _playerException = new WeakEventManager<Exception>();
        }
        public static bool MusicPlayerInterstitialIsLoadded { get; set; }
        public static bool HasInterstitialToShow { get; set; }
        public static bool HasMusicTotalTime { get; set; }
        public static long WhenNullSetMusicTimeMilliseconds(ICommonMusicModel musicModel)
        {
            if (musicModel.MusicTimeTotalSeconds == 0 && !string.IsNullOrEmpty(musicModel.MusicTime))
                musicModel.MusicTimeTotalSeconds = AppHelper.HourOrSecondsToMilliseconds(musicModel.MusicTime);

            return musicModel.MusicTimeTotalSeconds;
        }
        public static string ReadFile(string file)
        {
            const int BufferSize = 128;
            StringBuilder sb = new StringBuilder();

            using (FileStream fileStream = File.OpenRead(file))
            {
                using (StreamReader streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        sb.Append(line);
                    }
                }
            }

            return sb.ToString();
        }
        public static FontImageSource FaviconImageSource(string glyph, int imageSize, Color color)
        {
            FontImageSource source = new FontImageSource();
            source.Glyph = glyph;
            source.Size = imageSize;// 35;
            source.FontFamily = "FontAwesomeBold";
            source.Color = color;

            return source;
        }
        public static string ToTitleCase(string str)
        {
            str = str.ToLower();
            var strArray = str.Split(' ');
            if (strArray.Length > 1)
            {
                StringBuilder sbStr = new StringBuilder();
                foreach (string value in strArray)
                {
                    if (!string.IsNullOrEmpty(sbStr.ToString()))
                        sbStr.Append(" ");

                    sbStr.Append(_ci.TextInfo.ToTitleCase(value));
                }

                return sbStr.ToString();
            }

            return _ci.TextInfo.ToTitleCase(str);
        }
        public static string GetIntertistialAdsVisibleScreen()
        {
            return Album.FormIsVisible ? App.AppConfigAdMob.AdsAlbumIntersticial
                                       : Music.FormIsVisible ? App.AppConfigAdMob.AdsMusicIntersticial
                                       : SavedMusic.FormIsVisible ? App.AppConfigAdMob.AdsSavedMusicIntersticial
                                       : AlbumPlayer.FormIsVisible ? App.AppConfigAdMob.AdsAlbumPlayerIntersticial
                                       : throw new InvalidOperationException("Intersticial has not been defined");
        }
        public static long ExoplayerTimeToTocaTudo(long totalSeconds) => totalSeconds * 1000;
        public static long HourOrSecondsToMilliseconds(string time)
        {
            if (string.IsNullOrEmpty(time))
                return 0;

            string tempoFormat = string.Join("", time.Where(c => char.IsDigit(c) || c == ':' || c == ' ' || c == '-'));
            string[] tempoFormatArray = tempoFormat.Split(' ')
                                                   .Where(c => c.Contains(":"))
                                                   .ToArray();
            if (tempoFormatArray.Count() >= 2)
            {
                tempoFormat = tempoFormatArray.FirstOrDefault();

                tempoFormat = tempoFormat.Split(' ')
                                         .Where(c => c.Contains(":"))
                                         .FirstOrDefault();
            }
            else
            {
                tempoFormatArray = tempoFormat.Split('-')
                                              .Where(c => c.Contains(":"))
                                              .ToArray();

                tempoFormat = tempoFormatArray.FirstOrDefault();

                tempoFormat = tempoFormat.Split(' ')
                                         .Where(c => c.Contains(":"))
                                         .FirstOrDefault();
            }

            int horas = 0, minutos = 0, segundos = 0;
            (int Kind, long Milliseconds) timeToMilliseconds = (0, 0);

            string[] tempoSplit = tempoFormat.Split(':');

            bool minutosSegundos = (tempoSplit.Count() == 2);
            bool horasMinutosSegundos = (tempoSplit.Count() == 3);

            if (minutosSegundos)
            {
                minutos = Convert.ToInt16(tempoSplit[0].OnlyNumbers());
                segundos = Convert.ToInt16(tempoSplit[1].OnlyNumbers());

                timeToMilliseconds = (AppConst.KindParseTimeMinutes, (minutos * 60) + segundos);
            }
            else if (horasMinutosSegundos)
            {
                horas = Convert.ToInt16(tempoSplit[0].OnlyNumbers());
                minutos = Convert.ToInt16(tempoSplit[1].OnlyNumbers());
                segundos = Convert.ToInt16(tempoSplit[2].OnlyNumbers());

                timeToMilliseconds = (AppConst.KindParseTimeHours, (minutos * 60) + (horas * 3600) + segundos)
                                    ;
            }

            //long milliseconds = timeToMilliseconds.Kind == AppConst.KindParseTimeMinutes
            //                    ? TimeSpan.FromMinutes(timeToMilliseconds.Milliseconds).Milliseconds
            //                    : timeToMilliseconds.Kind == AppConst.KindParseTimeHours ? TimeSpan.FromHours(timeToMilliseconds.Milliseconds).Milliseconds
            //                    : 0;

            return timeToMilliseconds.Milliseconds;
        }
    }
}
