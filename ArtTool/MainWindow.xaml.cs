using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;
using ArtTool.classes;
using System.Diagnostics;
using System.Text.Json;

namespace ArtTool
{
    public partial class MainWindow : Window
    {
        #region Main Window Logic
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
            /// <summary>Return the HashCode for this struct (not guaranteed to be unique)</summary>
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

        public MainWindow()
        {
            InitializeComponent();
            SourceInitialized += (s, e) =>
            {
                IntPtr handle = (new WindowInteropHelper(this)).Handle;
                HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(WindowProc));
            };
            ReadSettingsJson();
            // window control button logic
            MinimizeButton.Click += (s, e) => WindowState = WindowState.Minimized;
            MaximizeButton.Click += (s, e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            CloseButton.Click += (s, e) => SaveAndClose();
        }


        private void SaveAndClose()
        {
            WriteSettingsJson();
            Close();
        }

        #region Settings Logic
        private void ReadSettingsJson()
        {
            if (File.Exists("settings.json"))
            {
                string jsonString = File.ReadAllText("settings.json");
                SavedSettings settings = JsonSerializer.Deserialize<SavedSettings>(jsonString);

                DirectoryTextBox.Text = settings.Directory;
                DurationsTextBox.Text = settings.Durations;
                LoopDurationsCheckbox.IsChecked = settings.LoopDurations;
                RemakeIndexCheckBox.IsChecked = settings.ForceIndex;
                SettingsMenuGrid.Visibility = settings.SettingsVisible ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        private void WriteSettingsJson()
        {
            SavedSettings settings = new SavedSettings
            {
                Directory = DirectoryTextBox.Text,
                Durations = DurationsTextBox.Text,
                LoopDurations = LoopDurationsCheckbox.IsChecked.Value,
                ForceIndex = RemakeIndexCheckBox.IsChecked.Value,
                SettingsVisible = SettingsMenuGrid.IsVisible
            };

            string jsonString = JsonSerializer.Serialize(settings, new JsonSerializerOptions());

            File.WriteAllText("settings.json", jsonString);
        }
        #endregion

        private void ButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            if (!SettingsMenuGrid.IsVisible)
            {
                SettingsMenuGrid.Visibility = Visibility.Visible;
            }
            else
            {
                SettingsMenuGrid.Visibility = Visibility.Collapsed;
            }
        }

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
                    CenterLabel.Content = string.Empty;
                    ButtonIndex_Click(null, null);
                }
                else
                {
                    CenterLabel.Content = "No Images Found";
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

        private ImageManager imageManager = null;
        private void ButtonIndex_Click(object sender, RoutedEventArgs e)
        {
            imageManager = new ImageManager();
            imageManager.IndexImages(DirectoryTextBox.Text, RemakeIndexCheckBox.IsChecked ?? false);
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

        // reads the text in the DurationsTextBox and makes an array of the numbers
        private int[] ParseDurations()
        {
            string input = DurationsTextBox.Text;
            if (input == "")
                return null;

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

        #region Bottom nav button logic
        private int runningStatus = 0; // 0 = stopped // 1 = playing // 2 = paused
        bool next = false; // if skipping current image
        bool previous = false; // if going back to previous image
        private void Button_previous_Click(object sender, RoutedEventArgs e)
        {
            if (runningStatus == 0)
                return;
            previous = true;
        }
        private async void Button_playpause_Click(object sender, RoutedEventArgs e)
        {
            if (runningStatus == 0) // if the timer hasn't started, start it
            {
                runningStatus = 1;
                int[] durations;
                if (!(DurationsTextBox.Text == ""))
                {
                    durations = ParseDurations();
                }
                else
                    return;
                ButtonIndex_Click(null, null);
                await DisplayLogic(imageManager, durations);
            }
            else if (runningStatus == 1)
            {
                runningStatus++; // add 1 to pause
            }
            else if (runningStatus == 2)
            {
                runningStatus--; // subtract 1 to play
            }
        }
        private void Button_stop_Click(object sender, RoutedEventArgs e)
        {
            runningStatus = 0;
        }

        private void Button_next_Click(object sender, RoutedEventArgs e)
        {
            if (runningStatus == 0)
                return;
            next = true;
        }
        #endregion

        private Stopwatch stopwatch = new Stopwatch();
        // manages play/pause and next/prev
        private async Task DisplayLogic(ImageManager imageManager, int[] durations)
        {
            for (int i = 0; i < durations.Length; i++)
            {
                if (runningStatus == 0) // stop button pushed
                    break;

                string imgPath;
                if (!previous)
                    imgPath = imageManager.GetNextImage();
                else // if going to previous image
                {
                    previous = false; // reset the flag
                    imgPath = imageManager.GetPreviousImage();
                }
                ImageSourceConverter converter = new ImageSourceConverter();
                if (!File.Exists(imgPath))
                {
                    Console.WriteLine("File does not exist. [" + imgPath + "]"); // display an error to the user instead, currently just shows no image and still does the countdown
                }
                else
                {
                    displayedImage.Source = (ImageSource)converter.ConvertFromString(imgPath);
                }
                await Task.Run(() =>
                {
                    stopwatch.Restart();
                    TimeSpan duration = TimeSpan.FromSeconds(durations[i]);
                    while (stopwatch.Elapsed.TotalSeconds < duration.TotalSeconds)
                    {
                        if (runningStatus == 1)
                        {
                            stopwatch.Start();
                            Dispatcher.Invoke(() =>
                            {
                                if (duration.TotalSeconds - stopwatch.Elapsed.TotalSeconds <= 4)
                                {
                                    RemainingTime.Background = new SolidColorBrush(Color.FromArgb(128, 255, 0, 0)); // 50% opacity (128/255)
                                }
                                else
                                {
                                    RemainingTime.Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)); // 50% opacity (128/255)
                                }
                                RemainingTime.Text = string.Format("{0:D2}:{1:D2}", (duration - stopwatch.Elapsed).Minutes, (duration - stopwatch.Elapsed).Seconds); // formatted to mm:ss
                            });
                        }
                        else if (runningStatus == 2)
                        {
                            stopwatch.Stop();
                        }
                        else if (runningStatus == 0) // stop button pushed
                        {
                            stopwatch.Stop();
                            break;
                        }
                        // the only way next having priority over previous is if someone manages to press both buttons before the program loops again
                        if (next)
                        {
                            next = false; // reset the flag
                            break;
                        }
                        if (previous)
                        {
                            if (i < 1)
                            {
                                break;
                            }
                            i=i-2; // go back a duration
                            break;
                        }
                        Thread.Sleep(10); // update rate, might not be needed
                    }
                });
                if (LoopDurationsCheckbox.IsChecked == true && (i+1 >= durations.Length))
                {
                    i = -1;
                }
            }
            runningStatus = 0;
            displayedImage.Source = null;
            ButtonIndex_Click(null, null);
        }
    }

    public class SavedSettings
    {
        public string Directory { get; set; }
        public string Durations { get; set; }
        public bool LoopDurations { get; set; }
        public bool ForceIndex { get; set; }
        public bool SettingsVisible { get; set; }
    }
}
