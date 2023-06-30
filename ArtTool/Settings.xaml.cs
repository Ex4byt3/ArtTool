using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Text.Json;
using System;
using System.Text.Json.Serialization;
using System.Xml;
using System.Threading.Tasks;

namespace ArtTool
{
    /// <summary>
    /// Логика взаимодействия для Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();

            if (!File.Exists("settings.json")) {
                CreateSettingsAsync();
            } 

        }

        private async void CreateSettingsAsync()
        {
            var imgs = new List<ImageData>();
            var ext = new List<string> { "jpg", "png" };
            var myFiles = Directory.EnumerateFiles(".\\refs", "*.*", SearchOption.AllDirectories)
                .Where(s => ext.Contains(System.IO.Path.GetExtension(s).TrimStart('.').ToLowerInvariant()));

            foreach (string img in myFiles)
            {
                imgs.Add(new ImageData(img, 20));
            }

            using (FileStream fs = new FileStream("settings.json", FileMode.OpenOrCreate))
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                await JsonSerializer.SerializeAsync(fs, imgs);
            }
            
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }
    }

    class ImageData
    {
        public string Filename { get; set; }
        public int Duration { get; set; }

        public ImageData(string filename, int duration)
        {
            Filename = filename;
            Duration = duration;
        }

    }
}
