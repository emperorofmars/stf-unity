using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using STF.Serialisation;
using System.Linq;
using STF.Util;
using STF.Types;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AVA.Types
{
	// partial implementation, to be completed whenever, currently exists only for proof of concept purposes
	[DisallowMultipleComponent]
	public class AVAVRCPhysbones : ISTFNodeComponent
	{
		public static string _TYPE = "AVA.VRC.physbones";
		public override string Type => _TYPE;
		public NodeReference target = new();
		public string targetId;
		public string version = "1.1";
		public string integration_type = "simplified";
		public float pull = 0.2f; // TODO: support curves for each appropriate parameter
		public float stiffness = 0.2f;
		public float spring = 0.2f;
		public float gravity;
		public float gravity_falloff;
		public string immobile_type = "all_motion";
		public float immobile;
	}

	public class AVAVRCPhysbonesImporter : ASTFNodeComponentImporter
	{
		public override string ConvertPropertyPath(STFImportState State, Component Component, string STFProperty)
		{
			throw new NotImplementedException();
		}

		public override void ParseFromJson(STFImportState State, JObject Json, string Id, GameObject Go)
		{
			var c = Go.AddComponent<AVAVRCPhysbones>();
			c.Id = Id;
			ParseRelationships(Json, c);
			var rf = new RefDeserializer(Json);

			if(State.Nodes.ContainsKey((string)Json["target"])) c.target = State.Nodes[rf.NodeRef(Json["target"])];
			c.targetId = (string)Json["target"];

			c.version = (string)Json["version"];
			c.integration_type = (string)Json["integration_type"];
			c.pull = (float)Json["pull"];
			c.stiffness = (float)Json["stiffness"];
			c.spring = (float)Json["spring"];
			c.gravity = (float)Json["gravity"];
			c.gravity_falloff = (float)Json["gravity_falloff"];
			c.immobile_type = (string)Json["immobile_type"];
			c.immobile = (float)Json["immobile"];

			State.AddNodeComponent(c);
		}
	}

	public class AVAVRCPhysbonesExporter : ASTFNodeComponentExporter
	{
		public override string ConvertPropertyPath(STFExportState State, Component Component, string UnityProperty)
		{
			throw new NotImplementedException();
		}

		public override (string Id, JObject JsonComponent) SerializeToJson(STFExportState State, Component Component)
		{
			var c = (AVAVRCPhysbones)Component;
			var ret = new JObject();
			SerializeRelationships(c, ret);
			var rf = new RefSerializer(ret);
			ret.Add("type", AVAVRCPhysbones._TYPE);
			ret.Add("target", c.target != null ? rf.NodeRef(c.target.Node.GetComponents<ISTFNode>().OrderByDescending(c => c.PrefabHirarchy).FirstOrDefault().Id) : rf.NodeRef(c.targetId));
			ret.Add("version", c.version);
			ret.Add("integration_type", c.integration_type);
			ret.Add("pull", c.pull);
			ret.Add("stiffness", c.stiffness);
			ret.Add("spring", c.spring);
			ret.Add("gravity", c.gravity);
			ret.Add("gravity_falloff", c.gravity_falloff);
			ret.Add("immobile_type", c.immobile_type);
			ret.Add("immobile", c.immobile);
			
			// used nodes

			return (c.Id, ret);
		}
	}

#if UNITY_EDITOR
	[InitializeOnLoad]
	public class Register_AVAVRCPhysbones
	{
		static Register_AVAVRCPhysbones()
		{
			STFRegistry.RegisterNodeComponentImporter(AVAVRCPhysbones._TYPE, new AVAVRCPhysbonesImporter());
			STFRegistry.RegisterNodeComponentExporter(typeof(AVAVRCPhysbones), new AVAVRCPhysbonesExporter());
		}
	}
#endif
}