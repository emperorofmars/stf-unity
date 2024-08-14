
using Newtonsoft.Json.Linq;
using STF.Types;
using STF.Util;
using UnityEngine;

namespace STF.Serialisation
{
	public static class ImportUtil
	{
		public static GameObject ParseNode(STFImportState State, string Id)
		{
			return ParseNode(State, (JObject)State.JsonRoot[STFKeywords.ObjectType.Nodes][Id], Id);
		}

		public static GameObject ParseNode(STFImportState State, JObject Json, string Id)
		{
			var type = (string)Json[STFKeywords.Keys.Type];
			if(string.IsNullOrWhiteSpace(type)) type = STFNode._TYPE;
			return State.Context.GetNodeImporter(type).ParseFromJson(State, Json, Id);
		}

		public static void ParseNodeChildrenAndComponents(STFImportState State, GameObject Go, JObject Json)
		{
			ParseNodeChildren(State, Go, Json);
			ParseNodeComponents(State, Go, Json);
		}

		public static void ParseNodeChildren(STFImportState State, GameObject Go, JObject Json)
		{
			if(Json.ContainsKey("children") || Json["children"].Type == JTokenType.Null) return;
			foreach(string childId in Json["children"])
			{
				var childGo = ParseNode(State, childId);
				childGo.transform.SetParent(Go.transform, false);
			}
		}

		public static void ParseNodeComponents(STFImportState State, GameObject Go, JObject Json)
		{
			if(Json.ContainsKey("components") || Json["components"].Type == JTokenType.Null) return;
			foreach(var entry in (JObject)Json["components"])
			{
				State.Context.GetNodeComponentImporter((string)entry.Value["type"]).ParseFromJson(State, (JObject)entry.Value, entry.Key, Go);
			}
		}
		public static void ParseResource(STFImportState State, JObject Json, string Id)
		{
			State.Context.GetResourceImporter((string)Json["type"]).ParseFromJson(State, Json, Id);
		}

		public static void ParseResourceComponents(STFImportState State, ISTFResource Resource, JObject JsonResource)
		{
			if(!JsonResource.ContainsKey("components") || JsonResource["components"].Type == JTokenType.Null) return;
			foreach(var entry in (JObject)JsonResource["components"])
			{
				State.Context.GetResourceComponentImporter((string)entry.Value["type"]).ParseFromJson(State, (JObject)entry.Value, entry.Key, Resource);
			}
		}
	}
}
