using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using stf.serialisation;
using UnityEngine;
using UnityEngine.Animations;

namespace stf.Components
{
	public class STFTwistConstraintBack : MonoBehaviour, ISTFComponent
	{
		public string _id = Guid.NewGuid().ToString();
		public string id {get => _id; set => _id = value;}
		public List<string> _extends;
		public List<string> extends {get => _extends; set => _extends = value;}
		public List<string> _overrides;
		public List<string> overrides {get => _overrides; set => _overrides = value;}
		public List<string> _targets;
		public List<string> targets {get => _targets; set => _targets = value;}
		
		public static string _TYPE = "STF.constraint.twist_back";
		public float weight = 0.5f;

		public string GetId() { return id; }
		public List<string> GetExtends() { return null; }
		public List<string> GetOverrides() { return null; }
	}

	public class STFTwistConstraintBackImporter : ASTFComponentImporter
	{
		override public void parseFromJson(ISTFImporter state, ISTFAsset asset, JToken json, string id, GameObject go)
		{
			var c = go.AddComponent<STFTwistConstraintBack>();
			state.AddComponent(id, c);
			c.id = id;
			c.weight = (float)json["weight"];
		}
	}

	public class STFTwistConstraintBackExporter : ASTFComponentExporter
	{
		override public JToken serializeToJson(ISTFExporter state, Component component)
		{
			var ret = new JObject();
			STFTwistConstraintBack c = (STFTwistConstraintBack)component;
			ret.Add("type", STFTwistConstraintBack._TYPE);
			ret.Add("weight", c.weight);
			return ret;
		}
	}

	public class STFTwistConstraintBackConverter : ISTFSecondStageConverter
	{
		public void convert(Component component, GameObject root, List<UnityEngine.Object> resources, STFSecondStageContext context)
		{
			var stfComponent = (STFTwistConstraintBack)component;
			var converted = component.gameObject.AddComponent<RotationConstraint>();

			converted.weight = stfComponent.weight;
			converted.rotationAxis = UnityEngine.Animations.Axis.Y;

			var source = new UnityEngine.Animations.ConstraintSource();
			source.weight = 1;
			source.sourceTransform = component.transform.parent.parent;
			converted.AddSource(source);

			Quaternion rotationOffset = Quaternion.Inverse(source.sourceTransform.rotation) * converted.transform.rotation;
			converted.rotationOffset = rotationOffset.eulerAngles;

			converted.locked = true;
			converted.constraintActive = true;
			
			context.RelMat.AddConverted(component, converted);
		}
	}
}
