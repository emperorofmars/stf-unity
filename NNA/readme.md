# ⛧ NNA - Node Name Abuse ⛧
Extend any 3d format by abusing node-names to store JSON.

This project is an abomination and the sooner it can burn in a fire, the better.

## Purpose
Existing 3d interchange formats are bad. The least horrible one, fbx, is not extensible.

This is a way to add additional information to 3d models in any format, including fbx, as JSON into node-names.
This Unity AssetPostprocessor will parse and convert information serialized into node-names into the appropriate Unity constructs.

Since to my knowledge Blender allows only a maximum node-name length 61 bytes, NNA definitions can be split up into multiple node-names of child nodes.

## Current status
I just started making this, but generally this is how it will work.

## Example
An example of a model-hirachy using NNA In Blender could look like this:
![](./Docs/img/blender_armature_hirarchy.png)

The `c-twist` component will be converted in Unity into a `RotationConstraint`:
![](./Docs/img/unity_twist-bone_component.png)

## How it works
Assume the following hirachy: `Hips` → `UpperLeg.R` → `UpperLegTwist.R`
In order to define a `c-twist` (Twist Constraint) component on the `UpperLegTwist.R` node which uses `Hips` as the source, rename it the following way:
```
UpperLegTwist.R$nna[{"t":"c-twist","w":0.5}]
```
The actual node name is `UpperLegTwist.R` and the NNA definition contains one component whose type is `c-twist`.
The NNA definition, starting with `$nna` will be removed from the node-name. If the node-name consists only of the NNA definition, the entire node will be deleted after processing.

The Parser for the type `c-twist` will create a Unity `RotationConstraint` and set the source weight to 0.6.
Since no explicit source is specified, it will take the parent of the parent as the source.

If the source was to be specified, the character limit for a node in Blender would be very likely reached.
In that case the JSON string must be split across multiple child-nodes.
The `UpperLegTwist.R` node should be named the following way:
```
UpperLegTwist.L$nna-multinode
```
The child nodes which are part of the multinode string have to start with a line number enclosed in `$` signs.
```
$01$[{"t":"c-twist","w":0.66,"t
$02$p":"../LowerLeg.R"}]
```
I have no clue if the node-order is guaranteed to be preserved across various file-formats and their various implementations, hence the explicit line-number. The JSON string will simply be concatenated.
All child-nodes of a `$nna-multinode` will be removed after processing.

### Import Context
All processors for nna types are registered in a context.

Processors in the default context are always applied, unless another processor for the specified import context is registered.

For example, the `c-twist` type in the default context will always create a Unity `RotationConstraint` component.
If a Processor for `c-twist` was also registered with a `vrchat_avatar3` context, and the models import context is set to that, then the VRChat specific Processor will be choosen. That one could create a VRChat Constraint component instead.

## Supported Components
Support for additional components can always be hot loaded.

### Default Components

#### `c-twist` **Twist Constraint**
| parameter | type | description | default |
| --- | --- | --- | --- |
| `w` | float | Weight of the source | `0.5` |
| `tp` | string/path | Path to source node | `../..` |

Creates a `RotationConstraint` component with one source limited to the Y axis.

#### `humanoid` **Humanoid Mappings**
| parameter | type | description | default |
| --- | --- | --- | --- |
| `lt` | string | Locomotion type. Supported values are `planti` & `digi`. | `planti` |

Generates a humanoid `Avatar`. Creates an `Animator` component on the root node if not present and sets the avatar.
Currently, only automatic mapping of bones is supported. Explicit mapping may be added in the future.

### Avatar Components from the AVA subproject
Supported target applications:
* VRChat SDK3: `vrchat_avatar3`

#### `ava.avatar` **Base Avatar Definition**
Base component for avatars.

#### `ava.viewport` **Avatar Viewport**
This nodes position will be set as the avatar's viewport position.

#### `ava.eyetracking` **Avatar Eyetracking Information**
| parameter | type | description | default |
| --- | --- | --- | --- |
| `lkt` | string | Eyelook Type (`r`: Rotations, `b`: Blendshapes) | `r` |
| `ldt` | string | Eyelid Type (`r`: Rotations, `b`: Blendshapes) | `b` |
| `m` | string/path | Path to the skinned mesh which contains the target blendshapes | `Body` |
| `u` | float | Up angle | `15` |
| `d` | float | Down angle | `12` |
| `i` | float | Inner angle | `15` |
| `o` | float | Outer angle | `16` |

For now blendshapes are matched automatically based on naming. It should be possible to override that in the future.

## TODO
* More constraint types
* General avatar components (in progress)
* Bone physics, colliders, etc.
* Material mappings. As in map a material slot to a material, or perhaps a set of materials, within the project automatically. (Very maybe implement the MTF subproject from STF)
* Maybe eventually a Blender addon to make defining these components easier.
* IDK, suggest me more!
---
I am disgusted by myself for making this.
Cheers!
Or something.
