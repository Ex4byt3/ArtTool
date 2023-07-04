using System;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Linq;

namespace ArtTool
{
    public partial class RefImage : MainWindow
    {
        public int duration;
        ImageSourceConverter converter = new ImageSourceConverter();
        public RefImage(string imagePath, int duration)
        {
            displayedImage.Source = (ImageSource)converter.ConvertFromString(imagePath);
            this.duration = duration;
        }
        ~RefImage()
        {
            // TODO
        }
    }


}
