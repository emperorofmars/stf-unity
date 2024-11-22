# STF - Scene Transfer Format [v0.3]
**An Extensible Interchange Format For 3d Assets**

Implementation for Unity 2022.3 or higher.

# **! This is a WIP and not ready for productive use !**

STF is a binary format based on the concept of glTF 2.0, consisting of a definition in JSON and a bunch of binary buffers.

The JSON definition consists of 2 `UUID → object` dictionaries, a buffer list, and some meta information. All objects must contain a `type` property.
- `asset` Information about the file. Has to define one or more root-nodes, depending on the `type`. The default asset-type has a single root node.
- `nodes` An object of UUID → node pairs.
	- `components` A node's components describe additional information and behavior. For example mesh-instances or rotation constraints.
- `resources` An object of UUID → resource pairs.
	- `components` A resource's components describe additional information and behavior. For example humanoid-mappings for armatures or LOD's for meshes.
- `buffers` A list of buffer UUID's in the order of the binary chunks. The index of the buffer UUID corresponds to the index of the buffer in the STF file + 1. (The JSON definition is at the first index)

By default, only the basic types a 3d format has to support are included. These include for example meshes, armatures, images/textures, and a shader agnostic material.
Support for additional types can be easily hot-loaded.

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
A good place to start exploring the codebase is [./Runtime/STFRegistry.cs](./Runtime/STFRegistry.cs), where types get registered, or [./Runtime/Serialisation/ImportExport/STFFile.cs](./Runtime/Serialisation/ImportExport/STFFile.cs) which handles the parsing and serialisation of binary STF files.

**You are very welcome to open discussions & issues with your ideas, suggestions and questions about the format and its possibilities. Pull requests are very welcome!**

---

Cheers!
