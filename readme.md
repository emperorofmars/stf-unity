# STF - Scene Transfer Format [v0.1]
**The Worlds Most Extensible File Format for 3D Models and Scenes**

Implementation for Unity 2022.3 or higher.

# **! This is a WIP and not ready for productive use !**

### [Find an example model here!](https://emperorofmars.itch.io/stf-avatar-showcase)
<!-- ## [Watch the video presentation about STF and its AVA extensions here!](https://youtu.be/ozkmGxFG_ug) -->

## Introduction
The single relevant format for VR & V-Tubing avatars is a `.unitypackage` that contains a scene with a setup somewhere.
There is no interoperability for avatars. And it is severely lacking for 3d models by themselves.
Using those avatars as an end-user is prohibitively difficult as it requires the use of a Game-Engine like Unity.

This project is supposed to improve that, by creating a prototype/proof of concept of an extensible file format for 3d models. It should be trivial to extend to support fully featured VR & V-Tubing Avatars.

## STF Format
The STF format is heavily based on the concept of glTF 2.0. It's essentially my attempt to create a glTF-done-right format with a practically usable extension system, while avoiding its severe flaws. [Read more on glTF's issues here.](./Docs/stf_format.md#gltf-20-issues)

Unlike glTF, there are no hard definitions of specific resource types. Instead, every object has a `type` property. Based on an object's `type`, a registered converter will be selected to process this object. These converters can be easily hot loaded, enabling an unprecedented ease of writing and rapidly prototyping extensions.

In STF every object is addressed by a unique ID, which persists across imports and exports. This way 'Addon' assets can be applied even after the original target file has been modified. This is useful for clothing assets for example.

An STF setup in Unity can be further processed into specific target applications formats. For example, an application agnostic VR-Avatar definition implemented in STF can be converted into an VRChat avatar setup that is ready to be uploaded by the VRChat SDK.

### [Read up on how the STF Format works here.](./Docs/stf_format.md)

## How to Use
- Ensure you have the Newtonsoft JSON package imported in Unity. If you set up your Unity project with the VRC Creator Companion, it will be already imported. If not, install the official package in UPM.
- Either:
	- Download the latest release from this repository and import the .unitypackage into Unity.
	- Or clone this repository into the 'Assets' folder of your Unity project.
- Import a .fbx model, put it into the scene and export it as STF by going to `STF Tools` → `Export`
- If you exported it into the Assets hierarchy, just press CTRL+R for Unity to refresh its asset database and see it appear.
- Play around

![Screenshot of an STF file's inspector in Unity.](./Docs/Images/import_settings.png)

# This Repository
Apart from the core STF format implementation, this repository contains a subproject called MTF, which adds support for arbitrary and shader-agnostic material definitions. It's independent of STF and could be further expanded into its own project.

Support for VR-Avatars is contained in the AVA directory. It's a proof-of-concept set of application-agnostic avatar components and converters for VRChat and VRM.

**You are very welcome to open discussions & issues with your ideas, suggestions and questions about the format and its possibilities. Pull requests are very welcome!**

---

Cheers!
