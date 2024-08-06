
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
			STFDirectoryUtil.EnsureUnpackLocation(assetPath);

			var importInfo = STFImportInfo.CreateInstance(new STFFile(ctx.assetPath), ctx.assetPath);

			ctx.AddObjectToAsset("main", importInfo);
			if(importInfo.Preview) ctx.AddObjectToAsset("preview", importInfo.Preview);
			ctx.SetMainObject(importInfo);

			//new STFImporter(STFDirectoryUtil.GetUnpackLocation(assetPath), assetPath);
		}
	}
}

#endif
