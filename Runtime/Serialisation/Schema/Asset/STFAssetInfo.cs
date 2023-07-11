
using System.Collections.Generic;
using UnityEngine;

namespace stf
{
	public class STFAssetInfo : MonoBehaviour
	{
		public string assetId;
		public string assetType;
		public string assetName;
		public STFMeta originalMetaInformation;
		public List<STFMeta> addonMetaInformation = new List<STFMeta>();
	}
}
