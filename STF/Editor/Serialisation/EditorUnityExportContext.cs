
#if UNITY_EDITOR

using UnityEditor;
using System.IO;
using STF.Types;

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
			return AssetDatabase.LoadAssetAtPath<T>(Path.ChangeExtension(assetPath, "Asset"));
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
