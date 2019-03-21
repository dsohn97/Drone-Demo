# Droneflight_Demo

Welcome to the Droneflight Demo FUSEE App, which illustrates a method to calculate movement of objects and the camera in Space using `Quaternions`. This method works for 1st and 3rd person views.

## Problem

	If you Just use normal rotations for moving around in 3D space you get the Problem that your rotatons will depend on each other so if you want to rotate an object like this 

	```csharp
		_drone.transform.RotationY(Yaw)
		_drone.transform.RotationX(Pitch)
	```
	
`Droneflight.cs` contains the source code for a working FUSEE application showing 
a 3D drone model with 3 different camera types.  
The drone model was created using Blender and imported as `.fus` file. 

The Controls are mapped to

* Camera
	* `Q` change `_cameraType`
		* `Free Camera` 1st person free flight
			* `WASD` move in Space
			* `LMB + mouse move` rotate view
		
		* `Drone Camera` 3rd person follows drones FOV
			* Drone movement
				* `WS` fowards, backwards
				* `AD` to the sides
				* `RF` up, down
				* `LMB + Mouse move` rotates Drone
		* `Follow Camera` 3rd person rotates around drone
			* Camera movement
				* `RMB + Mouse move` rotates camera around drone

	

