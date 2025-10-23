using MauiPuzzleHeroGame.Utils;
using MauiPuzzleHeroGame.ViewModels;

namespace MauiPuzzleHeroGame.Views;

public partial class GamePage : ContentPage
{
	public GamePage()
	{
		// Log
		Util.Log("[GamePage] Initializing GamePage...");

        InitializeComponent();
        // binding the ViewModel
        _viewModel = new ViewModels.GameViewModel();
        this.BindingContext = _viewModel;
    }

    private GameViewModel _viewModel;


    protected override void OnAppearing()
    {
        // Log
        Util.Log("[GamePage] OnAppearing called.");

        base.OnAppearing();

        // get size form preferences
        int gridSize = Preferences.Get(Util.PREFS_PUZZLE_GRID_SIZE, 3);

        // update game didplay
        _ = _viewModel.ApplyDifficultyAsync(gridSize);

    }
}