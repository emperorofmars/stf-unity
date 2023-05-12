
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using stf.serialisation;
using UnityEngine;

namespace stf.Components
{
	public class STFTwistConstraintForward : MonoBehaviour, ISTFComponent
	{
		public string _id;
		public string id {get => _id; set => _id = value;}
		public List<string> _extends;
		public List<string> extends {get => _extends; set => _extends = value;}
		public List<string> _overrides;
		public List<string> overrides {get => _overrides; set => _overrides = value;}
		public List<string> _targets;
		public List<string> targets {get => _targets; set => _targets = value;}
		
		public static string _TYPE = "STF.constraint.twist_forward";
		public float weight = 0.5f;
	}

	public class STFTwistConstraintForwardImporter : ASTFComponentImporter
	{
		override public void parseFromJson(ISTFImporter state, JToken json, string id, GameObject go)
		{
			var component = go.AddComponent<STFTwistConstraintForward>();
			component.name = id + (string)json["name"];
			component.id = id;
			component.weight = (float)json["weight"];
		}
	}

	public class STFTwistConstraintForwardExporter : ASTFComponentExporter
	{
		override public JToken serializeToJson(ISTFExporter state, Component component)
		{
			var ret = new JObject();
			STFTwistConstraintForward c = (STFTwistConstraintForward)component;
			ret.Add("type", STFTwistConstraintForward._TYPE);
			ret.Add("weight", c.weight);
			return ret;
		}
	}
}
