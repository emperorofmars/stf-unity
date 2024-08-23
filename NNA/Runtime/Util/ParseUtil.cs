
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace nna
{
	public enum NNAValueType
	{
		Null, Bool, String, Int, Float, Reference
	}
	public class NNAValue
	{
		public NNAValue(NNAValueType ValueType, object Value) { this.ValueType = ValueType; this.Value = Value; }
		public NNAValueType ValueType { get; private set; }
		public object Value { get; private set; }
	}

	public static class ParseUtil
	{
		public static bool IsNNANode(string NodeName)
		{
			return NodeName.Contains("$nna") && !Regex.IsMatch(NodeName, @"^\$[0-9]+\$") /*!NodeName.StartsWith("$")*/;
		}
		public static bool IsNNAMultiNode(string NodeName)
		{
			return NodeName.Contains("$nna-multinode") && !Regex.IsMatch(NodeName, @"^\$[0-9]+\$") /*!NodeName.StartsWith("$")*/;
		}
		public static string GetActualNodeName(string NodeName)
		{
			return IsNNANode(NodeName) && !NodeName.StartsWith("$nna") ? NodeName.Substring(0, NodeName.IndexOf("$nna")).Trim() : NodeName;
		}
		public static string GetNNAString(string NodeName)
		{
			return NodeName.Substring(NodeName.IndexOf("$nna") + 4).Trim();;
		}

		public static JArray ParseNode(Transform Node, List<Transform> Trash)
		{
			if(IsNNANode(Node.name))
			{
				if(IsNNAMultiNode(Node.name))
				{
					return JArray.Parse(CombineMultinodeDefinition(Node, Trash));
				}
				else
				{
					return JArray.Parse(GetNNAString(Node.name));
				}
			}
			return new JArray();
		}

		private static string CombineMultinodeDefinition(Transform NNANode, List<Transform> Trash)
		{
			List<(int, string)> NNAStrings = new List<(int, string)>();
			for(int childIdx = 0; childIdx < NNANode.childCount; childIdx++)
			{
				var child = NNANode.GetChild(childIdx);
				if(Regex.IsMatch(child.name, @"^\$[0-9]+\$"))
				{
					var matchLen = Regex.Match(child.name, @"^\$[0-9]+\$").Length;
					NNAStrings.Add((int.Parse(child.name.Substring(1, matchLen-2)), child.name.Substring(matchLen)));
					Trash.Add(child);
				}
			}
			return NNAStrings
				.OrderBy(s => s.Item1)
				.Select(s => s.Item2)
				.Aggregate((a, b) => a + b);
		}

		public static GameObject ResolvePath(Transform Root, Transform NNANode, string Path)
		{
			Transform location = NNANode;
			foreach(var part in Path.Split('/'))
			{
				if(string.IsNullOrEmpty(part))
				{
					location = Root;
					continue;
				}
				var partProcessed = IsNNANode(part) ? GetActualNodeName(part) : part;
				if(partProcessed == "..")
				{
					location = location.parent;
					if(location == null) throw new Exception($"Invalid ref path in: {NNANode.name} (No parent node)");
				}
				else
				{
					location = location.Find(partProcessed);
					if(location == null) throw new Exception($"Invalid ref path in: {NNANode.name} (No child node named {partProcessed})");
				}
			}
			return location.gameObject;
		}

		public static bool HasMulkikey(JObject Json, params string[] Keys)
		{
			foreach(var key in Keys)
			{
				if(Json.ContainsKey(key)) return true;
			}
			return false;
		}

		public static JToken GetMulkikey(JObject Json, params string[] Keys)
		{
			foreach(var key in Keys)
			{
				if(Json.ContainsKey(key)) return Json[key];
			}
			return null;
		}

		public static JToken GetMulkikeyOrDefault(JObject Json, JToken DefaultValue, params string[] Keys)
		{
			foreach(var key in Keys)
			{
				if(Json.ContainsKey(key)) return Json[key];
			}
			return DefaultValue;
		}

		/*public static JToken GetMulkikeyOrDefault<T>(JObject Json, T DefaultValue, params string[] Keys)
		{
			return GetMulkikeyOrDefault(Json, new JValue(DefaultValue), Keys);
		}*/
	}
}
