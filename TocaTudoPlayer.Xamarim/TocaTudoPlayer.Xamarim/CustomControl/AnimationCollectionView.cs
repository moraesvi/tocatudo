using System.Threading.Tasks;
using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim
{
    public class AnimationCollectionView : CollectionView
    {
        protected override async void OnChildAdded(Element child)
        {
            View view = (View)child;
            ICommonMusicModel musicModel = view.BindingContext as ICommonMusicModel;

            if (musicModel != null && !musicModel.IsAnimated)
            {
                await view.FadeTo(0, 0);
                await Task.Delay(20);
                await view.FadeTo(1);

                musicModel.IsAnimated = true;
            }

            base.OnChildAdded(child);
        }
    }
}
