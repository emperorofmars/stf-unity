using Newtonsoft.Json.Linq;
using UnityEngine;
using STF.Serialisation;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AVA.Serialisation
{
	public class AVAEyeBoneLimitsSimple : ISTFNodeComponent
	{
		public const string _TYPE = "AVA.eye_bone_limits_simple";
		public override string Type => _TYPE;
		public float up = 15;
		public float down = 12;
		public float inner = 15;
		public float outer = 18;
	}

	public class AVAEyeBoneLimitsSimpleExporter : ASTFNodeComponentExporter
	{
		public override string ConvertPropertyPath(STFExportState State, Component Component, string UnityProperty)
		{
			throw new System.NotImplementedException();
		}

		public override (string Id, JObject JsonComponent) SerializeToJson(STFExportState State, Component Component)
		{
			var c = (AVAEyeBoneLimitsSimple)Component;
			var ret = new JObject {
				{ "type", AVAEyeBoneLimitsSimple._TYPE },
				{ "up", c.up },
				{ "down", c.down },
				{ "inner", c.inner },
				{ "outer", c.outer }
			};
			SerializeRelationships(c, ret);
			return (c.Id, ret);
		}
	}

	public class AVAEyeBoneLimitsSimpleImporter : ASTFNodeComponentImporter
	{
		public override string ConvertPropertyPath(STFImportState State, Component Component, string STFProperty)
		{
			throw new System.NotImplementedException();
		}

		public override void ParseFromJson(STFImportState State, JObject Json, string Id, GameObject Go)
		{
			var c = Go.AddComponent<AVAEyeBoneLimitsSimple>();
			State.AddNodeComponent(c, Id);
			c.Id = Id;
			ParseRelationships(Json, c);

			c.up = (float)Json["up"];
			c.down = (float)Json["down"];
			c.inner = (float)Json["inner"];
			c.outer = (float)Json["outer"];
		}
	}

#if UNITY_EDITOR
	[InitializeOnLoad]
	public class Register_AVAEyeBoneLimitsSimple
	{
		static Register_AVAEyeBoneLimitsSimple()
		{
			STFRegistry.RegisterNodeComponentImporter(AVAEyeBoneLimitsSimple._TYPE, new AVAEyeBoneLimitsSimpleImporter());
			STFRegistry.RegisterNodeComponentExporter(typeof(AVAEyeBoneLimitsSimple), new AVAEyeBoneLimitsSimpleExporter());
		}
	}
#endif
}