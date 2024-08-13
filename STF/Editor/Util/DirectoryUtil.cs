
#if UNITY_EDITOR

using System.IO;
using STF.Types;
using UnityEditor;

namespace STF.Tools
{
	public static class STFDirectoryUtil
	{
		public const string DefaultUnpackFolder = "STF Imports";
		public const string ResourceFolder = "Resources";

		public static string GetFolderName(string assetPath)
		{
			return assetPath.Replace('\\', '_').Replace('/', '_');
		}

		public static string GetUnpackLocation(string assetPath)
		{
			return Path.Combine("Assets", STFDirectoryUtil.DefaultUnpackFolder, GetFolderName(assetPath));
		}

		public static string EnsureUnpackLocation(string assetPath)
		{
			var foldername = GetFolderName(assetPath);
			if(!Directory.Exists(Path.Combine("Assets", DefaultUnpackFolder)))
			{
				AssetDatabase.CreateFolder("Assets", DefaultUnpackFolder);
				AssetDatabase.Refresh();
			}
			if(!Directory.Exists(Path.Combine("Assets", DefaultUnpackFolder, foldername)))
			{
				AssetDatabase.CreateFolder(Path.Combine("Assets", DefaultUnpackFolder), foldername);
				AssetDatabase.Refresh();
			}
			if(!Directory.Exists(Path.Combine("Assets", DefaultUnpackFolder, foldername, ResourceFolder)))
			{
				AssetDatabase.CreateFolder(Path.Combine("Assets", DefaultUnpackFolder, foldername), ResourceFolder);
				AssetDatabase.Refresh();
			}
			return GetUnpackLocation(assetPath);
		}

		public static string EnsureConvertLocation(ISTFAsset Asset, string TargetName)
		{
			string path = null;
			if(PrefabUtility.IsPartOfAnyPrefab(Asset))
			{
				path = Path.GetDirectoryName(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(Asset));
				if(!Directory.Exists(path))
				{
					AssetDatabase.CreateFolder(path, DefaultUnpackFolder);
					AssetDatabase.Refresh();
				}
				path = Path.Combine(path, DefaultUnpackFolder);
			}
			else
			{
				EnsureUnpackLocation("__TMP");
				path = GetUnpackLocation("__TMP");
			}
			if(!Directory.Exists(Path.Combine(path, TargetName)))
			{
				AssetDatabase.CreateFolder(path, TargetName);
				AssetDatabase.Refresh();
			}
			return Path.Combine(path, TargetName);
		}
	}
}

#endif
