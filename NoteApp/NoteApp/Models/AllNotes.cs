using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteApp.Models
{
    internal class AllNotes
    {
        // create observable collection
        public ObservableCollection<Note> Notes { get; set;} = new ObservableCollection<Note>();

        public AllNotes() => LoadNotes();

        // load all notes
        public void LoadNotes()
        {
            // clear Notes
            Notes.Clear();

            // Get folder where the notes are stored
            var appDataPath = FileSystem.AppDataDirectory;

            // load notes from folder
            IEnumerable<Note> notes = Directory
                .EnumerateFiles(appDataPath, "*.notes.txt")
                .Select(filename => new Note()
                {
                    Filename = filename,
                    Textinfo = File.ReadAllText(filename),
                    Date = File.GetLastWriteTime(filename)
                })
                .OrderBy(note => note.Date);

            // Add note in observable
            foreach (var note in notes)
            {
                Notes.Add(note);
            }
        }

    }
}
