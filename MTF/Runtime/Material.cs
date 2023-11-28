using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MTF
{
	[CreateAssetMenu(fileName = "MTF Material", menuName = "MTF/Material", order = 1)]
	public class Material : ScriptableObject, ISerializationCallbackReceiver
	{
		private class MaterialPropertyValueExportState : IPropertyValueExportState
		{
			public List<IdResourcePair> Resources = new List<IdResourcePair>();
			public string AddResource(UnityEngine.Object Resource)
			{
				var ret = Guid.NewGuid().ToString();
				Resources.Add(new IdResourcePair {Id = ret, Resource = Resource});
				return ret;
			}
		}
		private class MaterialPropertyValueImportState : IPropertyValueImportState
		{
			public List<IdResourcePair> Resources;
			public UnityEngine.Object GetResource(string Id)
			{
				return Resources.FirstOrDefault(r => r.Id == Id)?.Resource;
			}
		}

		[Serializable] public class ShaderTarget { public string Platform; public List<string> Shaders = new List<string>(); }
		[Serializable] public class IdResourcePair { public string Id; public UnityEngine.Object Resource; }
		[Serializable] public class SerializedProperty { public string Type; public string Json; public List<IdResourcePair> Resources = new List<IdResourcePair>(); }
		[Serializable] public class Property { public string Type; public List<IPropertyValue> Values = new List<IPropertyValue>(); public List<SerializedProperty> SerializedValues = new List<SerializedProperty>(); }

		public string Id = Guid.NewGuid().ToString();
		public UnityEngine.Material ConvertedMaterial;
		public List<ShaderTarget> PreferedShaderPerTarget = new List<ShaderTarget>();
		public List<string> StyleHints = new List<string>();
		public List<Property> Properties = new List<Property>();

		public static Material CreateDefaultMaterial()
		{
			var ret = CreateInstance<Material>();
			ret.PreferedShaderPerTarget.Add(new ShaderTarget{ Platform = "unity3d", Shaders = new List<string>{ "Standard" } });
			ret.Properties.Add(new Property { Type = "Albedo", Values = new List<IPropertyValue> { new ColorPropertyValue{ Color = Color.white } } } );
			return ret;
		}

		public void OnBeforeSerialize()
		{
			foreach(var property in Properties)
			{
				property.SerializedValues.Clear();
				foreach(var propertyValue in property.Values)
				{
					var state = new MaterialPropertyValueExportState();
					property.SerializedValues.Add(new SerializedProperty {
						Type = propertyValue.Type,
						Json = PropertyValueRegistry.PropertyValueExporters[propertyValue.Type].SerializeToJson(state, propertyValue).ToString(),
						Resources = state.Resources,
					});
				}
			}
		}
		public void OnAfterDeserialize()
		{
			foreach(var property in Properties)
			{
				property.Values = new List<IPropertyValue>();
				foreach(var serializedPropertyValue in property.SerializedValues)
				{
					var state = new MaterialPropertyValueImportState {Resources = serializedPropertyValue.Resources};
					property.Values.Add(PropertyValueRegistry.DefaultPropertyValueImporters[serializedPropertyValue.Type].ParseFromJson(state, JObject.Parse(serializedPropertyValue.Json)));
				}
			}
		}
	}
}
