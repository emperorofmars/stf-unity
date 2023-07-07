using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using stf.serialisation;
using UnityEngine;
using UnityEngine.Animations;

namespace stf.Components
{
	public class STFTwistConstraintBack : ASTFComponent
	{
		public static string _TYPE = "STF.constraint.twist_back";
		public float weight = 0.5f;
	}

	public class STFTwistConstraintBackImporter : ASTFComponentImporter
	{
		override public void ParseFromJson(ISTFImporter state, ISTFAsset asset, JToken json, string id, GameObject go)
		{
			var c = go.AddComponent<STFTwistConstraintBack>();
			state.AddComponent(id, c);
			this.ParseRelationships(json, c);
			c.id = id;
			c.weight = (float)json["weight"];
		}
	}

	public class STFTwistConstraintBackExporter : ASTFComponentExporter
	{
		override public JToken SerializeToJson(ISTFExporter state, Component component)
		{
			var ret = new JObject();
			STFTwistConstraintBack c = (STFTwistConstraintBack)component;
			ret.Add("type", STFTwistConstraintBack._TYPE);
			this.SerializeRelationships(c, ret);
			ret.Add("weight", c.weight);
			return ret;
		}
	}

	public class STFTwistConstraintBackConverter : ISTFSecondStageConverter
	{
		public void Convert(Component component, GameObject root, ISTFSecondStageContext context)
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
