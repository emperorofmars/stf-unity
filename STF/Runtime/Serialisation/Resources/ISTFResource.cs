
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using STF_Util;
using UnityEngine;
using UnityExtensions;

namespace STF.Serialisation
{
	public abstract class  ISTFResource : ScriptableObject
	{
		public virtual string Id {get; set;}
		public virtual string Name {get; set;}
		public virtual string ResourceLocation {get; set;}
		public virtual UnityEngine.Object Resource {get; set;}
		public virtual List<ISTFResourceComponent> Components {get; set;}
	}

	public abstract class ASTFResource : ISTFResource
	{
		[Serializable] public class SerializedResourceComponent {
			public string Id = System.Guid.NewGuid().ToString();
			public string Type;
			public string Json;
			public List<ResourceIdPair> Resources = new List<ResourceIdPair>();
		}

		[Id] public string _Id = System.Guid.NewGuid().ToString();
		public override string Id {get => _Id; set => _Id = value;}

		public string _Name;
		public override string Name {get => _Name; set => _Name = value;}
		
		public UnityEngine.Object _Resource;
		public override UnityEngine.Object Resource {get => _Resource; set => _Resource = value;}

		public string _ResourceLocation;
		public override string ResourceLocation {get => _ResourceLocation; set => _ResourceLocation = value;}
		
		[ReorderableList(elementsAreSubassets = true)] public List<ISTFResourceComponent> _Components = new List<ISTFResourceComponent>();
		public override List<ISTFResourceComponent> Components { get => _Components; set => _Components = value; }

	}

	public interface ISTFResourceExporter
	{
		string SerializeToJson(ISTFExportState State, UnityEngine.Object Resource, UnityEngine.Object Context = null);
		string ConvertPropertyPath(string UnityProperty);
	}

	public interface ISTFResourceImporter
	{
		void ParseFromJson(ISTFImportState State, JObject Json, string Id);
		string ConvertPropertyPath(string STFProperty);
	}
}
