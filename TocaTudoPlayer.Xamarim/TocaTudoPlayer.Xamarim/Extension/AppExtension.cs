using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TocaTudoPlayer.Xamarim
{
    public static class AppExtension
    {
        //Fastest way
        public static string ToUpperFirst(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;

            Span<char> a = stackalloc char[s.Length];
            s.AsSpan(1).CopyTo(a.Slice(1));
            a[0] = char.ToUpper(s[0]);

            return new string(a);
        }
        public static Task OnError(this Task task, string pageName, Action action)
        {
            return task.ContinueWith((tsk) =>
            {
                if (!task.IsFaulted)
                    return;

                if (tsk.IsFaulted)
                {
                    App.EventTracker.SendEvent($"{pageName}_Exception", new Dictionary<string, string>()
                    {
                        { "Msg", tsk.Exception?.Message },
                        { "InnerMsg", tsk.Exception?.InnerException?.Message },
                        { "StackTrace", tsk.Exception?.StackTrace },
                    });

                    action();
                }
            });
        }
        public static string OnlyNumbers(this string value)
        {
            IReadOnlyCollection<char> array = value.ToCharArray();

            ReadOnlySpan<char> resultArray = array.Where(ar => (char.IsLetterOrDigit(ar)
                                                            || char.IsWhiteSpace(ar)
                                                            || ar == '-'))
                                                  .ToArray()
                                                  .AsSpan();

            return new string(resultArray);
        }
    }
}
