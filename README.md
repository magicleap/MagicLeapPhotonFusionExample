# Magic Leap Colocation With Photon Fusion

This repository contains an example project demonstrating how to use Photon Fusion to create a colocation application for the Magic Leap 2. This project provides a simple multiuser and colocation application using Photon Fusion. The example is designed to work with the Magic Leap 2 and demonstrates the basics of creating a shared AR experience.



## Prerequisites

Before you start, please ensure that you have the following software and hardware:

1.  Unity 2022.x 
3.  Magic Leap SDK and Unity Package
4.  Photon Fusion SDK
5.  A Photon Fusion account (for the AppID)

## Installation

1.  Clone this repository to your local machine:

```bash
git clone https://github.com/magicleap/MagicLeapPhotonFusionExample.git
```
3.  Open the project in Unity.
5.  Set up your [Photon Fusion AppID](https://doc.photonengine.com/fusion/current/fusion-100/fusion-101#step_7___add_an_appid) in the Photon Fusion Hub.


## How to build and run the example scene:

- [Create a Magic Leap 2 Unity project](https://developer-docs.magicleap.cloud/docs/guides/unity/getting-started/create-a-project) 
- [Import the Universal Render Pipeline](https://developer-docs.magicleap.cloud/docs/guides/unity/getting-started/graphics-settings)
- [Download and import the Photon Fusion package](https://doc.photonengine.com/fusion/current/fusion-100/fusion-101#step_1___download_sdk)
- [Assign a Photon Fusion App ID](https://doc.photonengine.com/fusion/current/fusion-100/fusion-101#step_7___add_an_appid)
- Import the Magic Leap - Photon Fusion package
- Print out the AprilTag marker or export Magic Leap Map
- Build and Run the example scene


### Colocation Methods
- **Marker Tracking**
	- To colocate using marker tracking, print out marker `0` from the tag family `36h11`
- **Magic Leap Anchors**
	- To colocate using Magic Leap Anchors, localize into the same [AR Cloud Space](https://www.magicleap.care/hc/en-us/articles/9312806819597-AR-Cloud) or [share your Space manually](https://developer-docs.magicleap.cloud/docs/guides/features/spaces/import-export-spaces)


## Project Structure

The project is organized as follows:
```bash
Assets/MagicLeap/PhotonFusionExample
├───DesktopARBackground/    # Contains materials for the 3D objects
├───Images/          		# Contains the April Tag marker
├───Materials/             	# Contains materials for the 3D objects
├───Models/             	# Contains the virtual avatar models used in the scene
├───Prefabs/            	# Contains prefabs for easy scene setup
├───Scenes/             	# Contains the example scenes for marker and anchor colocation
├───Scripts/            	# Contains custom C# scripts for the project
	├───Editor/					# Scripts installing the generic marker tracker
	├───MagicLeapSpaces/		# Scripts for localizing the Magic Leap into a device or AR Cloud Space
	├───MarkerTracking/			# Scripts for Magic Leap and generic marker tracking
	├───PhotonFusion/			# Scripts coupled to Photon Fusion
	├───Utilities/				# Scripts such as a ThreadDispatcher
└───Sprites/           		# Contains textures for UI
└───ThirdParty/           	# Contains a modified version of the April Tag for Unity
└───URP/           			# Contains the Universal Render Pipeline assets
└───Webcam/           		# Contains components to obtain a generic webcam stream
```

## Disclaimers

- This project uses a modified version of the [AprilTag package for Unity](https://github.com/keijiro/jp.keijiro.apriltag) which is licensed under [BSD 2-Clause](https://github.com/keijiro/jp.keijiro.apriltag/blob/main/LICENSE)
- This project uses the [Keijiro Takahashi's TestTools Package](https://github.com/keijiro/TestTools) which is licensed under [The Unlicensed](https://github.com/keijiro/TestTools/blob/main/LICENSE)



## License

This project is governed by the [ Magic Leap 2 License Agreement](https://www.magicleap.com/software-license-agreement-ml2)