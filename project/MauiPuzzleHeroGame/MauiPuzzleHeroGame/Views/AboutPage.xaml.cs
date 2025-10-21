using MauiPuzzleHeroGame.ViewModels;

namespace MauiPuzzleHeroGame.Views;

public partial class AboutPage : ContentPage
{
	public AboutPage()
	{
		InitializeComponent();
        // binding the ViewModel
		this.BindingContext = new AboutViewModel();
    }
}