
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
		public override JToken SerializeToJson(ISTFExporter state, UnityEngine.Object resource)
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
						var id = keyframe.value is GameObject || keyframe.value is Transform ?
								state.GetNodeId(keyframe.value is GameObject ? (GameObject)keyframe.value : ((Transform)keyframe.value).gameObject) : state.GetResourceId(keyframe.value);
						keysJson.Add(new JObject() {{"time", keyframe.time}, {"value", id}});
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

		public override UnityEngine.Object ParseFromJson(ISTFImporter state, JToken json, string id, JObject jsonRoot)
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
						var target_id = (string)track["target_id"];
						var property = (string)track["property"];
						if(target_id == null || String.IsNullOrWhiteSpace(target_id)) throw new Exception("Target id for animation is null!");

						var targetNode = state.GetNode(target_id);
						var targetComponent = state.GetComponent(target_id);
						var targetResource = state.GetResource(target_id);

						if(track["keys"] == null) throw new Exception("Animation track must have keys!");

						// add keys depending on curve type
						if((string)track["type"] != "reference")
						{
							var curve = new AnimationCurve();
							foreach(JObject key in track["keys"])
							{
								curve.AddKey((float)key["time"], (float)key["value"]);
							}
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
#if UNITY_EDITOR
						else
						{
							var keyframes = new ObjectReferenceKeyframe[track["keys"].Count()];
							for(int i = 0; i < keyframes.Length; i++)
							{
								keyframes[i].time = (float)track["keys"][i]["time"];
								keyframes[i].value = state.GetResource((string)track["keys"][i]["value"]);
							}
							if(targetNode != null)
							{
								if(!state.GetContext().AnimationTranslators.ContainsKey(targetNode.GetType()))
									throw new Exception("Property can't be translated: " + property);
								var translatedProperty = state.GetContext().AnimationTranslators[targetNode.GetType()].ToUnity(property);
								var targetType = (property.StartsWith("translation") || property.StartsWith("rotation") || property.StartsWith("scale")) ? typeof(Transform) : typeof(GameObject);
								
								var newBinding = new EditorCurveBinding();
								newBinding.path = "STF_NODE:" + target_id;
								newBinding.propertyName = translatedProperty;
								newBinding.type = targetType;
								AnimationUtility.SetObjectReferenceCurve(ret, newBinding, keyframes);
							}
							else if(targetComponent != null)
							{
								if(!state.GetContext().AnimationTranslators.ContainsKey(targetComponent.GetType()))
									throw new Exception("Property can't be translated: " + property);
								var translatedProperty = state.GetContext().AnimationTranslators[targetComponent.GetType()].ToUnity(property);
								var goId = targetComponent.gameObject.GetComponent<STFUUID>().id;
								
								var newBinding = new EditorCurveBinding();
								newBinding.path = "STF_COMPONENT:" + goId + ":" + target_id;
								newBinding.propertyName = translatedProperty;
								newBinding.type = targetComponent.GetType();
								AnimationUtility.SetObjectReferenceCurve(ret, newBinding, keyframes);
							}
							else if(targetResource != null)
							{
								if(!state.GetContext().AnimationTranslators.ContainsKey(targetResource.GetType()))
									throw new Exception("Property can't be translated: " + property);
								var translatedProperty = state.GetContext().AnimationTranslators[targetResource.GetType()].ToUnity(property);
								
								var newBinding = new EditorCurveBinding();
								newBinding.path = "STF_RESOURCE:" + target_id;
								newBinding.propertyName = translatedProperty;
								newBinding.type = targetResource.GetType();
								AnimationUtility.SetObjectReferenceCurve(ret, newBinding, keyframes);
							}
							else
							{
								throw new Exception("Target id for animation is invalid");
							}
						}
#else
						else
						{
							throw new Exception("Cannot handle reference animation-curves at runtime!");
						}
#endif
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
		public UnityEngine.Object Convert(GameObject root, UnityEngine.Object resource, ISTFSecondStageContext context)
		{
			var originalAnim = (AnimationClip)resource;
			var convertedAnim = new AnimationClip();

			ConvertCurve(root, AnimationUtility.GetCurveBindings(originalAnim), originalAnim, convertedAnim, context);
			ConvertCurve(root, AnimationUtility.GetObjectReferenceCurveBindings(originalAnim), originalAnim, convertedAnim, context);

			return convertedAnim;
		}

		private void ConvertCurve(GameObject root, EditorCurveBinding[] bindings, AnimationClip originalAnim, AnimationClip convertedAnim, ISTFSecondStageContext context)
		{
			foreach(var binding in bindings)
			{
				UnityEngine.Object animatedObject = null;
				ISTFSecondStageAnimationPathTranslator translator = new DefaultSecondStageAnimationPathTranslator();
				if(binding.path.StartsWith("STF_NODE") || binding.path.StartsWith("STF_COMPONENT") || binding.path.StartsWith("STF_RESOURCE"))
				{
					var pathSplit = binding.path.Split(':');
					var objectId = pathSplit[1];
					if(binding.path.StartsWith("STF_NODE"))
					{
						animatedObject = root.GetComponentsInChildren<STFUUID>().First(id => id.id == objectId)?.gameObject;
					}
					else if(binding.path.StartsWith("STF_COMPONENT"))
					{
						var componentId = pathSplit[2];
						var go = root.GetComponentsInChildren<STFUUID>().First(id => id.id == objectId)?.gameObject;
						animatedObject = (UnityEngine.Object)go.GetComponents<ISTFComponent>()?.FirstOrDefault(c => c.id == componentId);
						if(animatedObject == null) animatedObject = go.GetComponent<STFUUID>().componentIds.Find(c => c.id == componentId)?.component;
						if(STFSecondStageAnimationPathTranslatorRegistry.Translators.ContainsKey(animatedObject.GetType()))
						{
							translator = STFSecondStageAnimationPathTranslatorRegistry.Translators[animatedObject.GetType()];
						}
					}
					else if(binding.path.StartsWith("STF_RESOURCE"))
					{
						animatedObject = context.GetOriginalResource(objectId);
					}
					
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
				else
				{
					// No need for translation assumed
					//animatedObject = AnimationUtility.GetAnimatedObject(root, binding);
				}
			}
		}
	}
#endif
}
