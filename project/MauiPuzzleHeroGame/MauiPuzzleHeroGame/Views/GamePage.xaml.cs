namespace MauiPuzzleHeroGame.Views;

public partial class GamePage : ContentPage
{
	public GamePage()
	{
		InitializeComponent();
        // binding the ViewModel
		this.BindingContext = new ViewModels.GameViewModel();
    }
}