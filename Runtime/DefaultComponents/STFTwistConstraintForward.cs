
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using stf.serialisation;
using UnityEngine;

namespace stf.Components
{
	public class STFTwistConstraintForward : MonoBehaviour, ISTFComponent
	{
		public string id {get; set;}
		public List<string> extends {get; set;}
		public List<string> overrides {get; set;}
		public List<string> targets {get; set;}
		public static string _TYPE = "STF.constraint.twist_forward";
		public float weight = 0.5f;
	}

	public class STFTwistConstraintForwardImporter : ASTFComponentImporter
	{
		override public void parseFromJson(ISTFImporter state, JToken json, string id, GameObject go)
		{
			var component = go.AddComponent<STFTwistConstraintForward>();
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
