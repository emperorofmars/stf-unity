I have been working on my STF project for more or less two years!
It's a prototype of a 3D asset interchange format, with extensibility being a primary feature!

But for now I have decided to retire it.

Here is the final showcase!
https://youtu.be/LF-7Atpddrg

I have been trying to create an easily extensible file format for 3D asset interchange & authoring.
Such a format does not exist, unfortunately, but having one would be incredibly useful for my VR avatar making hobby.

STF is at a point where I'm pretty confident about the container format and the core concept.
At this point a lot more people than just me would have to have a go at it, people with different use cases and experiences, to get this somewhere.
Of course, I was aware that alone it is impossible to create a serious file format like that, but unfortunately this failed to gain any real attention.

It was an interesting project, but I made my point with this.
I am thankful for the people that cared and listened.
It is time to move on to something that I can develop in reasonable time to be practically useful for me.

FBX reigns supreme for now. Sadly, it is undocumented and non-extensible. But at least it works.

---

So I started work on an extension system for FBX! (or any file format really)

Introducing [NNA - Node Name Abuse](https://github.com/emperorofmars/nna-unity).

It works by serializing JSON into node-names. These get parsed in a Unity AssetPostprocessor for example.
It is a horrible abomination, it works, very well. And it eases my workflow right now (or at least very soon).

With this, I hope to be finally able to create a universal avatar format, based on FBX I guess.
I hope to be able to distribute full VR avatars as a single file, compatible with as many target applications (like VRChat or VSF/VRM) as somebody bothers to implement a converter for.
Realistically, this will work only whenever Blender finally gets a proper animation system (sometime 2025 according to https://code.blender.org/2024/02/animation-2025-progress-planning/#-what-were-working-on-right-now).

Should I feel really depraved at some point, I may even encode binary data as base64 into FBX node-names.

---

Cheers!

\- Mars

---

# Original Readme

For Unity 2022.3 or higher.

# **! This is a WIP and not ready for productive use !**

## [STF - Scene Transfer Format](./STF/readme.md)
STF is an extensible interchange & authoring format for 3d assets.\
It is a binary format consisting of a definition in JSON and a bunch of binary buffers.

## [MTF - Material Transfer Format](./MTF/readme.md)
MTF is a shader & game engine agnostic material system used by STF.

[Video Showcase (1 Minute)](https://youtu.be/VJUYrmEb-WQ?si=HTr7UoJRZlGc0YfE)

## [AVA - Components For VR & V-Tubing Avatars](./AVA/readme.md)
AVA is a proof-of-concept set of extensions for STF, which support VR & V-tubing avatar components.

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
