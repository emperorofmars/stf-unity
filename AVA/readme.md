# AVA [v0.3]
**An Application & Game Engine Agnostic VR & V-Tubing Avatar Extension-Set For STF**

Implementation for Unity 2022.3 or higher.

# **! This is a WIP and not ready for productive use !**

## The Goal
The goal is to be able to represent VR & V-Tubing avatars, in a target application agnostic manner, without compromise.

Target applications include VRChat, VRM & VSF, ChilloutVR, ...

Currently, this project is in a proof-of-concept and research stage.

This project contains the application agnostic avatar STF types, and a set of converters, currently only for VRChat and VRM.

## How?
Avatar formats contain mutually exclusive implementations of the same concept.

For example, bone physics:
* VRM has spring bones.
* VRChat has physics bones.
* ChilloutVR has dynamic bones or magicka cloth.

There is no possibility to create a superset setup for bone physics. The next best way would be to have a small fallback setup, which can be reasonably converted into any of these, at the cost of not supporting all features and potentially different resulting physics behavior.

On top of that, these application specific bone physics setups can also be included and override the generic component

If a generic fallback and a physics bone component targeting the same bone is present, and the avatar is being converted into a VRChat avatar, then the physics bone component will be chosen, and the generic fallback will be ignored.

With this approach, it is possible to support all the common functionality of avatars without redundancy, while also supporting application specific and mutually exclusive features.
