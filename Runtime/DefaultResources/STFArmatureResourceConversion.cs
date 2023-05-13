
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace stf.serialisation
{
	public class STFArmatureResourceExporter
	{
		public string id;
		public string rootBoneId;
		public List<string> boneIds = new List<string>();

		public void SetupFromSkinnedMeshRenderer(ISTFExporter state, SkinnedMeshRenderer smr)
		{
			var rootBone = smr.rootBone;
			var bones = smr.bones;
			var bindposes = smr.sharedMesh.bindposes;

			var bonesList = new List<Transform>(bones);

			// get armature from stored resource
			if(smr.rootBone.parent != null && smr.rootBone.parent.GetComponent<STFArmatureInstance>() != null)
			{
				var armatureInstance = smr.rootBone.parent.GetComponent<STFArmatureInstance>();
				this.id = armatureInstance.armature.id;
				rootBoneId = armatureInstance.armature.rootId;
				foreach(var bone in armatureInstance.armature.bones) boneIds.Add(bone.id);
				
				armatureInstance.armature.serializeToJson(state);
			}
			else // determine armature from skinned mesh renderer and bindposes
			{
				this.id = Guid.NewGuid().ToString();
				var boneNodes = new List<JObject>();
				for(int i = 0; i < bindposes.Length; i++)
				{
					var boneIdComponent = bones[i].GetComponent<STFUUID>();
					var boneId = boneIdComponent != null && boneIdComponent.boneId != null && boneIdComponent.boneId.Length > 0 ? boneIdComponent.boneId : Guid.NewGuid().ToString();
					var bone = new JObject();

					bone.Add("name", bones[i].name);
					bone.Add("type", "bone");

					// get this from the bindposes instead at some point
					/*bone.Add("trs", new JArray() {
						new JArray() {bones[i].localPosition.x, bones[i].localPosition.y, bones[i].localPosition.z},
						new JArray() {bones[i].localRotation.x, bones[i].localRotation.y, bones[i].localRotation.z, bones[i].localRotation.w},
						new JArray() {bones[i].localScale.x, bones[i].localScale.y, bones[i].localScale.z}
					});*/

					if(bones[i].parent != null)
					{
						Matrix4x4 tmpMat = Matrix4x4.identity;
						var parentMatched = false;
						for(int boneIdx = 0; boneIdx < bones.Length; boneIdx++)
						{
							if(bones[i].parent == bones[boneIdx])
							{
								tmpMat = bindposes[i] * bindposes[boneIdx].inverse;
								parentMatched = true;
								break;
							}
						}
						if(!parentMatched)
						{
							tmpMat = (bindposes[i] * bones[i].parent.worldToLocalMatrix).inverse;
						}

						//var tmpMat = bindposes[i] * parentBindpose;

						/*var localPosition = bones[i].parent.position - (Vector3)tmpMat.GetColumn(3);
						var localRotation = tmpMat.rotation * Quaternion.Inverse(bones[i].parent.rotation);
						var localScale = new Vector3(tmpMat.lossyScale.x / bones[i].parent.lossyScale.x, tmpMat.lossyScale.y / bones[i].parent.lossyScale.y, tmpMat.lossyScale.z / bones[i].parent.lossyScale.z);
						bone.Add("trs", new JArray() {
							new JArray() {localPosition.x, localPosition.y, localPosition.z},
							new JArray() {localRotation.x, localRotation.y, localRotation.z, localRotation.w},
							new JArray() {localScale.x, localScale.y, localScale.z}
						});*/
						bone.Add("trs", new JArray() {
							new JArray() {tmpMat.GetColumn(3).x, tmpMat.GetColumn(3).y, tmpMat.GetColumn(3).z},
							new JArray() {tmpMat.rotation.x, tmpMat.rotation.y, tmpMat.rotation.z, tmpMat.rotation.w},
							new JArray() {tmpMat.lossyScale.x, tmpMat.lossyScale.y, tmpMat.lossyScale.z}
						});
					}
					else
					{
						bone.Add("trs", new JArray() {
							new JArray() {bindposes[i].GetColumn(3).x, bindposes[i].GetColumn(3).y, bindposes[i].GetColumn(3).z},
							new JArray() {bindposes[i].rotation.x, bindposes[i].rotation.y, bindposes[i].rotation.z, bindposes[i].rotation.w},
							new JArray() {bindposes[i].lossyScale.x, bindposes[i].lossyScale.y, bindposes[i].lossyScale.z}
						});
					}

					state.RegisterNode(boneId, bone);
					boneNodes.Add(bone);
					boneIds.Add(boneId);
					if(bones[i] == rootBone) rootBoneId = boneId;
				}
				for(int i = 0; i < bindposes.Length; i++)
				{
					var children = new List<string>();
					for(int childIdx = 0; childIdx < bones[i].childCount; childIdx++)
					{
						for(var boneIdx = 0; boneIdx < bones.Length; boneIdx++)
						{
							if(bones[boneIdx] == bones[i].GetChild(childIdx))
							{
								children.Add(boneIds[boneIdx]);
								break;
							}
						}
					}
					if(children.Count > 0) boneNodes[i].Add("children", new JArray(children));
				}

				var armatureJson = new JObject();
				armatureJson.Add("type", STFArmatureImporter._TYPE);
				if(rootBone.parent != null)
				{
					armatureJson.Add("name", rootBone.parent.name);
					armatureJson.Add("trs", new JArray() {
						new JArray() {rootBone.parent.localPosition.x, rootBone.parent.localPosition.y, rootBone.parent.localPosition.z},
						new JArray() {rootBone.parent.localRotation.x, rootBone.parent.localRotation.y, rootBone.parent.localRotation.z, rootBone.parent.localRotation.w},
						new JArray() {rootBone.parent.localScale.x, rootBone.parent.localScale.y, rootBone.parent.localScale.z}
					});
				}
				armatureJson.Add("root", rootBoneId);
				armatureJson.Add("bones", new JArray(boneIds));
				state.RegisterResource(id, armatureJson);
			}
		}
	}

	public class STFArmatureImporter : ASTFResourceImporter
	{
		public static string _TYPE = "STF.armature";

		public override UnityEngine.Object parseFromJson(ISTFImporter state, JToken json, string id, JObject jsonRoot)
		{
			var armatureResource = ScriptableObject.CreateInstance<STFArmatureResource>();
			armatureResource.parseFromJson(state, json, id, jsonRoot);
			return armatureResource;
		}
	}
}
