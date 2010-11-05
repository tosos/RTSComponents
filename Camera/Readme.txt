RTS Camera code as used in Rising Storm.  http://www.risingstormhq.com

In order to construct this:

Make one empty GameObject and name it CameraTarget.  Attach the two scripts
RotateTarget and PanTarget.  Position the CameraTarget at ground level (probably
y = 0).  It'll move horizontally in the xz-plane and rotation will be about the y.

Create a Camera object and attach Unity's SmoothFollow. (Menu:
Component->Camera-Control->Smooth Follow)  Set the target variable to
"CameraTarget" created above.  Then attach ScrollSmoothFollow included here.

It might be wise at this point to create a parent GameObject to contain both
the Camera and Camera target and put it into a prefab for use in other scenes.

This should be all that's necessary to get it working.  Good luck!
