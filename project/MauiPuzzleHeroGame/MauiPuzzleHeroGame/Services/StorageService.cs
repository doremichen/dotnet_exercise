/**
 * Copyright (c) 2025 Adam Game. All rights reserved.
 * 
 * Description: This class is used to access game progress, read and write files (JSON format)
 * Functions:
 * Access game progress, settings, leaderboards, etc.
 * 
 * Author: Adam Chen
 * Date: 2025/10/20
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MauiPuzzleHeroGame.Services
{
    public class StorageService
    {
        private readonly string _basePath;

        /**
         * Constructor to initialize the storage service with the base path.
         */
        public StorageService()
        {
            _basePath = FileSystem.AppDataDirectory;
        }

        /**
         * SaveAsync 
         * Save text content to a file asynchronously.
         * 
         * <param name="fileName">The name of the file</param>
         * <param name="content">The text content to save</param>
         */
        public async Task SaveAsync<T>(string fileName, T data)
        {
            try
            {
                var filePath = Path.Combine(_basePath, fileName);
                var json = System.Text.Json.JsonSerializer.Serialize(data,
                    new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Save file fail: {ex.Message}");
            }
            
        }

        /**
         * LoadAsync
         * Load text content from a file asynchronously.
         * 
         * <param name="fileName">The name of the file</param>
         * <returns>The text content loaded from the file</returns>
         */
        public async Task<T?> LoadAsync<T>(string fileName)
        {
            try
            {
                var filePath = Path.Combine(_basePath, fileName);
                if (!File.Exists(filePath))
                    return default;
                var json = await File.ReadAllTextAsync(filePath);
                return System.Text.Json.JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Load file fail: {ex.Message}");
                return default;
            }
        }

        /**
         * Exists
         * Check if the file exists
         */
        public bool Exists(string fileName)
        {
            var filePath = Path.Combine(_basePath, fileName);
            return File.Exists(filePath);
        }

        /**
         * Delete
         * Delete a file
         */
        public void Delete(string fileName)
        {
            var filePath = Path.Combine(_basePath, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

    }
}
