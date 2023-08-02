using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using stf.serialisation;
using UnityEngine;
using UnityEngine.Animations;

namespace stf.Components
{
	public class STFAnimationHolder : ASTFComponent
	{	
		public static string _TYPE = "STF.animation_holder";

		public List<AnimationClip> animations = new List<AnimationClip>();
	}

	public class STFAnimationHolderImporter : ASTFComponentImporter
	{
		override public void ParseFromJson(ISTFImporter state, ISTFAsset asset, JToken json, string id, GameObject go)
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
		public override List<KeyValuePair<UnityEngine.Object, Dictionary<string, System.Object>>> GatherResources(Component component)
		{
			return ((STFAnimationHolder)component).animations.Select(a => new KeyValuePair<UnityEngine.Object, Dictionary<string, System.Object>>(a, new Dictionary<string, System.Object> {{"root", component.gameObject}})).ToList();
		}

		override public JToken SerializeToJson(ISTFExporter state, Component component)
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
