
using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.IO;

namespace STF.Serialisation
{
	public class STFTexture : ISTFResource
	{
		public string TextureType = "color"; // linear | normal
		public Vector2Int TextureSize;
		//public bool Linear;
		public string OriginalBufferId;
	}

	public class STFTexture2dExporter : ISTFResourceExporter
	{
		public string ConvertPropertyPath(ISTFExportState State, UnityEngine.Object Resource, string UnityProperty)
		{
			throw new NotImplementedException();
		}

		public string SerializeToJson(ISTFExportState State, UnityEngine.Object Resource, UnityEngine.Object Context = null)
		{
			var texture = (Texture2D)Resource;
			var (arrayBuffer, meta, fileName) = State.LoadAsset<STFTexture>(texture);

			var ret = new JObject {
				{ "type", STFTextureImporter._TYPE },
				{ "name", string.IsNullOrWhiteSpace(meta?.Name) ? meta.Name : Path.GetFileNameWithoutExtension(fileName) },
				{ "image_format", Path.GetExtension(fileName) },
				{ "texture_width", meta?.TextureSize != null ? meta.TextureSize.x : texture.width },
				{ "texture_height", meta?.TextureSize != null ? meta.TextureSize.y : texture.height },
			};

			if(meta != null && meta.TextureType != null && meta.TextureType.Length >= 0) ret.Add("texture_type", meta.TextureType);
			else ret.Add("texture_type", texture.graphicsFormat.ToString().ToLower().EndsWith("unorm") ? "linear" : "color");

			var bufferId = State.AddBuffer(arrayBuffer, meta?.OriginalBufferId);
			ret.Add("buffer", bufferId);

			// serialize resource components
			ret.Add("components", SerdeUtil.SerializeResourceComponents(State, meta));

			ret.Add("used_buffers", new JArray() {bufferId});
			return State.AddResource(Resource, ret, meta ? meta.Id : Guid.NewGuid().ToString());
		}
	}

	public class STFTextureImporter : ISTFResourceImporter
	{
		public const string _TYPE = "STF.texture";

		public string ConvertPropertyPath(STFImportState State, UnityEngine.Object Resource, string STFProperty)
		{
			throw new NotImplementedException();
		}

		public void ParseFromJson(STFImportState State, JObject Json, string Id)
		{
			var meta = ScriptableObject.CreateInstance<STFTexture>();
			meta.Id = Id;
			meta.Name = (string)Json["name"];
			meta.TextureType = (string)Json["texture_type"];

			if(Json["texture_width"] != null && Json["texture_height"] != null)
				meta.TextureSize = new Vector2Int((int)Json["texture_width"], (int)Json["texture_height"]);
			//meta.Linear = Json["linear"] != null ? (bool)Json["linear"] : false;
			meta.OriginalBufferId = (string)Json["buffer"];
			
			var arrayBuffer = State.Buffers[meta.OriginalBufferId];
			
			State.UnityContext.SaveResource<STFTexture, Texture2D>(arrayBuffer, (string)Json["image_format"], meta, Id);
			SerdeUtil.ParseResourceComponents(State, meta, Json);
			return;
		}
	}
}
