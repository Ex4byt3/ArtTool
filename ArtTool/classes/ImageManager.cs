using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ArtTool.classes
{
    public class ImageManager
    {
        private List<string> imageFiles;
        private Random random;

        public ImageManager()
        {
            imageFiles = new List<string>();
            random = new Random();
        }

        // Creates the index file if it does not exist or reads from it if it does
        // Or remaking the index can be forced
        public void IndexImages(string directoryPath, bool remakeIndex)
        {
            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine("Directory does not exist.");
                return;
            }

            // Check if index file already exists
            string indexPath = Path.Combine(directoryPath, "index.txt");
            if (File.Exists(indexPath) && !remakeIndex)
            {
                // Load existing index
                imageFiles = File.ReadAllLines(indexPath).ToList();
                return;
            }

            // Index all image files in the directory
            string[] extensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            imageFiles = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories)
                .Where(file => extensions.Contains(Path.GetExtension(file).ToLower()))
                .ToList();

            // Save the index to a file
            File.WriteAllLines(indexPath, imageFiles);
        }

        // returns a random exact file path
        public string GetRandomImage()
        {
            if (imageFiles.Count == 0)
            {
                Console.WriteLine("No images indexed.");
                return null;
            }

            // Pick a random image from the list
            int index = random.Next(imageFiles.Count);
            string randomImage = imageFiles[index];

            // Remove the image from the list to prevent duplicates in a session
            imageFiles.RemoveAt(index);

            return randomImage;
        }
    }
}
