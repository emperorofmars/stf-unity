
using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace stf.serialisation
{
	public class STFAnimationExporter : ASTFResourceExporter
	{
		public override JToken serializeToJson(ISTFExporter state, UnityEngine.Object resource)
		{
			var clip = (AnimationClip)resource;
			var ret = new JObject();
			ret.Add("type", STFAnimationImporter._TYPE);
			ret.Add("name", clip.name);
			ret.Add("fps", clip.frameRate);
			switch(clip.wrapMode)
			{
				case WrapMode.Loop: ret.Add("loop_type", "cycle"); break;
				case WrapMode.PingPong: ret.Add("loop_type", "pingpong"); break;
				default: ret.Add("loop_type", "none"); break;
			}

			var floatCurves = AnimationUtility.GetCurveBindings(clip);
			var BindingCurves = AnimationUtility.GetObjectReferenceCurveBindings(clip);

			var curvesJson = new JArray();
			ret.Add("curves", curvesJson);

			state.AddTask(new Task(() => {
				foreach(var c in floatCurves)
				{
					var curveJson = new JObject();
					curvesJson.Add(curveJson);

					var keysJson = new JArray();
					curveJson.Add("keys", keysJson);

					var ctx = state.GetResourceContext(resource);
					if(!ctx.ContainsKey("root")) throw new Exception($"Animation Clip {clip} error: no resourse context provided");
					var curveTarget = AnimationUtility.GetAnimatedObject((GameObject)ctx["root"], c);

					var targetId = Utils.getIdFromUnityObject(curveTarget);
					if(targetId == null) throw new Exception($"Animation Clip {clip} error: invalid target");
					curveJson.Add("target_id", targetId);

					// TODO: convert property path dependent on target component translator
					curveJson.Add("property", c.propertyName);

					// TODO: move curve data into a binary buffer
					var curve = AnimationUtility.GetEditorCurve(clip, c);
					foreach(var keyframe in curve.keys)
					{
						keysJson.Add(new JObject() {{"time", keyframe.time}, {"value", keyframe.value}});
					}
				}
			}));
			
			return ret;
		}
	}

	public class STFAnimationImporter : ASTFResourceImporter
	{
		public static string _TYPE = "STF.animation";

		public override UnityEngine.Object parseFromJson(ISTFImporter state, JToken json, string id, JObject jsonRoot)
		{
			var ret = new AnimationClip();
			ret.name = (string)json["name"];
			ret.frameRate = (float)json["fps"];
			switch((string)json["loop_type"])
			{
				case "cycle": ret.wrapMode = WrapMode.Loop; break;
				case "pingpong": ret.wrapMode = WrapMode.PingPong; break;
				default: ret.wrapMode = WrapMode.Once; break;
			}

			return ret;
		}
	}
}
