
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using stf.serialisation;
using UnityEngine;
using UnityEngine.Animations;
using System;

namespace stf.Components
{
	public class STFTwistConstraintForward : MonoBehaviour, ISTFComponent
	{
		public string _id = Guid.NewGuid().ToString();
		public string id {get => _id; set => _id = value;}
		public List<string> _extends;
		public List<string> extends {get => _extends; set => _extends = value;}
		public List<string> _overrides;
		public List<string> overrides {get => _overrides; set => _overrides = value;}
		public List<string> _targets;
		public List<string> targets {get => _targets; set => _targets = value;}
		
		public static string _TYPE = "STF.constraint.twist_forward";
		public GameObject target;
		public float weight = 0.5f;
	}

	public class STFTwistConstraintForwardImporter : ASTFComponentImporter
	{
		override public void ParseFromJson(ISTFImporter state, ISTFAsset asset, JToken json, string id, GameObject go)
		{
			var c = go.AddComponent<STFTwistConstraintForward>();
			state.AddComponent(id, c);
			this.ParseRelationships(json, c);
			c.id = id;
			c.target = state.GetNode((string)json["target"]);
			c.weight = (float)json["weight"];
		}
	}

	public class STFTwistConstraintForwardExporter : ASTFComponentExporter
	{
		override public List<GameObject> GatherNodes(Component component)
		{
			var c = (STFTwistConstraintForward)component;
			var ret = new List<GameObject>();
			if(c.target) ret.Add(c.target);
			return ret;
		}
		override public JToken SerializeToJson(ISTFExporter state, Component component)
		{
			var ret = new JObject();
			STFTwistConstraintForward c = (STFTwistConstraintForward)component;
			ret.Add("type", STFTwistConstraintForward._TYPE);
			this.SerializeRelationships(c, ret);
			ret.Add("target", state.GetNodeId(c.target));
			ret.Add("weight", c.weight);
			return ret;
		}
	}

	public class STFTwistConstraintForwardConverter : ISTFSecondStageConverter
	{
		public void Convert(Component component, GameObject root, List<UnityEngine.Object> resources, ISTFSecondStageContext context)
		{
			var stfComponent = (STFTwistConstraintForward)component;
			var converted = component.gameObject.AddComponent<RotationConstraint>();

			converted.weight = stfComponent.weight;
			converted.rotationAxis = UnityEngine.Animations.Axis.Y;

			var source = new UnityEngine.Animations.ConstraintSource();
			source.weight = 1;
			source.sourceTransform = stfComponent.target.transform;
			converted.AddSource(source);

			Quaternion rotationOffset = Quaternion.Inverse(source.sourceTransform.rotation) * converted.transform.rotation;
			converted.rotationOffset = rotationOffset.eulerAngles;

			converted.locked = true;
			converted.constraintActive = true;
			
			context.RelMat.AddConverted(component, converted);
		}
	}
}
