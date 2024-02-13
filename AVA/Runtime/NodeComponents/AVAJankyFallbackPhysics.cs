using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Threading.Tasks;
using STF.Serialisation;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AVA.Serialisation
{
	// absolute jank, only for proof of concept purposes
	public class AVAJankyFallbackPhysics : ISTFNodeComponent
	{
		public static string _TYPE = "AVA.janky_fallback_physics";
		public override string Type => _TYPE;
		public GameObject target;
		public string targetId;
		public float pull = 0.2f;
		public float spring = 0.2f;
		public float stiffness = 0.2f;
	}

	public class AVAJankyFallbackPhysicsImporter : ASTFNodeComponentImporter
	{
		public override string ConvertPropertyPath(ISTFImportState State, Component Component, string STFProperty)
		{
			throw new NotImplementedException();
		}

		public override void ParseFromJson(ISTFAssetImportState State, JObject Json, string Id, GameObject Go)
		{
			var c = Go.AddComponent<AVAJankyFallbackPhysics>();
			c.Id = Id;
			ParseRelationships(Json, c);
			c.Extends = Json["extends"]?.ToObject<List<string>>();

			if(State.Nodes.ContainsKey((string)Json["target"])) c.target = State.Nodes[(string)Json["target"]];
			c.targetId = (string)Json["target"];
			c.pull = (float)Json["pull"];
			c.spring = (float)Json["spring"];
			c.stiffness = (float)Json["stiffness"];
			
			State.AddComponent(c, Id);
		}
	}

	public class AVAJankyFallbackPhysicsExporter : ASTFNodeComponentExporter
	{
		public override string ConvertPropertyPath(ISTFExportState State, Component Component, string UnityProperty)
		{
			throw new NotImplementedException();
		}

		public override (string Id, JObject JsonComponent) SerializeToJson(ISTFExportState State, Component Component)
		{
			var c = (AVAJankyFallbackPhysics)Component;
			var ret = new JObject();
			SerializeRelationships(c, ret);
			ret.Add("type", AVAJankyFallbackPhysics._TYPE);
			ret.Add("target", c.target != null ? c.target?.GetComponent<ASTFNode>()?.Id : c.targetId);
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
}