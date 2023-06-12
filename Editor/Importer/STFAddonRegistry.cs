
#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using System.Linq;
using stf.serialisation;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;


namespace stf
{
	public static class STFAddonRegistry
	{
		public static Dictionary<string, List<STFAddonAsset>> addons = new Dictionary<string, List<STFAddonAsset>>();
	}
}

#endif
