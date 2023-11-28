
using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace STF.Serialisation
{
	public class STFTexture : ASTFResource
	{
		public bool Linear;
		public string OriginalBufferId;
	}

	public class STFTexture2dExporter : ISTFResourceExporter
	{
		public string ConvertPropertyPath(string UnityProperty)
		{
			throw new NotImplementedException();
		}

		public string SerializeToJson(ISTFExportState State, UnityEngine.Object Resource, UnityEngine.Object Context = null)
		{
			var texture = (Texture2D)Resource;
			var ret = new JObject{
				{"type", STFTextureImporter._TYPE}
			};

			var (arrayBuffer, meta, fileName) = State.LoadAsset<STFTexture>(texture);

			ret.Add("name", meta != null ? meta.Name : Path.GetFileNameWithoutExtension(fileName));
			ret.Add("format", Path.GetExtension(fileName));
			ret.Add("width", texture.width);
			ret.Add("height", texture.height);
			ret.Add("linear", texture.graphicsFormat.ToString().ToLower().EndsWith("unorm"));

			var bufferId = State.AddBuffer(arrayBuffer, meta?.OriginalBufferId);
			ret.Add("buffer", bufferId);

			ret.Add("used_buffers", new JArray() {bufferId});
			return State.AddResource(Resource, ret, meta ? meta.Id : Guid.NewGuid().ToString());
		}
	}

	public class STFTextureImporter : ISTFResourceImporter
	{
		public const string _TYPE = "STF.texture";

		public string ConvertPropertyPath(string STFProperty)
		{
			throw new NotImplementedException();
		}

		public void ParseFromJson(ISTFImportState State, JObject Json, string Id)
		{
			var meta = ScriptableObject.CreateInstance<STFTexture>();
			meta.Id = Id;
			meta.Name = (string)Json["name"];
			meta.Linear = Json["linear"] != null ? (bool)Json["linear"] : false;
			meta.OriginalBufferId = (string)Json["buffer"];
			
			var arrayBuffer = State.Buffers[meta.OriginalBufferId];
			State.SaveResource(arrayBuffer, (string)Json["format"], meta, Id);
			return;
		}
	}
}
