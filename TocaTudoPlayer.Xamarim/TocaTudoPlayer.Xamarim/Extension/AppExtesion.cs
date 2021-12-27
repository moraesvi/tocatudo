using System;

namespace TocaTudoPlayer.Xamarim
{
    public static class AppExtesion
    {
        //Faster way
        public static string ToUpperFirst(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;

            Span<char> a = stackalloc char[s.Length];
            s.AsSpan(1).CopyTo(a.Slice(1));
            a[0] = char.ToUpper(s[0]);

            return new string(a);
        }
    }
}
