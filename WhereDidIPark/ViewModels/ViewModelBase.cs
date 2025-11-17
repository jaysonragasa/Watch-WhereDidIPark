using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WhereDidIPark.ViewModels;

/// <summary>
/// A base class for ViewModels that implements the INotifyPropertyChanged interface.
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler PropertyChanged;

	/// <summary>
	/// Raises the PropertyChanged event for a given property.
	// </summary>
	/// <param name="propertyName">The name of the property that has changed. 
	/// This is automatically provided by the compiler.</param>
	protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}