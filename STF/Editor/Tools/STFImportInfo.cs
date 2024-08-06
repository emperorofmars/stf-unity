
#if UNITY_EDITOR

using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using STF.Serialisation;
using System;
using System.Drawing;
using UnityEditorInternal;

namespace STF.Tools
{
	[Serializable]
	public class STFImportInfo// : ScriptableObject
	{
		public string Id = Guid.NewGuid().ToString();
		public string Type = "STF.asset";

		public string Name;
		public string Version = "0.0.1";
		public string Author;
		public string URL;
		public string License;
		public string LicenseLink;
		public Texture2D Preview;

		public STFFile Buffers;
		public JObject JsonRoot;

		public STFImportInfo(STFFile Buffers, string path)
		{
			this.Buffers = Buffers;
			JsonRoot = JObject.Parse(Buffers.Json);

			var jsonAsset = JsonRoot[STFKeywords.ObjectType.Asset];
			Id = (string)jsonAsset[STFKeywords.Keys.Id];
			Type = (string)jsonAsset[STFKeywords.Keys.Type];
			Name = (string)jsonAsset[STFKeywords.Keys.Name];
			Version = (string)jsonAsset["version"];
			Author = (string)jsonAsset["author"];
			URL = (string)jsonAsset["url"];
			License = (string)jsonAsset["license"];
			LicenseLink = (string)jsonAsset["license_link"];

			var previewID = (string)jsonAsset["preview"];
			if(!string.IsNullOrWhiteSpace(previewID))
			{
				var previewJson = (JObject)JsonRoot[STFKeywords.ObjectType.Resources][previewID];
				var previewImporter = STFRegistry.ResourceImporters[(string)previewJson[STFKeywords.Keys.Type]];
				var importContext = STFRegistry.GetDefaultImportContext();
				var unityContext = new RuntimeUnityImportContext();
				var importState = new STFImportState(importContext, unityContext, Buffers);
				previewImporter.ParseFromJson(importState, previewJson, previewID);
				var previewResource = (STFTexture)importState.Resources[previewID];
				Preview = (Texture2D)previewResource.Resource;
			}
		}

		/*public static STFImportInfo CreateInstance(STFFile Buffers, string path)
		{
			var ret = ScriptableObject.CreateInstance<STFImportInfo>();
			ret.Buffers = Buffers;
			ret.JsonRoot = JObject.Parse(Buffers.Json);

			var jsonAsset = ret.JsonRoot[STFKeywords.ObjectType.Asset];
			ret.Id = (string)jsonAsset[STFKeywords.Keys.Id];
			ret.Type = (string)jsonAsset[STFKeywords.Keys.Type];
			ret.Name = (string)jsonAsset[STFKeywords.Keys.Name];
			ret.Version = (string)jsonAsset["version"];
			ret.Author = (string)jsonAsset["author"];
			ret.URL = (string)jsonAsset["url"];
			ret.License = (string)jsonAsset["license"];
			ret.LicenseLink = (string)jsonAsset["license_link"];

			var previewID = (string)jsonAsset["preview"];
			if(!string.IsNullOrWhiteSpace(previewID))
			{
				var previewJson = (JObject)ret.JsonRoot[STFKeywords.ObjectType.Resources][previewID];
				var previewImporter = STFRegistry.ResourceImporters[(string)previewJson[STFKeywords.Keys.Type]];
				var importContext = STFRegistry.GetDefaultImportContext();
				var unityContext = new RuntimeUnityImportContext();
				var importState = new STFImportState(importContext, unityContext, Buffers);
				previewImporter.ParseFromJson(importState, previewJson, previewID);
				var previewResource = (STFTexture)importState.Resources[previewID];
				ret.Preview = (Texture2D)previewResource.Resource;
			}

			return ret;
		}*/
	}
}

#endif
