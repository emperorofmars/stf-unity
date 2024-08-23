# AVA Goal
The goal is to be able to represent VR & V-Tubing avatars without compromise.

The following example STF types are intended to model as many common features of avatars as possible.

All of these types are subject to change and don't represent the current state of this project.


## Base Avatar Definition [`node_component`]
This is the bare minimum definition of an avatar. It must sit on the root node of the asset.
```
{
	"type": "ava.avatar",
	"main_meshinstance": "",
	"viewport_parent": "",
	"viewport_offset": []
}
```

## Facial Tracking [`node_component`]
This contains all the methods to animate the face of an avatar, based on face & eye tracking methods, voice recognition, or simply a simulation.

Each of the definitions is an array. It contains objects of various types in order of priority. The first supported type should be taken. This way, animation based facial_tracking setup can be supported in VRChat, but an application which supports only the direct mapping of blendshapes could use the second object.
```
{
	"type": "ava.facial_tracking",
	"voice_visemes": [
		{
			"type": "blendshapes",
			"map": "auto"
		}
	],
	"eye_rotations": [
		{
			"type": "bone_rotations",
			"up": 15.0
			...
		}
	],
	"eye_lids": [
		{
			"type": "blendshapes",
			"map": "auto"
		}
	],
	"facial_tracking": [
		{
			"type": "universal_expressions_animations"
			"map": "auto"
		},
		{
			"type": "universal_expressions_blendshapes"
			"map": "auto"
		}
	]
}
```

## Gestures [`node_component`]
This contains definitions to animate an avatar beyond facial tracking. These could be puppet controls for parts of the avatar which can't be tracked or manual facial gestures.

Each gesture gets a semantic meaning, to help with automatically binding it, and to make it easy for end users to rebind them to their liking.
```
{
	"type": "ava.gestures",
	"gestures": [
		{
			"name": "smile",
			"implementations": [
				{
					"type": "animation",
					"animation": ""
				},
				{
					"type": "vrm_blendshape_bullshit",
					"blendshapes": {
						"smile_left": 0.7,
						...
					}
				}
			]
		}
	]
}
```

## Gesture Bindings [`node_component`]
This component binds gestures to an activation method. Activation method could range from a VR controller gesture, in-game menu button, an external parameter input like in the case of OSC, to a specific facial tracking expression.
```
{
	"type": "ava.gesture_bindings",
	"bindings": [
		{
			"type": "vr_controllers_two_handed",
			"left_thumb_up": ...
		}
	]
}
```
