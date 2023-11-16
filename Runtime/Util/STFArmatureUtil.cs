using System;
using System.Collections.Generic;
using System.Linq;
using stf.Components;
using stf.serialisation;
using UnityEngine;

namespace stf
{
	// Determine armatures in a Unity gameobject tree.

	public static class STFArmatureUtil
	{
		public static void FindAndSetupExternalArmatures(GameObject root)
		{
			var tree = root.GetComponentsInChildren<Transform>();
			var skinnedMeshRenderers = root.GetComponentsInChildren<SkinnedMeshRenderer>();

			var rootBones = new Dictionary<Transform, List<SkinnedMeshRenderer>>();

			foreach(var smr in skinnedMeshRenderers)
			{
				// Not in tree
				if(tree.FirstOrDefault(t => t == smr.rootBone) == null)
				{
					var externalArmatureInstance = smr.rootBone?.parent?.GetComponent<STFArmatureInstance>();
					if(externalArmatureInstance)
					{
						var smrAddon = smr.gameObject.AddComponent<STFSkinnedMeshRendererAddon>();
						smrAddon.ArmatureInstanceId = externalArmatureInstance.GetComponent<STFUUID>().id;
					}
					continue;
				}
			}
		}


		public static List<STFArmature> FindAndSetupArmatures(GameObject root)
		{
			var ret = new List<STFArmature>();
			var tree = root.GetComponentsInChildren<Transform>();
			var skinnedMeshRenderers = root.GetComponentsInChildren<SkinnedMeshRenderer>();

			var rootBones = new Dictionary<Transform, List<SkinnedMeshRenderer>>();

			foreach(var smr in skinnedMeshRenderers)
			{
				// collect mesh renderers that share the same root bone
				if(tree.FirstOrDefault(t => t == smr.rootBone?.parent) != null)
				{
					if(!rootBones.ContainsKey(smr.rootBone)) rootBones.Add(smr.rootBone, new List<SkinnedMeshRenderer>{smr});
					else rootBones[smr.rootBone].Add(smr);
				}
			}

			// TODO: compare bind poses to see if the same armature, or a subset of one, is used multiple times in the scene

			foreach(var rootBone in rootBones)
			{
				// Armature already exists
				if(rootBone.Key.parent != null && rootBone.Key.parent.GetComponent<STFArmatureInstance>()?.armature != null) continue;

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

				var armatureGo = new GameObject();
				var armature = armatureGo.AddComponent<STFArmature>();
				armature.armatureId = Guid.NewGuid().ToString();
				armature.bindposes = bindposes;

				for(int i = 0; i < bindposes.Length; i++)
				{
					var bone = new GameObject();
					var boneId = bone.AddComponent<STFUUID>();
					boneId.boneId = Guid.NewGuid().ToString();
					armature.bones.Add(bone.transform);
				}
				foreach(var smr in rootBone.Value)
				{
					for(int i = 0; i < bindposes.Length; i++)
					{
						smr.bones[i].GetComponent<STFUUID>().boneId = armature.bones[i].GetComponent<STFUUID>().boneId;
					}
				}
				armature.root = takenSmr.rootBone;

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
								armature.bones[boneIdx].SetParent(armature.bones[i], false);
								break;
							}
						}
					}
				}

				armature.armatureName = rootBone.Key.parent.name;
				armature.name = rootBone.Key.parent.name + "Armature";
				if(rootBone.Key.parent != null)
				{
					armature.transform.localPosition = new Vector3(rootBone.Key.parent.localPosition.x, rootBone.Key.parent.localPosition.y, rootBone.Key.parent.localPosition.z);
					armature.transform.localRotation = new Quaternion(rootBone.Key.parent.localRotation.x, rootBone.Key.parent.localRotation.y, rootBone.Key.parent.localRotation.z, rootBone.Key.parent.localRotation.w);
					armature.transform.localScale = new Vector3(rootBone.Key.parent.localScale.x, rootBone.Key.parent.localScale.y, rootBone.Key.parent.localScale.z);
				}

				var armatureInstance = rootBone.Key.parent.gameObject.AddComponent<STFArmatureInstance>();
				armatureInstance.armature = armature;
				armatureInstance.root = takenSmr.bones.First(b => b.GetComponent<STFUUID>().boneId == armature.root.GetComponent<STFUUID>().boneId);
				armatureInstance.bones = takenSmr.bones;

				ret.Add(armature);
			}

			return ret;
		}
	}
}
