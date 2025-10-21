using MauiPuzzleHeroGame.Utils;

namespace MauiPuzzleHeroGame.Views;

public partial class GamePage : ContentPage
{
	public GamePage()
	{
		// Log
		Util.Log("[GamePage] Initializing GamePage...");

        InitializeComponent();
        // binding the ViewModel
		this.BindingContext = new ViewModels.GameViewModel();
    }
}