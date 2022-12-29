using Xamarin.CommunityToolkit.Behaviors;
using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim
{
    internal static class AnimationExtensions
    {
        public static void FadeOutFadeInAnimation(this View view, string animationName)
        {
            Animation commit = new Animation();

            Animation fadeOutAnimation = new Animation(d => view.Opacity = d, 1, 0.5);
            Animation fadeInAnimation = new Animation(d => view.Opacity = d, 0.5, 1);

            commit.Add(0, 0.5, fadeOutAnimation);
            commit.Add(0.5, 1, fadeInAnimation);

            commit.Commit(view, animationName);
        }
        public static void SizeUpSizeDownAnimation(this Label lbl, string animationName)
        {
            Animation commit = new Animation();

            Animation musicNameSizeUpAnimation = new Animation(d => lbl.FontSize = d, lbl.FontSize, lbl.FontSize + 1.5, Easing.Linear);
            Animation musicNameSizeDownAnimation = new Animation(d => lbl.FontSize = d, lbl.FontSize + 1.5, lbl.FontSize, Easing.Linear);

            commit.Add(0, 0.5, musicNameSizeUpAnimation);
            commit.Add(0.5, 1, musicNameSizeDownAnimation);

            commit.Commit(lbl, animationName, 16, 250, Easing.Linear);
        }
    }
}
