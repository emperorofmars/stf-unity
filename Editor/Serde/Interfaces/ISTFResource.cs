
#if UNITY_EDITOR

using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace STF.Serde
{
	public interface ISTFResource
	{
		string Id {get; set;}
		string Name {get; set;}
		string ResourceLocation {get; set;}
		Object Resource {get; set;}
		List<ISTFResourceComponent> Components {get; set;}
	}

	public abstract class ASTFResource : ScriptableObject, ISTFResource
	{
		public string _Id = System.Guid.NewGuid().ToString();
		public string Id {get => _Id; set => _Id = value;}

		public string _Name;
		public string Name {get => _Name; set => _Name = value;}
		
		public Object _Resource;
		public Object Resource {get => _Resource; set => _Resource = value;}

		public string _ResourceLocation;
		public string ResourceLocation {get => _ResourceLocation; set => _ResourceLocation = value;}

		public List<ISTFResourceComponent> _Components;
		public List<ISTFResourceComponent> Components {get => _Components; set => _Components = value;}
	}

	public interface ISTFResourceExporter
	{
		string SerializeToJson(ISTFExportState State, Object Resource);
		string ConvertPropertyPath(string UnityProperty);
	}

	public interface ISTFResourceImporter
	{
		Object ParseFromJson(ISTFImportState State, JObject Json, string Id);
		string ConvertPropertyPath(string STFProperty);
	}
}

#endif
