using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace Notes.ViewModels
{
    internal class AboutViewModel
    {

        public string Title => AppInfo.Name;
        public string Version => AppInfo.VersionString;
        public string MoerInfoUrl = "https://aka.ms/maui";
        public string Message = "This is practice home work of mvvm!!!";
        // show information
        public ICommand ShowMoreInfoCommand { get; }

        // set info command by construct
        public AboutViewModel()
        {
            ShowMoreInfoCommand = new AsyncRelayCommand(ShowMoreInfo);
        }

        async Task ShowMoreInfo() =>
            await Launcher.Default.OpenAsync(MoerInfoUrl);
    }
}
