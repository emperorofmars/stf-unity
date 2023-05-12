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
		public string _id;
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
		override public void parseFromJson(ISTFImporter state, JToken json, string id, GameObject go)
		{
			var component = go.AddComponent<STFTwistConstraintBack>();
			component.name = id + (string)json["name"];
			component.id = id;
			component.weight = (float)json["weight"];
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
		public void convert(Component component, GameObject root)
		{
			var stfComponent = (STFTwistConstraintBack)component;
			var converted = component.gameObject.AddComponent<RotationConstraint>();

			converted.weight = stfComponent.weight;
			converted.rotationAxis = UnityEngine.Animations.Axis.Y;

			var source = new UnityEngine.Animations.ConstraintSource();
			source.weight = 1;
			//GameObject sourceTransformGO = s.source;// TreeUtils.findByUUID(root, s.source_uuid);
			//if(sourceTransformGO != null) source.sourceTransform = sourceTransformGO.transform;
			source.sourceTransform = component.transform.parent.parent;

			converted.AddSource(source);
			converted.locked = true;
			converted.constraintActive = true;


			#if UNITY_EDITOR
            UnityEngine.Object.DestroyImmediate(component);
			#else
            UnityEngine.Object.Destroy(component);
			#endif
		}
	}
}
