# WhereDidIPark? 🚗⌚

A simple yet powerful smartwatch app built with .NET MAUI to help you find your parked car. Never get lost in a parking lot again!

This project serves as a practical example of a watch-first application using .NET MAUI, demonstrating how to effectively use device sensors and create a responsive, glanceable UI for wearables.

*A short GIF showing the app saving a location and navigating back.*

### Key Features

* **One-Tap Save:** Instantly save your car's GPS coordinates.
* **Live Compass Navigation:** A full-screen compass experience guides you back.
* **Dynamic Rotating Bezel:** A persistent N-S-E-W bezel rotates in real-time based on the device's heading.
* **Directional Arrow:** A clear arrow points directly towards your saved location, adjusting as you move.
* **Real-Time Distance:** Get live updates on the distance to your car, switching from meters to kilometers automatically.
* **Clean, MVVM Architecture:** The codebase is structured using the Model-View-ViewModel pattern for clarity, testability, and maintainability.

### How to Use

1. **Open the App:** The compass bezel will immediately be active.
2. **Save Location:** Tap the "Save Parking Spot" button when you've parked.
3. **Navigate Back:** When you're ready to return, open the app. The arrow will point towards your car and show the distance.
4. **Clear Location:** Once you've found your car, tap the "Clear Spot" button to reset.

### Getting Started (For Developers)

To run this project, you will need:
* Visual Studio 2022 with the .NET Multi-platform App UI development workload installed.
* An Android device/emulator or a configured Tizen-based watch (like a Samsung Galaxy Watch) for deployment.

Clone the repository and open the `.sln` file in Visual Studio.
