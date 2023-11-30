
#if UNITY_EDITOR

using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using STF.Serialisation;
using System;

namespace STF.Tools
{
	[Serializable]
	public class STFAssetInfo
	{
		public string assetName;
		public string assetVersion = "0.0.1";
		public string assetAuthor;
		public string assetURL;
		public string assetLicense;
		public string assetLicenseLink;
		public Texture2D assetPreview;

		public string assetId = Guid.NewGuid().ToString();
		public string assetType = "STF.asset";
	}
	
	public class STFImportInfo : ScriptableObject
	{
		public string MainAssetId;
		public List<STFAssetInfo> Assets;
		[SerializeField]
		public STFFile Buffers;

		public static STFImportInfo CreateInstance(STFFile Buffers)
		{
			var ret = ScriptableObject.CreateInstance<STFImportInfo>();
			ret.Assets = new List<STFAssetInfo>();
			ret.Buffers = Buffers;
			var jsonRoot = JObject.Parse(Buffers.Json);

			ret.MainAssetId = (string)jsonRoot["main"];

			foreach(var jsonAsset in (JObject)jsonRoot["assets"])
			{
				ret.Assets.Add(new STFAssetInfo {
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

			return ret;
		}
	}
}

#endif
