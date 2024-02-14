using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using STF.Serialisation;
//using UnityEditor;
using UnityEngine;

namespace STF.Tools
{
	// Utility to setup a non STF Unity scene into the STF Unity intermediary format. Adds uuid's to everything, determines armatures and places the appropriate STF components. Will be used by the default exporter automatically if the exported scene is not set up.
	public static class STFSetup
	{
		public static (List<GameObject> CreatedGos, Dictionary<UnityEngine.Object, UnityEngine.Object> ResourceMeta) SetupStandaloneAssetInplace(GameObject root)
		{
			var CreatedGos = new List<GameObject>();
			// Setup main asset info
			var asset = root.GetComponent<ISTFAsset>();
			if(asset == null)
			{
				asset = root.AddComponent<STFAsset>();
				asset.Name = root.name;
				asset.Version = "0.0.1";
			}
			var armatureResult = STFArmatureUtil.FindAndSetupArmaturesInplace(root);
			CreatedGos.AddRange(armatureResult.CreatedGos);
			SetupIds(root);
			return (CreatedGos, armatureResult.ResourceMeta);
		}

		/*public static List<UnityEngine.Object> SetupAddonAssetInplace(GameObject root)
		{
			var ret = new List<UnityEngine.Object>();
			if(root.GetComponent<STFAssetInfo>() == null)
			{
				var asset = root.AddComponent<STFAssetInfo>();
				asset.assetId = Guid.NewGuid().ToString();
				asset.assetType = STFAddonAssetExporter._TYPE;
				asset.assetName = root.name;
			}
			for(int i = 0; i < root.transform.childCount; i++)
			{
				var child = root.transform.GetChild(i).gameObject;
				SetupIds(child);
			}

			ret.AddRange(STFArmatureUtil.FindAndSetupArmatures(root));
			STFArmatureUtil.FindAndSetupExternalArmatures(root);
			return ret;
		}*/

		public static void SetupIds(GameObject root)
		{
			// Setup Id's for all gameobjects and suitable components
			foreach(var c in root.GetComponentsInChildren<Transform>())
			{
				var id = c.gameObject.GetComponent<ISTFNode>();
				if(id == null) id = c.gameObject.AddComponent<STFNode>();
				// handle supported non stf components
			}
		}
		
		// TODO: seperate optional function to determine some components from naming, like twistbones and such
	}
	
	// Determine armatures in a Unity gameobject tree.
	public static class STFArmatureUtil
	{
		/*public static void FindAndSetupExternalArmatures(GameObject root)
		{
			var tree = root.GetComponentsInChildren<Transform>();
			var skinnedMeshRenderers = root.GetComponentsInChildren<SkinnedMeshRenderer>();

			var rootBones = new Dictionary<Transform, List<SkinnedMeshRenderer>>();

			foreach(var smr in skinnedMeshRenderers)
			{
				// Not in tree
				if(tree.FirstOrDefault(t => t == smr.rootBone) == null)
				{
					var externalArmatureInstance = smr.rootBone?.parent?.GetComponent<STFArmatureInstanceNode>();
					if(externalArmatureInstance)
					{
						var smrAddon = smr.gameObject.AddComponent<STFSkinnedMeshRendererAddon>();
						smrAddon.ArmatureInstanceId = externalArmatureInstance.GetComponent<ISTFNode>().NodeId;
					}
					continue;
				}
			}
		}*/

		public class Result
		{
			public Dictionary<UnityEngine.Object, UnityEngine.Object> ResourceMeta = new Dictionary<UnityEngine.Object, UnityEngine.Object>();
			public List<GameObject> CreatedGos = new List<GameObject>();
		}

		public static Result FindAndSetupArmaturesInplace(GameObject Root)
		{
			var ret = new Result();

			var armatureInstancesToSetup = new List<STFArmatureInstanceNode>();
			foreach(var smr in Root.GetComponentsInChildren<SkinnedMeshRenderer>())
			{
				// Check if meshInstance is already set up
				var meshInstance = smr.GetComponent<STFMeshInstance>();
				if(meshInstance == null) meshInstance = smr.gameObject.AddComponent<STFMeshInstance>();
				// Check if an armatureInstance exists
				var armatureInstanceGo = smr.rootBone?.parent?.gameObject;
				if(armatureInstanceGo == null)
				{
					Debug.LogWarning("Incorrectly setup SkinnedMeshRenderer: " + smr);
					continue;
				}

				var armatureInstance = armatureInstanceGo.GetComponent<STFArmatureInstanceNode>();
				if(armatureInstance == null) armatureInstance = armatureInstanceGo.AddComponent<STFArmatureInstanceNode>();

				// Setup armatureInstance to the definetively correct values
				meshInstance.ArmatureInstance = armatureInstance;
				armatureInstance.root = smr.rootBone?.gameObject;

				if(smr.bones != null) armatureInstance.bones = new List<GameObject>(smr.bones.Select(b => b?.gameObject));

				// If the armatureInstance doesn't have an armature (eg was just created), then determine the armature from the smr bone hirarchy and the smr bind poses
				if(armatureInstance.armature == null && !armatureInstancesToSetup.Contains(armatureInstance))
				{
					armatureInstancesToSetup.Add(armatureInstance);
					ret.CreatedGos.Add(SetupArmature(smr, armatureInstance));
				}
				if(!ret.ResourceMeta.ContainsKey(smr.sharedMesh))
				{
					/*if(AssetDatabase.IsMainAsset(smr.sharedMesh))
					{
						var assetPath = AssetDatabase.GetAssetPath(smr.sharedMesh);
						var metaPath = Path.ChangeExtension(assetPath, "Asset");
						ret.ResourceMeta.Add(smr.sharedMesh, AssetDatabase.LoadAssetAtPath<STFMesh>(metaPath));
					}
					else
					{*/
						var stfMesh = ScriptableObject.CreateInstance<STFMesh>();
						stfMesh.ArmatureId = armatureInstance.armature.Id;
						ret.ResourceMeta.Add(smr.sharedMesh, stfMesh);
					//}
				}
			}
			// TODO: compare bind poses to see if the same armature is used multiple times in the scene

			return ret;
		}

		public static GameObject SetupArmature(SkinnedMeshRenderer Smr, STFArmatureInstanceNode ArmatureInstance)
		{
			var armatureGo = new GameObject();
			var armatureResource = armatureGo.AddComponent<STFArmatureNodeInfo>();
			armatureResource.ArmatureName = Smr.rootBone.parent.name;
			armatureResource.name = Smr.rootBone.parent.name;

			var armatureMeta = ScriptableObject.CreateInstance<STFArmature>();
			armatureMeta.Id = armatureResource.ArmatureId;
			armatureMeta.Bindposes = Smr.sharedMesh.bindposes;
			armatureMeta.Resource = armatureGo;

			ArmatureInstance.armature = armatureMeta;

			for(int i = 0; i < armatureMeta.Bindposes.Length; i++)
			{
				// Create Go's for the armature prefab
				var boneGo = new GameObject();
				var bone = boneGo.AddComponent<STFBoneNode>();
				bone.Id = Guid.NewGuid().ToString();
				bone.name = Smr.bones[i].name;
				armatureResource.Bones.Add(boneGo);
				if(Smr.bones[i] == Smr.rootBone)
				{
					armatureResource.Root = boneGo;
					armatureResource.Root.transform.SetParent(armatureResource.transform);
				}

				// Set reference from the bone instance to the bone
				var boneInstance = Smr.bones[i].GetComponent<STFBoneInstanceNode>();
				if(boneInstance == null) boneInstance = Smr.bones[i].gameObject.AddComponent<STFBoneInstanceNode>();
				boneInstance.BoneId = bone.Id;

				// calculate trs from bindposes
				if(Smr.bones[i].parent != null)
				{
					Matrix4x4 tmpMat = Matrix4x4.identity;
					var parentMatched = false;
					for(int boneIdx = 0; boneIdx < Smr.bones.Length; boneIdx++)
					{
						if(Smr.bones[i].parent == Smr.bones[boneIdx])
						{
							tmpMat = (Smr.sharedMesh.bindposes[i] * Smr.sharedMesh.bindposes[boneIdx].inverse).inverse;
							parentMatched = true;
							break;
						}
					}
					if(!parentMatched)
					{
						tmpMat = Smr.sharedMesh.bindposes[i].inverse;
					}
					armatureResource.Bones[i].transform.localPosition = new Vector3(tmpMat.GetColumn(3).x, tmpMat.GetColumn(3).y, tmpMat.GetColumn(3).z);
					armatureResource.Bones[i].transform.localRotation = new Quaternion(tmpMat.rotation.x, tmpMat.rotation.y, tmpMat.rotation.z, tmpMat.rotation.w);
					armatureResource.Bones[i].transform.localScale = new Vector3(tmpMat.lossyScale.x, tmpMat.lossyScale.y, tmpMat.lossyScale.z);
				}
				else
				{
					throw new Exception($"Bone has no parent: {Smr.bones[i].name}");
				}
			}
			for(int i = 0; i < armatureMeta.Bindposes.Length; i++)
			{
				// determine hirarchy from armature instance
				for(int childIdx = 0; childIdx < Smr.bones[i].childCount; childIdx++)
				{
					for(var boneIdx = 0; boneIdx < Smr.bones.Length; boneIdx++)
					{
						if(Smr.bones[boneIdx] == Smr.bones[i].GetChild(childIdx))
						{
							armatureResource.Bones[boneIdx].transform.SetParent(armatureResource.Bones[i].transform, false);
							break;
						}
					}
				}
			}

			if(Smr.rootBone.parent != null)
			{
				armatureResource.transform.localPosition = new Vector3(Smr.rootBone.parent.localPosition.x, Smr.rootBone.parent.localPosition.y, Smr.rootBone.parent.localPosition.z);
				armatureResource.transform.localRotation = new Quaternion(Smr.rootBone.parent.localRotation.x, Smr.rootBone.parent.localRotation.y, Smr.rootBone.parent.localRotation.z, Smr.rootBone.parent.localRotation.w);
				armatureResource.transform.localScale = new Vector3(Smr.rootBone.parent.localScale.x, Smr.rootBone.parent.localScale.y, Smr.rootBone.parent.localScale.z);
			}
			return armatureGo;
		}
	}
}
