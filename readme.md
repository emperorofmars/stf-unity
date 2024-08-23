# Monorepo for Unity Implementations of STF and Related Projects

For Unity 2022.3 or higher.

# **! This is a WIP and not ready for productive use !**

## [STF - Scene Transfer Format](./STF/readme.md)
STF is an extensible interchange & authoring format for 3d assets.\
It is a binary format consisting of a definition in JSON and a bunch of binary buffers.

## [MTF - Material Transfer Format](./MTF/readme.md)
MTF is a shader & game engine agnostic material system used by STF.

## [NNA - Node Name Abuse](./NNA/readme.md)
Since establishing a new file format as a single developer with a limited amount of free time is rather unlikely, this provides a way to make `.fbx` files extensible, by serializing JSON into node names.\
This project is an abomination, but it should be achievable in reasonable time.

## [AVA - Components For VR & V-Tubing Avatars](./AVA/readme.md)
AVA is a proof-of-concept set of extensions for STF and NNA, which support VR & V-tubing avatar components.

# How to Use
* Create a Unity 2022.3 or higher Project.
* Ensure you have the Newtonsoft JSON package imported in Unity. If you set up your Unity project with the VRC Creator Companion, it will be already imported. If not, install the official package in UPM.
* Import the all-in-one `.unitypackage` from the latest release.
* Play around!

[Find an example model here!](https://emperorofmars.itch.io/stf-avatar-showcase)

![Screenshot of an STF file's inspector in Unity.](./STF/Docs/Images/import_settings.png)
![Screenshot of an STF model with its authoring components shown in the Unity inspector.](./STF/Docs/Images/scene.png)

# Why
I am in need of an open & extensible interchange format for 3d assets.

Unfortunately, such a format does not exist.

`fbx` is the next best thing, being the most widely supported and able to store the most of my models.
However, it is proprietary, undocumented and not extensible. Some open source implementations are unfortunately faulty. Blender for example won't export animation curves, baking animations instead and making them useless for further editing. The paid [Better FBX Importer & Exporter](https://blendermarket.com/products/better-fbx-importer--exporter) addon for Blender does the job.

`glTF 2.0` was originally designed as a distribution format, intended to be easily loaded into GPU memory. Some projects are trying to use it as an authoring/interchange format. Apparently this is a matter of a somewhat active debate. After trying to work with glTF 2.0 in this manner and analyzing its spec I don't think it can work for interchange/authoring. [Read in detail why here!](./STF/Docs/gltf_doesnt_work_as_an_interchange_format.md)

**My core requirements for an open & extensible 3d interchange format are:**
* Extensions must be hot loadable and trivial to implement, enabling the rapid prototyping of extensions.
* Between import and export, the file can not change (except for some metadata perhaps). If an extension is not supported, it and all of its dependencies must be preserved and re-exported, unless manually removed by the author.
* Everything must be addressed by a unique ID. This makes third party addons for a base model more robust for example.
* Materials must be arbitrary and shader agnostic.

STF is supposed to become the ideal 3d interchange & authoring format.

NNA is an abomination that makes `fbx` extensible and should become quickly practicable.

---

**You are very welcome to open discussions & issues with your ideas, suggestions and questions about the format and its possibilities. Pull requests are very welcome!**

# My Motivation
I make avatars for VR. The by far most popular and relevant social VR application is VRChat. VR avatars can also be used for V-Tubing, rendering/filmmaking and various other applications.

The single relevant format for VR & V-Tubing avatars is a `.unitypackage` that contains a scene with a setup for a specific application, usually VRChat only, somewhere in its hierarchy.

There is no interoperability for avatars. Using those avatars as an end-user is prohibitively difficult as it requires the use of a Game-Engine like Unity.

STF should be able to easily host an extension for application agnostic & fully featured VR & V-Tubing avatars.

Once such a format exists, I hope a sort of 'Character Editor' application can be created. End-users would be able to adapt their avatars as easily as in a video-game character creation screen and easily use them in applications like VRChat and VSeeFace. Currently, there is not even a 3d asset interchange format that satisfies basic needs, so this lies in the far future.

# Current Status
* This codebase, as well as the format itself, are the result of a lot of experimentation.
* Most functionality which can be expected of a 3d model format is implemented, not to full production readiness, but enough to show how the format is supposed to work. The focus is to make the 'container' format work. Specific types can be perfected at a later date.
* The UI/UX of STF tooling is not a priority, but some work has been done to make it reasonable.
* The codebase is tested only in a 'good weather flight' manner.

Alone, I can't bring a project like this to completion, as I can work on this only in my free time, while also making VR avatars as a hobby.

**I alone can only try to prove that this works and is very possible.**

I am available for questions and discussions.

My next step is likely going to be a cleanup of the functionality, and then an implementation for the Godot 4 engine. Once Blender's project 'Baklava' is released (Support for full animation**s**) I may implement STF also in Blender.

---

Cheers!
