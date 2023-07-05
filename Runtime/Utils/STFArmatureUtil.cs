using System;
using System.Collections.Generic;
using System.Linq;
using stf.Components;
using stf.serialisation;
using UnityEngine;

namespace stf
{
	public static class STFArmatureUtil
	{
		public static List<STFArmatureResource> FindAndSetupArmatures(GameObject root)
		{
			var ret = new List<STFArmatureResource>();
			var tree = root.GetComponentsInChildren<Transform>();
			var skinnedMeshRenderers = root.GetComponentsInChildren<SkinnedMeshRenderer>();

			var rootBones = new Dictionary<Transform, List<SkinnedMeshRenderer>>();

			foreach(var smr in skinnedMeshRenderers)
			{
				// Not in tree
				if(tree.FirstOrDefault(t => t == smr.rootBone.parent) == null)
				{
					var externalArmatureInstance = smr.rootBone?.parent?.GetComponent<STFArmatureInstance>();
					if(externalArmatureInstance)
					{
						var smrAddon = smr.gameObject.AddComponent<STFSkinnedMeshRendererAddon>();
						smrAddon.ArmatureInstanceId = externalArmatureInstance.GetComponent<STFUUID>().id;
					}
					continue;
				}
				else // collect mesh renderers that share the same root bone
				{
					if(!rootBones.ContainsKey(smr.rootBone)) rootBones.Add(smr.rootBone, new List<SkinnedMeshRenderer>{smr});
					else rootBones[smr.rootBone].Add(smr);
				}
			}

			// TODO: compare bind poses to see if the same armature is used multiple times in the scene

			foreach(var rootBone in rootBones)
			{
				var maxLength = 0;
				SkinnedMeshRenderer takenSmr = null;
				foreach(var smr in rootBone.Value)
				{
					if(smr.bones.Length > maxLength)
					{
						takenSmr = smr;
						maxLength = smr.bones.Length;
					}
				}

				var bones = takenSmr.bones;
				var bindposes = takenSmr.sharedMesh.bindposes;

				var armature = ScriptableObject.CreateInstance<STFArmatureResource>();
				armature.id = Guid.NewGuid().ToString();
				armature.bindposes = takenSmr.sharedMesh.bindposes;

				for(int i = 0; i < bindposes.Length; i++)
				{
					armature.bones.Add(new STFArmatureResource.Bone {id = Guid.NewGuid().ToString()});
				}
				foreach(var smr in rootBone.Value)
				{
					for(int i = 0; i < bindposes.Length; i++)
					{
						smr.bones[i].GetComponent<STFUUID>().boneId = armature.bones[i].id;
					}
				}
				armature.rootId = takenSmr.rootBone.GetComponent<STFUUID>().boneId;

				// calculate trs from bindposes
				for(int i = 0; i < bindposes.Length; i++)
				{
					armature.bones[i].name = bones[i].name;

					if(bones[i].parent != null)
					{
						Matrix4x4 tmpMat = Matrix4x4.identity;
						var parentMatched = false;
						for(int boneIdx = 0; boneIdx < bones.Length; boneIdx++)
						{
							if(bones[i].parent == bones[boneIdx])
							{
								tmpMat = (bindposes[i] * bindposes[boneIdx].inverse).inverse;
								parentMatched = true;
								break;
							}
						}
						if(!parentMatched)
						{
							tmpMat = bindposes[i].inverse;
						}
						armature.bones[i].localPosition = new Vector3(tmpMat.GetColumn(3).x, tmpMat.GetColumn(3).y, tmpMat.GetColumn(3).z);
						armature.bones[i].localRotation = new Quaternion(tmpMat.rotation.x, tmpMat.rotation.y, tmpMat.rotation.z, tmpMat.rotation.w);
						armature.bones[i].localScale = new Vector3(tmpMat.lossyScale.x, tmpMat.lossyScale.y, tmpMat.lossyScale.z);
					}
					else
					{
						throw new Exception($"Bone has no parent: {bones[i].name}");
					}
				}

				// determine hirarchy
				for(int i = 0; i < bindposes.Length; i++)
				{
					for(int childIdx = 0; childIdx < bones[i].childCount; childIdx++)
					{
						for(var boneIdx = 0; boneIdx < bones.Length; boneIdx++)
						{
							if(bones[boneIdx] == bones[i].GetChild(childIdx))
							{
								armature.bones[i].children.Add(armature.bones[boneIdx].id);
								break;
							}
						}
					}
				}

				armature.armatureName = rootBone.Key.parent.name;
				armature.name = rootBone.Key.parent.name + "Armature";
				if(rootBone.Key.parent != null)
				{
					armature.armaturePosition = new Vector3(rootBone.Key.parent.localPosition.x, rootBone.Key.parent.localPosition.y, rootBone.Key.parent.localPosition.z);
					armature.armatureRotation = new Quaternion(rootBone.Key.parent.localRotation.x, rootBone.Key.parent.localRotation.y, rootBone.Key.parent.localRotation.z, rootBone.Key.parent.localRotation.w);
					armature.armatureScale = new Vector3(rootBone.Key.parent.localScale.x, rootBone.Key.parent.localScale.y, rootBone.Key.parent.localScale.z);
				}

				var armatureInstance = rootBone.Key.parent.gameObject.AddComponent<STFArmatureInstance>();
				armatureInstance.armature = armature;
				armatureInstance.root = takenSmr.bones.First(b => b.GetComponent<STFUUID>().boneId == armature.rootId).gameObject;
				armatureInstance.bones = takenSmr.bones.Select(b => b.gameObject).ToArray();

				ret.Add(armature);
			}

			return ret;
		}
	}
}
