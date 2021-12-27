using System;
using System.Collections.Generic;

namespace TocaTudoPlayer.Xamarim
{
    public class CompareIgnoreCase : IEqualityComparer<string>
    {
        public bool Equals(string x, string y) => string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
        public int GetHashCode(string obj) => obj.GetHashCode();
    }
}
