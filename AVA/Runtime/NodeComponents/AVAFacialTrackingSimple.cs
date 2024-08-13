using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Threading.Tasks;
using STF.Serialisation;
using STF.Util;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AVA.Serialisation
{
	public class AVAFacialTrackingSimple : ISTFNodeComponent
	{
		public const string _TYPE = "AVA.facial_tracking_simple";
		public override string Type => _TYPE;

		[Serializable]
		public class BlendshapeMapping
		{
			public string VisemeName;
			public string BlendshapeName;
		}
		public static readonly List<string> VoiceVisemes15 = new List<string> {
			"sil", "aa", "ch", "dd", "e", "ff", "ih", "kk", "nn", "oh", "ou", "pp", "rr", "ss", "th"
		};

		public static readonly List<string> FacialExpressions = new List<string> {
			"eye_closed", "look_up", "look_down" // add all the facial tracking ones
		};

		public STFMeshInstance TargetMeshInstance;
		public List<BlendshapeMapping> Mappings = new List<BlendshapeMapping>();

		public void Map()
		{
			var renderer = TargetMeshInstance.GetComponent<Renderer>();
			if(TargetMeshInstance == null || renderer == null) throw new Exception("Meshinstance must be mapped!");

			Mesh mesh = renderer is SkinnedMeshRenderer ? (renderer as SkinnedMeshRenderer).sharedMesh : TargetMeshInstance.GetComponent<MeshFilter>().sharedMesh;

			Mappings = new List<BlendshapeMapping>();
			foreach(var v in VoiceVisemes15)
			{
				string match = null;
				for(int i = 0; i < mesh.blendShapeCount; i++)
				{
					var bName = mesh.GetBlendShapeName(i);
					if(bName.ToLower().Contains("vrc." + v)) { match = bName; break; }
					else if(bName.ToLower().Contains("vrc.v_" + v)) { match = bName; break; }
					else if(bName.ToLower().Contains("vis." + v)) { match = bName; break; }
					else if(bName.ToLower().Contains("vis_" + v)) { match = bName; break; }
				}
				Mappings.Add(new BlendshapeMapping{VisemeName = v, BlendshapeName = match});
			}
			foreach(var v in FacialExpressions)
			{
				var compare = v.Split('_');
				string match = null;
				for(int i = 0; i < mesh.blendShapeCount; i++)
				{
					var bName = mesh.GetBlendShapeName(i);
					bool matchedAll = true;
					foreach(var c in compare)
					{
						if(!bName.ToLower().Contains(c)) { matchedAll = false; break; }
					}
					if(matchedAll && (match == null || bName.Length < match.Length)) match = bName;
				}
				Mappings.Add(new BlendshapeMapping{VisemeName = v, BlendshapeName = match});
			}
		}
	}

	public class AVAFacialTrackingSimpleExporter : ASTFNodeComponentExporter
	{
		public override string ConvertPropertyPath(STFExportState State, Component Component, string UnityProperty)
		{
			throw new System.NotImplementedException();
		}

		public override (string Id, JObject JsonComponent) SerializeToJson(STFExportState State, Component Component)
		{
			var c = (AVAFacialTrackingSimple)Component;
			var ret = new JObject {
				{"type", AVAFacialTrackingSimple._TYPE},
			};
			var rf = new RefSerializer(ret);
			if(c.TargetMeshInstance) ret.Add("target_mesh_instance", rf.NodeComponentRef(c.TargetMeshInstance.GetComponent<STFMeshInstance>().Id));
			SerializeRelationships(c, ret);
			foreach(var m in c.Mappings)
			{
				ret.Add(m.VisemeName, m.BlendshapeName);
			}
			return (c.Id, ret);
		}
	}

	public class AVAFacialTrackingSimpleImporter : ASTFNodeComponentImporter
	{
		public override string ConvertPropertyPath(STFImportState State, Component Component, string STFProperty)
		{
			throw new NotImplementedException();
		}

		override public void ParseFromJson(STFImportState State, JObject Json, string Id, GameObject Go)
		{
			var c = Go.AddComponent<AVAFacialTrackingSimple>();
			State.AddNodeComponent(c, Id);
			c.Id = Id;
			ParseRelationships(Json, c);

			var rf = new RefDeserializer(Json);

			State.AddTask(new Task(() => {
				if(Json["target_mesh_instance"] != null) c.TargetMeshInstance = (STFMeshInstance)State.NodeComponents[rf.NodeComponentRef(Json["target_mesh_instance"])];
			}));
			foreach(var vis in AVAFacialTrackingSimple.VoiceVisemes15)
			{
				c.Mappings.Add(new AVAFacialTrackingSimple.BlendshapeMapping {VisemeName = vis, BlendshapeName = (string)Json[vis]});
			}
			foreach(var vis in AVAFacialTrackingSimple.FacialExpressions)
			{
				c.Mappings.Add(new AVAFacialTrackingSimple.BlendshapeMapping {VisemeName = vis, BlendshapeName = (string)Json[vis]});
			}
		}
	}

#if UNITY_EDITOR
	[InitializeOnLoad]
	public class Register_AVAFacialTrackingSimple
	{
		static Register_AVAFacialTrackingSimple()
		{
			STFRegistry.RegisterNodeComponentImporter(AVAFacialTrackingSimple._TYPE, new AVAFacialTrackingSimpleImporter());
			STFRegistry.RegisterNodeComponentExporter(typeof(AVAFacialTrackingSimple), new AVAFacialTrackingSimpleExporter());
		}
	}
#endif
}