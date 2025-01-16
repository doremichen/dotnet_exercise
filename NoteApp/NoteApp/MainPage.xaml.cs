namespace NoteApp
{
    public partial class MainPage : ContentPage
    {
        string _fileName = Path.Combine(FileSystem.AppDataDirectory, "note.txt");

        public MainPage()
        {
            InitializeComponent();

            // check file ecistence
            if (File.Exists(_fileName))
            {
                editor.Text = File.ReadAllText(_fileName);
            }
        }

        async void OnSave(object sender, EventArgs e)
        {
            System.Diagnostics.Trace.WriteLine("+++ OnSave() +++");
            await DisplayAlert("Info", "Save button is clicked!!!", "Ok");
            File.WriteAllText(_fileName, editor.Text);
            System.Diagnostics.Trace.WriteLine("xxx OnSave() xxx");
        }

        async void OnCancel(object sender, EventArgs e)
        {
            await DisplayAlert("Info", "Cancel button is clicked!!!", "Ok");
            // clear text in screen
            editor.Text = "";

        }

        private void OnRead(object sender, EventArgs e)
        {
            // read text from note.txt
            editor.Text = File.ReadAllText(_fileName);
        }

        private async void onExit(object sender, EventArgs e)
        {
            bool shouldExit = await DisplayAlert("Confirm", "Are you sure you want to exit?", "Yes", "No");
            if (shouldExit)
            {
                Application.Current?.Quit();
            }
        }
    }

}
