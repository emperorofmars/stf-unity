# STF - Scene Transfer Format
**The Worlds Most Extensible File Format for 3D Models and Scenes**

Implementation for Unity 2019.4 or higher.

# **! This is a WIP and not ready for productive use !**

# **! Currently, I am significantly reworking the codebase. It won't be based on a ScriptedImporter, as that has too many limitations. !**

You are very welcome to open discussions with your ideas, suggestions and questions about the format and its possibilities. Open issues for concrete changes. Pull requests are very welcome!

## [Check out a showcase file here!](https://emperorofmars.itch.io/stf-avatar-showcase)
<!-- ## [Watch the video presentation about STF and its AVA extensions here!](https://youtu.be/ozkmGxFG_ug) -->

# STF Format
The STF format is heavily based on the concept of glTF 2.0. It's essentially my attempt to create a glTF-done-right format with a practically usable extension system, while avoiding its breaking issues. [Read more on glTF's issues here.](./Docs/stf_format.md#gltf-20-issues)

Unlike glTF, there are no hard definitions of specific resource types. Instead, every object has a `type` property. Based on an objects type, a registered converter will be selected to process this object. These converters can be easily hot loaded, enabling an unprecedented ease of writing extensions.

An STF setup in Unity can be further processed into specific target applications formats. For example, an application agnostic VR-Avatar definition implemented in STF can be converted into an VRChat avatar setup that is ready to be uploaded by the VRChat SDK.

### [Read up on how STF works in detail here.](./Docs/stf_format.md)

# How to Use
- Ensure you have the Newtonsoft JSON package imported in Unity. If you set up your Unity project with the VRC Creator Companion, it will be already imported. If not, install the official package in UPM.
- Either:
	- Download the latest release from this repository and import the .unitypackage into Unity.
	- Or clone this repository into the 'Assets' folder of your Unity project.
- Import a .fbx model, put it into the scene and export it as STF by going to `STF Tools` → `Export`
- If you exported it into the Assets hierarchy, just press CTRL+R for Unity to refresh its asset database and see it appear.
- Play around

<!-- ![Screenshot of an STF file's inspector in Unity.](./Docs/Images/import_settings.png) -->

# This Repository
Apart from the core STF format support, this repository contains a subproject called MTF, which adds support for arbitrary material/shader definitions. It's independent of STF and could be further expanded into its own project.

The `Addons` directory contains support for additional material converters and a set of STF extensions called AVA. AVA is a proof-of-concept set of extensions which add support for VR-Avatars to STF.

---

Cheers!
