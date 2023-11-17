
#if UNITY_EDITOR

using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.IO;
using UnityEditor;
using System.Collections.Generic;

namespace STF.Serde
{
	public class STFTexture : ScriptableObject
	{
		public string Id;
		public string ResourceLocation;
		public string Name;
		public bool Linear;
		public string OriginalBufferId;
	}

	public class STFTexture2dExporter : ISTFResourceExporter
	{
		public string ConvertPropertyPath(string UnityProperty)
		{
			throw new NotImplementedException();
		}

		public List<string> GatherUsedBuffers(UnityEngine.Object resource)
		{
			throw new NotImplementedException();
		}

		public List<GameObject> GatherUsedNodes(UnityEngine.Object resource)
		{
			throw new NotImplementedException();
		}

		public List<UnityEngine.Object> GatherUsedResources(UnityEngine.Object resource)
		{
			throw new NotImplementedException();
		}

		public JToken SerializeToJson(STFExportState state, UnityEngine.Object resource)
		{
			var texture = (Texture2D)resource;
			var ret = new JObject();
			ret.Add("type", STFTextureImporter._TYPE);

			var assetPath = AssetDatabase.GetAssetPath(texture);
			var arrayBuffer = File.ReadAllBytes(assetPath);
			var metaPath = Path.Combine(Path.GetDirectoryName(assetPath), Path.ChangeExtension(assetPath, "Asset"));
			var meta = AssetDatabase.LoadAssetAtPath<STFTexture>(metaPath);

			ret.Add("name", meta.Name);
			ret.Add("format", Path.GetExtension(assetPath));
			ret.Add("width", texture.width);
			ret.Add("height", texture.height);
			if(texture.graphicsFormat.ToString().ToLower().EndsWith("unorm")) ret.Add("linear", true);

			var bufferId = state.AddBuffer(arrayBuffer, meta?.OriginalBufferId);
			ret.Add("buffer", bufferId);

			ret.Add("used_buffers", new JArray() {bufferId});

			state.AddResource(resource, ret, meta?.Id);
			return ret;
		}
	}

	public class STFTextureMetaExporter : ISTFResourceExporter
	{
		public string ConvertPropertyPath(string UnityProperty)
		{
			throw new NotImplementedException();
		}

		public List<string> GatherUsedBuffers(UnityEngine.Object resource)
		{
			throw new NotImplementedException();
		}

		public List<GameObject> GatherUsedNodes(UnityEngine.Object resource)
		{
			throw new NotImplementedException();
		}

		public List<UnityEngine.Object> GatherUsedResources(UnityEngine.Object resource)
		{
			throw new NotImplementedException();
		}

		public JToken SerializeToJson(STFExportState state, UnityEngine.Object resource)
		{
			var meta = (STFTexture)resource;
			var ret = new JObject();
			ret.Add("type", STFTextureImporter._TYPE);

			var arrayBuffer = File.ReadAllBytes(meta.ResourceLocation);
			var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(meta.ResourceLocation);
			
			ret.Add("name", meta.Name);
			ret.Add("format", Path.GetExtension(meta.ResourceLocation));
			ret.Add("width", texture.width);
			ret.Add("height", texture.height);
			if(texture.graphicsFormat.ToString().ToLower().EndsWith("unorm")) ret.Add("linear", true);

			var bufferId = state.AddBuffer(arrayBuffer, meta.OriginalBufferId);
			ret.Add("buffer", bufferId);

			ret.Add("used_buffers", new JArray() {bufferId});
			return ret;
		}
	}

	public class STFTextureImporter : ISTFResourceImporter
	{
		public static string _TYPE = "STF.texture";

		public string ConvertPropertyPath(string STFProperty)
		{
			throw new NotImplementedException();
		}

		public UnityEngine.Object ParseFromJson(STFImportState State, JObject Json, string Id)
		{
			var ret = ScriptableObject.CreateInstance<STFTexture>();
			ret.Id = Id;
			ret.Name = (string)Json["name"];
			ret.ResourceLocation = Path.Combine(State.GetResourceLocation(), ret.Name + "_" + Id + "." + (string)Json["format"]);
			Debug.Log(State.GetResourceLocation());
			Debug.Log(ret.ResourceLocation);
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
