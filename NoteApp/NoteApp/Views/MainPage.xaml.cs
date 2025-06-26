namespace NoteApp.Views
{
    [QueryProperty(nameof(ItemId), nameof(ItemId))]
    public partial class MainPage : ContentPage
    {
        public string ItemId
        {
            set { LoadNote(value); }
        }


        public MainPage()
        {
            InitializeComponent();

            string appDataPath = FileSystem.AppDataDirectory;
            string randomFileName = $"{Path.GetRandomFileName()}.notes.txt";

            LoadNote(Path.Combine(appDataPath, randomFileName));
        }

        async void OnSave(object sender, EventArgs e)
        {
            System.Diagnostics.Trace.WriteLine("+++ OnSave() +++");

            // 檢查 TextEditor.Text 是否為空
            if (string.IsNullOrWhiteSpace(TextEditor.Text))
            {
                // 顯示警告訊息
                await DisplayAlert("Warning", "空值無法存入!!!", "Ok");
                return; // 終止方法執行
            }

            if (BindingContext is Models.Note note)
                File.WriteAllText(note.Filename, TextEditor.Text);

            await Shell.Current.GoToAsync("..");
            System.Diagnostics.Trace.WriteLine("xxx OnSave() xxx");
        }

        async void OnCancel(object sender, EventArgs e)
        {
            //await DisplayAlert("Info", "Cancel button is clicked!!!", "Ok");
            // clear text in screen
            TextEditor.Text = string.Empty;

        }

        private void OnRead(object sender, EventArgs e)
        {
            // check file existence
            if (BindingContext is Models.Note note)
            {
                // Delete the file.
                if (File.Exists(note.Filename))
                    info.Text = File.ReadAllText(note.Filename);
            }
        }

        private async void onExit(object sender, EventArgs e)
        {
            bool shouldExit = await DisplayAlert("Confirm", "Are you sure you want to exit?", "Yes", "No");
            if (shouldExit)
            {
                Application.Current?.Quit();
            }
        }

        private void LoadNote(string fileName)
        {
            Models.Note noteModel = new Models.Note();
            noteModel.Filename = fileName;

            if (File.Exists(fileName))
            {
                noteModel.Date = File.GetCreationTime(fileName);
                noteModel.Textinfo = File.ReadAllText(fileName);
            }

            BindingContext = noteModel;
        }

        private async void OnDelete(object sender, EventArgs e)
        {
            System.Diagnostics.Trace.WriteLine("+++ OnDelete() +++");
            if (BindingContext is Models.Note note)
            {
                // Delete the file.
                if (File.Exists(note.Filename))
                    File.Delete(note.Filename);
            }

            await Shell.Current.GoToAsync("..");
            System.Diagnostics.Trace.WriteLine("xxx OnDelete() xxx");
        }
    }

}
