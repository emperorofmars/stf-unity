using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using STF.ApplicationConversion;
using UnityEngine;

namespace STF.Serialisation
{
	public class STFResourceHolder : ISTFNodeComponent
	{
		public const string _TYPE = "STF.resource_holder";
		public override string Type => _TYPE;

		public List<Object> Resources = new List<Object>();
	}

	public class STFResourceHolderExporter : ASTFNodeComponentExporter
	{
		public override string ConvertPropertyPath(ISTFExportState State, Component Component, string UnityProperty)
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
		public override string ConvertPropertyPath(ISTFImportState State, Component Component, string STFProperty)
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
				if(resource is AnimationClip) State.SetPostprocessContext(resource, Go);
				else if(resource is ISTFResource && ((ISTFResource)resource).Resource is AnimationClip) State.SetPostprocessContext(((ISTFResource)resource).Resource, Go);
			}
		}
	}

	public class STFResourceHolderApplicationConverter : ISTFNodeComponentApplicationConverter
	{
		public void ConvertResources(ISTFApplicationConvertState State, Component Component)
		{
			foreach(var r in (Component as STFResourceHolder).Resources)
			{
				State.RegisterResource(r);
			}
		}

		public void Convert(ISTFApplicationConvertState State, Component Component)
		{
			List<Object> newResources = new List<Object>();
			foreach(var r in (Component as STFResourceHolder).Resources)
			{
				newResources.Add(State.ConvertedResources.ContainsKey(r) ? State.ConvertedResources[r] : r);
			}
			(Component as STFResourceHolder).Resources = newResources;
		}
	}
}

