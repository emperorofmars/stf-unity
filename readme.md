# STF - Scene Transfer Format
## The Worlds Most Extensible Fileformat for 3D Models and Scenes

**This is a prototype and not intended for productive use!**

## Basic Structure
STF is a binary format. It can have an arbitrary amount of chunks, but one is the minimum. The first chunk is a definition in the JSON format. All further chunks are binary buffers which have to be referenced by the JSON definition.

The JSON definition has 6 root objects
- meta: Information about the author, copyright, etc...
- main: UUID of the main asset
- assets: A list of assets. Assets can reference a list of nodes and resources, depending on the asset type
- nodes: A list of hirarchical nodes. Nodes can have a list of components
- resources: A list of resources, referenced by nodes and components
- buffers: A list of buffers, in the order of the binary chunks, referenced by resources

The STF format is similar to GLTF 2.0, especially in concept, but differs in significant ways.
Everything has an UUID. This UUID must persist between import and export of any implementation.
Every asset, node, component and resource has a type. These objects are parsed based on the type. If a type is not supported by an implementation, the JSON and all referenced objects have to be preserved and reexported unless manually removed. A file cannot be changed automatically between import and export, unless expoicitly desired by the user.
Armatures are a resource. Armatures can be instanciated as a node of the 'STF.armature-instance' type and. The armature-instance can be referenced by one or more mesh-instance components.

## STF-Unity Specific Notes
This implementation for Unity uses a two stage design. The first one parses any STF file into Unity, however it uses its own components which represent the STF file 1:1 with no regard for Unity functionality. Multiple second stages can be registered, to convert the intermediary scene into an application-specific one. Included is a basic second stage which converts into a pure Unity scene, and throws everything else away.
The intermediary format is intended for authoring STF files. A second-stage will throw all STF related information away, resolve all relationships between components and will potentially apply optimizations.

## Extensibility
The extensibility of this format is a first class feature. All implementations must provide an easy way to add and hotload support for additional types.
By default STF supports only a limited set of features, which can be expected from a 3d file-format. These include support for meshes, skinned meshes and therefore armatures, animations, arbitrary materials and textures.

If for example the included mesh type is not satisfactory, a different mesh type can be implemented. These can exist in paralell and will work as long as the importer has support of the one used. All types are namespaced, and can be versioned. It is the responsibility of the importer/exporter code for a type to handle that.

Components can have defined relationships to other components and be specific to a target applications.
Components can extend or override others.
For example, multiple Social VR applications support one or another library for bone physics. None of which are compatible with each other, but they generally work the same. A generic component can be used to describe the common features and be conversible to all implemented applications formats, erring on the side of the resulting component not spazzing out. If there exists a dedicated STF-component for a specific application, it can override the basic generic component.

To extend STF with the ability to represend VR & V-Tubing avatars, the [AVA Proof of Concept](https://github.com/emperorofmars/ava-unity) was created. This shows the potential and ease of extending STF.

## Materials
As part of creating this format, i created the beginning of a universal material format, preliminarily called: MTF - Material Transfer Format.
Its nor fleshed out at all and exists in an incredibly basic form, but this is the idea:

The material consists of a dictionary of properties. A set of universally defined properties will be defined and must be used in the specified manner. These include albedo, roughness, specular, glossyness, ... The name is used as the key for the dictionary.
Each property has a list of objects, in order of priority. Each object has a type property, and can be an scalar, integer, string, texture reference (by UUID) and anything that the importer/exporter has support for.
This is to account for the case in which not every implementation can understand every type of property. The upper most object which is understood by the implementation/target-material will be used.

Properties not defined in the MTF format, can be freely used. Properties can indicate to which target application/shader they belong and so can the entire material. The material can also have a set of hint properties, indication wether it should be rendered in a cartoony or realistic style for example.


