using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using ArtTool.ViewModels;
using ArtTool.Views;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace ArtTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _viewModel;
        private SettingsMenuWindow settingsMenuWindow;

        private bool isSettingsMenuVisible;

        #region something Bit asked not to touch. It just works
        private static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
            }
            return (IntPtr)0;
        }

        private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));
            int MONITOR_DEFAULTTONEAREST = 0x00000002;
            IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
            if (monitor != IntPtr.Zero)
            {
                MonitorInfo monitorInfo = new MonitorInfo();
                GetMonitorInfo(monitor, monitorInfo);
                RECT rcWorkArea = monitorInfo.rcWork;
                RECT rcMonitorArea = monitorInfo.rcMonitor;
                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
            }
            Marshal.StructureToPtr(mmi, lParam, true);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            /// <summary>x coordinate of point.</summary>
            public int x;
            /// <summary>y coordinate of point.</summary>
            public int y;
            /// <summary>Construct a point of coordinates (x,y).</summary>
            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };



        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
            public static readonly RECT Empty = new RECT();
            public int Width { get { return Math.Abs(right - left); } }
            public int Height { get { return bottom - top; } }
            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }
            public RECT(RECT rcSrc)
            {
                left = rcSrc.left;
                top = rcSrc.top;
                right = rcSrc.right;
                bottom = rcSrc.bottom;
            }
            public bool IsEmpty { get { return left >= right || top >= bottom; } }
            public override string ToString()
            {
                if (this == Empty) { return "RECT {Empty}"; }
                return "RECT { left : " + left + " / top : " + top + " / right : " + right + " / bottom : " + bottom + " }";
            }
            public override bool Equals(object obj)
            {
                if (!(obj is Rect)) { return false; }
                return (this == (RECT)obj);
            }
            /// <summary>Return the HashCode for this struct (not garanteed to be unique)</summary>
            public override int GetHashCode() => left.GetHashCode() + top.GetHashCode() + right.GetHashCode() + bottom.GetHashCode();
            /// <summary> Determine if 2 RECT are equal (deep compare)</summary>
            public static bool operator ==(RECT rect1, RECT rect2) { return (rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right && rect1.bottom == rect2.bottom); }
            /// <summary> Determine if 2 RECT are different(deep compare)</summary>
            public static bool operator !=(RECT rect1, RECT rect2) { return !(rect1 == rect2); }
        }

        [DllImport("user32")]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, MonitorInfo lpmi);

        [DllImport("User32")]
        internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);
        #endregion


        #region imageData struct
        struct ImageData
        {
            public string imagePath;
            public int duration;

            public ImageData(string imagePath, int duration)
            {
                this.imagePath = imagePath;
                this.duration = duration;
            }
        }
        #endregion


        private List<ImageData> GetData()
        {
            var imgs = new List<ImageData>();
            var ext = new List<string> { "jpg", "png" };
            var myFiles = Directory.EnumerateFiles(".\\refs", "*.*", SearchOption.AllDirectories)
                .Where(s => ext.Contains(Path.GetExtension(s).TrimStart('.').ToLowerInvariant()));

            foreach (string img in myFiles)
            {
                imgs.Add(new ImageData(img, 20));
            }

            return imgs;
        }


        public MainWindow()
        {
            InitializeComponent();

            isSettingsMenuVisible = false;

            DataContext = _viewModel = new MainWindowViewModel(); // create VM
            SourceInitialized += (s, e) =>
            {
                IntPtr handle = (new WindowInteropHelper(this)).Handle;
                HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(WindowProc));
            };
            // window control button logic
            MinimizeButton.Click += (s, e) => WindowState = WindowState.Minimized;
            MaximizeButton.Click += (s, e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            CloseButton.Click += (s, e) => Close();

            // init settings window
            settingsMenuWindow = new SettingsMenuWindow();

            // test image

            //~sampleImageTest();
        }


        private void ButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            if (isSettingsMenuVisible)
            {
                SettingsMenuGrid.Visibility = Visibility.Visible;
            }
            else
            {
                SettingsMenuGrid.Visibility = Visibility.Collapsed;
            }
            isSettingsMenuVisible = !isSettingsMenuVisible;
        }

        // TODO: something to save and load last used settings

        private void SelectDirectory_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Select Folder",
                Filter = "Image files (*.jpg, *.jpeg, *.png, *.gif, *.bmp)|*.jpg;*.jpeg;*.png;*.gif;*.bmp",
                Title = "Browse..."
            };

            if (dialog.ShowDialog() == true)
            {
                var directoryPath = Path.GetDirectoryName(dialog.FileName);
                var imageFiles = Directory.GetFiles(directoryPath)
                                          .Where(file => IsImageFile(file));

                if (imageFiles.Any())
                {
                    DirectoryTextBox.Text = directoryPath;
                    ButtonIndex_Click(null, null);
                }
                else
                {
                    // Handle the case where no image files were found in the selected directory
                }
            }
        }
        private void DirectoryTextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            textBox.Focus();
            textBox.SelectAll();
            e.Handled = true;
        }
        private void DirectoryTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(DirectoryTextBox.Text))
            {
                var imageFiles = Directory.GetFiles(DirectoryTextBox.Text)
                                          .Where(file => IsImageFile(file));

                if (imageFiles.Any())
                {
                    ButtonIndex_Click(null, null);
                }
                else
                {
                    // Handle the case where no image files were found in the selected directory
                }
            }
        }
        private void MainGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsTextBoxOrDescendant(e.OriginalSource as DependencyObject))
            {
                DirectoryTextBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }
        private bool IsTextBoxOrDescendant(DependencyObject element)
        {
            while (element != null)
            {
                if (element is TextBox)
                    return true;

                element = VisualTreeHelper.GetParent(element);
            }
            return false;
        }
        private bool IsImageFile(string fileName)
        {
            var extension = Path.GetExtension(fileName)?.ToLower();
            return extension == ".jpg" || extension == ".jpeg" || extension == ".png" ||
                   extension == ".gif" || extension == ".bmp";
        }

        private classes.ImageManager imageManager = null;
        private void ButtonIndex_Click(object sender, RoutedEventArgs e)
        {
            imageManager = new classes.ImageManager();
            imageManager.IndexImages(DirectoryTextBox.Text, ForceIndexCheckBox.IsChecked ?? false);
        }

        // PreviewTextInput function to only allow properly formatted durations
        private void DurationsTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Regex pattern to allow numbers separated by at most 1 comma or any number of spaces or both
            string regexPattern = @"^(?:\d+\s*,?\s*)*$";

            // Get the current TextBox
            TextBox textBox = (TextBox)sender;

            // Create a regular expression object
            Regex regex = new Regex(regexPattern);

            // Check if the new input matches the pattern
            bool isMatch = regex.IsMatch(textBox.Text + e.Text);

            // If the input does not match, mark the event as handled
            e.Handled = !isMatch;
        }

        // reads the text in the DuraionsTextBox and makes an array of the numbers
        private int[] ParseDurations()
        {
            string input = DurationsTextBox.Text;

            // Split input using regular expression pattern to handle various scenarios
            string[] stringArray = Regex.Split(input, @"\s*,\s*|\s+");

            // Remove empty entries from the resulting string array
            stringArray = stringArray.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

            int[] durations = new int[stringArray.Length];

            for (int i = 0; i < stringArray.Length; i++)
            {
                if (int.TryParse(stringArray[i], out int duration))
                {
                    durations[i] = duration;
                }
                else
                {
                    // Handle invalid duration values
                    // You can display an error message or take appropriate action here
                    throw new FormatException("Invalid duration value: " + stringArray[i]);
                }
            }

            return durations;
        }

        private async void Button_playpause_Click(object sender, RoutedEventArgs e)
        {
            // TODO: currently can't be pasued, pressing play/pause again just makes the loop run on top of itself making the timer look very wacky

            int[] durations = ParseDurations();

            for (int i = 0; i < durations.Length; i++)
            {
                await DrawImageLogic(imageManager.GetRandomImage(), durations[i]);
            }

            //var imgs = GetData();

            //loops thru two test images as a test
            //while (true)
            //{
            //    await DrawImageLogic("./refs/dergref1.jpg", 5);
            //    await DrawImageLogic("./refs/dergref2.jpg", 5);
            //    await DrawImageLogic("./refs/sample.jpg", 5);
            //    await DrawImageLogic("./refs/sample.png", 5);
            //    await DrawImageLogic("./refs/koboldref1.png", 5);
            //    await DrawImageLogic("./refs/koboldref2.png", 5);
            //}
        }

        //it just works. I really hope we won't need to touch it later.
        #region I am so sad

        private async Task DrawImageLogic(string imgPath, int duration)
        {
            ImageSourceConverter converter = new ImageSourceConverter();
            if (!File.Exists(imgPath))
            {
                Console.WriteLine("File does not exist."); // probably display an error to the user instead, currently just shows no image and still does the countdown
            }
            else
            {
                displayedImage.Source = (ImageSource)converter.ConvertFromString(imgPath);
            }

            await Task.Run(() =>
            {
                while (duration >= 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (duration <= 3)
                        {
                            RemainingTime.Background = new SolidColorBrush(Color.FromArgb(128, 255, 0, 0)); // 50% opacity (128/255)
                        }
                        else
                        {
                            RemainingTime.Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)); // 50% opacity (128/255)
                        }
                        TimeSpan timeSpan = TimeSpan.FromSeconds(duration);
                        RemainingTime.Text = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds); // formatted to mm:ss
                    });
                    duration--;
                    Thread.Sleep(1000);
                }
            });
        }


        #endregion
    }
}
