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
            await DisplayAlert("Info", "Save button is clicked!!!", "Ok");
        }

        async void OnCancel(object sender, EventArgs e)
        {
            await DisplayAlert("Info", "Cancel button is clicked!!!", "Ok");
        }
    }

}
