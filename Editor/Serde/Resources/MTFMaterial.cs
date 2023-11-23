
#if UNITY_EDITOR

using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;

namespace STF.Serde
{
	public class STFMTFImportState : MTF.IImportState
	{
		ISTFImportState State;
		public STFMTFImportState(ISTFImportState State)
		{
			this.State = State;
		}

		public UnityEngine.Object GetResource(string Id)
		{
			var r = State.Resources[Id];
			if(r is ISTFResource)
			{
				if(((ISTFResource)r).Resource != null) return ((ISTFResource)r).Resource;
				else return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(((ISTFResource)r).ResourceLocation);
			}
			return State.Resources[Id];
		}
	}
	public class STFMTFExportState : MTF.IExportState
	{
		ISTFExportState State;
		public List<string> UsedResources = new List<string>();
		public STFMTFExportState(ISTFExportState State)
		{
			this.State = State;
		}
		public string AddResource(UnityEngine.Object Resource)
		{
			string id = STFSerdeUtil.SerializeResource(State, Resource);
			UsedResources.Add(id);
			return id;
		}
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
			if(MTF.ShaderConverterRegistry.DefaultMaterialParsers.ContainsKey(mat.shader.name))
			{
				var mtfMaterial = MTF.ShaderConverterRegistry.MaterialParsers[mat.shader.name].ParseFromUnityMaterial(mat);
				return STFSerdeUtil.SerializeResource(State, mtfMaterial);
			}
			else
			{
				Debug.LogWarning("Material Converter Not registered for shader: " + mat.shader.name + ", falling back.");
				var mtfMaterial = MTF.ShaderConverterRegistry.MaterialParsers[MTF.StandardConverter._SHADER_NAME].ParseFromUnityMaterial(mat);
				return STFSerdeUtil.SerializeResource(State, mtfMaterial);
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
			var mtfExportState = new STFMTFExportState(State);
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
			
			var mtfImportState = new STFMTFImportState(State);
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
			MTF.IMaterialConverter converter = MTF.ShaderConverterRegistry.DefaultMaterialConverters[MTF.StandardConverter._SHADER_NAME];
			if(shaderTargets != null) foreach(var shaderTarget in shaderTargets.Shaders)
			{
				if(MTF.ShaderConverterRegistry.DefaultMaterialConverters.ContainsKey(shaderTarget))
				{
					converter = MTF.ShaderConverterRegistry.DefaultMaterialConverters[shaderTarget];
					break;
				}
			}
			var unityMaterial = converter.ConvertToUnityMaterial(mat);
			mat.ConvertedMaterial = unityMaterial;
			State.SaveResourceBelongingToId(unityMaterial, "Asset", Id);
			State.SaveResource(mat, "Asset", Id);
			return;
		}
	}
}

#endif