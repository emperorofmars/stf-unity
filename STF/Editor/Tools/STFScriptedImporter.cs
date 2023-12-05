
#if UNITY_EDITOR

using System.IO;
using UnityEditor.Experimental.AssetImporters;
using STF.Serialisation;
using UnityEditor;

namespace STF.Tools
{
	// A scripted importer for STF files. It only parses the basic info about the contained assets, the full import happens on an explicit user action. Full import puts everything into the Assets folder.
	[ScriptedImporter(1, new string[] {"stf"})]
	public class STFScriptedImporter : ScriptedImporter
	{
		public string UnpackFolder;

		public override void OnImportAsset(AssetImportContext ctx)
		{
			var importInfo = STFImportInfo.CreateInstance(new STFFile(ctx.assetPath));

			ctx.AddObjectToAsset("main", importInfo);
			ctx.SetMainObject(importInfo);

			STFDirectoryUtil.EnsureDefaultUnpackFolder(Path.GetFileNameWithoutExtension(ctx.assetPath));
		}
	}
}

#endif
