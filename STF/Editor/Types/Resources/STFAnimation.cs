
#if UNITY_EDITOR

using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Threading.Tasks;
using UnityEditor;
using System.Linq;
using STF.Util;
using STF.ApplicationConversion;
using static STF.Util.STFConstants;
using STF.Types;

namespace STF.Serialisation
{
	public class STFAnimation : ISTFResource
	{
		public const string _TYPE = "STF.animation";
		public override string Type => _TYPE;
	}

	public class STFAnimationExporter : ISTFResourceExporter
	{
		public string ConvertPropertyPath(STFExportState State, UnityEngine.Object Resource, string UnityProperty)
		{
			throw new NotImplementedException();
		}

		public string SerializeToJson(STFExportState State, UnityEngine.Object Resource, UnityEngine.Object Context = null)
		{
			if(!(Context is GameObject)) throw new Exception("AnimationClip requires Context!");

			var clip = (AnimationClip)Resource;
			var ret = new JObject{
				{"type", STFAnimation._TYPE},
				{"name", clip.name},
				{"fps", clip.frameRate},
			};
			var rf = new RefSerializer(ret);
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
				curvesJson.Merge(convertCurves(State, AnimationUtility.GetCurveBindings(clip), clip, (GameObject)Context, rf));
				curvesJson.Merge(convertCurves(State, AnimationUtility.GetObjectReferenceCurveBindings(clip), clip, (GameObject)Context, rf));
			}));
			return State.AddResource(Resource, ret, meta ? meta.Id : Guid.NewGuid().ToString());
		}

		protected static JArray convertCurves(STFExportState State, EditorCurveBinding[] bindings, AnimationClip clip, GameObject root, RefSerializer rf)
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
						curveJson.Add("node_id", rf.NodeRef(c.path.Split(':').Skip(1).First()));
						curveJson.Add("property", c.propertyName);
					}
					else
					{
						var curveTarget = AnimationUtility.GetAnimatedObject(root, c);
						var stfNode = curveTarget is GameObject ? Utils.GetNodeComponent((GameObject)curveTarget) : Utils.GetNodeComponent((Transform)curveTarget);
						
						curveJson.Add("node_id", rf.NodeRef(stfNode.Id));
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
						curveJson.Add("node_id", rf.NodeRef(pathSplit[0]));
						curveJson.Add("component_id", rf.NodeComponentRef(componentId));
						curveJson.Add("property", c.propertyName);
					}
					else
					{
						var curveTarget = (Component)AnimationUtility.GetAnimatedObject(root, c);
						var stfNode = Utils.GetNodeComponent(curveTarget.gameObject);
						//var nodes = curveTarget.gameObject.GetComponents<ISTFNode>().OrderByDescending(k => k.PrefabHirarchy).ToArray();
						curveJson.Add("node_id", rf.NodeRef(stfNode.Id));
						if(!c.type.IsSubclassOf(typeof(ISTFNodeComponent)))
						{
							var stfOwner = curveTarget.gameObject.GetComponents<ISTFNodeComponent>()?.FirstOrDefault(nc => nc.OwnedUnityComponent == curveTarget);
							if(stfOwner != null) curveJson.Add("component_id", rf.NodeComponentRef(stfOwner.Id));
							else curveJson.Add("component_id", rf.NodeComponentRef(State.NodeComponents[curveTarget].Id));
						}
						else
						{
							curveJson.Add("component_id", rf.NodeComponentRef(((ISTFNodeComponent)curveTarget).Id));
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
						keysJson.Add(new JObject() {
							{"time", keyframe.time},
							{"value", keyframe.value},
							{"in_tangent", keyframe.inTangent},
							{"in_weight", keyframe.inWeight},
							{"out_tangent", keyframe.outTangent},
							{"out_weight", keyframe.outWeight}
						});
					}
				}
				else
				{
					var keyframes = AnimationUtility.GetObjectReferenceCurve(clip, c);
					foreach(var keyframe in keyframes)
					{
						var resourceId = ExportUtil.SerializeResource(State, keyframe.value);
						keysJson.Add(new JObject() {{"time", keyframe.time}, {"value", resourceId}});
					}
				}
			}
			return curvesJson;
		}
	}

	public class STFAnimationImporter : ISTFResourceImporter
	{
		public string ConvertPropertyPath(STFImportState State, UnityEngine.Object Resource, string STFProperty)
		{
			throw new NotImplementedException();
		}

		public void ParseFromJson(STFImportState State, JObject Json, string Id)
		{
			var meta = ScriptableObject.CreateInstance<STFAnimation>();
			meta.Id = Id;
			meta.STFName = (string)Json["name"];

			var ret = new AnimationClip {
				name = meta.STFName,
				frameRate = (float)Json["fps"]
			};

			var rf = new RefDeserializer(Json);
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
								var keyframe = new Keyframe { time = (float)key["time"], value = (float)key["value"], };
								if(key.ContainsKey("in_tangent") && key.ContainsKey("out_tangent"))
								{
									keyframe.inTangent = (float)key["in_tangent"];
									keyframe.inWeight = (float)key["in_weight"];
									keyframe.outTangent = (float)key["in_tangent"];
									keyframe.outWeight = (float)key["out_weight"];
								}
								curve.AddKey(keyframe);
							}
							if(targetObjectType == STFObjectType.Node) // node
							{
								var nodeId = rf.NodeRef(track["node_id"]);
								var targetType = (string)State.JsonRoot["nodes"][nodeId]["type"];

								// Implement a way for a node implementation to give the proper unity type. If for example mesh-instances are implemented as a node, this will have to happen.
								var unityType = (property.StartsWith("translation") || property.StartsWith("rotation") || property.StartsWith("scale")) ? typeof(Transform) : typeof(GameObject);
								try
								{
									var targetNode = Root.GetComponentsInChildren<ISTFNode>().FirstOrDefault(n => n.Id == nodeId);
									var translatedProperty = State.Context.NodeImporters[targetType].ConvertPropertyPath(property);
									var path = Utils.getPath(Root.transform, targetNode.transform, true);
									ret.SetCurve(path, unityType, translatedProperty, curve);
								}
								catch(Exception)
								{
									ret.SetCurve("STF_NODE:" + nodeId, unityType, property, curve);
								}
							}
							else if(targetObjectType == STFObjectType.NodeComponent) // component
							{
								var nodeId = rf.NodeRef(track["node_ids"]);
								var componentId = rf.NodeComponentRef(track["component_id"]);
								try
								{
									var targetNode = Root.GetComponentsInChildren<ISTFNode>().FirstOrDefault(n => n.Id == nodeId);
									var targetSTFComponent = targetNode.GetComponents<ISTFNodeComponent>().FirstOrDefault(nc => nc.Id == componentId);

									// Implement a way for a node component implementation to give the proper unity type instead of this.
									var targetComponent =  targetSTFComponent.OwnedUnityComponent != null ? targetSTFComponent.OwnedUnityComponent : targetSTFComponent;
									var translatedProperty = State.Context.NodeComponentImporters[targetSTFComponent.Type].ConvertPropertyPath(State, targetSTFComponent, property);
									
									var path = Utils.getPath(Root.transform, targetNode.transform, true);
									ret.SetCurve(path, targetComponent.GetType(), translatedProperty, curve);
								}
								catch(Exception)
								{
									ret.SetCurve("STF_NODE_COMPONENT:" + nodeId + ":" + componentId, typeof(Component), property, curve);
								}
							}
						}
						else
						{
							var keyframes = new ObjectReferenceKeyframe[track["keys"].Count()];
							for(int i = 0; i < keyframes.Length; i++)
							{
								keyframes[i].time = (float)track["keys"][i]["time"];
								keyframes[i].value = State.Resources[(string)track["keys"][i]["value"]];
							}

							if(targetObjectType == STFObjectType.Node) // node
							{
								var nodeId = rf.NodeRef(track["node_id"]);
								var targetType = (string)State.JsonRoot["nodes"][nodeId]["type"];
								var unityType = (property.StartsWith("translation") || property.StartsWith("rotation") || property.StartsWith("scale")) ? typeof(Transform) : typeof(GameObject);
								try
								{
									var targetNode = Root.GetComponentsInChildren<ISTFNode>().FirstOrDefault(n => n.Id == nodeId);
									var translatedProperty = State.Context.NodeImporters[targetType].ConvertPropertyPath(property);
									var path = Utils.getPath(Root.transform, targetNode.transform, true);

									var newBinding = new EditorCurveBinding {
										path = path,
										propertyName = translatedProperty,
										type = unityType
									};
									AnimationUtility.SetObjectReferenceCurve(ret, newBinding, keyframes);
								}
								catch(Exception)
								{
									var newBinding = new EditorCurveBinding {
										path = "STF_NODE:" + nodeId,
										propertyName = property,
										type = unityType
									};
									AnimationUtility.SetObjectReferenceCurve(ret, newBinding, keyframes);
								}
							}
							else if(targetObjectType == STFObjectType.NodeComponent) // node component
							{
								var nodeId = rf.NodeRef(track["node_ids"]);
								var componentId = rf.NodeComponentRef(track["component_id"]);
								try
								{
									var targetNode = Root.GetComponentsInChildren<ISTFNode>().FirstOrDefault(n => n.Id == nodeId);
									var targetSTFComponent = targetNode.GetComponents<ISTFNodeComponent>().FirstOrDefault(nc => nc.Id == componentId);

									var targetComponent =  targetSTFComponent.OwnedUnityComponent != null ? targetSTFComponent.OwnedUnityComponent : targetSTFComponent;
									var translatedProperty = State.Context.NodeComponentImporters[targetSTFComponent.Type].ConvertPropertyPath(State, targetSTFComponent, property);
									
									var path = Utils.getPath(Root.transform, targetNode.transform, true);

									var newBinding = new EditorCurveBinding {
										path = path,
										propertyName = translatedProperty,
										type = targetComponent.GetType()
									};
									AnimationUtility.SetObjectReferenceCurve(ret, newBinding, keyframes);
								}
								catch(Exception)
								{
									var newBinding = new EditorCurveBinding {
										path = "STF_NODE_COMPONENT:" + nodeId + ":" + componentId,
										propertyName = property,
										type = typeof(Component)
									};
									AnimationUtility.SetObjectReferenceCurve(ret, newBinding, keyframes);
								}
							}
						}
					}
					catch(Exception e)
					{
						Debug.LogWarning(e);
					}
				}));
			}
			State.UnityContext.SaveGeneratedResource(ret, "anim");
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
			var clip = State.DuplicateResource(Resource) as AnimationClip;
			State.AddTask(new Task(() => {
				convertCurves(State, AnimationUtility.GetCurveBindings(clip), clip, (GameObject)State.RegisteredResourcesContext[Resource]);
				convertCurves(State, AnimationUtility.GetObjectReferenceCurveBindings(clip), clip, (GameObject)State.RegisteredResourcesContext[Resource]);
			}));
		}

		protected void convertCurves(ISTFApplicationConvertState State, EditorCurveBinding[] Bindings, AnimationClip Clip, GameObject Root)
		{
			for(int i = 0; i < Bindings.Count(); i++)
			{
				var editorCurve = AnimationUtility.GetEditorCurve(Clip, Bindings[i]);
				AnimationUtility.SetEditorCurve(Clip, Bindings[i], null);
				if(Bindings[i].type == typeof(GameObject) || Bindings[i].type == typeof(Transform))
				{
					var curveTarget = AnimationUtility.GetAnimatedObject(Root, Bindings[i]);
				}
				else if(Bindings[i].type.IsSubclassOf(typeof(Component)))
				{
					var curveTarget = (Component)AnimationUtility.GetAnimatedObject(Root, Bindings[i]);

					if(State.ConverterContext.NodeComponent.ContainsKey(curveTarget.GetType()))
					{
						var translated = State.ConverterContext.NodeComponent[curveTarget.GetType()].ConvertPropertyPath(State, curveTarget, Bindings[i].propertyName);
						Bindings[i].propertyName = translated;
					}
				}
				AnimationUtility.SetEditorCurve(Clip, Bindings[i], editorCurve);
			}
		}

		protected void convertRefCurves(ISTFApplicationConvertState State, EditorCurveBinding[] Bindings, AnimationClip Clip, GameObject Root)
		{
			for(int i = 0; i < Bindings.Count(); i++)
			{
				var editorCurve = AnimationUtility.GetObjectReferenceCurve(Clip, Bindings[i]);
				AnimationUtility.SetObjectReferenceCurve(Clip, Bindings[i], null);
				// TODO actually implement object reference curves
				if(Bindings[i].type == typeof(GameObject) || Bindings[i].type == typeof(Transform))
				{
					var curveTarget = AnimationUtility.GetAnimatedObject(Root, Bindings[i]);
				}
				else if(Bindings[i].type.IsSubclassOf(typeof(Component)))
				{
					var curveTarget = (Component)AnimationUtility.GetAnimatedObject(Root, Bindings[i]);

					if(State.ConverterContext.NodeComponent.ContainsKey(curveTarget.GetType()))
					{
						var translated = State.ConverterContext.NodeComponent[curveTarget.GetType()].ConvertPropertyPath(State, curveTarget, Bindings[i].propertyName);
						Bindings[i].propertyName = translated;
					}
				}
				AnimationUtility.SetObjectReferenceCurve(Clip, Bindings[i], editorCurve);
			}
		}
	}
}

#endif
