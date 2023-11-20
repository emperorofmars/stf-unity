
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using STF.Util;
using UnityEditor;
using UnityEngine;

namespace STF.Serde
{
	public class STFArmatureInstanceNode : ASTFNode
	{
		public static string _TYPE = "STF.armature_instance";
		public override string Type => _TYPE;

		public STFArmature armature;
		public GameObject root;
		public List<GameObject> bones;
	}

	public class STFArmatureInstanceExporter : ISTFNodeExporter
	{
		public string SerializeToJson(ISTFExportState State, GameObject Go)
		{
			var node = Go.GetComponent<STFArmatureInstanceNode>();
			var ret = new JObject
			{
				{"type", STFArmatureInstanceNode._TYPE},
				{"name", Go.name},
				{"trs", TRSUtil.SerializeTRS(Go)},
				{"armature", STFSerdeUtil.SerializeResource(State, node.armature)}
			};
			// bone instances


			return State.AddNode(Go, ret, node.NodeId);
		}
	}

	public class STFArmatureInstanceImporter : ISTFNodeImporter
	{
		public GameObject ParseFromJson(ISTFAssetImportState State, JObject JsonAsset, string Id)
		{
			var armatureResource = (STFArmature)State.Resources[(string)JsonAsset["armature"]];
			var go = (GameObject)PrefabUtility.InstantiatePrefab(armatureResource._Resource);
			State.AddNode(go, Id);
			var armatureInfo = go.GetComponent<STFArmatureNodeInfo>();

			go.name = (string)JsonAsset["name"];

			var armatureInstance = go.AddComponent<STFArmatureInstanceNode>();
			armatureInstance.NodeId = Id;
			armatureInstance.name = (String)JsonAsset["name"];
			armatureInstance.Origin = State.AssetInfo.assetId;
			
			TRSUtil.ParseTRS(go, JsonAsset);

			go.transform.SetParent(go.transform, false);
			var boneInstanceIds = JsonAsset["bone_instances"].ToObject<List<string>>();

			armatureInstance.armature = armatureResource;
			armatureInstance.root = armatureInfo.Root;
			armatureInstance.bones = new List<GameObject>(new GameObject[boneInstanceIds.Count]);

			for(int i = 0; i < boneInstanceIds.Count; i++)
			{
				var boneInstanceJson = (JObject)State.JsonRoot["nodes"][boneInstanceIds[i]];
				var bone = armatureInstance.GetComponentsInChildren<STFBoneNode>().First(bi => (string)boneInstanceJson["bone"] == bi.NodeId);
				var boneInstance = bone.gameObject.AddComponent<STFBoneInstanceNode>();
				boneInstance.NodeId = boneInstanceIds[i];
				boneInstance.Origin = State.AssetInfo.assetId;
				armatureInstance.bones[i] = boneInstance.gameObject;

				TRSUtil.ParseTRS(boneInstance.gameObject, boneInstanceJson);
				State.AddNode(boneInstance.gameObject, boneInstance.NodeId);

				STFSerdeUtil.ParseNode(State, boneInstance.gameObject, boneInstanceJson);
			}

			STFSerdeUtil.ParseNode(State, go, JsonAsset);
			return go;
		}
	}
}

#endif
