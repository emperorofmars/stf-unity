

# STF Format
STF is a binary format made up of at least one chunk, which is always a UTF-8 encoded definition in the JSON format. All further chunks are optional buffers which have to be referenced by the JSON definition.

The STF format is similar to GLTF 2.0, but differs in significant ways.

Everything is addressed by UUID. It must persist between import and export.

Every asset, node, component and resource has a type. The importer/exporter for each object is selected by its type. Support for additional types can be hot-loaded.
If a type is not supported, the JSON and all referenced objects have to be preserved and reexported unless manually removed. (This is implemented only for components currently)

A file cannot be changed automatically between import and export, unless explicitly desired by the user.

## Table of Content
- [JSON Definition](#json-definition)
- [STF-Unity Specific Notes](#stf-unity-specific-notes)
- [Extensibility](#extensibility)
- [Addons](#addons)
- [Material Format](#material-format)
- [Current Status and Considerations](#current-status-and-considerations)

## JSON Definition
The JSON definition has 6 properties in the root object.
- `meta` Information about the author, copyright, etc...
- `main` UUID of the main asset.
- `assets` A dict of UUID → assets pairs. Assets can list node UUID's and resource UUID's, depending on the asset type.
- `nodes` A dict of UUID → node pairs. Nodes can have a list of components and child-node UUID's. Specific node types can reference resources, other nodes and assets. (assets for example for a prefab instance, alternatively these could be done as components)
	- `components` A node's components describe additional information and behavior. For example mesh-instances or rotation constraints. Components can reference other nodes, resources and assets.
- `resources` A list of UUID → resource pairs. Resources can be referenced by nodes, components and assets. Resources can reference nodes, other resources and buffers. A resource's importer/exporter is responsible for dealing with any referenced buffer.
- `buffers` A list of buffer UUID's in the order of the binary chunks. The index of the buffer UUID corresponds to the index of the buffer in the STF file + 1. (The JSON definition is at the first index)

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
This implementation for Unity uses a two stage design. The first one parses an STF file into a Unity scene using its own components which represent the STF file 1:1 with no regard for Unity functionality. This is called the authoring scene, as it can be used to export STF files.

Multiple second stages can be registered to convert the intermediary authoring scene into an application-specific one. This step is destructive and throws information not relevant for the target application away, including all STF related meta-information, resolves all relationships between components and potentially applies optimizations.

Included is a basic second stage which converts into a pure Unity scene, and throws everything else away.
The intermediary format is intended for authoring STF files.

## Extensibility
The extensibility of this format is a first class feature. All implementations must provide an easy way to add and hotload support for additional types.

By default, STF supports only a limited set of features which can be expected from a common 3d file-format. These include support for skinned meshes, armatures, animations, materials and textures.

If for example the included mesh type is not satisfactory, a different mesh type can be implemented. These can exist in parallel and will work as long as the importer/exporter for the new mesh-type is present. All types are namespaced, and can be versioned. It is the responsibility of the importer/exporter for a type to handle versioning. Importers are implemented for each type in an encapsulated manner. As such it is trivial to register additional ones.

Components can have defined relationships to other components and be specific to a target application.
Components can extend or override others.

For example, multiple Social VR applications support one or another library for bone physics. None of which are compatible with each other, but they generally work the same. A generic STF-component can be used to describe the common features and be conversible to all implemented applications formats, err-ing on the side of the resulting physics not flipping out in the application. If there exists a dedicated STF-component for a specific application, it can override the basic generic component. This way multiple mutually exclusive and application/game-engine specific features can be supported simultaneously.

**To extend STF with the ability to represent VR & V-Tubing avatars, the [AVA Proof of Concept](https://github.com/emperorofmars/ava-unity) was created. This shows the potential and ease of extending STF.**

## Addons

It is possible to create assets of the type `STF.addon`. These provide a list of nodes that can be of the types `STF.appendage_node` and `STF.patch_node`. They target the nodes of other assets and either parent themselves to the target (appendage) or add their child nodes and components to the target (patch).

That way it becomes trivial for a third party to create assets like a set of clothing for a base character model. The STF importer scans the Unity project for STF addons targeting the selected asset and presents the user with a simple checkbox to apply it.

![Screenshot of an STF file's inspector in Unity, containing a list of detected addons, with a checkbox to apply it to the current model.](./doc/img/import_settings_addons.png)

## Material Format
As part of creating this format, I created the beginning of a universal material format, preliminarily called: MTF - Material Transfer Format.
It's not fleshed out at all and exists in an incredibly basic form, but this is the idea:

Materials consist of a dictionary of properties. A set of universal properties will be defined and must be used in its specified manner. These include albedo, roughness, specular, glossiness, ... The name is used as the key for the dictionary.

Each property has a list of objects, in order of priority. Each object has a type property and can be a scalar, integer, string, texture reference (by UUID), texture channel reference, ..., and anything else that the importer/exporter has support for.

It is a list to account for the case in which not every implementation can understand every type of property. The first object which is understood by the implementation and target-material will be used.

Example: The first and most prioritized object could be a mathematical definition, which is only understood by a few specific applications. To make it work elsewhere, the second object could be a texture, rendered from the mathematical definition.

Properties, not specified by the MTF format, can be freely used. Properties can indicate to which target application/shader they belong and so can the entire material. The material can also have a set of hint properties (just a list of string key-value pairs), indicating whether it should be rendered in a cartoony or realistic style for example.

Converters for specific shaders can be implemented, otherwise properties can be converted based on Unity's system.
The "target_shader" property indicates which converter is to be used. If a converter or target shader is not present, a default will be chosen, or the user can specify an alternative shader.

Even if a perfect conversion is not possible, the hope is that at least the best possible conversion can happen. This will also ease the switching to a different target shader.

	...
	"resources": {
		"d2a3568f-0116-4f3d-866d-9ce420035de6": {
			"type": "STF.material",
			"name": "Body",
			"target_shader": "Poiyomi 8",
			"render-hints": { "style": "toony" },
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

# Current Status and Considerations
- Due to Unity limitatins, importing a STF file fully through the Scripted Importer won't work. Encoded images can't be imported that way for example. STF will be rewritten to import models into Assets, while still using the Scripted Importer Inspector get an overview and easily instantiate the STF assets.
- Animation paths import in the STF representation into Unity (prefix + UUID + property path). They only get resolved during a second stage's process. To make animations easier to work with for authoring, they should convert into the authoring format, targeting STF specific components with valid Unity paths.
Component and Resource Converters should get a 'convert property path' method for this purpose.
- Assets should have a single representation, that being a component on the root object of an imported prefab.
This component should offer a nice to use UI to apply addons and to convert to a target format. This would replace the current second stage system.
- Create more addon applier classes. For example one to set specific blendshapes on the target asset or one to merge meshes together.
- Generally refine the entire user experience of using STF. Build better inspectors for components and resources.
- Significantly expand the material system. Build a proper UI for it. It should make it easy to add properties and generate Unity materials for whatever shader is selected. Maybe make MTF its own exportable file-format as well!