
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using STF.IdComponents;
using STF.Util;
using UnityEngine;

namespace STF.Serde
{
	public class STFNode : ASTFNode
	{
		public static string _TYPE = "STF.node";
		public override string Type => _TYPE;
	}

	public class STFNodeExporter : ISTFNodeExporter
	{
		public string SerializeToJson(ISTFExportState State, GameObject Go)
		{
			var node = Go.GetComponent<STFNode>();
			var ret = new JObject
			{
				{"type", STFNode._TYPE},
				{"name", Go.name},
				{"trs", TRSUtil.SerializeTRS(Go)}
			};

			var childIds = new JArray();
			for(int childIdx = 0; childIdx < Go.transform.childCount; childIdx++)
			{
				var child = STFSerdeUtil.SerializeNode(State, Go.transform.GetChild(childIdx).gameObject);
				if(child != null) childIds.Add(child);
				else
				{
					Debug.LogWarning($"Skipping Unrecognized Unity Node: {Go.transform.GetChild(childIdx)}");
				}
			}
			ret.Add("children", childIds);

			var components = new JObject();
			foreach(var component in Go.GetComponents<Component>())
			{
				if(component.GetType() == typeof(Transform)) continue;
				if(component is ISTFNode) continue;
				if(component.GetType() == typeof(Animator)) continue;
				if(component.GetType() == typeof(STFAsset)) continue;

				var serializedComponent = STFSerdeUtil.SerializeComponent(State, component);
				if(serializedComponent.Key != null)	components.Add(serializedComponent.Key, serializedComponent.Value);
				else
				{
					Debug.LogWarning($"Skipping Unrecognized Unity Component: {component}");
				}
			}
			ret.Add("components", components);

			return State.AddNode(Go, ret, node.NodeId);
		}
	}

	public class STFNodeImporter : ISTFNodeImporter
	{
		public GameObject ParseFromJson(ISTFAssetImportState State, JObject JsonAsset, string Id)
		{
			var ret = new GameObject();
			State.AddNode(ret, Id);
			
			var node = ret.AddComponent<STFNode>();
			node.NodeId = Id;
			node.name = (String)JsonAsset["name"];
			node.Origin = State.AssetInfo.assetId;

			TRSUtil.ParseTRS(ret, JsonAsset);
			STFSerdeUtil.ParseNode(State, ret, JsonAsset);
			return ret;
		}
	}
}

#endif
