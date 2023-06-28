using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using stf.serialisation;
using UnityEngine;
using UnityEngine.Animations;

namespace stf.Components
{
	public class STFAnimationHolder : MonoBehaviour, ISTFComponent
	{
		public string _id = Guid.NewGuid().ToString();
		public string id {get => _id; set => _id = value;}
		public List<string> _extends;
		public List<string> extends {get => _extends; set => _extends = value;}
		public List<string> _overrides;
		public List<string> overrides {get => _overrides; set => _overrides = value;}
		public List<string> _targets;
		public List<string> targets {get => _targets; set => _targets = value;}
		
		public static string _TYPE = "STF.animation_holder";

		public List<AnimationClip> animations = new List<AnimationClip>();
	}

	public class STFAnimationHolderImporter : ASTFComponentImporter
	{
		override public void parseFromJson(ISTFImporter state, ISTFAsset asset, JToken json, string id, GameObject go)
		{
			var c = go.AddComponent<STFAnimationHolder>();
			state.AddComponent(id, c);
			this.ParseRelationships(json, c);
			c.id = id;
			foreach(var animId in json["animations"].ToObject<List<string>>())
			{
				c.animations.Add((AnimationClip)state.GetResource(animId));
			}
		}
	}

	public class STFAnimationHolderExporter : ASTFComponentExporter
	{
		public override List<UnityEngine.Object> gatherResources(Component component)
		{
			return new List<UnityEngine.Object>(((STFAnimationHolder)component).animations);
		}

		override public JToken serializeToJson(ISTFExporter state, Component component)
		{
			var ret = new JObject();
			STFAnimationHolder c = (STFAnimationHolder)component;
			ret.Add("type", STFAnimationHolder._TYPE);
			this.SerializeRelationships(c, ret);
			var animIds = new JArray();
			foreach(var a in c.animations)
			{
				animIds.Add(state.GetResourceId(a));
			}
			ret.Add("animations", animIds);
			return ret;
		}
	}
}
