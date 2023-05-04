using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using stf.serialisation;
using UnityEngine;

namespace stf.Components
{
	public class STFTwistConstraint : MonoBehaviour, ISTFComponent
	{
		public string id {get; set;}
		public List<string> extends {get; set;}
		public List<string> overrides {get; set;}
		public List<string> targets {get; set;}
		public static string _TYPE = "STF.constraint.twist";
		public GameObject source;
		public float weight;
	}

	public class STFTwistConstraintImporter : ASTFComponentImporter
	{
		override public void parseFromJson(ISTFImporter state, JToken json, string id, GameObject go)
		{
			var component = go.AddComponent<STFTwistConstraint>();
			component.id = id;
			string sourceId = (string)json["source"];
			component.weight = (float)json["weight"];
			component.source = state.GetNode(sourceId);
		}
	}

	public class STFTwistConstraintExporter : ASTFComponentExporter
	{

		override public List<GameObject> gatherNodes(Component component)
		{
			return new List<GameObject> {((STFTwistConstraint)component).source};
		}
		
		override public JToken serializeToJson(ISTFExporter state, Component component)
		{
			var ret = new JObject();
			STFTwistConstraint c = (STFTwistConstraint)component;
			var source_node = state.GetNodeId(c.source);
			ret.Add("type", STFTwistConstraint._TYPE);
			ret.Add("source", source_node);
			ret.Add("weight", c.weight);
			return ret;
		}
	}
}
