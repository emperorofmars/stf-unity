
#if UNITY_EDITOR

using System.IO;
using UnityEditor.Experimental.AssetImporters;
using STF.Serialisation;
using UnityEditor;

namespace STF.Tools
{
	public static class STFDirectoryUtil
	{
		public const string DefaultUnpackFolder = "/STF Imports";

		public static void EnsureDefaultUnpackFolder(string filename)
		{
			if(!Directory.Exists("Assets/" + DefaultUnpackFolder))
			{
				AssetDatabase.CreateFolder("Assets", DefaultUnpackFolder);
				AssetDatabase.Refresh();
			}
			if(!Directory.Exists("Assets/" + DefaultUnpackFolder + "/" + filename))
			{
				AssetDatabase.CreateFolder("Assets/" + DefaultUnpackFolder, filename);
				AssetDatabase.Refresh();
			}
		}
	}
}

#endif
