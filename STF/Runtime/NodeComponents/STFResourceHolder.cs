using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace STF.Serialisation
{
	public class STFResourceHolder : ASTFNodeComponent
	{
		public const string _TYPE = "STF.resource_holder";
		public override string Type => _TYPE;

		public List<Object> Resources = new List<Object>();
	}

	public class STFResourceHolderExporter : ASTFNodeComponentExporter
	{
		public override string ConvertPropertyPath(string UnityProperty)
		{
			throw new System.NotImplementedException();
		}

		public override (string, JObject) SerializeToJson(ISTFExportState State, Component Component)
		{
			var c = (STFResourceHolder)Component;
			var ret = new JObject {
				{"type", c.Type},
				{"resources_used", new JArray(c.Resources.Select(r => SerdeUtil.SerializeResource(State, r, r is AnimationClip ? c.gameObject : null)).ToList())},
			};
			SerializeRelationships(c, ret);
			return (c.Id, ret);
		}
	}

	public class STFResourceHolderImporter : ASTFNodeComponentImporter
	{
		public override string ConvertPropertyPath(string STFProperty)
		{
			throw new System.NotImplementedException();
		}

		public override void ParseFromJson(ISTFAssetImportState State, JObject Json, string Id, GameObject Go)
		{
			var c = Go.AddComponent<STFResourceHolder>();
			ParseRelationships(Json, c);
			c.Id = Id;
			foreach(string r in Json["resources_used"])
			{
				var resource = State.Resources[r];
				c.Resources.Add(resource is ISTFResource ? ((ISTFResource)resource).Resource : resource);
			}
		}
	}
}