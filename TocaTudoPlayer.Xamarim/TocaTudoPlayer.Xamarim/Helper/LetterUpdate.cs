using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim
{
    public class LetterUpdate : IDisposable
    {
        private bool _stopLetterMoving;
        private Label _lbl;
        public async Task Translate(Label lbl, StackLayout stl)
        {
            _lbl = lbl;

            await Task.Delay(3000);

            if (lbl.Width < 0 || stl.Width < 0)
                return;

            int letterExtraSpaceRigthProgress = -((int)(lbl.Width - stl.Width));

            if (lbl.Width < stl.Width)
                return;

            while (true)
            {
                letterExtraSpaceRigthProgress = -((int)(lbl.Width - stl.Width));

                if (lbl.Width < stl.Width)
                {
                    break;
                }

                if (lbl.Width > -1 && (lbl.Width == stl.Width || lbl.Text == null))
                {
                    if (lbl.Width == stl.Width || lbl.Text == null)
                        break;
                }

                if (_stopLetterMoving)
                    break;

                await lbl.TranslateTo(letterExtraSpaceRigthProgress, 0, 5000);
                await Task.Delay(1500);
                await lbl.TranslateTo(0, 0, 5000);
                await Task.Delay(1500);
            }
        }
        public async void Dispose()
        {
            _stopLetterMoving = true;
            await _lbl.TranslateTo(0, 0);

            GC.SuppressFinalize(this);
        }
    }
}
