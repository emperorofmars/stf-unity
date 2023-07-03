
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
	public class STFAnimation : ScriptableObject
	{

	}

#if UNITY_EDITOR
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

			var curvesJson = new JArray();
			ret.Add("tracks", curvesJson);

			state.AddTask(new Task(() => {
				var ctx = state.GetResourceContext(resource);
				if(!ctx.ContainsKey("root")) throw new Exception($"Animation Clip {clip} error: no resourse context provided");
				var root = (GameObject)ctx["root"];

				curvesJson.Merge(convertCurves(state, AnimationUtility.GetCurveBindings(clip), clip, root));
				//curvesJson.Merge(convertCurves(state, AnimationUtility.GetObjectReferenceCurveBindings(clip), clip, root));
			}));
			return ret;
		}

		protected static JArray convertCurves(ISTFExporter state, EditorCurveBinding[] bindings, AnimationClip clip, GameObject root)
		{
			var curvesJson = new JArray();

			foreach(var c in bindings)
			{
				var curveJson = new JObject();
				curvesJson.Add(curveJson);

				var curveTarget = AnimationUtility.GetAnimatedObject(root, c);

				var targetId = Utils.getIdFromUnityObject(curveTarget);
				if(targetId == null) throw new Exception($"Animation Clip {clip} error: invalid target");
				curveJson.Add("target_id", targetId);

				// TODO: convert property path dependent on target component translator
				curveJson.Add("property", c.propertyName);
				
				// curveJson.Add("property", c.type);

				// TODO: move curve data into a binary buffer
				var keysJson = new JArray();
				curveJson.Add("keys", keysJson);
				var curve = AnimationUtility.GetEditorCurve(clip, c);

				// convert based on curve type
				foreach(var keyframe in curve.keys)
				{
					keysJson.Add(new JObject() {{"time", keyframe.time}, {"value", keyframe.value}});
				}
			}
			return curvesJson;
		}
	}
#endif

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

			if(json["tracks"] != null) foreach(JObject track in json["tracks"])
			{
				state.AddPostprocessTask(new Task(() => {
					var curve = new AnimationCurve();
					var target_id = (string)track["target_id"];
					var property = (string)track["property"];

					Debug.Log($"AAAAAAAAA: {property}");

					if(target_id == null || String.IsNullOrWhiteSpace(target_id)) throw new Exception("Target id for animation is null!");

					var targetNode = state.GetNode(target_id);
					var targetComponent = state.GetComponent(target_id);
					var targetResource = state.GetResource(target_id);
					
					if(targetNode != null)
					{
						ret.SetCurve("STF_NODE:" + target_id, typeof(Transform), property, curve);
					}
					else if(targetComponent != null)
					{
						var goId = targetComponent.gameObject.GetComponent<STFUUID>().id;
						ret.SetCurve("STF_COMPONENT:" + goId + ":" + target_id, targetComponent.GetType(), property, curve);
					}
					else if(targetResource != null)
					{
						ret.SetCurve("STF_RESOURCE:" + target_id, targetResource.GetType(), property, curve);
					}
					else
					{
						throw new Exception("Target id for animation is invalid");
					}
				}));
			}
			return ret;
		}
	}
}
