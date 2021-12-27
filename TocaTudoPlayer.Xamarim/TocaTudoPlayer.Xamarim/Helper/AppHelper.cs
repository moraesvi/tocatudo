using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim
{
    public class AppHelper
    {
        public static bool MusicPlayerInterstitialWasShowed { get; set; }
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
        public static string FirstLetterToUpper(string value) 
        {
            return Regex.Replace(value, @"(^\w)|(\s\w)", m => m.Value.ToUpper());
        }
    }
}
