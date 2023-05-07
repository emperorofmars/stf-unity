
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace stf.serialisation
{
	public class STFArmatureResource
	{
		public string id;
		public string rootBoneId;
		public List<string> boneIds = new List<string>();
		public JArray boneMappings = new JArray();

		//public void setup(ISTFExporter state, Transform rootBone, Transform[] bones, Matrix4x4[] bindposes)
		public void setupFromSkinnedMeshRenderer(ISTFExporter state, SkinnedMeshRenderer smr)
		{
			var rootBone = smr.rootBone;
			var bones = smr.bones;
			var bindposes = smr.sharedMesh.bindposes;

			// get id from original armature definition
			this.id = Guid.NewGuid().ToString();


			//boneMappings = new JArray();
			var boneNodes = new List<JObject>();
			for(int i = 0; i < bindposes.Length; i++)
			{
				var boneIdComponent = bones[i].GetComponent<STFUUID>();
				var boneId = boneIdComponent != null && boneIdComponent.boneId != null && boneIdComponent.boneId.Length > 0 ? boneIdComponent.boneId : Guid.NewGuid().ToString();
				var bone = new JObject();

				// get from original bone definition if present

				bone.Add("name", bones[i].name);
				
				Matrix4x4 parentBindposeInverse = Matrix4x4.identity;
				if(bones[i] != rootBone)
				{
					for(int bindposeIdx = 0; bindposeIdx < bindposes.Length; bindposeIdx++)
					{
						if(bones[i].parent == bones[i])
						{
							parentBindposeInverse = bindposes[i].inverse;
							break;
						}

					}
				}
				var transformMat = bindposes[i] * parentBindposeInverse;

				bone.Add("trs", new JArray() {
					new JArray() {transformMat.GetColumn(3).x, transformMat.GetColumn(3).y, transformMat.GetColumn(3).z},
					new JArray() {transformMat.rotation.x, transformMat.rotation.y, transformMat.rotation.z, transformMat.rotation.w},
					new JArray() {transformMat.lossyScale.x, transformMat.lossyScale.y, transformMat.lossyScale.z}
				});
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
					var isBone = false;
					foreach(var bone in bones) if(bone == bones[i].GetChild(childIdx))
					{
						isBone = true;
						break;
					}
					if(isBone) children.Add(state.GetNodeId(bones[i].GetChild(childIdx).gameObject));
				}
				if(children.Count > 0) boneNodes[i].Add("children", new JArray(children));
			}
			var armatureJson = new JObject();
			armatureJson.Add("type", "armature");
			if(rootBone.parent != null) armatureJson.Add("name", rootBone.parent.name);
			armatureJson.Add("root", rootBoneId);
			armatureJson.Add("bones", boneMappings);
			state.RegisterResource(id, armatureJson);
		}
	}
}
