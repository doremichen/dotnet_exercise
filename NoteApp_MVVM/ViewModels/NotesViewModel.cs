using CommunityToolkit.Mvvm.Input;
using Notes.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Diagnostics;
using System.Net;

namespace Notes.ViewModels
{
    internal class NotesViewModel : IQueryAttributable
    {
        public ObservableCollection<ViewModels.NoteViewModel> AllNotes { get;}
        public ICommand NewCommand { get; }
        public ICommand SelectNoteCommand { get; }

        public NotesViewModel()
        {
            AllNotes = new ObservableCollection<NoteViewModel>(Models.Note.LoadAll().Select(x => new NoteViewModel(x)));
            NewCommand = new AsyncRelayCommand(NewNoteAsync);
            SelectNoteCommand = new AsyncRelayCommand<ViewModels.NoteViewModel>(SelectNoteAsync);
        }

        private async Task NewNoteAsync()
        {
            await Shell.Current.GoToAsync(nameof(Views.NotePage));
        }

        private async Task SelectNoteAsync(ViewModels.NoteViewModel note)
        {
            if (note != null)
            {
                string encodedFilename = WebUtility.UrlEncode(note.Identifier);
                await Shell.Current.GoToAsync($"{nameof(Views.NotePage)}?load={encodedFilename}");
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
                string encodeNoteId = query["deleted"].ToString();
                string noteId = WebUtility.UrlDecode(encodeNoteId);
                NoteViewModel matchedNote = AllNotes.Where((n) =>  n.Identifier == noteId).FirstOrDefault();
                // delete if mached note is existenced
                if (matchedNote != null)
                {
                    AllNotes.Remove(matchedNote);
                   
                }
            }
            else if (query.ContainsKey("saved"))
            {
                string encodeNoteId = query["saved"].ToString();
                string noteId = WebUtility.UrlDecode(encodeNoteId);
                Debug.WriteLine($"noteId: {noteId}");
                NoteViewModel matchedNote = AllNotes.Where((n) => n.Identifier == noteId).FirstOrDefault();

                if (matchedNote != null)
                {
                    matchedNote.Reload();
                    AllNotes.Move(AllNotes.IndexOf(matchedNote), 0);
                } 
                else
                {
                    AllNotes.Insert(0, new NoteViewModel(Models.Note.Load(noteId)));
                }
            }
            Debug.WriteLine("xxx ApplyQueryAttributes@NotessssssViewModel xxx");
        }
    }

   
}
