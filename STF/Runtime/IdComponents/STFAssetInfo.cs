
using System;
using System.Collections.Generic;
using UnityEngine;

namespace STF.IdComponents
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
}
