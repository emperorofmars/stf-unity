
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
			var meta = State.LoadMeta<STFMesh>(Resource);

			ret.Add("name", meta.Name);
			ret.Add("format", Path.GetExtension(assetPath));
			ret.Add("width", texture.width);
			ret.Add("height", texture.height);
			if(texture.graphicsFormat.ToString().ToLower().EndsWith("unorm")) ret.Add("linear", true);

			var bufferId = State.AddBuffer(arrayBuffer, meta?.OriginalBufferId);
			ret.Add("buffer", bufferId);

			ret.Add("used_buffers", new JArray() {bufferId});
			return State.AddResource(Resource, ret, meta?.Id);
		}
	}

	public class STFTextureImporter : ISTFResourceImporter
	{
		public static string _TYPE = "STF.texture";

		public string ConvertPropertyPath(string STFProperty)
		{
			throw new NotImplementedException();
		}

		public UnityEngine.Object ParseFromJson(ISTFImportState State, JObject Json, string Id)
		{
			var ret = ScriptableObject.CreateInstance<STFTexture>();
			ret.Id = Id;
			ret.Name = (string)Json["name"];
			ret.ResourceLocation = Path.Combine(State.TargetLocation, STFConstants.ResourceDirectoryName, ret.Name + "_" + Id + "." + (string)Json["format"]);
			ret.Linear = (bool)Json["linear"];
			ret.OriginalBufferId = (string)Json["buffer"];
			
			var arrayBuffer = State.Buffers[ret.OriginalBufferId];
			File.WriteAllBytes(ret.ResourceLocation, arrayBuffer);
			AssetDatabase.CreateAsset(ret, Path.ChangeExtension(ret.ResourceLocation, "Asset"));
			AssetDatabase.Refresh();

			State.AddResource(ret, Id);
			return ret;
		}
	}
}

#endif
