
# GLTF 2.0 Doesn't Work As An Interchange Format

Originally, glTF's tagline was 'The JPEG of 3d'. For a format to work for 3d interchange/authoring, something which qualifies more as an 'open PSD of 3d' would be needed.

Many people especially in the 'open source gamedev sphere' seem to believe that glTF 2.0 is qualified for that.

Yet, I was not able to do even the most basic of things with glTF 2.0. I specifically tried to work with its Blender, Godot 4 and UnityGLTF & GLTFast implementations, so my judgement is derived from working with these.

## The biggest issues with glTF 2.0 as an interchange format:
* The animation system doesn't support animation curves, only an interpolation type parameter.
* Animations can't target anything other than node transforms and morphtarget values per mesh resource (not mesh instance).
* Morphtarget values and material references sit on the mesh resource. You can instance the mesh multiple times, but if you need to have different materials or target values on these instances, you have to create multiple mesh resources. Some implementations may deduplicate the buffer views, but since this is not in the spec, so some glTF implementations may just load the same mesh resource twice. In the context of a distribution format this works fine as the whole buffer gets shoved into GPU memory, taking the role of the resource.
* Materials support a hardcoded number of properties. Materials are arbitrary, and so are their properties. Additional properties get slowly added as extensions, which are likely not supported in your glTF implementation anyway. What if I want to add an 'Audiolink Baseband Emission' texture?
* For some reason, Morphtarget names are not present in the spec. If glTF is really intended for video game development, then that's a mistake. Applications put morphtarget names in the extras field of either the `mesh` object, or the first `primitive` of the `mesh`. For a supposedly well standardized format for interchange/authoring, this is not ok.
* The buffer system is convoluted. Until late 2023, Blender would produce comically large files (over 100MB instead of under 4MB in the case of the [example model](https://emperorofmars.itch.io/stf-avatar-showcase)), because it didn't implement sparse accessors. Why couldn't morphtargets be stored as just simple buffers, which can be indexed? That's simple, the construct of Buffers, Buffer Views, Accessors, Sparse Accessors is not. This construct works for a distribution format to be efficiently loaded into GPU memory, not for interchange/authoring.

## And last but not least, Extensibility
This I view as glTF's biggest flaw by far.

At first glance, glTF 2.0 looks supremely extensible on paper.

In practice, implementing extensions is either not supported at all (UnityGLTF, GLTFast), or if it is, it is either undocumented (Blender) or at least partially broken (Godot 4).

Actually trying to implement a custom extension (for example one for social-VR avatars), requires in some cases having to fork the entire glTF library, hard-coding your extension into it, and getting your users to use your fork.
If one extension is not supported, it will just be thrown away upon import and lost. What if I need to add a Unity specific extension and a Godot specific extension to the same file? In the case of VR avatars, which optimally would support many target applications across many game engines, this is a necessity.

Just implementing a new extension across all relevant glTF implementations is practically impossible, and nobody does it either. Even official extensions like KHR_animation_pointer, which would at least half fix the animation system, is implemented next to nowhere. An extension existing only a JSON schema file in the Khronos GitHub account is not very useful.
Sometimes people send me large lists of extensions which they think would solve all of these issues, yet next to none of them exist in any relevant glTF implementation I would need to use.

glTF 2.0 was released over 8 years ago. This is its current state.

# Conclusion
At least a JPEG is smaller than the PNG image. glTF files tend to be far larger than the original project file, for example due to the need to bake animations, while still loosing a lot of information.

I am not convinced glTF 2.0 is intended for 3d Assets in a video game context.

I would like for a format to exist which is.

---
In order for video game development with open-source tools to become viable for anything other than limited-in-fidelity indie games, an ecosystem and possibilities for proper asset pipelines are needed.

I think a proper open, interoperable & extensible 3d format could really help in making open source tools viable for many and improve the industry as a whole.
