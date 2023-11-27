
#if UNITY_EDITOR

using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using STF.IdComponents;
using STF.Serialisation;

namespace STF.Tools
{
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
