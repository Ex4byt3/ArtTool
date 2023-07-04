using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ArtTool
{ // no touchy
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class MonitorInfo
    {
        public int cbSize = Marshal.SizeOf(typeof(MonitorInfo));
        public ArtTool.MainWindow.RECT rcMonitor = new ArtTool.MainWindow.RECT();
        public ArtTool.MainWindow.RECT rcWork = new ArtTool.MainWindow.RECT();
        public int dwFlags = 0;
    }
}
