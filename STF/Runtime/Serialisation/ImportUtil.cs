
using Newtonsoft.Json.Linq;
using STF.Types;
using STF.Util;
using UnityEngine;

namespace STF.Serialisation
{
	public static class ImportUtil
	{
		public static GameObject ParseNode(STFImportState State, JObject Json, string Id)
		{
			var type = (string)Json[STFKeywords.Keys.Type];
			if(State.Context.NodeImporters.ContainsKey(type))
			{
				return State.Context.NodeImporters[type].ParseFromJson(State, Json, Id);
			}
			else
			{
				return STFUnrecognizedNodeImporter.ParseFromJson(State, Json, Id);
			}
		}

		public static void ParseNodeChildrenAndComponents(STFImportState State, GameObject Go, JObject Json)
		{
			ParseNodeChildren(State, Go, Json);
			ParseNodeComponents(State, Go, Json);
		}

		public static void ParseNodeChildren(STFImportState State, GameObject Go, JObject Json)
		{
			if(Json["children"] == null || Json["children"].Type == JTokenType.Null) return;
			foreach(string childId in Json["children"])
			{
				var childJson = (JObject)State.JsonRoot["nodes"][childId];
				var type = (string)childJson["type"];
				if(type == null || type.Length == 0) type = STFNode._TYPE;
				if(State.Context.NodeImporters.ContainsKey(type))
				{
					var childGo = State.Context.NodeImporters[type].ParseFromJson(State, childJson, childId);
					childGo.transform.SetParent(Go.transform, false);
				}
				else
				{
					Debug.LogWarning($"Unrecognized Node: {type}");
					var childGo = STFUnrecognizedNodeImporter.ParseFromJson(State, childJson, childId);
					childGo.transform.SetParent(Go.transform, false);
				}
			}
		}

		public static void ParseNodeComponents(STFImportState State, GameObject Go, JObject Json)
		{
			if(Json["components"] == null || Json["components"].Type == JTokenType.Null) return;
			foreach(var entry in (JObject)Json["components"])
			{
				var type = (string)entry.Value["type"];
				if(type != null && State.Context.NodeComponentImporters.ContainsKey(type))
				{
					State.Context.NodeComponentImporters[type].ParseFromJson(State, (JObject)entry.Value, entry.Key, Go);
				}
				else
				{
					Debug.LogWarning($"Unrecognized Component: {type}");
					STFUnrecognizedNodeComponentImporter.ParseFromJson(State, (JObject)entry.Value, entry.Key, Go);
				}
			}
		}
		public static void ParseResource(STFImportState State, JObject Json, string Id)
		{
			var type = (string)Json["type"];
			if(State.Context.ResourceImporters.ContainsKey(type))
			{
				State.Context.ResourceImporters[type].ParseFromJson(State, Json, Id);
			}
			else
			{
				Debug.LogWarning($"Unrecognized Resource: {type}");
				STFUnrecognizedResourceImporter.ParseFromJson(State, Json, Id);
			}
		}

		public static void ParseResourceComponents(STFImportState State, ISTFResource Resource, JObject JsonResource)
		{
			if(JsonResource.ContainsKey("components") || JsonResource["components"].Type == JTokenType.Null) return;
			foreach(var entry in (JObject)JsonResource["components"])
			{
				var type = (string)entry.Value["type"];
				if(type != null && State.Context.ResourceComponentImporters.ContainsKey(type))
				{
					State.Context.ResourceComponentImporters[type].ParseFromJson(State, (JObject)entry.Value, entry.Key, Resource);
				}
				else
				{
					Debug.LogWarning($"Unrecognized Component: {type}");
					STFUnrecognizedResourceComponentImporter.ParseFromJson(State, (JObject)entry.Value, entry.Key, Resource);
				}
			}
		}
	}
}
