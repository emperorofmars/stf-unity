using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using STF.ApplicationConversion;
using STF.Serialisation;
using STF.Util;
using UnityEngine;

namespace STF.Types
{
	public class STFResourceHolder : ISTFNodeComponent
	{
		public const string _TYPE = "STF.resource_holder";
		public override string Type => _TYPE;

		public List<Object> Resources = new List<Object>();
	}

	public class STFResourceHolderExporter : ASTFNodeComponentExporter
	{
		public override string ConvertPropertyPath(STFExportState State, Component Component, string UnityProperty)
		{
			throw new System.NotImplementedException();
		}

		public override (string, JObject) SerializeToJson(STFExportState State, Component Component)
		{
			var c = (STFResourceHolder)Component;
			var ret = new JObject {
				{"type", c.Type},
			};
			var rf = new RefSerializer(ret);
			foreach(var res in c.Resources)
			{
				var id = ExportUtil.SerializeResource(State, res, res is AnimationClip ? c.gameObject : null);
				rf.ResourceRef(id);
			}

			SerializeRelationships(c, ret);
			return (c.Id, ret);
		}
	}

	public class STFResourceHolderImporter : ASTFNodeComponentImporter
	{
		public override string ConvertPropertyPath(STFImportState State, Component Component, string STFProperty)
		{
			throw new System.NotImplementedException();
		}

		public override void ParseFromJson(STFImportState State, JObject Json, string Id, GameObject Go)
		{
			var c = Go.AddComponent<STFResourceHolder>();
			ParseRelationships(Json, c);
			c.Id = Id;
			var rf = new RefDeserializer(Json);
			foreach(string r in rf.ResourceRefs())
			{
				var resource = State.Resources[r];
				c.Resources.Add(resource);
				if(resource.Resource is AnimationClip) State.SetPostprocessContext(resource, Go);
			}
		}
	}

	public class STFResourceHolderApplicationConverter : ISTFNodeComponentApplicationConverter
	{
		public void ConvertResources(ISTFApplicationConvertState State, Component Component)
		{
			foreach(var r in (Component as STFResourceHolder).Resources)
			{
				State.RegisterResource(r, Component.gameObject);
			}
		}

		public string ConvertPropertyPath(ISTFApplicationConvertState State, Component Resource, string STFProperty)
		{
			throw new System.NotImplementedException();
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

