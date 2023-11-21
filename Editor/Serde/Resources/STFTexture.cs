
#if UNITY_EDITOR

using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.IO;
using UnityEditor;
using System.Collections.Generic;

namespace STF.Serde
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

		public string SerializeToJson(ISTFExportState State, UnityEngine.Object Resource)
		{
			var texture = (Texture2D)Resource;
			var ret = new JObject{
				{"type", STFTextureImporter._TYPE}
			};

			var assetPath = AssetDatabase.GetAssetPath(texture);
			var arrayBuffer = File.ReadAllBytes(assetPath);
			var meta = State.LoadMeta<STFTexture>(Resource);

			ret.Add("name", meta ? meta.name : Path.GetFileNameWithoutExtension(assetPath));
			ret.Add("format", Path.GetExtension(assetPath));
			ret.Add("width", texture.width);
			ret.Add("height", texture.height);
			if(texture.graphicsFormat.ToString().ToLower().EndsWith("unorm")) ret.Add("linear", true);

			var bufferId = State.AddBuffer(arrayBuffer, meta?.OriginalBufferId);
			ret.Add("buffer", bufferId);

			ret.Add("used_buffers", new JArray() {bufferId});
			return State.AddResource(Resource, ret, meta ? meta.Id : Guid.NewGuid().ToString());
		}
	}

	public class STFTextureImporter : ISTFResourceImporter
	{
		public static string _TYPE = "STF.texture";

		public string ConvertPropertyPath(string STFProperty)
		{
			throw new NotImplementedException();
		}

		public void ParseFromJson(ISTFImportState State, JObject Json, string Id)
		{
			var meta = ScriptableObject.CreateInstance<STFTexture>();
			meta.Id = Id;
			meta.Name = (string)Json["name"];
			meta.ResourceLocation = Path.Combine(State.TargetLocation, STFConstants.ResourceDirectoryName, meta.Name + "_" + Id + "." + (string)Json["format"]);
			meta.Linear = (bool)Json["linear"];
			meta.OriginalBufferId = (string)Json["buffer"];
			
			var arrayBuffer = State.Buffers[meta.OriginalBufferId];
			State.SaveResource(arrayBuffer, (string)Json["format"], meta, Id);
			return;
		}
	}
}

#endif
