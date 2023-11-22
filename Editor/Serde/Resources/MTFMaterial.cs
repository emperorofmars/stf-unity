
#if UNITY_EDITOR

using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.IO;
using System.Linq;

namespace STF.Serde
{
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
			
			State.SaveResource(mat, "Asset", Id);
			return;
		}
	}
}

#endif
