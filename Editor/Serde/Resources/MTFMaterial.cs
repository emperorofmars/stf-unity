
#if UNITY_EDITOR

using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.IO;

namespace STF.Serde
{
	public class MTFMaterialExporter : ISTFResourceExporter
	{
		public string ConvertPropertyPath(string UnityProperty)
		{
			throw new NotImplementedException();
		}

		public string SerializeToJson(ISTFExportState State, UnityEngine.Object Resource)
		{
			var texture = (Texture2D)Resource;
			var ret = new JObject{
				{"type", MTFMaterialImporter._TYPE}
			};


			//ret.Add("used_buffers", new JArray() {bufferId});
			//return State.AddResource(Resource, ret, meta ? meta.Id : Guid.NewGuid().ToString());
			return null;
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
			/*var meta = ScriptableObject.CreateInstance<STFTexture>();
			meta.Id = Id;
			meta.Name = (string)Json["name"];
			meta.ResourceLocation = Path.Combine(State.TargetLocation, STFConstants.ResourceDirectoryName, meta.Name + "_" + Id + "." + (string)Json["format"]);
			meta.Linear = (bool)Json["linear"];
			meta.OriginalBufferId = (string)Json["buffer"];
			
			var arrayBuffer = State.Buffers[meta.OriginalBufferId];
			State.SaveResource(arrayBuffer, (string)Json["format"], meta, Id);*/
			return;
		}
	}
}

#endif
