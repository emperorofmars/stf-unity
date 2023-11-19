
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
	public class STFArmatureInstance : MonoBehaviour
	{
		public string NodeId = Guid.NewGuid().ToString();
		public string Type = "STF.armature_instance";
		public string Origin;

		
		public STFArmature armature;
		public GameObject root;
		public List<GameObject> bones;
	}

	public class STFArmatureInstanceExporter : ISTFNodeExporter
	{
		public JObject SerializeToJson(STFExportState State, GameObject Go)
		{
			throw new NotImplementedException();
		}
	}

	public class STFArmatureInstanceImporter : ISTFNodeImporter
	{
		public static string _TYPE = "STF.armature_instance";

		public GameObject ParseFromJson(ISTFAssetImportState State, JObject JsonAsset, string Id)
		{
			/*var ret = new GameObject();
			State.AddNode(ret, Id);
			
			var node = ret.AddComponent<STFNode>();
			node.NodeId = Id;
			node.name = (String)JsonAsset["name"];
			node.Origin = State.AssetInfo.assetId;

			TRSUtil.ParseTRS(ret, JsonAsset);

			foreach(string childId in JsonAsset["children"])
			{
				var childJson = (JObject)State.JsonRoot["nodes"][childId];
				var type = (string)childJson["type"];
				if(type == null || type.Length == 0) type = STFNodeImporter._TYPE;
				if(State.Context.NodeImporters.ContainsKey(type))
				{
					Debug.Log($"Parsing Node: {type}");
					var childGo = State.Context.NodeImporters[type].ParseFromJson(State, childJson, childId);
					childGo.transform.SetParent(ret.transform);
				}
				else
				{
					Debug.LogWarning($"Unrecognized Node: {type}");
					// Unrecognized Node
				}
			}

			//Components
			return ret;*/

			
			var armatureResource = (STFArmature)State.Resources[(string)JsonAsset["armature"]];
			var go = (GameObject)PrefabUtility.InstantiatePrefab(armatureResource._Resource);
			State.AddNode(go, Id);
			var armature = go.GetComponent<STFArmatureNodeInfo>();

			go.name = (string)JsonAsset["name"];
			var children = JsonAsset["children"].ToObject<List<string>>();

			var armatureInstance = go.AddComponent<STFArmatureInstance>();
			armatureInstance.NodeId = Id;
			armatureInstance.name = (String)JsonAsset["name"];
			armatureInstance.Origin = State.AssetInfo.assetId;
			
			TRSUtil.ParseTRS(go, JsonAsset);

			armatureInstance.armature = armatureResource;

			armatureInstance.root = armature.Root.gameObject;
			armatureInstance.bones = armature.Bones;

			armature.transform.SetParent(go.transform, false);
			var boneInstanceIds = JsonAsset["bone_instances"].ToObject<List<string>>();

			for(int i = 0; i < boneInstanceIds.Count; i++)
			{
				var boneInstanceJson = (JObject)State.JsonRoot["nodes"][boneInstanceIds[i]];
				var bone = armatureInstance.GetComponentsInChildren<STFNode>().First(bi => (string)boneInstanceJson["bone"] == bi.NodeId);
				var boneInstance = bone.gameObject.AddComponent<STFNode>();
				boneInstance.NodeId = boneInstanceIds[i];
				boneInstance.Type = "STF.bone_instance";
				boneInstance.Origin = State.AssetInfo.assetId;
				armatureInstance.bones[i] = boneInstance.gameObject;

				TRSUtil.ParseTRS(boneInstance.gameObject, boneInstanceJson);
				State.AddNode(boneInstance.gameObject, boneInstance.NodeId);

				// Parse BoneInstance Children
				foreach(string childId in boneInstanceJson["children"])
				{
					var childJson = (JObject)State.JsonRoot["nodes"][childId];
					var type = (string)childJson["type"];
					if(type == null || type.Length == 0) type = STFNodeImporter._TYPE;
					if(State.Context.NodeImporters.ContainsKey(type))
					{
						Debug.Log($"Parsing Node: {type}");
						var childGo = State.Context.NodeImporters[type].ParseFromJson(State, childJson, childId);
						childGo.transform.SetParent(boneInstance.transform);
					}
					else
					{
						Debug.LogWarning($"Unrecognized Node: {type}");
						// Unrecognized Node
					}
				}
				// Parse Bone Instance Components
			}

			foreach(string childId in JsonAsset["children"])
			{
				var childJson = (JObject)State.JsonRoot["nodes"][childId];
				var type = (string)childJson["type"];
				if(type == null || type.Length == 0) type = STFNodeImporter._TYPE;
				if(State.Context.NodeImporters.ContainsKey(type))
				{
					Debug.Log($"Parsing Node: {type}");
					var childGo = State.Context.NodeImporters[type].ParseFromJson(State, childJson, childId);
					childGo.transform.SetParent(go.transform);
				}
				else
				{
					Debug.LogWarning($"Unrecognized Node: {type}");
					// Unrecognized Node
				}
			}
			return go;
		}
	}
}

#endif
