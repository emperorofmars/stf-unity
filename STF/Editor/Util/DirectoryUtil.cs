
#if UNITY_EDITOR

using System.IO;
using UnityEditor;

namespace STF.Tools
{
	public static class STFDirectoryUtil
	{
		public const string DefaultUnpackFolder = "STF Imports";

		public static void EnsureUnpackLocation(string assetPath)
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
		}

		public static string GetFolderName(string assetPath)
		{
			return assetPath.Replace('\\', '_').Replace('/', '_');
		}

		public static string GetUnpackLocation(string assetPath)
		{
			return Path.Combine("Assets", STFDirectoryUtil.DefaultUnpackFolder, GetFolderName(assetPath));
		}
	}
}

#endif
