
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using STF.Util;
using UnityEngine;

namespace STF.Serialisation
{
	public class STFArmatureInstanceNode : ISTFNode
	{
		public const string _TYPE = "STF.armature_instance";
		public override string Type => _TYPE;

		public STFArmature armature;
		public GameObject root;
		public List<GameObject> bones;
	}

	public class STFArmatureInstanceExporter : ASTFNodeExporter
	{
		public override string SerializeToJson(ISTFExportState State, GameObject Go)
		{
			var node = Go.GetComponent<STFArmatureInstanceNode>();
			var armatureInstanceChildren = new JArray();
			for(int childIdx = 0; childIdx < Go.transform.childCount; childIdx++)
			{
				var child = Go.transform.GetChild(childIdx);
				if(child.GetComponent<STFBoneNode>() == null && child.GetComponent<STFBoneInstanceNode>() == null)
				{
					armatureInstanceChildren.Add(SerdeUtil.SerializeNode(State, child.gameObject));
				}
			}
			var ret = new JObject
			{
				{"type", STFArmatureInstanceNode._TYPE},
				{"name", Go.name},
				{"trs", TRSUtil.SerializeTRS(Go)},
				{"children", armatureInstanceChildren},
				{"components", SerdeUtil.SerializeNodeComponents(State, Go.GetComponents<Component>())},
				{"armature", SerdeUtil.SerializeResource(State, node.armature)},
			};
			var boneInstances = new JArray();
			foreach(var entry in node.bones)
			{
				var boneInstance = entry.GetComponent<STFBoneInstanceNode>();
				var boneInstanceChildren = new JArray();
				for(int childIdx = 0; childIdx < boneInstance.transform.childCount; childIdx++)
				{
					var child = boneInstance.transform.GetChild(childIdx);
					if(child.GetComponent<STFBoneNode>() == null && child.GetComponent<STFBoneInstanceNode>() == null)
					{
						boneInstanceChildren.Add(SerdeUtil.SerializeNode(State, child.gameObject));
					}
				}
				var boneInstanceJson = new JObject {
					{"type", STFBoneInstanceNode._TYPE},
					{"name", boneInstance.name},
					{"bone", boneInstance.BoneId},
					{"trs", TRSUtil.SerializeTRS(boneInstance.gameObject)},
					{"children", boneInstanceChildren},
					{"components", SerdeUtil.SerializeNodeComponents(State, boneInstance.GetComponents<Component>())},
				};
				boneInstances.Add(State.AddNode(boneInstance.gameObject, boneInstanceJson, boneInstance.Id));
			}
			ret.Add("bone_instances", boneInstances);

			ret.Add("used_resources", new JArray{ret["armature"]});
			ret.Add("used_nodes", new JArray{ret["bone_instances"]});
			return State.AddNode(Go, ret, node.Id);
		}
	}

	public class STFArmatureInstanceImporter : ASTFNodeImporter
	{
		public override GameObject ParseFromJson(ISTFImportState State, JObject JsonAsset, string Id)
		{
			var armatureResource = (STFArmature)State.Resources[(string)JsonAsset["armature"]];
			var go = (GameObject)State.Instantiate(armatureResource.Resource);
			State.AddNode(go, Id);
			var armatureInfo = go.GetComponent<STFArmatureNodeInfo>();

			go.name = (string)JsonAsset["name"];

			var armatureInstance = go.AddComponent<STFArmatureInstanceNode>();
			armatureInstance.Id = Id;
			armatureInstance.name = (string)JsonAsset["name"];
			armatureInstance.PrefabHirarchy = 1;
			armatureInstance.Origin = State.Asset.Id;
			
			TRSUtil.ParseTRS(go, JsonAsset);

			go.transform.SetParent(go.transform, false);
			var boneInstanceIds = JsonAsset["bone_instances"].ToObject<List<string>>();

			armatureInstance.armature = armatureResource;
			armatureInstance.root = armatureInfo.Root;
			armatureInstance.bones = new List<GameObject>(new GameObject[boneInstanceIds.Count]);

			for(int i = 0; i < boneInstanceIds.Count; i++)
			{
				var boneInstanceJson = (JObject)State.JsonRoot["nodes"][boneInstanceIds[i]];
				var bone = armatureInstance.GetComponentsInChildren<STFBoneNode>().First(bi => (string)boneInstanceJson["bone"] == bi.Id);
				var boneInstance = bone.gameObject.AddComponent<STFBoneInstanceNode>();
				boneInstance.Id = boneInstanceIds[i];
				boneInstance.BoneId = bone.Id;
				boneInstance.PrefabHirarchy = 1;
				boneInstance.Origin = State.Asset.Id;
				armatureInstance.bones[i] = boneInstance.gameObject;

				TRSUtil.ParseTRS(boneInstance.gameObject, boneInstanceJson);
				State.AddNode(boneInstance.gameObject, boneInstance.Id);

				SerdeUtil.ParseNode(State, boneInstance.gameObject, boneInstanceJson);
			}

			SerdeUtil.ParseNode(State, go, JsonAsset);
			return go;
		}
	}
}
