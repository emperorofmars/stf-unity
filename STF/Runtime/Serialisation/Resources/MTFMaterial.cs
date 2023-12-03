
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
	
	public class MTFPropertyValueImportState : MTF.IPropertyValueImportState
	{
		ISTFImportState State;
		public MTFPropertyValueImportState(ISTFImportState State)
		{
			this.State = State;
		}

		public UnityEngine.Object GetResource(string Id)
		{
			var r = State.Resources[Id];
			if(r is ISTFResource resource)
			{
				return State.LoadResource(resource);
			}
			return r;
		}
	}
	public class MTFMaterialConvertState : MTF.IMaterialConvertState
	{
		ISTFImportState State;
		string Name;
		public MTFMaterialConvertState(ISTFImportState State, string Name)
		{
			this.State = State;
			this.Name = Name;
		}
		
		public void SaveResource(UnityEngine.Object Resource, string FileExtension)
		{
			State.SaveGeneratedResource(Resource, FileExtension);
		}
		public Texture2D SaveImageResource(byte[] Bytes, string Name, string Extension)
		{
			return State.SaveAndLoadResource<Texture2D>(Bytes, this.Name + Name, Extension);
		}
	}

	public class UnityMaterialExporter : ISTFResourceExporter
	{
		public string ConvertPropertyPath(string UnityProperty)
		{
			throw new NotImplementedException();
		}

		public string SerializeToJson(ISTFExportState State, UnityEngine.Object Resource, UnityEngine.Object Context = null)
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

		public string SerializeToJson(ISTFExportState State, UnityEngine.Object Resource, UnityEngine.Object Context = null)
		{
			var mat = (MTF.Material)Resource;
			var ret = new JObject{
				{"type", MTFMaterialImporter._TYPE},
				{"name", mat.name},
				{"targets", new JObject(mat.PreferedShaderPerTarget.Select(e => new JProperty(e.Platform, new JArray(e.Shaders))))},
			};
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
					if(MTF.PropertyValueRegistry.PropertyValueExporters.ContainsKey(value.Type))
					{
						valuesJson.Add(MTF.PropertyValueRegistry.PropertyValueExporters[value.Type].SerializeToJson(mtfExportState, value));
					}
					else
					{
						Debug.LogWarning($"Unrecognized Material PropertyValue: {value.Type}");
						// Unrecognized Material Property
					}
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
					// improve this, not just default ones & fall back to unrecognized
					mtfProperty.Values.Add(MTF.PropertyValueRegistry.PropertyValueImporters[(string)valueJson["type"]].ParseFromJson(mtfImportState, (JObject)valueJson));
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
			mat.ConvertedMaterial = unityMaterial;
			//mat.OnBeforeSerialize();
			State.SaveResourceBelongingToId(unityMaterial, "Asset", Id);
			State.SaveResource(mat, "Asset", Id);
			return;
		}
	}
}
