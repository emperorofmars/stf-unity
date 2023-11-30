
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace STF.Serialisation
{
	public interface ISTFResource
	{
		string Id {get; set;}
		string Name {get; set;}
		string ResourceLocation {get; set;}
		UnityEngine.Object Resource {get; set;}
		List<ISTFResourceComponent> Components {get; set;}
	}

	public abstract class ASTFResource : ScriptableObject, ISTFResource, ISerializationCallbackReceiver
	{
		[Serializable] public class SerializedResourceComponent {
			public string Id = System.Guid.NewGuid().ToString();
			public string Type;
			public string Json;
			public List<ResourceIdPair> Resources = new List<ResourceIdPair>();
		}

		public string _Id = System.Guid.NewGuid().ToString();
		public string Id {get => _Id; set => _Id = value;}

		public string _Name;
		public string Name {get => _Name; set => _Name = value;}
		
		public UnityEngine.Object _Resource;
		public UnityEngine.Object Resource {get => _Resource; set => _Resource = value;}

		public string _ResourceLocation;
		public string ResourceLocation {get => _ResourceLocation; set => _ResourceLocation = value;}

		public List<ISTFResourceComponent> _Components = new List<ISTFResourceComponent>();
		public List<ISTFResourceComponent> Components {get => _Components; set => _Components = value;}

		public List<SerializedResourceComponent> SerializedResourceComponents = new List<SerializedResourceComponent>();

		public void OnBeforeSerialize()
		{
			SerializedResourceComponents.Clear();
			foreach(var resourceComponent in Components)
			{
				var serialized = STFRegistry.ResourceComponentExporters[resourceComponent.Type].SerializeForUnity(resourceComponent);
				SerializedResourceComponents.Add(new SerializedResourceComponent {
					Id = resourceComponent.Id,
					Type = resourceComponent.Type,
					Json = serialized.Json,
					Resources = serialized.ResourceReferences
				});
			}
		}
		public void OnAfterDeserialize()
		{
			foreach(var serializedResourceComponent in SerializedResourceComponents)
			{
				_Components.Add(STFRegistry.ResourceComponentImporters[serializedResourceComponent.Type].DeserializeForUnity(serializedResourceComponent.Json, serializedResourceComponent.Resources));
			}
		}
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
