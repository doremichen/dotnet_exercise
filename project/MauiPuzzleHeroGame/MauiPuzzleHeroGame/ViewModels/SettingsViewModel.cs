/**
 * Copyright (c) 2025 Adam Game. All rights reserved.
 * 
 * Description: This Class is the ViewModel for the Settings in the Maui Puzzle Hero Game.
 * 
 * Author: Adam Chen
 * Date: 2025/10/22
 * 
 */
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiPuzzleHeroGame.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiPuzzleHeroGame.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<DifficultyOption> difficulties;

        [ObservableProperty]
        private DifficultyOption? selectedDifficulty;

        /// <summary>
        /// constructor
        /// </summary>
        public SettingsViewModel()
        {
            difficulties = new ObservableCollection<DifficultyOption>
            {
                new DifficultyOption("Easy (3x3)", 3),
                new DifficultyOption("Medium (4x4)", 4),
                new DifficultyOption("Hard (5x5)", 5),
                new DifficultyOption("Expert (6x6)", 6)
            };

            // get last selected difficulty from preferences
            var lastSelected = Preferences.Get(Util.PREFS_PUZZLE_GRID_SIZE, 3);

            // set default selected difficulty
            var match = difficulties.FirstOrDefault(d => d.Size == lastSelected);

            if (match != null)
            {
                // set SelectedDifficulty using property to trigger OnSelectedDifficultyChanged
                SelectedDifficulty = match;
            }

        }

        /// <summary>
        /// SaveAsync
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        private async Task SaveAsync()
        {
            if (selectedDifficulty != null)
            {
                // save selected difficulty to preferences
                Preferences.Set(Util.PREFS_PUZZLE_GRID_SIZE, selectedDifficulty.Size);
                Util.Log($"[SettingsViewModel] Saved difficulty: {selectedDifficulty.Name} ({selectedDifficulty.Size}x{selectedDifficulty.Size})");
            }
            // navigate back
            await Shell.Current.GoToAsync("///GamePage");
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            await Shell.Current.GoToAsync("///GamePage");
        }

        partial void OnSelectedDifficultyChanged(DifficultyOption? value)
        {

            if (Difficulties == null || value == null)
            {
                return;
            }

            // update IsSelected for each difficulty option
            foreach (var difficulty in difficulties)
            {
                difficulty.IsSelected = (difficulty == value);
            }
        }


    }


    public partial class DifficultyOption : ObservableObject
    {
        public string Name { get; }
        public int Size { get; }

        [ObservableProperty]
        private bool isSelected;

        public DifficultyOption(string name, int size)
        {
            Name = name;
            Size = size;
            isSelected = false;
        }
    }

}
