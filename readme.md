# STF - Scene Transfer Format
## The Worlds Most Extensible Fileformat for 3D Models and Scenes

## **This is a prototype and not intended for productive use!**

## Basic Structure
STF is a binary format. It can have an arbitrary amount of chunks, but one is the minimum. The first chunk is a definition in the JSON format. All further chunks are binary buffers which have to be referenced by the JSON definition.

The JSON definition has 6 root objects:
- meta: Information about the author, copyright, etc...
- main: UUID of the main asset.
- assets: A dict of UUID -> assets pairs. Assets can reference a list of nodes and resources, depending on the asset type.
- nodes: A dict of UUID -> node pairs. Nodes can have a list of components and child-nodes.
- resources: A list of UUID -> resource pairs. Resouces can be referenced by nodes, components and potentially assets.
- buffers: A list of buffer UUID's in the order of the binary chunks, which referenced by resources.

The STF format is similar to GLTF 2.0, especially in concept, but differs in significant ways.
Everything has an UUID. This UUID must persist between import and export of any implementation.
Every asset, node, component and resource has a type. These objects are parsed based on the type. If a type is not supported by an implementation, the JSON and all referenced objects have to be preserved and reexported unless manually removed. A file cannot be changed automatically between import and export, unless expoicitly desired by the user.
Armatures are a resource. Armatures can be instanciated as a node of the 'STF.armature-instance' type and. The armature-instance can be referenced by one or more mesh-instance components.

Example:

	{
		"meta": {"author":"Emperor of Mars"},
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
This implementation for Unity uses a two stage design. The first one parses any STF file into Unity, however it uses its own components which represent the STF file 1:1 with no regard for Unity functionality. Multiple second stages can be registered, to convert the intermediary scene into an application-specific one. Included is a basic second stage which converts into a pure Unity scene, and throws everything else away.
The intermediary format is intended for authoring STF files. A second-stage will throw all STF related information away, resolve all relationships between components and will potentially apply optimizations.

## Extensibility
The extensibility of this format is a first class feature. All implementations must provide an easy way to add and hotload support for additional types.
By default STF supports only a limited set of features which can be expected from a common 3d file-format. These include support for meshes, skinned meshes, armatures, animations, materials and textures.

If for example the included mesh type is not satisfactory, a different mesh type can be implemented. These can exist in paralell and will work as long as the importer has support of the one used. All types are namespaced, and can be versioned. It is the responsibility of the importer/exporter code for a type to handle versioning. Importers are implemented for each type in an encapsulated manner. As such it is trivial to register additional ones.

Components can have defined relationships to other components and be specific to a target applications.
Components can extend or override others.
For example, multiple Social VR applications support one or another library for bone physics. None of which are compatible with each other, but they generally work the same. A generic STF-component can be used to describe the common features and be conversible to all implemented applications formats, err-ing on the side of the resulting application-component not spazzing out. If there exists a dedicated STF-component for a specific application, it can override the basic generic component. This way multiple mutually exclusive and application/game-engine specific features can be supported simultanously.

To extend STF with the ability to represend VR & V-Tubing avatars, the [AVA Proof of Concept](https://github.com/emperorofmars/ava-unity) was created. This shows the potential and ease of extending STF.

## Materials
As part of creating this format, i created the beginning of a universal material format, preliminarily called: MTF - Material Transfer Format.
Its not fleshed out at all and exists in an incredibly basic form, but this is the idea:

The material consists of a dictionary of properties. A set of universall properties will be defined and must be used in its specified manner. These include albedo, roughness, specular, glossyness, ... The name is used as the key for the dictionary.

Each property has a list of objects, in order of priority. Each object has a type property, and can be an scalar, integer, string, texture reference (by UUID), texture channel reference, ..., and anything that the importer/exporter has support for.
It is a list to account for the case in which not every implementation can understand every type of property. The first object which is understood by the implementation/target-material will be used.

Example: The first and post prioritized object could be a methematical definition, which is only understood by a few specific applications. To make it work elsewhere, the second object could be a texture, rendered from the mathematical definition.

Properties not defined in the MTF format, can be freely used. Properties can indicate to which target application/shader they belong and so can the entire material. The material can also have a set of hint properties (just a list of string key-value pairs), indicating wether it should be rendered in a cartoony or realistic style for example.

Converters for specific shaders can be implemented, otherwise properties can be converted based on Unitys system.
The "target_shader" property indicates which converter is to be used. If a converter or target shader is not present, a default will be chosen, or the user can specify an alternative shader.
This way, even if a perfect conversion is not possible, the hope is that at least the best possible conversion can happen. This will also ease the switching of shaders.

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
	}


