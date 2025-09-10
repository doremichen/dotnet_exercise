/**
 * Copyright (c) 2024 Adam chen
 * 
 * Description: This is the NoteViewModel class for the NoteApp_MVVM application.
 * 
 * Author: Adam Chen
 * Date: 2025/09/10
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NoteApp_MVVM.viewModels
{
    internal class NoteViewModel : ObservableObject, IQueryAttributable
    {
        // Note model
        private models.Note _note;

        // Note content
        public string Content
        {
            get => _note.Content;
            set
            {
                // log change
                Debug.WriteLine($"Content changed: {value}");
                // log content of note
                Debug.WriteLine($"Note content: {_note.Content}");

                if (_note.Content != value)
                {
                    _note.Content = value;
                    OnPropertyChanged();
                }
            }
        }

        // Note date
        public DateTime Date => _note.Date;

        // Note file name
        public string Identifier => _note.FileName;

        public ICommand SaveCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }

        public NoteViewModel()
        {
            Debug.WriteLine("+++ NoteViewModel() +++");
            _note = new models.Note();
            Debug.WriteLine($"filename: {_note.FileName}");
            SaveCommand = new AsyncRelayCommand(Save);
            DeleteCommand = new AsyncRelayCommand(Delete);
            Debug.WriteLine("xxx NoteViewModel() xxx");
        }

        public NoteViewModel(models.Note note)
        {
            Debug.WriteLine("+++ NoteViewModel(note) +++");
            _note = note;
            SaveCommand = new AsyncRelayCommand(Save);
            DeleteCommand = new AsyncRelayCommand(Delete);
            Debug.WriteLine("+++ NoteViewModel(note) xxx");
        }

        private async Task Save()
        {
            Debug.WriteLine("+++ Save@NoteViewModel +++");
            _note.Date = DateTime.Now;
            _note.save();
            Debug.WriteLine($"+++ Save file: {_note.FileName} +++");
            string encodedFilename = WebUtility.UrlEncode(_note.FileName);
            await Shell.Current.GoToAsync($"..?saved={encodedFilename}");
            Debug.WriteLine("xxx Save@NoteViewModel xxx");
        }

        private async Task Delete()
        {
            _note.delete();
            string encodedFilename = WebUtility.UrlEncode(_note.FileName);
            await Shell.Current.GoToAsync($"..?deleted={encodedFilename}");
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            Debug.WriteLine("+++ ApplyQueryAttributes@NoteViewModel +++");

            if (query.ContainsKey("load"))
            {
                string? encodeNoteId = query["load"].ToString();
                string? noteId = WebUtility.UrlDecode(encodeNoteId);
                Debug.WriteLine($"The load file: {noteId}");
                _note = models.Note.LoadAsync(noteId!).Result;
                // log start refresh
                Debug.WriteLine("Start RefreshProperties()");
                RefreshProperties();
            }
            Debug.WriteLine("xxx ApplyQueryAttributes@NoteViewModel xxx");
        }

        public async void Reload()
        {
            _note = await models.Note.LoadAsync(_note.FileName) ?? new models.Note();
            RefreshProperties();
        }

        public void RefreshProperties()
        {
            OnPropertyChanged(nameof(Content));
            OnPropertyChanged(nameof(Date));
        }
    }
}
