using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Linq;

#if STF
using STF.Util;
using STF.Types;
using STF.Serialisation;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AVA.Types
{
	// absolute jank, only for proof of concept purposes
	[DisallowMultipleComponent]
	public class AVAJankyFallbackPhysics : ISTFNodeComponent
	{
		public static string _TYPE = "AVA.janky_fallback_physics";
		public override string Type => _TYPE;
		public NodeReference target = new();
		public string targetId;
		public float pull = 0.2f;
		public float spring = 0.2f;
		public float stiffness = 0.2f;
	}

#if STF
	public class AVAJankyFallbackPhysicsImporter : ASTFNodeComponentImporter
	{
		public override string ConvertPropertyPath(STFImportState State, Component Component, string STFProperty)
		{
			throw new NotImplementedException();
		}

		public override void ParseFromJson(STFImportState State, JObject Json, string Id, GameObject Go)
		{
			var c = Go.AddComponent<AVAJankyFallbackPhysics>();
			c.Id = Id;
			ParseRelationships(Json, c);
			c.Extends = Json["extends"]?.ToObject<List<string>>();
			var rf = new RefDeserializer(Json);

			if(State.Nodes.ContainsKey((string)Json["target"])) c.target = State.Nodes[rf.NodeRef(Json["target"])];
			c.targetId = (string)Json["target"];
			c.pull = (float)Json["pull"];
			c.spring = (float)Json["spring"];
			c.stiffness = (float)Json["stiffness"];
			
			State.AddNodeComponent(c);
		}
	}

	public class AVAJankyFallbackPhysicsExporter : ASTFNodeComponentExporter
	{
		public override string ConvertPropertyPath(STFExportState State, Component Component, string UnityProperty)
		{
			throw new NotImplementedException();
		}

		public override (string Id, JObject JsonComponent) SerializeToJson(STFExportState State, Component Component)
		{
			var c = (AVAJankyFallbackPhysics)Component;
			var ret = new JObject();
			var rf = new RefSerializer(ret);
			SerializeRelationships(c, ret);
			ret.Add("type", AVAJankyFallbackPhysics._TYPE);
			ret.Add("target", c.target != null ? rf.NodeRef(c.target.Node.GetComponents<ISTFNode>().OrderByDescending(c => c.PrefabHirarchy).FirstOrDefault().Id) : rf.NodeRef(c.targetId));
			ret.Add("pull", c.pull);
			ret.Add("spring", c.spring);
			ret.Add("stiffness", c.stiffness);

			// used nodes

			return (c.Id, ret);
		}
	}

#if UNITY_EDITOR
	[InitializeOnLoad]
	public class Register_AVAJankyFallbackPhysics
	{
		static Register_AVAJankyFallbackPhysics()
		{
			STFRegistry.RegisterNodeComponentImporter(AVAJankyFallbackPhysics._TYPE, new AVAJankyFallbackPhysicsImporter());
			STFRegistry.RegisterNodeComponentExporter(typeof(AVAJankyFallbackPhysics), new AVAJankyFallbackPhysicsExporter());
		}
	}
#endif
#endif
}