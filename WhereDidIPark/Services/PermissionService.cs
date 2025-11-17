namespace WhereDidIPark.Services;

public class PermissionService
{
	public async Task<PermissionStatus> CheckAndRequestLocationPermission()
	{
		PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

		if (status == PermissionStatus.Granted)
			return status;

		if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
		{
			// Prompt the user to turn on in settings
			// On iOS once a permission has been denied it may not be requested again from the application
			return status;
		}

		if (Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>())
		{
			// Prompt the user with a nice message
		}

		status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

		return status;
	}
}
