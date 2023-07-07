using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ArtTool.classes
{
    public class ImageManager
    {
        private List<string> imagePaths;
        private List<string> usedImagePaths;
        private int imageIdx = 0;
        private Random random;

        public ImageManager()
        {
            imagePaths = new List<string>();
            usedImagePaths = new List<string>();
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
                imagePaths = File.ReadAllLines(indexPath).ToList();
                return;
            }

            // Index all image files in the directory
            string[] extensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            imagePaths = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories)
                .Where(file => extensions.Contains(Path.GetExtension(file).ToLower()))
                .ToList();

            // Save the index to a file
            File.WriteAllLines(indexPath, imagePaths);
        }

        // if there is no next image in the usedImagePaths list return a random exact file path
        // else read from usedImagePaths and move imageIdx accordingly
        public string GetNextImage()
        {
            if (usedImagePaths.Count == imageIdx)
            {
                if (imagePaths.Count == 0)
                {
                    Console.WriteLine("No images indexed.");
                    return null;
                }

                // Pick a random image from the list
                int index = random.Next(imagePaths.Count);
                string randomImage = imagePaths[index];

                // Keeps a history
                usedImagePaths.Add(randomImage);
                // Remove the image from the list to prevent duplicates in a session
                imagePaths.RemoveAt(index);

                imageIdx++;
                return randomImage;
            }
            else
            {
                imageIdx = imageIdx + 2;
                return usedImagePaths[imageIdx-1];
            }
        }

        // Gets the previous image that was displayed
        public string GetPreviousImage()
        {
            if (imageIdx < usedImagePaths.Count)
            {
                return null;
            }
            imageIdx = imageIdx - 2;
            return usedImagePaths[imageIdx];
        }
    }
}
