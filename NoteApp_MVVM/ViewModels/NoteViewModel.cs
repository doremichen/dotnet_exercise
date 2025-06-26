using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;
using System.Diagnostics;
using System.Net;

namespace Notes.ViewModels
{
    internal class NoteViewModel : ObservableObject, IQueryAttributable
    {
        private Models.Note _note;

        public string Text
        {
            get => _note.Text;
            set
            {
                if (_note.Text != value)
                {
                    _note.Text = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime Date => _note.Date;

        public string Identifier => _note.Filename;

        public ICommand SaveCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }

        public NoteViewModel()
        {
            Debug.WriteLine("+++ NoteViewModel() +++");
            _note = new Models.Note();
            Debug.WriteLine($"filename: {_note.Filename}");
            SaveCommand = new AsyncRelayCommand(Save);
            DeleteCommand = new AsyncRelayCommand(Delete);
            Debug.WriteLine("xxx NoteViewModel() xxx");
        }

        public NoteViewModel(Models.Note note)
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
            _note.Save();
            Debug.WriteLine($"+++ Save file: {_note.Filename} +++");
            string encodedFilename = WebUtility.UrlEncode(_note.Filename);
            await Shell.Current.GoToAsync($"..?saved={encodedFilename}");
            Debug.WriteLine("xxx Save@NoteViewModel xxx");
        }

        private async Task Delete()
        {
            _note.Delete();
            string encodedFilename = WebUtility.UrlEncode(_note.Filename);
            await Shell.Current.GoToAsync($"..?deleted={encodedFilename}");
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            Debug.WriteLine("+++ ApplyQueryAttributes@NoteViewModel +++");

            if (query.ContainsKey("load"))
            {
                string encodeNoteId = query["load"].ToString();
                string noteId = WebUtility.UrlDecode(encodeNoteId);
                Debug.WriteLine($"The load file: {noteId}");
                _note = Models.Note.Load(noteId);
                RefreshProperties();
            }
            Debug.WriteLine("xxx ApplyQueryAttributes@NoteViewModel xxx");
        }

        public void Reload()
        {
            _note = Models.Note.Load(_note.Filename);
            RefreshProperties();
        }

        public void RefreshProperties()
        {
            OnPropertyChanged(nameof(Text));
            OnPropertyChanged(nameof(Date));
        }

    }
}
