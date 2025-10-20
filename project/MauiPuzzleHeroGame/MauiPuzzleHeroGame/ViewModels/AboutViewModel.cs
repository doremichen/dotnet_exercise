/**
 * Copyright (c) 2025 Adam Game. All rights reserved.
 * 
 * Description: This Class is the ViewModel for the About Page in the Maui Puzzle Hero Game.
 * 
 * Author: Adam Chen
 * Date: 2025/10/17
 * 
 */
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiPuzzleHeroGame.ViewModels
{
    public partial class AboutViewModel : ObservableObject
    {
        [RelayCommand]
        private async Task CloseAsync()
        {
            await Shell.Current.GoToAsync(".."); // back to previous page
        }
    }
}
