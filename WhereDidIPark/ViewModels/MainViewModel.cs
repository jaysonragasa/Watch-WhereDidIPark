using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Storage;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using WhereDidIPark.Services;
using Microsoft.Maui.Dispatching;


namespace WhereDidIPark.ViewModels;

public class MainViewModel : ViewModelBase
{
	#region Private Fields
	private bool _isLocationSaved;
	private double _arrowRotation;
	private double _compassHeading;
	private string _distanceText = "Calculating...";
	private readonly PermissionService _permissionService;
	private bool _isBusy;
	private Location? _parkedLocation;
	private bool _isNavigating;
	#endregion

	#region Public Properties
	public bool IsLocationSaved
	{
		get => _isLocationSaved;
		set
		{
			if (_isLocationSaved != value)
			{
				_isLocationSaved = value;
				OnPropertyChanged(); // Automatically uses "IsLocationSaved"
				OnPropertyChanged(nameof(IsNotLocationSaved));
			}
		}
	}

	public bool IsNotLocationSaved => !IsLocationSaved;

	public double ArrowRotation
	{
		get => _arrowRotation;
		set
		{
			if (_arrowRotation != value)
			{
				_arrowRotation = value;
				OnPropertyChanged();
			}
		}
	}

	public double CompassHeading
	{
		get => _compassHeading;
		set
		{
			if (_compassHeading != value)
			{
				_compassHeading = value;
				OnPropertyChanged();
			}
		}
	}

	public string DistanceText
	{
		get => _distanceText;
		set
		{
			if (_distanceText != value)
			{
				_distanceText = value;
				OnPropertyChanged();
			}
		}
	}

	public bool IsBusy
	{
		get => _isBusy;
		set
		{
			if (_isBusy != value)
			{
				_isBusy = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(IsNotBusy));
			}
		}
	}

	public bool IsNotBusy => !IsBusy;
	#endregion

	#region Commands
	public ICommand SaveLocationCommand { get; }
	public ICommand ClearLocationCommand { get; }
	#endregion

	#region Constructor
	public MainViewModel()
	{
		_permissionService = new PermissionService();
		SaveLocationCommand = new Command(async () => await OnSaveLocation());
		ClearLocationCommand = new Command(OnClearLocation);

		InitializeCompass(); // Start compass on app launch
		CheckForSavedLocation();
	}
	#endregion

	#region Private Methods

	/// <summary>
	/// Starts the compass sensor as soon as the app loads.
	/// </summary>
	private void InitializeCompass()
	{
		if (!Compass.IsSupported)
		{
			// Handle devices without a compass
			MainThread.BeginInvokeOnMainThread(async () =>
			{
				await Application.Current.MainPage.DisplayAlert("Error", "Compass is not supported on this device.", "OK");
			});
			return;
		}

		try
		{
			if (!Compass.IsMonitoring)
			{
				Compass.ReadingChanged += Compass_ReadingChanged;
				Compass.Start(SensorSpeed.UI, applyLowPassFilter: true);
			}
		}
		catch (Exception ex)
		{
			// Handle exceptions
			MainThread.BeginInvokeOnMainThread(async () =>
			{
				await Application.Current.MainPage.DisplayAlert("Error", $"Could not start compass: {ex.Message}", "OK");
			});
		}
	}

	/// <summary>
	/// Checks device storage to see if a location is already saved on startup.
	/// </summary>
	private void CheckForSavedLocation()
	{
		IsLocationSaved = Preferences.ContainsKey("parked_lat") && Preferences.ContainsKey("parked_lon");
		if (IsLocationSaved)
		{
			StartNavigationSensors();
		}
	}

	/// <summary>
	/// Handles the logic for the "Save Parking Spot" command.
	/// </summary>
	private async Task OnSaveLocation()
	{
		if (IsBusy)
			return;

		try
		{
			IsBusy = true;

			var permissionStatus = await _permissionService.CheckAndRequestLocationPermission();
			if (permissionStatus != PermissionStatus.Granted)
			{
				await Application.Current.MainPage.DisplayAlert("Permission Required", "Location permission is needed to save your parking spot.", "OK");
				return;
			}

			Location location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best));

			if (location != null)
			{
				Preferences.Set("parked_lat", location.Latitude);
				Preferences.Set("parked_lon", location.Longitude);
				HapticFeedback.Perform(HapticFeedbackType.LongPress);
				IsLocationSaved = true;
				StartNavigationSensors();
			}
		}
		catch (Exception ex)
		{
			await Application.Current.MainPage.DisplayAlert("Error", $"Unable to get location: {ex.Message}", "OK");
		}
		finally
		{
			IsBusy = false;
		}
	}

	/// <summary>
	/// Handles the logic for the "Clear Spot" command.
	/// </summary>
	private void OnClearLocation()
	{
		Preferences.Clear();
		HapticFeedback.Perform(HapticFeedbackType.Click);
		IsLocationSaved = false;
		StopNavigationSensors();
	}


	/// <summary>
	/// Activates the navigation logic.
	/// </summary>
	private void StartNavigationSensors()
	{
		if (_isNavigating) return;

		var lat = Preferences.Get("parked_lat", 0.0);
		var lon = Preferences.Get("parked_lon", 0.0);
		_parkedLocation = new Location(lat, lon);
		_isNavigating = true;
	}

	/// <summary>
	/// Deactivates the navigation logic.
	/// </summary>
	private void StopNavigationSensors()
	{
		if (!_isNavigating) return;

		_isNavigating = false;
		_parkedLocation = null;
		ArrowRotation = 0;
		DistanceText = "Calculating...";
	}

	/// <summary>
	/// Event handler for compass reading changes.
	/// </summary>
	private async void Compass_ReadingChanged(object? sender, CompassChangedEventArgs e)
	{
		// Always update the compass heading for the bezel rotation
		var currentHeading = e.Reading.HeadingMagneticNorth;

		// This must be on the main thread to update the UI
		MainThread.BeginInvokeOnMainThread(() =>
		{
			CompassHeading = currentHeading;
		});

		// Only perform navigation calculations if we are actively navigating
		if (!_isNavigating || _parkedLocation == null)
			return;

		try
		{
			Location? currentLocation = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(5)));
			if (currentLocation == null) return;

			double distance = currentLocation.CalculateDistance(_parkedLocation, DistanceUnits.Kilometers) * 1000;
			double bearing = CalculateBearing(currentLocation, _parkedLocation);
			double finalRotation = bearing - currentHeading;

			MainThread.BeginInvokeOnMainThread(() =>
			{
				DistanceText = FormatDistance(distance);
				ArrowRotation = finalRotation;
			});
		}
		catch (Exception)
		{
			// Silently fail or log, as this event fires rapidly.
		}
	}

	/// <summary>
	/// Formats the distance in meters to a readable string (m or km).
	/// </summary>
	private string FormatDistance(double meters)
	{
		if (meters < 1000)
		{
			return $"{meters:F0} m";
		}
		else
		{
			double kilometers = meters / 1000;
			return $"{kilometers:F2} km";
		}
	}

	/// <summary>
	/// Calculates the initial bearing (direction) from a start point to an end point.
	/// </summary>
	private double CalculateBearing(Location start, Location end)
	{
		double ToRadians(double degrees) => degrees * (Math.PI / 180.0);
		double ToDegrees(double radians) => radians * (180.0 / Math.PI);

		double startLat = ToRadians(start.Latitude);
		double startLon = ToRadians(start.Longitude);
		double endLat = ToRadians(end.Latitude);
		double endLon = ToRadians(end.Longitude);

		double deltaLon = endLon - startLon;

		double y = Math.Sin(deltaLon) * Math.Cos(endLat);
		double x = Math.Cos(startLat) * Math.Sin(endLat) -
				   Math.Sin(startLat) * Math.Cos(endLat) * Math.Cos(deltaLon);

		double bearingRadians = Math.Atan2(y, x);
		double bearingDegrees = ToDegrees(bearingRadians);

		return (bearingDegrees + 360) % 360; // Normalize to 0-360
	}

	#endregion
}

