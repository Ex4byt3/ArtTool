using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            // TODO
        }

        ~RefImage()
        {
            // TODO
        }
    }
}
