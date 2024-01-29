
using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Threading.Tasks;
using UnityEditor;
using System.Linq;
using static STF.Serialisation.STFConstants;
using System.Collections.Generic;
using STF.Util;
using STF.ApplicationConversion;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Engines;

namespace STF.Serialisation
{
	public class STFAnimation : ISTFResource
	{
		public const string _TYPE = "STF.animation";
	}

	public class STFAnimationExporter : ISTFResourceExporter
	{
		public string ConvertPropertyPath(ISTFExportState State, UnityEngine.Object Resource, string UnityProperty)
		{
			throw new NotImplementedException();
		}

		public string SerializeToJson(ISTFExportState State, UnityEngine.Object Resource, UnityEngine.Object Context = null)
		{
			if(!(Context is GameObject)) throw new Exception("AnimationClip requires Context!");

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

			var meta = State.LoadMeta<STFAnimation>(clip);
			
			State.AddTask(new Task(() => {
				curvesJson.Merge(convertCurves(State, AnimationUtility.GetCurveBindings(clip), clip, (GameObject)Context));
				curvesJson.Merge(convertCurves(State, AnimationUtility.GetObjectReferenceCurveBindings(clip), clip, (GameObject)Context));
			}));
			return State.AddResource(Resource, ret, meta ? meta.Id : Guid.NewGuid().ToString());
		}

		protected static JArray convertCurves(ISTFExportState State, EditorCurveBinding[] bindings, AnimationClip clip, GameObject root)
		{
			var curvesJson = new JArray();

			foreach(var c in bindings)
			{
				var curveJson = new JObject();
				curvesJson.Add(curveJson);

				if(c.type == typeof(GameObject) || c.type == typeof(Transform))
				{
					curveJson.Add("target_object_type", "node");
					if(c.path.StartsWith("STF_NODE"))
					{
						curveJson.Add("node_ids", new JArray(c.path.Split(':').Skip(1).ToArray()));
						curveJson.Add("property", c.propertyName);
					}
					else
					{
						var curveTarget = AnimationUtility.GetAnimatedObject(root, c);
						var nodes = curveTarget is GameObject ? ((GameObject)curveTarget).GetComponents<ISTFNode>() : ((Transform)curveTarget).GetComponents<ISTFNode>();
						nodes = nodes.OrderBy(k => k.PrefabHirarchy).ToArray();
						var stfNode = nodes.FirstOrDefault();
						
						curveJson.Add("node_ids", new JArray(nodes.Select(n => n.Id)));
						curveJson.Add("property", State.Context.NodeExporters[stfNode.Type].ConvertPropertyPath(c.propertyName));
					}
				}
				else if(c.type.IsSubclassOf(typeof(Component)))
				{
					curveJson.Add("target_object_type", "node_component");
					if(c.path.StartsWith("STF_NODE_COMPONENT"))
					{
						var pathSplit = c.path.Split(':').Skip(1).ToArray();
						var componentId = pathSplit[pathSplit.Count() -1];
						string[] nodeIds = new string[pathSplit.Count() -1];
						for(int i = 0; i < nodeIds.Count(); i ++) nodeIds[i] = pathSplit[i];
						curveJson.Add("node_ids", new JArray(nodeIds));
						curveJson.Add("component_id", componentId);
						curveJson.Add("property", c.propertyName);
					}
					else
					{
						var curveTarget = (Component)AnimationUtility.GetAnimatedObject(root, c);
						var nodes = curveTarget.gameObject.GetComponents<ISTFNode>().OrderBy(k => k.PrefabHirarchy).ToArray();
						curveJson.Add("node_ids", new JArray(nodes.Select(n => n.Id)));
						if(!c.type.IsSubclassOf(typeof(ISTFNodeComponent)))
						{
							var stfOwner = curveTarget.gameObject.GetComponents<ISTFNodeComponent>()?.FirstOrDefault(nc => nc.OwnedUnityComponent == curveTarget);
							if(stfOwner != null) curveJson.Add("component_id", stfOwner.Id);
							else curveJson.Add("component_id", State.Components[curveTarget].Id);
						}
						else
						{
							curveJson.Add("component_id", ((ISTFNodeComponent)curveTarget).Id);
						}
						curveJson.Add("property", State.Context.NodeComponentExporters[c.type].ConvertPropertyPath(State, curveTarget, c.propertyName));
					}
				}

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
						// TODO: also set ease in/out vectors and such, dont be like gltf
						keysJson.Add(new JObject() {{"time", keyframe.time}, {"value", keyframe.value}});
					}
				}
				else
				{
					var keyframes = AnimationUtility.GetObjectReferenceCurve(clip, c);
					foreach(var keyframe in keyframes)
					{
						if(keyframe.value.GetType().IsSubclassOf(typeof(ISTFResourceComponent)))
						{
							var resourceComponentId = State.ResourceComponents[(ISTFResourceComponent)keyframe.value].Id;
							keysJson.Add(new JObject() {{"time", keyframe.time}, {"value", resourceComponentId}});
						}
						else
						{
							var resourceId = State.Resources[keyframe.value].Id;
							keysJson.Add(new JObject() {{"time", keyframe.time}, {"value", resourceId}});
						}
					}
				}
			}
			return curvesJson;
		}
	}

	public class STFAnimationImporter : ISTFResourceImporter
	{
		public string ConvertPropertyPath(ISTFImportState State, UnityEngine.Object Resource, string STFProperty)
		{
			throw new NotImplementedException();
		}

		public void ParseFromJson(ISTFImportState State, JObject Json, string Id)
		{
			var meta = ScriptableObject.CreateInstance<STFAnimation>();
			meta.Id = Id;
			meta.Name = (string)Json["name"];
			var ret = new AnimationClip {
				name = meta.Name,
				frameRate = (float)Json["fps"]
			};
			switch ((string)Json["loop_type"])
			{
				case "cycle": ret.wrapMode = WrapMode.Loop; break;
				case "pingpong": ret.wrapMode = WrapMode.PingPong; break;
				default: ret.wrapMode = WrapMode.Once; break;
			}

			if(Json["tracks"] != null) foreach(JObject track in Json["tracks"])
			{
				State.AddPostprocessTask(new Task(() => {
					try
					{
						GameObject Root = null;
						if(State.PostprocessContext.ContainsKey(ret)) Root = (GameObject)State.PostprocessContext[ret];

						STFObjectType targetObjectType = STFObjectType.Unknown;
						switch((string)track["target_object_type"])
						{
							case "node": targetObjectType = STFObjectType.Node; break;
							case "node_component": targetObjectType = STFObjectType.NodeComponent; break;
						}
						var property = (string)track["property"];

						if(track["keys"] == null) throw new Exception("Animation track must have keys!");

						// add keys depending on curve type
						if((string)track["type"] != "reference")
						{
							var curve = new AnimationCurve();
							foreach(JObject key in track["keys"])
							{
								curve.AddKey((float)key["time"], (float)key["value"]);
							}
							if(targetObjectType == STFObjectType.Node) // node
							{
								var nodeIds = track["node_ids"].ToObject<List<string>>();
								var targetType = (string)State.JsonRoot["nodes"][nodeIds[nodeIds.Count() - 1]]["type"];
								var unityType = (property.StartsWith("translation") || property.StartsWith("rotation") || property.StartsWith("scale")) ? typeof(Transform) : typeof(GameObject);
								try
								{
									var targetNode = Root.GetComponentsInChildren<ISTFNode>().FirstOrDefault(n => n.Id == nodeIds[nodeIds.Count() - 1]);
									var translatedProperty = State.Context.NodeImporters[targetType].ConvertPropertyPath(property);
									var path = Utils.getPath(Root.transform, ((Component)targetNode).transform, true);
									ret.SetCurve(path, unityType, translatedProperty, curve);
								}
								catch(Exception)
								{
									var nodeIdsString = "";
									foreach(var nodeId in nodeIds)
									{
										if(nodeIdsString == "") nodeIdsString = nodeIdsString + nodeId;
										else nodeIdsString = nodeIdsString + ":" + nodeId;
									}
									ret.SetCurve("STF_NODE:" + nodeIdsString, unityType, property, curve);
								}
							}
							else if(targetObjectType == STFObjectType.NodeComponent) // component
							{
								var nodeIds = track["node_ids"].ToObject<List<string>>();
								var componentId = (string)track["component_id"];
								try
								{
									var targetNode = Root.GetComponentsInChildren<ISTFNode>().FirstOrDefault(n => n.Id == nodeIds[nodeIds.Count() - 1]);
									var targetSTFComponent = ((Component)targetNode).GetComponents<ISTFNodeComponent>().FirstOrDefault(nc => nc.Id == componentId);

									var targetComponent =  targetSTFComponent.OwnedUnityComponent != null ? targetSTFComponent.OwnedUnityComponent : targetSTFComponent;
									var translatedProperty = State.Context.NodeComponentImporters[targetSTFComponent.Type].ConvertPropertyPath(State, targetSTFComponent, property);
									
									var path = Utils.getPath(Root.transform, ((Component)targetNode).transform, true);
									ret.SetCurve(path, targetComponent.GetType(), translatedProperty, curve);
								}
								catch(Exception)
								{
									var nodeIdsString = "";
									foreach(var nodeId in nodeIds)
									{
										if(nodeIdsString == "") nodeIdsString = nodeIdsString + nodeId;
										else nodeIdsString = ":" + nodeIdsString + nodeId;
									}
									ret.SetCurve("STF_NODE_COMPONENT:" + nodeIdsString + ":" + componentId, typeof(Component), property, curve);
								}
							}
						}
						else
						{
							throw new Exception("Unhandled Animation Property");
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
						}*/
					}
					catch(Exception e)
					{
						Debug.LogWarning(e);
					}
				}));
			}
			State.SaveResource(ret, "anim", meta, Id);
			return;
		}
	}

	[InitializeOnLoad]
	public class Register_STFAnimation
	{
		static Register_STFAnimation()
		{
			STFRegistry.RegisterResourceImporter(STFAnimation._TYPE, new STFAnimationImporter());
			STFRegistry.RegisterResourceExporter(typeof(AnimationClip), new STFAnimationExporter());
		}
	}

	public class STFAnimationApplicationConverter : ISTFResourceApplicationConverter
	{
		public string ConvertPropertyPath(ISTFApplicationConvertState State, UnityEngine.Object Resource, string STFProperty)
		{
			throw new NotImplementedException();
		}

		public void Convert(ISTFApplicationConvertState State, UnityEngine.Object Resource)
		{
			var clip = Resource as AnimationClip;
			State.AddTask(new Task(() => {
				convertCurves(State, AnimationUtility.GetCurveBindings(clip), clip, (GameObject)State.RegisteredResourcesContext[Resource]);
				convertCurves(State, AnimationUtility.GetObjectReferenceCurveBindings(clip), clip, (GameObject)State.RegisteredResourcesContext[Resource]);
			}));

		}

		protected void convertCurves(ISTFApplicationConvertState State, EditorCurveBinding[] Bindings, AnimationClip Clip, GameObject Root)
		{
			foreach(var c in Bindings)
			{
				if(c.type == typeof(GameObject) || c.type == typeof(Transform))
				{
					var curveTarget = AnimationUtility.GetAnimatedObject(Root, c);
					Debug.Log("curveTarget: " + curveTarget);
				}
				else if(c.type.IsSubclassOf(typeof(Component)))
				{
					var curveTarget = (Component)AnimationUtility.GetAnimatedObject(Root, c);
					Debug.Log("curveTarget: " + curveTarget);
				}
			}
		}
	}
}
