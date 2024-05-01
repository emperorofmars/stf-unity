
#if UNITY_EDITOR

using System.IO;

using STF.Serialisation;
using UnityEditor;

namespace STF.Tools
{
	// A scripted importer for STF files. It only parses the basic info about the contained assets, the full import happens on an explicit user action. Full import puts everything into the Assets folder.
	[UnityEditor.AssetImporters.ScriptedImporter(1, new string[] {"stf"})]
	public class STFScriptedImporter : UnityEditor.AssetImporters.ScriptedImporter
	{
		public override void OnImportAsset(UnityEditor.AssetImporters.AssetImportContext ctx)
		{
			var importInfo = STFImportInfo.CreateInstance(new STFFile(ctx.assetPath));

			ctx.AddObjectToAsset("main", importInfo);
			ctx.SetMainObject(importInfo);

			STFDirectoryUtil.EnsureUnpackLocation(assetPath);

			//new STFImporter(STFDirectoryUtil.GetUnpackLocation(assetPath), assetPath);
		}
	}
}

#endif
