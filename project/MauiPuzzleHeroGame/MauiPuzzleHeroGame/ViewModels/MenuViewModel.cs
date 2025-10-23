/**
 * Copyright (c) 2025 Adam Game. All rights reserved.
 * 
 * Description: This Class is the ViewModel for the Menu in the Maui Puzzle Hero Game.
 * 
 * Author: Adam Chen
 * Date: 2025/10/17
 * 
 */
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiPuzzleHeroGame.Utils;
using MauiPuzzleHeroGame.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiPuzzleHeroGame.ViewModels
{
    public partial class MenuViewModel : ObservableObject
    {


        [RelayCommand]
        private async Task StartGameAsync()
        {
            // log the action
            Util.Log("Starting new game...");
            await Shell.Current.GoToAsync("///GamePage");

        }

        [RelayCommand]
        private async Task AboutAsync()
        {
            // log the action
            Util.Log("Navigating to About Page...");
            await Shell.Current.GoToAsync("///AboutPage");
        }

        [RelayCommand]
        private async Task ExitAsync()
        {
            Util.Log("Exiting application...");
#if ANDROID
            // Android exit application
            Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
#elif WINDOWS
            Environment.Exit(0);
#else
        await Shell.Current.DisplayAlert("Alert", "Not support in the current platform!!!", "OK");
#endif
        }


    }
}
