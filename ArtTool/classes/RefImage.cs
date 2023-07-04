using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace ArtTool
{
    public partial class RefImage
    {
        public int duration;
        ImageSourceConverter converter = new ImageSourceConverter();
        MainWindow _mainWindow = null;

        public RefImage(string imagePath, int duration, MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            this._mainWindow.displayedImage.Source = (ImageSource)converter.ConvertFromString(imagePath);
            this.duration = duration;
            TimerLogic();
        }

        internal async Task TimerLogic()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            TimeSpan elapsed = stopwatch.Elapsed;
            while (duration >= 0)
            {
                await UpdateTimer();
                await Task.Delay(TimeSpan.FromSeconds(1));
                duration--;
            }
            // something
        }

        internal async Task UpdateTimer()
        {
            _mainWindow.RemainingTime.Text = duration.ToString();
        }

        ~RefImage()
        {
            // TODO
        }
    }
}
