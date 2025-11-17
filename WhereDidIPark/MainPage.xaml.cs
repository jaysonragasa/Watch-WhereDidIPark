using System.ComponentModel;
using WhereDidIPark.ViewModels;

namespace WhereDidIPark;

public partial class MainPage : ContentPage
{
	private readonly MainViewModel _viewModel;

	public MainPage()
	{
		InitializeComponent();
		_viewModel = new MainViewModel();
		BindingContext = _viewModel;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		_viewModel.PropertyChanged += OnViewModelPropertyChanged;
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		_viewModel.PropertyChanged -= OnViewModelPropertyChanged;
	}

	private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(MainViewModel.ArrowRotation))
		{
			// Use the RotateTo extension method for a smooth animation
			// 250ms duration with a linear easing function
			CompassArrowImage.RotateTo(_viewModel.ArrowRotation, 250, Easing.CubicInOut);
		}
		else if (e.PropertyName == nameof(MainViewModel.CompassHeading))
		{
			// Rotate the bezel grid in the opposite direction of the heading
			// This ensures that "N" always points to the actual North
			CompassBezelGrid.RotateTo(-_viewModel.CompassHeading, 250, Easing.CubicInOut);
		}
	}
}

