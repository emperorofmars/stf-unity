using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using stf.serialisation;
using UnityEngine;

namespace stf.Components
{
	public class STFTwistConstraint : MonoBehaviour, ISTFComponent
	{
		public string _id;
		public string id {get => _id; set => _id = value;}
		public List<string> _extends;
		public List<string> extends {get => _extends; set => _extends = value;}
		public List<string> _overrides;
		public List<string> overrides {get => _overrides; set => _overrides = value;}
		public List<string> _targets;
		public List<string> targets {get => _targets; set => _targets = value;}
		
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
