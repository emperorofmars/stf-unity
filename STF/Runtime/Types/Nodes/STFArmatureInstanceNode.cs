
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using STF.Serialisation;
using STF.Util;
using UnityEngine;

namespace STF.Types
{
	public class STFArmatureInstanceNode : ISTFNode
	{
		public const string _TYPE = "STF.armature_instance";
		public override string Type => _TYPE;

		public ResourceReference Armature = new();
		public GameObject Root;
		public List<GameObject> Bones;
	}

	public class STFArmatureInstanceExporter : ASTFNodeExporter
	{
		public override string SerializeToJson(STFExportState State, GameObject Go)
		{
			var node = Go.GetComponent<STFArmatureInstanceNode>();
			var armatureInstanceChildren = new JArray();
			for(int childIdx = 0; childIdx < Go.transform.childCount; childIdx++)
			{
				var child = Go.transform.GetChild(childIdx);
				if(child.GetComponent<STFBoneNode>() == null && child.GetComponent<STFBoneInstanceNode>() == null)
				{
					armatureInstanceChildren.Add(ExportUtil.SerializeNode(State, child.gameObject));
				}
			}
			var ret = new JObject
			{
				{"type", STFArmatureInstanceNode._TYPE},
				{"name", Go.name},
				{"trs", TRSUtil.SerializeTRS(Go)},
				{"children", armatureInstanceChildren},
				{"components", ExportUtil.SerializeNodeComponents(State, Go.GetComponents<Component>())},
			};

			var rf = new RefSerializer(ret);
			var armatureId = node.Armature.IsRef ? ExportUtil.SerializeResource(State, node.Armature.Ref) : node.Armature.Id;
			ret.Add("armature", rf.ResourceRef(armatureId));

			foreach(var entry in node.Bones)
			{
				var boneInstance = entry.GetComponent<STFBoneInstanceNode>();
				var boneInstanceChildren = new JArray();
				for(int childIdx = 0; childIdx < boneInstance.transform.childCount; childIdx++)
				{
					var child = boneInstance.transform.GetChild(childIdx);
					if(child.GetComponent<STFBoneNode>() == null && child.GetComponent<STFBoneInstanceNode>() == null)
					{
						boneInstanceChildren.Add(ExportUtil.SerializeNode(State, child.gameObject));
					}
				}
				var boneInstanceJson = new JObject {
					{"type", STFBoneInstanceNode._TYPE},
					{"name", boneInstance.name},
					{"bone", boneInstance.BoneId},
					{"trs", TRSUtil.SerializeTRS(boneInstance.gameObject)},
					{"children", boneInstanceChildren},
					{"components", ExportUtil.SerializeNodeComponents(State, boneInstance.GetComponents<Component>())},
				};

				rf.NodeRef(State.AddNode(boneInstance.gameObject, boneInstanceJson, boneInstance.Id));
			}
			return State.AddNode(Go, ret, node.Id);
		}
	}

	public class STFArmatureInstanceImporter : ASTFNodeImporter
	{
		public override GameObject ParseFromJson(STFImportState State, JObject JsonAsset, string Id)
		{
			var rf = new RefDeserializer(JsonAsset);
			var armatureResource = (STFArmature)State.Resources[rf.ResourceRef(JsonAsset["armature"])];
			var go = (GameObject)State.UnityContext.Instantiate(armatureResource.Resource);
			var armatureInfo = go.GetComponent<STFArmatureNodeInfo>();

			go.name = (string)JsonAsset["name"];

			var armatureInstance = go.AddComponent<STFArmatureInstanceNode>();
			armatureInstance.Id = Id;

			State.AddNode(armatureInstance);

			armatureInstance.name = (string)JsonAsset["name"];
			armatureInstance.PrefabHirarchy = 1;
			armatureInstance.Origin = State.AssetId;
			
			TRSUtil.ParseTRS(go, JsonAsset);

			go.transform.SetParent(go.transform, false);
			var boneInstanceIds = rf.NodeRefs();

			armatureInstance.Armature = new ResourceReference(armatureResource);
			armatureInstance.Root = armatureInfo.Root;
			armatureInstance.Bones = new List<GameObject>(new GameObject[boneInstanceIds.Count]);

			for(int i = 0; i < boneInstanceIds.Count; i++)
			{
				var boneInstanceJson = (JObject)State.JsonRoot[STFKeywords.ObjectType.Nodes][boneInstanceIds[i]];
				var bone = armatureInstance.GetComponentsInChildren<STFBoneNode>().First(bi => (string)boneInstanceJson["bone"] == bi.Id);
				var boneInstance = bone.gameObject.AddComponent<STFBoneInstanceNode>();
				boneInstance.Id = boneInstanceIds[i];
				boneInstance.BoneId = bone.Id;
				boneInstance.PrefabHirarchy = 1;
				boneInstance.Origin = State.AssetId;
				armatureInstance.Bones[i] = boneInstance.gameObject;

				TRSUtil.ParseTRS(boneInstance.gameObject, boneInstanceJson);
				State.AddNode(boneInstance);

				ImportUtil.ParseNodeChildrenAndComponents(State, boneInstance.gameObject, boneInstanceJson);
			}
			Object.DestroyImmediate(armatureInfo);
			ImportUtil.ParseNodeChildrenAndComponents(State, go, JsonAsset);
			return go;
		}
	}
}
