
using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace STF.Serialisation
{
	public class MTFPropertyValueExportState : MTF.IPropertyValueExportState
	{
		ISTFExportState State;
		public List<string> UsedResources = new List<string>();
		public MTFPropertyValueExportState(ISTFExportState State)
		{
			this.State = State;
		}
		public string AddResource(UnityEngine.Object Resource)
		{
			string id = SerdeUtil.SerializeResource(State, Resource);
			UsedResources.Add(id);
			return id;
		}
	}
	public class MTFMaterialParseState : MTF.IMaterialParseState
	{

	}

	public class UnityMaterialExporter : ISTFResourceExporter
	{
		public string ConvertPropertyPath(string UnityProperty)
		{
			throw new NotImplementedException();
		}

		public string SerializeToJson(ISTFExportState State, UnityEngine.Object Resource)
		{
			var mat = (Material)Resource;
			// Convert to MTF.Material
			var mtfExportState = new MTFMaterialParseState();
			if(MTF.ShaderConverterRegistry.MaterialParsers.ContainsKey(mat.shader.name))
			{
				var mtfMaterial = MTF.ShaderConverterRegistry.MaterialParsers[mat.shader.name].ParseFromUnityMaterial(mtfExportState, mat);
				return SerdeUtil.SerializeResource(State, mtfMaterial);
			}
			else
			{
				Debug.LogWarning("Material Converter Not registered for shader: " + mat.shader.name + ", falling back.");
				var mtfMaterial = MTF.ShaderConverterRegistry.MaterialParsers[MTF.StandardConverter._SHADER_NAME].ParseFromUnityMaterial(mtfExportState, mat);
				return SerdeUtil.SerializeResource(State, mtfMaterial);
			}
		}
	}

	public class MTFMaterialExporter : ISTFResourceExporter
	{
		public string ConvertPropertyPath(string UnityProperty)
		{
			throw new NotImplementedException();
		}

		public string SerializeToJson(ISTFExportState State, UnityEngine.Object Resource)
		{
			var mat = (MTF.Material)Resource;
			var ret = new JObject{
				{"type", MTFMaterialImporter._TYPE},
				{"name", mat.name},
				{"targets", new JObject(mat.PreferedShaderPerTarget.Select(e => new JProperty(e.Platform, new JArray(e.Shaders))))},
				{"hints", new JArray(mat.StyleHints)}
			};
			var mtfExportState = new MTFPropertyValueExportState(State);
			var propertiesJson = new JObject();
			foreach(var property in mat.Properties)
			{
				var valuesJson = new JArray();
				foreach(var value in property.Values)
				{
					// improve this, not just default ones & fall back to unrecognized
					valuesJson.Add(MTF.PropertyValueRegistry.PropertyValueExporters[value.Type].SerializeToJson(mtfExportState, value));
				}
				propertiesJson.Add(property.Type, valuesJson);
			}
			ret.Add("properties", propertiesJson);

			ret.Add("used_resources", new JArray(mtfExportState.UsedResources));
			return State.AddResource(Resource, ret, mat.Id);
		}
	}

	public class MTFMaterialImporter : ISTFResourceImporter
	{
		public static string _TYPE = "MTF.material";

		public string ConvertPropertyPath(string STFProperty)
		{
			throw new NotImplementedException();
		}

		public void ParseFromJson(ISTFImportState State, JObject Json, string Id)
		{
			var mat = ScriptableObject.CreateInstance<MTF.Material>();
			mat.Id = Id;
			mat.name = (string)Json["name"];
			foreach(var entry in (JObject)Json["targets"])
			{
				mat.PreferedShaderPerTarget.Add(new MTF.Material.ShaderTarget{Platform = entry.Key, Shaders = entry.Value.ToObject<List<string>>()});
			}
			mat.StyleHints = Json["hints"].ToObject<List<string>>();
			
			//var mtfImportState = new MTFPropertyValueImportState(State);
			foreach(var propertyJson in (JObject)Json["properties"])
			{
				var mtfProperty = new MTF.Material.Property { Type = propertyJson.Key };
				foreach (var valueJson in propertyJson.Value)
				{
					// improve this, not just default ones & fall back to unrecognized
					mtfProperty.Values.Add(MTF.PropertyValueRegistry.PropertyValueImporters[(string)valueJson["type"]].ParseFromJson(State.MTFPropertyValueImportState, (JObject)valueJson));
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
			//var mtfConvertState = new MTFMaterialConvertState();
			var unityMaterial = converter.ConvertToUnityMaterial(State.MTFMaterialConvertState, mat);
			unityMaterial.name = mat.name + "_Converted";
			mat.ConvertedMaterial = unityMaterial;
			State.SaveResourceBelongingToId(unityMaterial, "Asset", Id);
			State.SaveResource(mat, "Asset", Id);
			return;
		}
	}
}
