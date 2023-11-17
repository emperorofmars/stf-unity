
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using STF.Serde;
using Newtonsoft.Json.Linq;


namespace STF.Tools
{
	// A scripted importer for STF files. All the work is done by the STFImporter.
	// It should be just as easy to create an explicit UI to import STF files into a folder structure, similar to how UniGLTF/UniVRM does it.

	[ScriptedImporter(1, new string[] {"stf"})]
	public class STFScriptedImporter : ScriptedImporter
	{
		public override void OnImportAsset(AssetImportContext ctx)
		{
			byte[] byteArray = File.ReadAllBytes(ctx.assetPath);
			var buffers = STFBufferImporter.ParseBuffersFromBinary(byteArray);

			var jsonRoot = JObject.Parse(buffers.json);
			var imporInfo = ScriptableObject.CreateInstance<STFImportInfo>();

			foreach(var jsonAsset in (JObject)jsonRoot["assets"])
			{
				imporInfo.assets.Add(new IdComponents.STFAssetInfo {
					assetId = jsonAsset.Key,
					assetType = (string)jsonAsset.Value["type"],
					assetName = (string)jsonAsset.Value["name"],
					assetVersion = (string)jsonAsset.Value["version"],
					assetAuthor = (string)jsonAsset.Value["author"],
					assetURL = (string)jsonAsset.Value["url"],
					assetLicense = (string)jsonAsset.Value["license"],
					assetLicenseLink = (string)jsonAsset.Value["license_link"]
				});
			}

			ctx.AddObjectToAsset("main", imporInfo);
			ctx.SetMainObject(imporInfo);
		}
	}
}

#endif
