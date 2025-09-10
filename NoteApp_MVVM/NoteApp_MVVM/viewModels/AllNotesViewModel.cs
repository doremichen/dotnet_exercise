/**
 * copyright (c) 2024 Adam chen
 * 
 * Description: This is the AllNotesViewModel class for the NoteApp_MVVM application.
 * 
 * Author: Adam Chen
 * Date: 2025/09/10
 */
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NoteApp_MVVM.viewModels
{
    internal class AllNotesViewModel : IQueryAttributable
    {

        public ObservableCollection<viewModels.NoteViewModel> AllNotes { get; }
        public ICommand NewCommand { get; }
        public ICommand SelectNoteCommand { get; }

        /**
         * Constructor
         */
        public AllNotesViewModel()
        {
            AllNotes = new ObservableCollection<NoteViewModel>(models.Note.LoadAll().Select(x => new NoteViewModel(x)));
            NewCommand = new AsyncRelayCommand(NewNoteAsync);
            SelectNoteCommand = new AsyncRelayCommand<viewModels.NoteViewModel>(SelectNoteAsync);
        }

        private async Task NewNoteAsync()
        {
            await Shell.Current.GoToAsync(nameof(views.NotePage));
        }

        private async Task SelectNoteAsync(viewModels.NoteViewModel note)
        {
            if (note != null)
            {
                string encodedFilename = WebUtility.UrlEncode(note.Identifier);
                await Shell.Current.GoToAsync($"{nameof(views.NotePage)}?load={encodedFilename}");
            }
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            Debug.WriteLine("+++ ApplyQueryAttributes@NotessssssViewModel +++");
            if (query == null)
            {
                return;
            }

            if (query.ContainsKey("deleted"))
            {
                string? encodeNoteId = query["deleted"].ToString();
                string? noteId = WebUtility.UrlDecode(encodeNoteId);
                NoteViewModel? matchedNote = AllNotes.Where((n) => n.Identifier == noteId).FirstOrDefault();
                // delete if mached note is existenced
                if (matchedNote != null)
                {
                    AllNotes.Remove(matchedNote);

                }
            }
            else if (query.ContainsKey("saved"))
            {
                string? encodeNoteId = query["saved"].ToString();
                string? noteId = WebUtility.UrlDecode(encodeNoteId);
                Debug.WriteLine($"noteId: {noteId}");
                NoteViewModel? matchedNote = AllNotes.Where((n) => n.Identifier == noteId).FirstOrDefault();

                if (matchedNote != null)
                {
                    matchedNote.Reload();
                    AllNotes.Move(AllNotes.IndexOf(matchedNote), 0);
                }
                else
                {
                    AllNotes.Insert(0, new NoteViewModel(models.Note.LoadAsync(noteId!).Result!));
                }
            }
            Debug.WriteLine("xxx ApplyQueryAttributes@NotessssssViewModel xxx");
        }

    }
}
