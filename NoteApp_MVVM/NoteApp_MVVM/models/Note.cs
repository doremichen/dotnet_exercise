/**
 * Copyright (c) 2024 Adam Chen
 * 
 * Description: This is the Note model class for the NoteApp_MVVM application.
 * 
 * 
 * Author: Adam Chen
 * Date: 2025/09/10
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteApp_MVVM.models
{
    internal class Note
    {
        // file name
        public string FileName { get; set; }
        // text content
        public string Content { get; set; }
        // Date time created
        public DateTime Date { get; set; }

        /**
         * Constructor
         */
        public Note()
        {
            FileName = $"{Path.GetRandomFileName()}.notes.txt";
            Content = string.Empty;
            Date = DateTime.Now;
        }

        /**
         * Save note to file
         */
        public void save()
        {
            // file path
            string filePath = Path.Combine(FileSystem.AppDataDirectory, FileName);
            File.WriteAllText(filePath, Content);
        }

        /**
         * Delete note file
         */
        public void delete()
        {
            // file path
            string filePath = Path.Combine(FileSystem.AppDataDirectory, FileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        /**
         * Load note from file
         */
        public static async Task<Note?> LoadAsync(string filename)
        {
            Debug.WriteLine($"+++ Load({filename}) +++");

            string filePath = Path.Combine(FileSystem.AppDataDirectory, filename);

            // check file existence
            if (!File.Exists(filePath))
            {
                await Application.Current.MainPage.DisplayAlert(
                    "File is not fond!!!",
                    $"404：{filePath}",
                    "Confirm"
                );

                return null; // file not found
            }

            return new Note
            {
                FileName = Path.GetFileName(filePath),
                Content = File.ReadAllText(filePath),
                Date = File.GetLastWriteTime(filePath)
            };
        }

        public static IEnumerable<Note?> LoadAll()
        {
            // Get folder path
            string appDataPath = FileSystem.AppDataDirectory;

            // lod the *.notes.txt files
            return Directory
                // Select the file names from the directory
                .EnumerateFiles(appDataPath, "*.notes.txt")
                // Each file name is used to load a note
                .Select(filename => Note.LoadAsync(Path.GetFileName(filename)).Result)
                // With the final collection of notes, order them by date
                .OrderByDescending(note => note?.Date);
        }


    }
}
