
#if UNITY_EDITOR

using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using STF.Serialisation;
using System;

namespace STF.Tools
{
	public class STFImportInfo : ScriptableObject
	{
		public string assetId = Guid.NewGuid().ToString();
		public string assetType = "STF.asset";

		public string assetName;
		public string assetVersion = "0.0.1";
		public string assetAuthor;
		public string assetURL;
		public string assetLicense;
		public string assetLicenseLink;
		public Texture2D assetPreview;

		public STFFile Buffers;
		public JObject jsonRoot;

		public static STFImportInfo CreateInstance(STFFile Buffers)
		{
			var ret = ScriptableObject.CreateInstance<STFImportInfo>();
			ret.Buffers = Buffers;
			var jsonRoot = JObject.Parse(Buffers.Json);

			var jsonAsset = (JObject)jsonRoot["asset"];
			ret.assetId = (string)jsonAsset["id"];
			ret.assetType = (string)jsonAsset["type"];
			ret.assetName = (string)jsonAsset["name"];
			ret.assetVersion = (string)jsonAsset["version"];
			ret.assetAuthor = (string)jsonAsset["author"];
			ret.assetURL = (string)jsonAsset["url"];
			ret.assetLicense = (string)jsonAsset["license"];
			ret.assetLicenseLink = (string)jsonAsset["license_link"];

			return ret;
		}
	}
}

#endif
