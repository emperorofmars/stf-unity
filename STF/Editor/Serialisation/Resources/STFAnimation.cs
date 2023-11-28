
using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using STF.Util;

namespace STF.Serialisation
{
	public class STFAnimation : ASTFResource
	{
		public const string _TYPE = "STF.animation";
		public GameObject TMPRoot;
	}

	public class STFAnimationExporter : ISTFResourceExporter
	{
		public string ConvertPropertyPath(string UnityProperty)
		{
			throw new NotImplementedException();
		}

		public string SerializeToJson(ISTFExportState State, UnityEngine.Object Resource)
		{
			var clip = (AnimationClip)Resource;
			var ret = new JObject{
				{"type", STFAnimation._TYPE},
				{"name", clip.name},
				{"fps", clip.frameRate},
			};
			switch(clip.wrapMode)
			{
				case WrapMode.Loop: ret.Add("loop_type", "cycle"); break;
				case WrapMode.PingPong: ret.Add("loop_type", "pingpong"); break;
				default: ret.Add("loop_type", "none"); break;
			}
			var curvesJson = new JArray();
			ret.Add("tracks", curvesJson);

			var (_, meta, fileName) = State.LoadAsset<STFAnimation>(clip);
			
			State.AddTask(new Task(() => {
				curvesJson.Merge(convertCurves(State, AnimationUtility.GetCurveBindings(clip), clip, meta.TMPRoot));
				curvesJson.Merge(convertCurves(State, AnimationUtility.GetObjectReferenceCurveBindings(clip), clip, meta.TMPRoot));
			}));

			ret.Add("name", meta != null ? meta.Name : Path.GetFileNameWithoutExtension(fileName));
			return State.AddResource(Resource, ret, meta ? meta.Id : Guid.NewGuid().ToString());
		}

		protected static JArray convertCurves(ISTFExportState State, EditorCurveBinding[] bindings, AnimationClip clip, GameObject root)
		{
			var curvesJson = new JArray();

			foreach(var c in bindings)
			{
				var curveJson = new JObject();
				curvesJson.Add(curveJson);

				var curveTarget = AnimationUtility.GetAnimatedObject(root, c);
				if(curveTarget.GetType() == typeof(GameObject) || curveTarget.GetType() == typeof(Transform))
				{
					// get components instead and select the one with the lowest prefab hirarchy
					var stfNode = curveTarget.GetType() == typeof(GameObject) ? ((GameObject)curveTarget).GetComponent<STFNode>() : ((Transform)curveTarget).GetComponent<STFNode>();
					
					curveJson.Add("target_id", stfNode.Id);
					curveJson.Add("property", State.Context.NodeExporters[stfNode.Type].ConvertPropertyPath(c.propertyName));
				}
				// TODO: move curve data into a binary buffer

				if(!c.isDiscreteCurve && !c.isPPtrCurve) curveJson.Add("type", "interpolated");
				else if(c.isDiscreteCurve && !c.isPPtrCurve) curveJson.Add("type", "discrete");
				else if(c.isPPtrCurve) curveJson.Add("type", "reference");

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
					/*var keyframes = AnimationUtility.GetObjectReferenceCurve(clip, c);
					foreach(var keyframe in keyframes)
					{
						var id = keyframe.value is GameObject || keyframe.value is Transform ?
								state.GetNodeId(keyframe.value is GameObject ? (GameObject)keyframe.value : ((Transform)keyframe.value).gameObject) : state.GetResourceId(keyframe.value);
						keysJson.Add(new JObject() {{"time", keyframe.time}, {"value", id}});
					}*/
				}
			}
			return curvesJson;
		}
	}

	public class STFAnimationImporter : ISTFResourceImporter
	{

		public string ConvertPropertyPath(string STFProperty)
		{
			throw new NotImplementedException();
		}

		public void ParseFromJson(ISTFImportState State, JObject Json, string Id)
		{
			var meta = ScriptableObject.CreateInstance<STFAnimation>();
			meta.Id = Id;
			meta.Name = (string)Json["name"];
			var ret = new AnimationClip();
			ret.name = (string)Json["name"];
			ret.frameRate = (float)Json["fps"];
			switch((string)Json["loop_type"])
			{
				case "cycle": ret.wrapMode = WrapMode.Loop; break;
				case "pingpong": ret.wrapMode = WrapMode.PingPong; break;
				default: ret.wrapMode = WrapMode.Once; break;
			}

			if(Json["tracks"] != null) foreach(JObject track in Json["tracks"])
			{
				State.AddTask(new Task(() => {
					try
					{
						var target_id = (string)track["target_id"];
						var property = (string)track["property"];
						if(target_id == null || String.IsNullOrWhiteSpace(target_id)) throw new Exception("Target id for animation is null!");

						/*var targetNode = state.GetNode(target_id);
						var targetComponent = state.GetComponent(target_id);
						var targetResource = state.GetResource(target_id);*/
						int targetObjectType = -1;
						string targetType = null;
						if(((JObject)State.JsonRoot["nodes"]).ContainsKey(target_id))
						{
							targetObjectType = 0;
							targetType = (string)State.JsonRoot["nodes"][target_id]["type"];
						}

						if(track["keys"] == null) throw new Exception("Animation track must have keys!");

						// add keys depending on curve type
						if((string)track["type"] != "reference")
						{
							var curve = new AnimationCurve();
							foreach(JObject key in track["keys"])
							{
								curve.AddKey((float)key["time"], (float)key["value"]);
							}
							if(targetObjectType == 0) // node
							{
								var translatedProperty = State.Context.NodeImporters[targetType].ConvertPropertyPath(property);
								
								var unityType = (property.StartsWith("translation") || property.StartsWith("rotation") || property.StartsWith("scale")) ? typeof(Transform) : typeof(GameObject);
								ret.SetCurve("STF_NODE:" + target_id, unityType, translatedProperty, curve);
							}
							/*else if(targetObjectType == 1) // component
							{
								if(!state.GetContext().AnimationTranslators.ContainsKey(targetComponent.GetType()))
									throw new Exception("Property can't be translated: " + property);
								var translatedProperty = state.GetContext().AnimationTranslators[targetComponent.GetType()].ToUnity(property);
								var goId = targetComponent.gameObject.GetComponent<STFUUID>().id;
								ret.SetCurve("STF_COMPONENT:" + goId + ":" + target_id, targetComponent.GetType(), translatedProperty, curve);
							}
							else if(targetResource != null) // resource
							{
								if(!state.GetContext().AnimationTranslators.ContainsKey(targetResource.GetType()))
									throw new Exception("Property can't be translated: " + property);
								var translatedProperty = state.GetContext().AnimationTranslators[targetResource.GetType()].ToUnity(property);
								ret.SetCurve("STF_RESOURCE:" + target_id, targetResource.GetType(), translatedProperty, curve);
							}*/
							// resource component
							else
							{
								throw new Exception("Target id for animation is invalid");
							}
						}
						/*else
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
						}*/
					}
					catch(Exception e)
					{
						Debug.LogWarning(e);
					}
				}));
			}
			return;
		}
	}
}
