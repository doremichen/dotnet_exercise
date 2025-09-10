namespace NoteApp_MVVM
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(views.NotePage), typeof(views.NotePage));
        }
    }
}
