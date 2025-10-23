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
            // Show alert to tell user the app is exiting
            await Shell.Current.DisplayAlert("離開", "此遊戲即將離開?", "確定").ContinueWith(
                 action =>
                 {
#if ANDROID
                     // Android exit application
                     Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
#elif WINDOWS
                     Environment.Exit(0);
#elif IOS
                    // iOS exit application
                    // Note: Apple discourages programmatic exits, this is just for demonstration.
                    UIKit.UIApplication.SharedApplication.PerformSelector(new ObjCRuntime.Selector("terminateWithSuccess"), null, 0);
#endif
                 }
                );

        }


    }
}
