/**
 * Copyright (c) 2025 Adam Game. All rights reserved.
 * 
 * Description: This Class is the code-behind for the Settings Page in the Maui Puzzle Hero Game.
 * Author: Adam Chen
 * Date: 2025/10/22
 * 
 */
namespace MauiPuzzleHeroGame.Views;

public partial class SettingsPage : ContentPage
{
	public SettingsPage()
	{
		InitializeComponent();
        // set the BindingContext to the SettingsViewModel
		BindingContext = new ViewModels.SettingsViewModel();
    }
}