
using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using stf.Components;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace stf.serialisation
{
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
				try
				{
					var ctx = state.GetResourceContext(resource);
					if(!ctx.ContainsKey("root")) throw new Exception($"Animation Clip {clip} error: no resourse context provided");
					var root = (GameObject)ctx["root"];

					curvesJson.Merge(convertCurves(state, AnimationUtility.GetCurveBindings(clip), clip, root));
					curvesJson.Merge(convertCurves(state, AnimationUtility.GetObjectReferenceCurveBindings(clip), clip, root));
				}
				catch(Exception e)
				{
					Debug.LogWarning(e);
				}
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

				if(!state.GetContext().AnimationTranslators.ContainsKey(curveTarget.GetType()))
					throw new Exception("Animation property can't be translated: " + c.propertyName + " ; type: " + curveTarget.GetType());
				curveJson.Add("property", state.GetContext().AnimationTranslators[curveTarget.GetType()].ToSTF(c.propertyName));

				//Debug.Log($"Curve: {c.propertyName} : {c.isDiscreteCurve} : {c.isPPtrCurve}");

				if(!c.isDiscreteCurve && !c.isPPtrCurve) curveJson.Add("type", "interpolated");
				else if(c.isDiscreteCurve && !c.isPPtrCurve) curveJson.Add("type", "discrete");
				else if(c.isPPtrCurve) curveJson.Add("type", "reference");

				// TODO: move curve data into a binary buffer
				var keysJson = new JArray();
				curveJson.Add("keys", keysJson);

				if(!c.isPPtrCurve)
				{
					var curve = AnimationUtility.GetEditorCurve(clip, c);
					foreach(var keyframe in curve.keys)
					{
						keysJson.Add(new JObject() {{"time", keyframe.time}, {"value", keyframe.value}});
					}
				}
				else
				{
					var keyframes = AnimationUtility.GetObjectReferenceCurve(clip, c);
					foreach(var keyframe in keyframes)
					{
						keysJson.Add(new JObject() {{"time", keyframe.time}, {"value", state.GetResourceId(keyframe.value)}});
					}
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
					try
					{
						var curve = new AnimationCurve();
						var target_id = (string)track["target_id"];
						var property = (string)track["property"];
						if(target_id == null || String.IsNullOrWhiteSpace(target_id)) throw new Exception("Target id for animation is null!");

						// add keys depending on curve type
						if((string)track["type"] != "reference")
						{
							if(track["keys"] != null) foreach(JObject key in track["keys"])
							{
								curve.AddKey((float)key["time"], (float)key["value"]);
							}
						}
						else
						{
							throw new NotImplementedException();
						}

						var targetNode = state.GetNode(target_id);
						var targetComponent = state.GetComponent(target_id);
						var targetResource = state.GetResource(target_id);
						if(targetNode != null)
						{
							if(!state.GetContext().AnimationTranslators.ContainsKey(targetNode.GetType()))
								throw new Exception("Property can't be translated: " + property);
							var translatedProperty = state.GetContext().AnimationTranslators[targetNode.GetType()].ToUnity(property);
							var targetType = (property.StartsWith("translation") || property.StartsWith("rotation") || property.StartsWith("scale")) ? typeof(Transform) : typeof(GameObject);
							ret.SetCurve("STF_NODE:" + target_id, targetType, translatedProperty, curve);
						}
						else if(targetComponent != null)
						{
							if(!state.GetContext().AnimationTranslators.ContainsKey(targetComponent.GetType()))
								throw new Exception("Property can't be translated: " + property);
							var translatedProperty = state.GetContext().AnimationTranslators[targetComponent.GetType()].ToUnity(property);
							var goId = targetComponent.gameObject.GetComponent<STFUUID>().id;
							ret.SetCurve("STF_COMPONENT:" + goId + ":" + target_id, targetComponent.GetType(), translatedProperty, curve);
						}
						else if(targetResource != null)
						{
							if(!state.GetContext().AnimationTranslators.ContainsKey(targetResource.GetType()))
								throw new Exception("Property can't be translated: " + property);
							var translatedProperty = state.GetContext().AnimationTranslators[targetResource.GetType()].ToUnity(property);
							ret.SetCurve("STF_RESOURCE:" + target_id, targetResource.GetType(), translatedProperty, curve);
						}
						else
						{
							throw new Exception("Target id for animation is invalid");
						}
					}
					catch(Exception e)
					{
						Debug.LogWarning(e);
					}
				}));
			}
			return ret;
		}
	}

	public static class STFSecondStageAnimationPathTranslatorRegistry
	{
		public static Dictionary<Type, ISTFSecondStageAnimationPathTranslator> Translators = new Dictionary<Type, ISTFSecondStageAnimationPathTranslator>();
	}

#if UNITY_EDITOR
	public class STFAnimationSecondStageProcessor : ISTFSecondStageResourceProcessor
	{
		public UnityEngine.Object Convert(GameObject root, UnityEngine.Object resource, STFSecondStageContext context)
		{
			var originalAnim = (AnimationClip)resource;
			var convertedAnim = new AnimationClip();

			ConvertCurve(root, AnimationUtility.GetCurveBindings(originalAnim), originalAnim, convertedAnim);
			ConvertCurve(root, AnimationUtility.GetObjectReferenceCurveBindings(originalAnim), originalAnim, convertedAnim);
			
			return convertedAnim;
		}

		private void ConvertCurve(GameObject root, EditorCurveBinding[] bindings, AnimationClip originalAnim, AnimationClip convertedAnim)
		{
			foreach(var binding in bindings)
			{
				var animatedObject = AnimationUtility.GetAnimatedObject(root, binding);
				var translator = STFSecondStageAnimationPathTranslatorRegistry.Translators.ContainsKey(animatedObject.GetType()) ?
						STFSecondStageAnimationPathTranslatorRegistry.Translators[animatedObject.GetType()] : new DefaultSecondStageAnimationPathTranslator();

				if(!binding.isPPtrCurve)
				{
					var translated = translator.Translate(root, binding.path, animatedObject.GetType(), binding.propertyName, AnimationUtility.GetEditorCurve(originalAnim, binding));
					if(translated.omit == false) convertedAnim.SetCurve(translated.path, translated.type, translated.property, translated.curve);
				}
				else
				{
					var translated = translator.Translate(root, binding.path, animatedObject.GetType(), binding.propertyName, AnimationUtility.GetObjectReferenceCurve(originalAnim, binding));
					if(translated.omit == false)
					{
						var newBinding = new EditorCurveBinding();
						newBinding.path = translated.path;
						newBinding.propertyName = translated.property;
						newBinding.type = translated.type;
						AnimationUtility.SetObjectReferenceCurve(convertedAnim, newBinding, translated.curve);
					}
				}
			}
		}
	}
#endif
}
