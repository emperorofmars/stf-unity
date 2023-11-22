
#if UNITY_EDITOR

using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;

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
			return State.Resources[Id];
		}
	}
	public class STFMTFExportState : MTF.IExportState
	{
		ISTFExportState State;
		public STFMTFExportState(ISTFExportState State)
		{
			this.State = State;
		}
		public string AddResource(UnityEngine.Object Resource)
		{
			return STFSerdeUtil.SerializeResource(State, Resource);
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
			var ret = new JObject{
				{"type", MTFMaterialImporter._TYPE},
				{"name", mat.name},
			};
			
			// Convert to MTF.Material

			return State.AddResource(Resource, ret);
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

			//ret.Add("used_resources", new JArray());
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
				var mtfProperty = new MTF.Material.Property();
				mtfProperty.Type = propertyJson.Key;
				foreach(var valueJson in propertyJson.Value["values"])
				{
					mtfProperty.Values.Add(MTF.PropertyValueRegistry.DefaultPropertyValueImporters[(string)valueJson["type"]].ParseFromJson(mtfImportState, (JObject)valueJson));
				}
				mat.Properties.Add(mtfProperty);
			}
			
			State.SaveResource(mat, "Asset", Id);
			return;
		}
	}
}

#endif
