/**
 * Copyright (c) 2025 Adam Game. All rights reserved.
 * 
 * Description: This class is used to Image loading, zooming, and album selection
 * Functions:
 * Image processing using SkiaSharp
 * 
 * Author: Adam Chen
 * Date: 2025/10/20
 * 
 */
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiPuzzleHeroGame.Services
{
    public class ImageService
    {
        /**
         * LoadFromFileAsync
         * Load an ImageSource from a file path
         * 
         * <param name="filePath">The file path of the image</param>
         * 
         * <returns>An ImageSource object</returns>
         */
        public async Task<ImageSource> LoadFromFileAsync(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                throw new FileNotFoundException("404: ", path);

            return await Task.Run(() => ImageSource.FromFile(path));
        }

        /**
         * PickFromGalleryAsync
         * picker an image from the device gallery
         * 
         * returns: The file path of the selected image, or null if no image was selected
         */
        public async Task<string?> PickFromGalleryAsync()
        {
            try
            {
                var file = await MediaPicker.Default.PickPhotoAsync();
                return file?.FullPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Picker file fail: {ex.Message}");
                return null;
            }
        }

        /**
         * ResizeImageAsync
         * resize an image to the specified width and height
         * 
         * param filePath: The file path of the image to resize
         * param width: The desired width
         * param height: The desired height
         * 
         * returns: An ImageSource of the resized image
         */
        public async Task<ImageSource> ResizeImageAsync(string filePath, int width, int height)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("404: ", filePath);

            return await Task.Run(() =>
            {
                using var input = File.OpenRead(filePath);
                using var original = SKBitmap.Decode(input);
                using var resized = original.Resize(new SKImageInfo(width, height), SKFilterQuality.Medium);
                using var image = SKImage.FromBitmap(resized);
                using var ms = new MemoryStream();
                image.Encode(SKEncodedImageFormat.Png, 100).SaveTo(ms);
                ms.Position = 0;
                return ImageSource.FromStream(() => ms);
            });
        }
    }
}
