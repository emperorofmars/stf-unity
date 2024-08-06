
#if UNITY_EDITOR

using System.IO;

using STF.Serialisation;
using UnityEditor;
using UnityEngine;

namespace STF.Tools
{
	// A scripted importer for STF files. It only parses the basic info about the contained assets, the full import happens on an explicit user action. Full import puts everything into the Assets folder.
	[UnityEditor.AssetImporters.ScriptedImporter(1, new string[] {"stf"})]
	public class STFScriptedImporter : UnityEditor.AssetImporters.ScriptedImporter
	{
		public override void OnImportAsset(UnityEditor.AssetImporters.AssetImportContext ctx)
		{
			var importInfo = STFImportInfo.CreateInstance(new STFFile(ctx.assetPath), ctx.assetPath);
			ctx.AddObjectToAsset("main", importInfo);
			if(importInfo.Preview) ctx.AddObjectToAsset("preview", importInfo.Preview);
			ctx.SetMainObject(importInfo);

			//new STFUnpackingImporter(STFDirectoryUtil.GetUnpackLocation(assetPath), assetPath);
			
			/*var Importer = new STFRuntimeImporter(ctx.assetPath);
			Debug.Log(Importer.Asset);
			ctx.AddObjectToAsset("main", Importer.Asset.gameObject);
			foreach(var resource in Importer.STFResources)
			{
				ctx.AddObjectToAsset("resource", resource);
			}
			foreach(var resource in Importer.UnityResources)
			{
				if(resource != null) ctx.AddObjectToAsset("resource", resource);
			}
			ctx.SetMainObject(Importer.Asset.gameObject);*/
		}
	}
}

#endif
