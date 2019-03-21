# CameraDemo

Welcome to the Dronflight Demo FUSEE App which shows one method to Create a Camera Pespective in 1st and 3rd Person.

`Droneflight.cs` contains the source code for a working FUSEE application showing 
a 3D drone model with 3 different camera types.  
The drone model was created using Blender and imported as `.fus` file. 

The Controls are mapped to

* Camera
	* `Q` change `_cameraType`
		* `Free Camera` 1st person free flight
		* `Follow Camera` 3rd person rotates around drone
		* `Drone Camera` 3rd person follows drones FOV
* Drone Movement
	* `WS` fowards, backwards
	* `AD` to the sides
	* `RF` up, down
	* `LMB + Mouse move` rotates Drone
	

