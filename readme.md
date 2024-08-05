# STF - Scene Transfer Format [v0.2]
**Extensible Interchange Format for 3d Assets**

Implementation for Unity 2022.3 or higher.

# **! This is a WIP and not ready for productive use !**
[Find an example model here!](https://emperorofmars.itch.io/stf-avatar-showcase)

[Watch the video presentation!](https://www.youtube.com/watch?v=cgY-faQrv78)

## Introduction
I am in need of an open & extensible interchange format for 3d assets.

Unfortunately, such a format does not exist.

`fbx` is the next best thing, being the most widely supported and able to store the most of my models.
However, it is proprietary, undocumented, not extensible and many open source implementations unfortunately faulty. Blender for example won't export animation curves, baking animations instead and making them useless for further editing. The paid [Better Fbx Importer & Exporter](https://blendermarket.com/products/better-fbx-importer--exporter) addon for Blender does the job.

`glTF 2.0` is not an interchange format at all and not made for this purpose. For some reason, many in the '*open source gamedev sphere*' believe it is, and I fell victim to this mis-believe as well. GLTF does not work as an interchange format, and due to my mis-believe, previously this readme even called STF a 'glTF-done-right' format. glTF is concerned with being efficiently loaded into a GPU, I need a format that is concerned with being loaded into an authoring tool like Blender or various game-engines.

**My core requirements for an open extensible 3d interchange format are:**
* Extensions must be hot loadable and trivial to implement, enabling rapid prototyping of extensions.
* Between import and export, the file can only change if the author does so explicitly. If an extension is not supported, it and all of its dependencies must be preserved and reexported, unless manually removed by the author. This is as trivial as storing the extensions JSON in a text field.
* Everything must be addressed by a unique ID. This makes third party addons for a base model more robust.
* Materials must be arbitrary and shader agnostic.

## STF Format
STF is a binary format based on the concept of glTF 2.0, consisting of a definition in JSON and a bunch of binary buffers.

STF consists of `nodes` with a list of `node_components` and `resources` with a list of `resource_components`.
All of these objects have a `type` property.

By default only the basic types a 3d format has to support are included.
Support for additional types can be easily hot-loaded.

Currently, an STF file can consist of multiple `assets` which reference a root node. This will change in the next version. An STF file will represent a single asset.

The `asset` also has a `type`. The default asset-type consists of a single root node. A 'scene' asset-type may be added in the future, and a 'patch' asset-type exists in a rudimentary form to experiment with the ability for third parties to create addons for other models.
For VR avatars this is very common, however the process of delivering and applying such addons is difficult for the creator and especially the end-user.

### [Read up on how the STF Format works in more detail here.](./Docs/stf_format.md)

## How to Use
- Ensure you have the Newtonsoft JSON package imported in Unity. If you set up your Unity project with the VRC Creator Companion, it will be already imported. If not, install the official package in UPM.
- Either:
	- Download the latest release from this repository and import the `.unitypackage` into Unity.
	- Or clone this repository into the 'Assets' folder of your Unity project.
- Import a `.fbx` model, put it into the scene and export it as STF by going to `STF Tools` → `Export`.
- If you exported it into the Assets hierarchy, just press `CTRL+R` for Unity to refresh its asset database and see it appear.
- Play around!

![Screenshot of an STF model with its authoring components shown in the Unity inspector.](./Docs/Images/scene.png)
![Screenshot of an STF file's inspector in Unity.](./Docs/Images/import_settings.png)

# This Repository
Apart from the core STF format implementation, this repository contains a subproject called MTF, which adds support for arbitrary and shader-agnostic materials. It's independent of STF and could be further expanded into its own project.

Support for VR-Avatars is contained in the AVA directory. It's a proof-of-concept set of application-agnostic avatar components and converters for VRChat and VRM.

**You are very welcome to open discussions & issues with your ideas, suggestions and questions about the format and its possibilities. Pull requests are very welcome!**

# Original Motivation
I make avatars for VR. The by far most popular and relevant social VR application is VRChat. VR avatars can also be used for V-Tubing, rendering/filmmaking and various other applications.

The single relevant format for VR & V-Tubing avatars is a `.unitypackage` that contains a scene with a setup for a specific application, usually VRChat only, somewhere in its hierarchy.

There is no interoperability for avatars. Using those avatars as an end-user is prohibitively difficult as it requires the use of a Game-Engine like Unity.

STF should be able to easily host an extension for application agnostic & fully featured VR & V-Tubing avatars.

Once such a format exists, I hope a sort of 'Character Editor' application can be created. End-users would be able to adapt their avatars as easily as in a video-game character creation screen and easily use them in applications like VRChat and VSeeFace. Currently, there is not even a 3d asset interchange format that satisfies basic needs, so this lies in the far future.

# Current Status
* Most functionality which can be expected of a 3d model format is implemented, not to full production readyness, but enough to show how the format is supposed to work.
* The UI/UX of STF tooling is at a bare minimum level.
* The codebase is tested only in a 'good weather flight' manner.

Alone, I can't bring a project like this to completion, as I can work on this only in my free time, while also making VR avatars as a hobby.

**I can only try to prove that this works and is very possible.**

I am available for questions and discussions.

My next step is likely going to be a cleanup of the functionality, and then an implementation for the Godot 4 engine. Once Blender's project 'Baklava' is released (Support for full animations) I may implement STF also in Blender.

---

Cheers!
