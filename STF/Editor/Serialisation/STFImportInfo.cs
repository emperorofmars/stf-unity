
#if UNITY_EDITOR

using UnityEngine;
using Newtonsoft.Json.Linq;
using STF.Serialisation;
using System;
using STF.Util;
using STF.Types;

namespace STF.Tools
{
	[Serializable]
	public class STFImportInfo
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

		public STFImportInfo(STFFile Buffers)
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
				var previewImporter = new STFTextureImporter();
				var importContext = STFRegistry.GetDefaultImportContext();
				var unityContext = new RuntimeUnityImportContext();
				var importState = new STFImportState(importContext, unityContext, Buffers);
				previewImporter.ParseFromJson(importState, previewJson, previewID);
				var previewResource = (STFTexture)importState.Resources[previewID];
				Preview = (Texture2D)previewResource.Resource;
			}
		}
	}
}

#endif
