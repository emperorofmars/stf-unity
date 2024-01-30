#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEngine;

namespace STF.ApplicationConversion
{
	public class STFApplicationConvertStorageContext : ISTFApplicationConvertStorageContext
	{
		public string _TargetPath;
		public string TargetPath => _TargetPath;

		public STFApplicationConvertStorageContext(string TargetPath)
		{
			_TargetPath = TargetPath;
		}

		public void SaveGeneratedResource(Object Resource, string FileExtension)
		{
			if(!FileExtension.StartsWith(".")) FileExtension = "." + FileExtension;
			AssetDatabase.CreateAsset(Resource, Path.Combine(TargetPath, Resource.name + FileExtension));
		}

		public Object DuplicateResource(Object Resource)
		{
			var path = AssetDatabase.GetAssetPath(Resource);
			var resourceTargetPath = Path.Combine(_TargetPath, Path.GetFileNameWithoutExtension(path) + "_Converted" + Path.GetExtension(path));
			File.WriteAllBytes(resourceTargetPath, File.ReadAllBytes(path));
			AssetDatabase.Refresh();
			return AssetDatabase.LoadAssetAtPath(resourceTargetPath, Resource.GetType());
		}

		public void SavePrefab(GameObject Go, string Name = null)
		{
			PrefabUtility.SaveAsPrefabAssetAndConnect(Go, Path.Combine(TargetPath, Name != null && Name.Length > 0 ? Name : Go.name) + ".prefab", InteractionMode.AutomatedAction);
		}

		public void SaveEverything()
		{
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
	}
}

#endif
