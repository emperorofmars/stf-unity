using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Animations;

namespace STF.Serde
{
	public class STFTwistConstraintBack : ASTFNodeComponent
	{
		public static string _TYPE = "STF.constraint.twist_back";
		public override string Type => _TYPE;
		public float weight = 0.5f;
	}

	public class STFTwistConstraintBackImporter : ASTFNodeComponentImporter
	{
		public override string ConvertPropertyPath(string STFProperty)
		{
			throw new NotImplementedException();
		}

		public override void ParseFromJson(ISTFAssetImportState State, JObject Json, string Id, GameObject Go)
		{
			var c = Go.AddComponent<STFTwistConstraintBack>();
			ParseRelationships(Json, c);
			c.Id = Id;
			c.weight = (float)Json["weight"];
			State.AddComponent(c, Id);
		}
	}

	public class STFTwistConstraintBackExporter : ASTFNodeComponentExporter
	{
		public override string ConvertPropertyPath(string UnityProperty)
		{
			throw new NotImplementedException();
		}

		public override (string, JObject) SerializeToJson(ISTFExportState State, Component Component)
		{
			STFTwistConstraintBack c = (STFTwistConstraintBack)Component;
			var ret = new JObject {
				{"type", STFTwistConstraintBack._TYPE},
				{"weight", c.weight}
			};
			SerializeRelationships(c, ret);
			return (c.Id, ret);
		}
	}
}