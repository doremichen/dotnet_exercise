/**
 * Copyright (c) 2025 Adam Game. All rights reserved.
 * 
 * Description:
 *  Handles image loading, resizing, and gallery selection.
 *  Supports cross-platform safe image access and SkiaSharp resizing.
 *  Includes thumbnail caching for performance optimization.
 * 
 * Author: Adam Chen
 * Date: 2025/10/21
 */
using MauiPuzzleHeroGame.Utils;
using SkiaSharp;

namespace MauiPuzzleHeroGame.Services
{
    public class ImageService
    {
#if WINDOWS
        private readonly string _cacheDir;
#endif
        private readonly TimeSpan _cacheExpireDuration = TimeSpan.FromDays(7); // Cache expiration duration


        public ImageService()
        {
            // log
           Util.Log("[ImageService] Initializing ImageService...");
#if WINDOWS
            _cacheDir = Path.Combine(FileSystem.CacheDirectory, "ResizedImages");
            createCachedDirectoryIfNeeded();

            // Auto clean old cache files at startup
            _ = Task.Run(AutoCleanOldCacheFiles);
#endif
        }

#if WINDOWS
        private void createCachedDirectoryIfNeeded()
        {
            // log
             Util.Log("[ImageService] Checking cache directory...");
            if (!Directory.Exists(_cacheDir))
            {
                // log
                Util.Log($"Creating cache directory at {_cacheDir}");
                try
                {
                    Directory.CreateDirectory(_cacheDir);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Cannot create cache dir: {_cacheDir}, {ex.Message}");
                }
            }
        }
#endif

        /// <summary>
        /// Pick an image from the gallery.
        /// </summary>
        public async Task<string?> PickFromGalleryAsync()
        {
            // log
            Util.Log("[ImageService] Opening media picker...");

            try
            {
                var file = await MediaPicker.Default.PickPhotoAsync();
                // log file picked
                Util.Log(file == null
                    ? "[ImageService] No file picked."
                    : $"[ImageService] Picked file: {file.FullPath}");
                Util.Log("[ImageService] Media picker completed.");
                return file?.FullPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Picker file fail: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Load image stream (default or from file path).
        /// </summary>
        public async Task<Stream> GetImageStreamAsync(string? filePath)
        {
            // LOG
            Util.Log("GetImageStreamAsync enter");

            if (string.IsNullOrEmpty(filePath))
            {
                const string imageName = "dotnet_default.png";
                Util.Log($"Loading default image from app package: {imageName}");

                // On other platforms, use MAUI's API
                return await FileSystem.OpenAppPackageFileAsync(imageName);
            }

            return File.OpenRead(filePath);
        }

        /// <summary>
        /// Resize an image and return both ImageSource and resized Stream.
        /// Includes auto-caching in app's cache directory.
        /// </summary>
        /// <param name="filePath">Image file path (nullable = use default image)</param>
        /// <param name="width">Target width</param>
        /// <param name="height">Target height</param>
        /// <returns>(ImageSource, Stream)</returns>
        public async Task<(ImageSource displayImage, Stream resizedStream)> ResizeImageAsync(string? filePath, int width, int height)
        {
            // log
            Util.Log($"ResizeImageAsync enter: filePath={filePath}, width={width}, height={height}");

            try
            {
#if WINDOWS
                // Determine cache file path (use hash of filename + dimensions)
                string key = string.IsNullOrEmpty(filePath)
                    ? "default_dotnet_bot"
                    : Path.GetFileNameWithoutExtension(filePath);
                string cacheFileName = $"{key}_{width}x{height}.png";
                string cachePath = Path.Combine(_cacheDir, cacheFileName);

                // log cache path
                Util.Log($"Cache path: {cachePath}");

                // If cache exists, load directly
                if (File.Exists(cachePath))
                {
                    Console.WriteLine($"[ImageService] Using cached image: {cachePath}");
                    var cachedStream = File.OpenRead(cachePath);
                    var displayCache = new MemoryStream(await File.ReadAllBytesAsync(cachePath));
                    var displayImg = ImageSource.FromStream(() => displayCache);
                    // log
                    Util.Log("ResizeImageAsync using cached image.");
                    return (displayImg, cachedStream);
                }
#endif

                // Otherwise, load original and resize
                using var inputStream = await GetImageStreamAsync(filePath);
                using var bitmap = SKBitmap.Decode(inputStream);

                var resized = bitmap.Resize(new SKImageInfo(width, height), SKFilterQuality.Medium);
                if (resized == null)
                    throw new Exception("Image resize failed.");

                using var image = SKImage.FromBitmap(resized);
                var encodedData = image.Encode(SKEncodedImageFormat.Png, 100);
                var buffer = encodedData.ToArray();

                // Return both ImageSource and Stream
                var displayImage = ImageSource.FromStream(() =>
                {
                    var ms = new MemoryStream(buffer);
                    ms.Position = 0;
#if WINDOWS
                //// Save to cache
                //await File.WriteAllBytesAsync(cachePath, ms.ToArray());
                //Console.WriteLine($"[ImageService] Cached resized image at: {cachePath}");
#endif
                    return ms;
                });
                // log
                Util.Log("ResizeImageAsync completed resizing.");
                return (displayImage, new MemoryStream(buffer));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ResizeImageAsync error: {ex.Message}");
                throw;
            }
        }

#if WINDOWS
        /// <summary>
        /// Clear all cached resized images.
        /// </summary>
        public void ClearCache()
        {
            try
            {
                if (Directory.Exists(_cacheDir))
                {
                    foreach (var file in Directory.GetFiles(_cacheDir))
                        File.Delete(file);

                    Console.WriteLine("[ImageService] Cache cleared.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ImageService] Clear cache fail: {ex.Message}");
            }
        }

        /// <summary>
        /// Auto clean cache files older than 7 days.
        /// Runs in background at service startup.
        /// </summary>
        private void AutoCleanOldCacheFiles()
        {
            try
            {
                if (!Directory.Exists(_cacheDir))
                    return;

                var files = Directory.GetFiles(_cacheDir);
                int removed = 0;

                foreach (var file in files)
                {
                    var info = new FileInfo(file);
                    if (DateTime.Now - info.LastWriteTime > _cacheExpireDuration)
                    {
                        info.Delete();
                        removed++;
                    }
                }

                if (removed > 0)
                    Console.WriteLine($"[ImageService] Auto-cleaned {removed} expired cache files.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ImageService] AutoCleanOldCacheFiles fail: {ex.Message}");
            }
        }
#endif


    }
}
