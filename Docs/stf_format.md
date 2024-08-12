# STF Format
The STF format is based on the concept of GLTF 2.0.

It is a binary format made up of at least one buffer, which is always a UTF-8 encoded definition in the JSON format. All further buffer are optional and have to be referenced by the JSON definition.

Every object in the JSON Definition is addressed by UUID. It must persist between import and export.

Every object has a `type`. The importer/exporter for each object is selected by its type. Support for additional types can be hot-loaded.
If a type is not supported, the JSON and all referenced objects have to be preserved and re-exported unless manually removed.

An STF file must stay the same between import and export, unless explicitly modified by the user.

## Table of Content
- [JSON Definition](#json-definition)
- [Extensibility](#extensibility)
- [Addons](#addons)
- [Material Format](#material-format)
- [STF-Unity Specific Notes](#stf-unity-specific-notes)

## JSON Definition
The JSON definition consists of 4 `UUID → object` dictionaries in the root object. All objects must contain a `type` property.
- `asset` Information about the file. Has to define one or more root-nodes, depending on the `type`. The default asset-type has a single root node.
- `nodes` An object of UUID → node pairs. Nodes can have a list of components and child-node UUID's.\
Nodes can reference: `node_components`, `resources`, `resource_components`
	- `components` A node's components describe additional information and behavior. For example mesh-instances or rotation constraints.\
	Node Components can reference: `nodes`, `node_components`, `resources`, `resource_components`
- `resources` An object of UUID → resource pairs.\
Resources can reference: `nodes`, `node_components`, `resources`, `resource_components`, `buffers`.
	- `components` A resource's components describe additional information and behavior. For example humanoid-mappings for armatures or LOD's for meshes.\
	Resource Components can reference: `nodes`, `node_components`, `resources`, `resource_components`, `buffers`
- `buffers` A list of buffer UUID's in the order of the binary chunks. The index of the buffer UUID corresponds to the index of the buffer in the STF file + 1. (The JSON definition is at the first index)\
A buffer can be only referenced once. Should the need arise for a buffer to be referenced in multiple places, it should be represented as a resource or resource component. The resource or resource component can be referenced multiple times.

Example:
```
{
	"asset": {
		"id": "2895582c-b9e0-4c6b-93eb-e78d4a6f517e",
		"type": "STF.asset",
		"name": "Super Awesome Model",
		"version": "1.0.0",
		"author": "Emperor of Mars",
		"preview": "dff26c0c-9fd7-4b52-a917-ce6e4055f4de",
		"root_node": "adf46b0b-5083-404c-9da1-624bf347ca8c",
		"generator": "stf-unity",
		"timestamp": "8/6/2024 11:02:49 PM"
	},
	"nodes": {
		"adf46b0b-5083-404c-9da1-624bf347ca8c": {
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
		"dff26c0c-9fd7-4b52-a917-ce6e4055f4de": {
			"type": "STF.texture",
			"name": "preview",
			"image_format": "png",
			"texture_width": 512,
			"texture_height": 512,
			"texture_type": "color",
			"buffer": "db07e0ad-4f2b-4555-b61d-220fbf6397ac",
			"references": {
				"buffers": [
					"db07e0ad-4f2b-4555-b61d-220fbf6397ac"
				]
			}
		},
		}
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
		"db07e0ad-4f2b-4555-b61d-220fbf6397ac",
		"4812027a-671e-42a2-8e29-187b08e8ce93"
	]
}
```

## Extensibility
The extensibility of the STF format is a first class feature. All implementations must provide an easy way to add and hot-load support for additional types.

By default, STF supports only a limited set of features, which can be expected from a common 3d file-format. These include support for meshes, armatures, animations, materials and images/textures.

Components can have defined relationships to other components and be specific to a target application.
Components can extend or override others.

For example, multiple Social VR applications support one or another library for bone physics. None of which are compatible with each other, but they generally work the same.
A generic STF-component for bone-physics can be convertible to all implemented applications formats, erring on the side of the resulting physics not flipping out in the application.
If there exists an application specific component, it can override the basic generic component. This way multiple mutually exclusive and application/game-engine specific features can be supported simultaneously.

**To extend STF with the ability to represent VR & V-Tubing avatars, the 'AVA' Proof of Concept extensions were created. This shows the potential and ease of extending STF and is included in this repository.**

## Addons

It is possible to create assets of the type `STF.addon`. These provide a list of nodes that can be of the types `STF.node_appendage` and `STF.node_patch`. They target the nodes of other assets and either parent themselves to the target (appendage) or add their child nodes and components to the target (patch).

That way it becomes trivial for a third party to create assets like a set of clothing for a base character model. To apply an addon as a Unity user, the 'STF-Tools/Apply Addons' tool can be used.
In the future, importing an STF file, applying addons and converting to a specific target application should happen in one UI flow.

## Material Format
As part of creating this format, I created a universal material format, called: **MTF - Material Transfer Format**.
It's included in this Repository, but not dependent on STF. STF merely makes use of MTF.

Materials consist of a dictionary of properties. A set of universal properties will be defined and must be used in its specified manner. These include albedo, roughness, specular, glossiness, etc.

Each property has a list of objects, in order of priority. Each object has a type property and can be a float, integer, string, color, texture, texture channel, etc.

It is a list to account for the case in which not every implementation can understand every type of property. The first object which is understood by the implementation and target-material will be used.

Example: The first and most prioritized object could be a mathematical definition, which is only understood by a few specific applications. To make it work elsewhere, the second object could be a texture, rendered from the mathematical definition.

Properties, not specified by the MTF format, can be freely used. Properties can indicate to which target application/shader they belong and so can the entire material. The material can also have a set of hint properties, indicating whether it should be rendered in a cartoony or realistic style for example.

Converters for specific shaders can be implemented, otherwise properties can be converted based on Unity's material-property system.
The "targets" property indicates which converter is to be preferably used. If a converter or target shader is not present, a default will be chosen, or the user can specify an alternative shader.

Even if a perfect conversion is not possible, the hope is that at least the best possible conversion can happen. This will also ease the switching to a different target shader.

	...
	"resources": {
		"d2a3568f-0116-4f3d-866d-9ce420035de6": {
			"type": "MTF.material",
			"name": "Body",
			"targets": {
				"unity3d": [
					".poiyomi/Poiyomi 8.1/Poiyomi Toon"
				],
				"godot4": [
					...
				]
			},
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

## STF-Unity Specific Notes
This implementation for Unity uses a two stage design. The first one parses an STF file into a Unity scene using its own components which represent the STF file 1:1 with no regard for Unity functionality. This is called the authoring scene, as it is intended for authoring STF files.

Application-converters can be made to convert the intermediary authoring scene into an application-specific one. This step is destructive and throws information not relevant for the target application away, including all STF related meta-information, resolves all relationships between components and potentially applies optimizations.

Included is a basic application-converter which converts into a pure Unity scene, and throws everything else away.
The AVA subproject includes Application-converters for VRChat and VRM/VSeeFace.
