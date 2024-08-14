
using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using STF.ApplicationConversion;
using STF.Util;
using STF.Serialisation;

namespace STF.Types
{
	public class MTFMaterial : ISTFResource
	{
		public const string _TYPE = "MTF.material";
		public override string Type => _TYPE;
	}

	public class MTFPropertyValueExportState : MTF.PropertyValues.IPropertyValueExportState
	{
		readonly STFExportState State;
		public List<string> UsedResources = new List<string>();
		public MTFPropertyValueExportState(STFExportState State)
		{
			this.State = State;
		}
		public string AddResource(UnityEngine.Object Resource)
		{
			string id = ExportUtil.SerializeResource(State, Resource);
			UsedResources.Add(id);
			return id;
		}
	}
	public class MTFMaterialParseState : MTF.IMaterialParseState
	{
		public void SavePropertyValue(MTF.PropertyValues.IPropertyValue PropertyValue, MTF.Material.Property Property, MTF.Material Material)
		{
			PropertyValue.name = Property.Type + ":" + PropertyValue.Type + ":" + Guid.NewGuid().ToString();
			Property.Values.Add(PropertyValue);
		}
	}

	public class MTFPropertyValueImportState : MTF.PropertyValues.IPropertyValueImportState
	{
		readonly STFImportState State;
		public MTFPropertyValueImportState(STFImportState State)
		{
			this.State = State;
		}

		public UnityEngine.Object GetResource(JToken Id)
		{
			return State.Resources[(string)Id].Resource;
		}
	}
	public class MTFMaterialConvertState : MTF.IMaterialConvertState
	{
		STFImportState State;
		string Name;
		public MTFMaterialConvertState(STFImportState State, string Name)
		{
			this.State = State;
			this.Name = Name;
		}
		
		public void SaveResource(UnityEngine.Object Resource, string FileExtension)
		{
			State.UnityContext.SaveGeneratedResource(Resource, FileExtension);
		}
		public Texture2D SaveImageResource(byte[] Bytes, string Name, string Extension)
		{
			(var texture, var _) = State.UnityContext.SaveGeneratedResource(Bytes, this.Name + Name, Extension);
			return (Texture2D)texture;
		}
	}

	public class UnityMaterialExporter : ISTFResourceExporter
	{
		public string ConvertPropertyPath(STFExportState State, UnityEngine.Object Resource, string UnityProperty)
		{
			if(UnityProperty.StartsWith("MTF.")) return UnityProperty.Substring(UnityProperty.IndexOf('.') + 1);
			else return MTF.ShaderConverterRegistry.MaterialParsers[((Material)Resource).shader.name].ConvertPropertyPath(UnityProperty, (Material)Resource);
		}

		public string SerializeToJson(STFExportState State, UnityEngine.Object Resource, UnityEngine.Object Context = null)
		{
			var mat = (Material)Resource;
			// Convert to MTF.Material
			var mtfExportState = new MTFMaterialParseState();
			if(MTF.ShaderConverterRegistry.MaterialParsers.ContainsKey(mat.shader.name))
			{
				var mtfMaterial = MTF.ShaderConverterRegistry.MaterialParsers[mat.shader.name].ParseFromUnityMaterial(mtfExportState, mat);
				mtfMaterial.MaterialName = Resource.name;
				var stfmtfMaterial = ScriptableObject.CreateInstance<MTFMaterial>();
				stfmtfMaterial.Resource = mtfMaterial;
				return ExportUtil.SerializeResource(State, stfmtfMaterial);
			}
			else
			{
				Debug.LogWarning("Material Converter Not registered for shader: " + mat.shader.name + ", falling back.");
				var mtfMaterial = MTF.ShaderConverterRegistry.MaterialParsers[MTF.StandardConverter._SHADER_NAME].ParseFromUnityMaterial(mtfExportState, mat);
				mtfMaterial.MaterialName = Resource.name;
				var stfmtfMaterial = ScriptableObject.CreateInstance<MTFMaterial>();
				stfmtfMaterial.Resource = mtfMaterial;
				return ExportUtil.SerializeResource(State, stfmtfMaterial);
			}
		}
	}

	public class MTFMaterialExporter : ISTFResourceExporter
	{
		public string ConvertPropertyPath(STFExportState State, UnityEngine.Object Resource, string UnityProperty)
		{
			return UnityProperty;
		}

		public string SerializeToJson(STFExportState State, UnityEngine.Object Resource, UnityEngine.Object Context = null)
		{
			var meta = (MTFMaterial)Resource;
			var mat = (MTF.Material)meta.Resource;
			var ret = new JObject{
				{"type", MTFMaterial._TYPE},
				{"name", mat.MaterialName?.Length > 0 ? mat.MaterialName : mat.name},
				{"targets", new JObject(mat.PreferedShaderPerTarget.Select(e => new JProperty(e.Platform, new JArray(e.Shaders))))},
			};
			var rf = new RefSerializer(ret);

			var renderHints = new JObject();
			foreach(var entry in mat.StyleHints)
			{
				renderHints.Add(entry.Name, entry.Value);
			}
			ret.Add("hints", renderHints);

			var mtfExportState = new MTFPropertyValueExportState(State);
			var propertiesJson = new JObject();
			foreach(var property in mat.Properties)
			{
				var valuesJson = new JArray();
				foreach(var value in property.Values)
				{
					valuesJson.Add(MTF.PropertyValueRegistry.GetPropertyValueExporter(value.Type).SerializeToJson(mtfExportState, value));
				}
				propertiesJson.Add(property.Type, valuesJson);
			}
			ret.Add("properties", propertiesJson);

			foreach(var res in mtfExportState.UsedResources) rf.ResourceRef(res);
			return State.AddResource(Resource, ret, mat.Id);
		}
	}

	public class MTFMaterialImporter : ISTFResourceImporter
	{
		public string ConvertPropertyPath(STFImportState State, UnityEngine.Object Resource, string STFProperty)
		{
			/*var material = (MTF.Material)Resource;
			if(material.ConvertedMaterial != null) return MTF.ShaderConverterRegistry.MaterialConverters[material.ConvertedMaterial.shader.name].ConvertPropertyPath(STFProperty, material.ConvertedMaterial);
			else return "MTF." + STFProperty;*/
			return "MTF." + STFProperty;
		}

		public void ParseFromJson(STFImportState State, JObject Json, string Id)
		{
			var meta = ScriptableObject.CreateInstance<MTFMaterial>();
			var mat = ScriptableObject.CreateInstance<MTF.Material>();
			
			meta.Id = mat.Id = Id;
			meta.STFName = mat.MaterialName = (string)Json["name"];

			mat.name = mat.MaterialName + "_" + Id + "_MTF";
			meta.name = mat.MaterialName + "_" + Id;

			meta.Resource = State.UnityContext.SaveGeneratedResource(mat, "Asset");

			foreach(var entry in (JObject)Json["targets"])
			{
				mat.PreferedShaderPerTarget.Add(new MTF.Material.ShaderTarget{Platform = entry.Key, Shaders = entry.Value.ToObject<List<string>>()});
			}
			foreach(var entry in (JObject)Json["hints"])
			{
				mat.StyleHints.Add(new MTF.Material.StyleHint {Name = entry.Key, Value = (string)entry.Value});
			}
			
			var mtfImportState = new MTFPropertyValueImportState(State);
			foreach(var propertyJson in (JObject)Json["properties"])
			{
				var mtfProperty = new MTF.Material.Property { Type = propertyJson.Key };
				foreach (var valueJson in propertyJson.Value)
				{
					var propertyValueType = (string)valueJson["type"];
					var propertyValue = MTF.PropertyValueRegistry.GetPropertyValueImporter(propertyValueType).ParseFromJson(mtfImportState, (JObject)valueJson);
					propertyValue.name = propertyJson.Key + ":" + propertyValueType + ":" + mtfProperty.Values.Count();
					mtfProperty.Values.Add(propertyValue);
					State.UnityContext.SaveSubResource(propertyValue, mat);
				}
				mat.Properties.Add(mtfProperty);
			}

			// Convert to MTF.Material
			var shaderTargets = mat.PreferedShaderPerTarget.Find(t => t.Platform == "unity3d");
			MTF.IMaterialConverter converter = MTF.ShaderConverterRegistry.MaterialConverters[MTF.StandardConverter._SHADER_NAME];
			if(shaderTargets != null) foreach(var shaderTarget in shaderTargets.Shaders)
			{
				if(MTF.ShaderConverterRegistry.MaterialConverters.ContainsKey(shaderTarget))
				{
					converter = MTF.ShaderConverterRegistry.MaterialConverters[shaderTarget];
					break;
				}
			}
			var mtfConvertState = new MTFMaterialConvertState(State, mat.name + Id);
			var unityMaterial = converter.ConvertToUnityMaterial(mtfConvertState, mat);
			unityMaterial.name = mat.name + "_Converted";
			mat.ConvertedMaterial = (Material)State.UnityContext.SaveGeneratedResource(unityMaterial, "Asset");
			State.AddResource(meta);
			return;
		}
	}

	// Application Resource Converter
	public class MTFMaterialApplicationConverter : ISTFResourceApplicationConverter
	{
		public string ConvertPropertyPath(ISTFApplicationConvertState State, UnityEngine.Object Resource, string STFProperty)
		{
			var material = (MTF.Material)Resource;
			if(STFProperty.StartsWith("MTF") && material.ConvertedMaterial != null)
			{
				STFProperty = STFProperty.Substring(STFProperty.IndexOf('.') + 1);
				return MTF.ShaderConverterRegistry.MaterialConverters[material.ConvertedMaterial.shader.name].ConvertPropertyPath(STFProperty, material.ConvertedMaterial);
			}
			// TODO: else generate material and then convert the property
			else return STFProperty;
		}

		public void Convert(ISTFApplicationConvertState State, UnityEngine.Object Resource)
		{
		}
	}
}
