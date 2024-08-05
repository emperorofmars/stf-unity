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
However, it is proprietary, undocumented and not extensible. Some open source implementations are unfortunately faulty. Blender for example won't export animation curves, baking animations instead and making them useless for further editing. The paid [Better Fbx Importer & Exporter](https://blendermarket.com/products/better-fbx-importer--exporter) addon for Blender does the job.

`glTF 2.0` was originally designed as a distribution format, intended to be easily loaded into GPU memory. Some projects are trying to use it as an authoring/interchange format. Apparently this is a matter of a somewhat active debate. After trying to work with glTF 2.0 in this manner and analyzing its spec I don't think it can work for interchange/authoring. [Read in detail why here!](./Docs/gltf_fails_as_an_interchange_format.md)

**My core requirements for an open & extensible 3d interchange format are:**
* Extensions must be hot loadable and trivial to implement, enabling rapid prototyping of extensions.
* Between import and export, the file can not change (except for some metadata perhaps). If an extension is not supported, it and all of its dependencies must be preserved and reexported, unless manually removed by the author.
* Everything must be addressed by a unique ID. This makes third party addons for a base model more robust for example.
* Materials must be arbitrary and shader agnostic.

## STF Format
STF is a binary format based on the concept of glTF 2.0, consisting of a definition in JSON and a bunch of binary buffers.

STF's JSON definition consists of `nodes` with a list of `node_components` and `resources` with a list of `resource_components`.
All of these objects have a `type` property.

By default, only the basic types a 3d format has to support are included.
Support for additional types can be easily hot-loaded.

Currently, an STF file can consist of multiple `assets`. This will change in the next version. An STF file will represent a single asset.

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

A good place to start exploring the codebase is [./STF/Runtime/STFRegistry.cs](./STF/Runtime/STFRegistry.cs), where types get registered, or [./STF/Runtime/Serialisation/ImportExport/STFFile.cs](./STF/Runtime/Serialisation/ImportExport/STFFile.cs) which handles the parsing and serialisation of binary STF files.

**You are very welcome to open discussions & issues with your ideas, suggestions and questions about the format and its possibilities. Pull requests are very welcome!**

# Original Motivation
I make avatars for VR. The by far most popular and relevant social VR application is VRChat. VR avatars can also be used for V-Tubing, rendering/filmmaking and various other applications.

The single relevant format for VR & V-Tubing avatars is a `.unitypackage` that contains a scene with a setup for a specific application, usually VRChat only, somewhere in its hierarchy.

There is no interoperability for avatars. Using those avatars as an end-user is prohibitively difficult as it requires the use of a Game-Engine like Unity.

STF should be able to easily host an extension for application agnostic & fully featured VR & V-Tubing avatars.

Once such a format exists, I hope a sort of 'Character Editor' application can be created. End-users would be able to adapt their avatars as easily as in a video-game character creation screen and easily use them in applications like VRChat and VSeeFace. Currently, there is not even a 3d asset interchange format that satisfies basic needs, so this lies in the far future.

# Current Status
* This codebase, as well as the format itself, are the result of a lot of experimentation and could use a bit of cleanup.
* The functionality which can be expected of a 3d model format is implemented, not to full production readiness, but enough to show how the format is supposed to work.
* The UI/UX of STF tooling is at a bare minimum level.
* The codebase is tested only in a 'good weather flight' manner.

Alone, I can't bring a project like this to completion, as I can work on this only in my free time, while also making VR avatars as a hobby.

**I can only try to prove that this works and is very possible.**

I am available for questions and discussions.

My next step is likely going to be a cleanup of the functionality, and then an implementation for the Godot 4 engine. Once Blender's project 'Baklava' is released (Support for full animations) I may implement STF also in Blender.

---

Cheers!
