# Changelog

## Version 0.5.2-preview
Requires: Unity 2019.1

### Added Features
* Controls: The `TeleporterControls` can be used to teleport to save locations nearby.
* Nintendo Joy-Cons: You are able to configure different input schemes with the `JoyconBinding` class.
* Speech to Text: The new namespace `SpeechToText` is designed to simplify the access of the built-in Windows Speech Recognition (WSR) feature.
* Controls: The new `FreeLookControls` component allows you to navigate in a scene freely.
* Legacy Components: Added support for old equalization files based on a Bimber matrix.

### Improvements
* Microsoft Kinect 2.0: The `KinectPlayArea` acquires new data automatically at a specified frame rate (30 fps) to improve performance.
* Microsoft Kinect 2.0: Reworked the API structure and introduced a `KinectTrackableObject` which can be configured to automatically update itself with tracking data.
* Microsoft Kinect 2.0: The `KinectTrackableHead` is behaves like the `KinectTrackableObject` but implements some fallbacks for the head tracking capabilities.
* Microsoft Kinect 2.0: The `KinectTrackableHand` is behaves like the `KinectTrackableObject` but is lighter and provides access to the hand state.
* Nintendo Joy-Cons: Rework of the `JoyconHelper` which is now called `JoyconController` with simpler API and caching support.
* Nintendo Joy-Cons: The new `JoyconInput` API takes care of mouse and keyboard axis input in combination with the Joy-Con stick input.
* Automatic Build: The virtual reality SDK can be installed automatically.
* Automatic Build: Exclusive full screen can now be set automatically.

### API Changes
* Editor: `KinectAddin` functionalities are now located in the namespace `Htw.Cave.Kinect.Addin`.
* Editor: Restructuring of the `KinectAddinHelper` which is now responsible for moving the `Plugins` folder via `MovePluginsToAssets` or `MovePluginsToPackage`.
* Math: `HolographicFastPrecompute` is now called `HolographicPrecompute`.
* Math: `HolographicFast` is now part of the `Holographic` method family.
* Controls: The `Controller` namespace is now called `Controls`.

### Fixes
* Microsoft Kinect 2.0: Fixed an issue where the `Plugins` folder fails to be moved before building.
* Nintendo Joy-Cons: Fixed the issue that Joy-Con controller and keyboard input blocking each other (see `JoyconInput`).
