# STF - Scene Transfer Format
## The Worlds Most Extensible Fileformat for 3D Models and Scenes

Implementation for Unity 2019.4 or higher.

Supports import and export!

**This is a prototype and not intended for productive use!**

## How to Use
- Ensure you have the Newtonsoft JSON package imported in Unity. If not, install the official package in UPM.
- Download or clone this repository and copy the entire folder into the 'Assets' folder of your Unity project.
- Import a .fbx model, put it into the scene and export it as STF by going to 'STF Tools' -> 'Export'
- If you exported it into the Assets hierarchy, just press CTRL+R for Unity to refresh its asset database and see it appear.
- Play around

## STF Format
STF is a binary format consisting of an arbitrary amount of chunks. The first chunk is always a definition in the JSON format. All further chunks are optional binary buffers which have to be referenced by the JSON definition.

The JSON definition has 6 properties in the root object:
- **meta:** Information about the author, copyright, etc...
- **main:** UUID of the main asset.
- **assets:** A dict of UUID -> assets pairs. Assets can list node UUID's and resource UUID's, depending on the asset type.
- **nodes:** A dict of UUID -> node pairs. Nodes can have a list of components and child-node UUID's. Specific node types can reference resources, other nodes and assets. (assets for example for a prefab instance, alternatively these could be done as components)
	- **components:** A node's components describe additional information and behavior. For example mesh-instances or rotation constraints. Components can reference other nodes, resources and assets.
- **resources:** A list of UUID -> resource pairs. Resources can be referenced by nodes, components and assets. Resources can reference nodes, other resources and buffers. A resource's importer/exporter is responsible for dealing with any referenced buffer. A buffer can be only referenced by one resource, but one resource can reference multiple buffers.
- **buffers:** A list of buffer UUID's in the order of the binary chunks. A UUID can be referenced by a resource. The index of the buffer UUID corresponds to the index of the buffer in the STF file + 1. (The JSON definition is at the first index)

The STF format is similar to GLTF 2.0, but differs in significant ways.

Everything is addressed by UUID. It must persist between import and export.

Every asset, node, component and resource has a type. The importer/exporter component for each object is selected by its type. Support for additional types can be hot-loaded.

If a type is not supported, the JSON and all referenced objects have to be preserved and reexported unless manually removed.

A file cannot be changed automatically between import and export, unless explicitly desired by the user.

Example:

	{
		"meta": {
			"author" : "Emperor of Mars"
		},
		"main": "dc96ec16-17c2-4ac2-bd27-21f4c9a34bba",
		"assets": {
			"dc96ec16-17c2-4ac2-bd27-21f4c9a34bba": {
				"name": "Test",
				"type": "STF.asset",
				"root": "4147da6b-e4ca-42db-826e-46a5dda9322f"
			}
		},
		"nodes": {
			"4147da6b-e4ca-42db-826e-46a5dda9322f": {
				"name": "Super Awesome Model",
				"type": "STF.node",
				"trs": [...]
				"children": [
					"4147da6b-e4ca-42db-826e-46a5dda9322f",
					"300799c5-0941-471f-b7a0-7cf17dcd1f10"
				]
			},
			"4147da6b-e4ca-42db-826e-46a5dda9322f": {
				"name": "armature"
				"type": "STF.armature_instance",
				"armature": "eb563e89-1e7c-40de-8c52-8a14e252f400",
				"children": [...]
			},
			"300799c5-0941-471f-b7a0-7cf17dcd1f10": {
				"name": "Super Awsome Mesh",
				"type": "STF.node",
				"components": {
					"beeed3cb-9a6f-45ac-b41b-5bdf8e773ed3": {
						"type": "STF.mesh_instance",
						"mesh": "c152e896-aba8-44d8-a810-724bc619abb1",
						"armature_instance": "4147da6b-e4ca-42db-826e-46a5dda9322f",
						"materials": [...],
						"morphtarget_values": [...]
					}
				}
			}
		},
		"resources": {
			"c152e896-aba8-44d8-a810-724bc619abb1": {
				"type": "STF.mesh",
				"buffer": "4812027a-671e-42a2-8e29-187b08e8ce93",
				...
			},
			"eb563e89-1e7c-40de-8c52-8a14e252f400": {
				"type": "STF.armature",
				...
			}
		},
		"buffers": [
			"4812027a-671e-42a2-8e29-187b08e8ce93"
		]
	}

## STF-Unity Specific Notes
This implementation for Unity uses a two stage design. The first one parses any STF file into a Unity scene using its own components which represent the STF file 1:1 with no regard for Unity functionality. This is called the authoring scene, as it can be used to export STF files.

Multiple second stages can be registered to convert the intermediary authoring scene into an application-specific one. This step is destructive and throws information not relevant for the target application away, including all STF related meta-information, resolves all relationships between components and potentially applies optimizations.

Included is a basic second stage which converts into a pure Unity scene, and throws everything else away.
The intermediary format is intended for authoring STF files.

## Extensibility
The extensibility of this format is a first class feature. All implementations must provide an easy way to add and hotload support for additional types.

By default, STF supports only a limited set of features which can be expected from a common 3d file-format. These include support for meshes, skinned meshes, armatures, animations, materials and textures.

If for example the included mesh type is not satisfactory, a different mesh type can be implemented. These can exist in parallel and will work as long as the importer/exporter for the new mesh-type is present. All types are namespaced, and can be versioned. It is the responsibility of the importer/exporter for a type to handle versioning. Importers are implemented for each type in an encapsulated manner. As such it is trivial to register additional ones.

Components can have defined relationships to other components and be specific to a target application.
Components can extend or override others.

For example, multiple Social VR applications support one or another library for bone physics. None of which are compatible with each other, but they generally work the same. A generic STF-component can be used to describe the common features and be conversible to all implemented applications formats, err-ing on the side of the resulting application-component not spazzing out. If there exists a dedicated STF-component for a specific application, it can override the basic generic component. This way multiple mutually exclusive and application/game-engine specific features can be supported simultaneously.

**To extend STF with the ability to represent VR & V-Tubing avatars, the [AVA Proof of Concept](https://github.com/emperorofmars/ava-unity) was created. This shows the potential and ease of extending STF.**

## Addons

It is possible to create assets of the type 'STF.addon'. These provide a list of nodes that can be of the types 'appendage' and 'patch'. They target the nodes of other assets and either append child nodes to these, patch in additional components, or replace existing components.

That way it becomes trivial for a third party to create assets like a set of clothing for a base character model. This STF importer scans the Unity project for STF addons targeting an asset and presents the user with a simple checkbox to apply it.

## Materials
As part of creating this format, i created the beginning of a universal material format, preliminarily called: MTF - Material Transfer Format.
It's not fleshed out at all and exists in an incredibly basic form, but this is the idea:

The material consists of a dictionary of properties. A set of universal properties will be defined and must be used in its specified manner. These include albedo, roughness, specular, glossiness, ... The name is used as the key for the dictionary.

Each property has a list of objects, in order of priority. Each object has a type property, and can be an scalar, integer, string, texture reference (by UUID), texture channel reference, ..., and anything that the importer/exporter has support for.
It is a list to account for the case in which not every implementation can understand every type of property. The first object which is understood by the implementation/target-material will be used.

Example: The first and post prioritized object could be a mathematical definition, which is only understood by a few specific applications. To make it work elsewhere, the second object could be a texture, rendered from the mathematical definition.

Properties not defined in the MTF format, can be freely used. Properties can indicate to which target application/shader they belong and so can the entire material. The material can also have a set of hint properties (just a list of string key-value pairs), indicating whether it should be rendered in a cartoony or realistic style for example.

Converters for specific shaders can be implemented, otherwise properties can be converted based on Unity's system.
The "target_shader" property indicates which converter is to be used. If a converter or target shader is not present, a default will be chosen, or the user can specify an alternative shader.
This way, even if a perfect conversion is not possible, the hope is that at least the best possible conversion can happen. This will also ease the switching of shaders.

	...
	"resources": {
		"d2a3568f-0116-4f3d-866d-9ce420035de6": {
			"type": "STF.material",
			"name": "Body",
			"target_shader": "Poiyomi 8",
			"render-hints": [ "style": "toony" ],
			"albedo": [
				{
					"type" : "texture",
					"texture" : "94899926-6827-4cd3-84f8-9dbeff553199"
				}
			],
			"roughness": [
				{
					"type": "texture_view",
					"texture": "70cb8395-5fc8-4eff-99d8-809a20439b11",
					"channel": 3
				},
				{
					"type": "scalar",
					"value": 0.67
				}
			],
			"audiolink_emission": [
				...
			],
			"fur_length": [
				...
			]
		},
		...
	},
	...

Such a material format could have use beyond just STF and should probably become its own project, which STF would merely make use of.

## Some Background and Motivation
VR Avatars are currently distributed as packages for game-engines, specifically Unity. This is an issue as end users have a hard time using professional tools. Additionally, Unity is not a character-editor, it's a tool with which a character-editor application can be created.

I wanted to create a universal character-editor application aimed at end users wishing to adapt their VR Avatar models but without the technical knowledge to do so in a game-engine.
To do so, i needed a file format that this character-editor-application could parse. This is where my descend into madness began.

Initially i wanted to create a format based on GLTF 2.0 to represent VR & V-Tubing avatars in a single file, agnostic of any target application, but with support for 100% of the features of each.

*VRM is a format also in the form of a GLTF extension, which also represents VR & V-Tubing avatars. However, it only supports a small subset of features, supports only a small number of hard-coded materials and doesn't support animations at all.*

I didn't think it would be too complicated to create something better than VRM, however i encountered countless issues with the GLTF 2.0 specification itself as well its implementations.
I wanted to avoid having to create my own format, but after 4 months of trying, i saw no way to make this work with GLTF 2.0.

After 4 more months, i have created this STF format prototype and the AVA proof of concept set of extensions. STF puts extensibility first, and supports most of everything that GLTF does, and makes it trivial to implement anything beside that.
STF was created with consideration of how most applications like Blender, Unity, Godot or Unreal Engine represent models and scenes. As such, most headaches from GLTF should have been solved here, hopefully.

### GLTF 2.0 Issues
- Material references and morphtarget values sit on the mesh, not its instances.
  https://github.com/KhronosGroup/glTF/issues/1249
  https://github.com/KhronosGroup/glTF/issues/1036
- In GLTF everything is addressed by index. Indices are very likely to break between import and export. (If an extension is not supported by an application and gets stored as raw JSON, that references other objects by index, it will break. Addons will also break.) 
- Limited animation support. Only transforms and morphtarget values (per mesh, not per mesh-instance) can be animated.
  The [KHR_animation_pointer](https://github.com/KhronosGroup/glTF/pull/2147) extension proposal would fix that partially.
- There is weirdness with multiple meshes sharing the same armature.
  https://github.com/KhronosGroup/glTF/issues/1285
- Morphtarget names are not supported by the specification. Sometimes these are stored on the 'extras' field of the mesh, sometimes on the first mesh primitive. The official Blender GLTF implementation does the first, the official Unity GLTF implementation does the latter.
- GLTF itself is supremely extensible, however to implement extensions in most implementations, the implementations have to be forked and modified at the core. When an implementation has support for implementing additional extensions, it is accompanied by significant issues.
- GLTF only supports specific hard-coded materials.
- The official Blender implementation exports insanely large files.
  https://github.com/KhronosGroup/glTF-Blender-IO/issues/1346
  Godot does this as well.
  A file being 95% larger and consisting of 95% zeros in the case of my VR Avatar Base (thanks to about 200 morphtargets) is just not serious.
- Some Godot issues and notes:
  - Godot has some issues with blendshapes: https://github.com/godotengine/godot/issues/63198
  - Godot blendshape implementation wishlist that would solve the biggest issue of stupid VRAM use and filesize in Godot at least: https://github.com/godotengine/godot-proposals/issues/2465#issuecomment-799892451
  - glTF import and export scene handling: https://github.com/godotengine/godot-proposals/discussions/6588
  - glTF export exclusions: https://github.com/godotengine/godot-proposals/discussions/6587
  - ImporterMeshInstance3D metadata lost in glTF import process: https://github.com/godotengine/godot-proposals/discussions/6586

To fix most of the issues, breaking changes would be needed for the GLTF specification.
Most of this has been known for a long time, and there has been no change, only a silent absence of general GLTF use, sadly.
