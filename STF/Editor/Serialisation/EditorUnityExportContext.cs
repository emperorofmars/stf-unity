
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEditor;
using System.Threading.Tasks;
using System.IO;

namespace STF.Serialisation
{
	public class EditorUnityExportContext : IUnityExportContext
	{
		public string TargetLocation;

		public EditorUnityExportContext(string TargetLocation)
		{
			this.TargetLocation = TargetLocation;
		}

		public T LoadMeta<T>(UnityEngine.Object Resource) where T: ISTFResource
		{
			var assetPath = AssetDatabase.GetAssetPath(Resource);
			var metaPath = Path.ChangeExtension(assetPath, "Asset");
			return AssetDatabase.LoadAssetAtPath<T>(metaPath);
		}
		public (byte[], T, string) LoadAsset<T>(UnityEngine.Object Resource) where T: ISTFResource
		{
			var assetPath = AssetDatabase.GetAssetPath(Resource);
			var arrayBuffer = File.ReadAllBytes(assetPath);
			var meta = AssetDatabase.LoadAssetAtPath<T>(Path.ChangeExtension(assetPath, "Asset"));
			return (arrayBuffer, meta, Path.GetFileName(assetPath));
		}
	}
}

#endif
