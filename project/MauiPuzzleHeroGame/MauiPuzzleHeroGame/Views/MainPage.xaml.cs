/**
 * Copyright (c) 2025 Adam Game. All rights reserved.
 * 
 * Description: This class is the main page code-behind for the Maui Puzzle Hero Game.
 * 
 * 
 * Author: Adam Chen
 * Date: 2025/10/20
 * 
 */


namespace MauiPuzzleHeroGame.Views
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
            // binding the ViewModel
            this.BindingContext = new ViewModels.MenuViewModel();
        }

    }
}
