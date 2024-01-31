# Some Background and Motivation
VR Avatars are currently distributed as packages for game-engines, specifically for Unity. This is an issue as end-users have a hard time using professional tools. Additionally, Unity is not a character-editor by itself, it's a tool with which a character-editor application could be built.

I wanted to create a universal character-editor application, aimed at end users wishing to adapt their VR Avatar models, but without the technical knowledge to work in a game-engine.
Therefore, I needed a file format that this character-editor-application could parse.

Initially I wanted to create a format based on glTF 2.0 to represent VR & V-Tubing avatars in a single file, agnostic of any target application, but with support for 100% of the features of each.

*VRM is a format also in the form of a glTF extension, which also represents VR & V-Tubing avatars. It was created before social-VR was figured out to the extent it is now, and doesn't support most basic features that users today expect and require*

I didn't think it would be too complicated to create something better than VRM, however I encountered countless issues with the glTF 2.0 specification itself as well as many of its implementations.
I wanted to avoid having to create my own format, but after 4 months of trying, I saw no way to make this work with glTF 2.0.

After 4 more months, I have created this STF format prototype and the AVA proof of concept set of extensions. STF puts extensibility first, and supports most of everything that glTF does, and makes it trivial to implement anything beside that.
STF was created with consideration of how most authoring tools like Blender, Unity, Godot or Unreal Engine represent models and scenes. As such, most headaches from glTF should have been solved here, hopefully.

# glTF 2.0 Issues
- Material references and morphtarget values sit on the mesh object, not its instances.
  https://github.com/KhronosGroup/glTF/issues/1249
  https://github.com/KhronosGroup/glTF/issues/1036
- In glTF everything is addressed by index. Indices are very likely to break between import and export. (If an extension is not supported by an application and gets stored as raw JSON, that references other objects by index, it will break. Addon assets like supported by STF would also break.)
- Limited animation support. Only transforms and morphtarget values (per mesh, not per mesh-instance) can be animated.
  The [KHR_animation_pointer](https://github.com/KhronosGroup/glTF/pull/2147) extension proposal would fix that partially.
- Morphtarget names are not supported by the specification. Sometimes these are stored on the 'extras' field of the mesh, sometimes on the first mesh primitive. The Blender glTF implementation does the former, UnityGLTF does the latter.
- glTF only supports specific hard-defined shaders.
- The buffer system is convoluted and a lot of implementations don't seem to bother with it. As such blendshapes store values for every vertex, even if a vertex not included in the blendshape. Typical VR avatars have multiple hundred blendshapes, which leads to comical file sizes and VRAM use.
- glTF in it's specification is supremely extensible, however implementing additional extensions in most glTF libraries is supported. They have to be forked and modified at the core. The way glTF does extensions, does not naturally lead to a good design in its implementations. STF is purposely designed to force a plugin based architecture.

## Issues in glTF 2.0 implementations I've tried to work with
- Blender
	- The Blender implementation exports insanely large files.
  https://github.com/KhronosGroup/glTF-Blender-IO/issues/1346
  A file being 95% larger and consisting of 95% zeros in the case of my Fox VR Avatar Base (thanks to about 200 morphtargets) is just not acceptable.
- Godot
	- Godot also exports ridiculously large files like Blender.
	- glTF import and export scene handling: https://github.com/godotengine/godot-proposals/discussions/6588
	- glTF export exclusions: https://github.com/godotengine/godot-proposals/discussions/6587
- Unity
	- Hardcoded extensions in both UnityGLTF and the new in-developement GLTFast implementation.

To fix most of the issues, breaking changes would be needed for the glTF specification, and a significant rethinking on how to implement it.
Most of this has been known for a long time, and there has been no movement, only a silent absence of general glTF use, sadly.

My hope is that I was able to account for all glTF issues with STF, and to create something that can be extended further to fit in any use case for a 3d file format, while being extremely easy to work with.