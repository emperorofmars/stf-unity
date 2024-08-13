
#if UNITY_EDITOR

using STF.Serialisation;
using UnityEditor;
using UnityEngine;

namespace STF.Tools
{
	// A scripted importer for STF files. It only parses the basic info about the contained assets, the full import happens on an explicit user action. Full import puts everything into the Assets folder.
	[UnityEditor.AssetImporters.ScriptedImporter(1, new string[] {"stf"})]
	public class STFScriptedImporter : UnityEditor.AssetImporters.ScriptedImporter
	{
		public bool UnpackingImport = true;
		public STFImportInfo AssetInfo;

		public override void OnImportAsset(UnityEditor.AssetImporters.AssetImportContext ctx)
		{
			AssetInfo = new STFImportInfo(new STFFile(ctx.assetPath), ctx.assetPath);
			
			if(UnpackingImport)
			{
				AssetInfo = new STFImportInfo(new STFFile(ctx.assetPath), ctx.assetPath);
				if(AssetInfo.Preview)
				{
					ctx.AddObjectToAsset("preview", AssetInfo.Preview);
					ctx.SetMainObject(AssetInfo.Preview);
				}
				// TODO: else use a generic logo

				// Auto Unpack?
			}
			else
			{
				var unityContext = new RuntimeUnityImportContext();
				var (asset, _state) = Importer.Parse(unityContext, ctx.assetPath);
				ctx.AddObjectToAsset("main", asset.gameObject);
				ctx.SetMainObject(asset.gameObject);
				foreach(var resource in unityContext.AssetCtxObjects)
				{
					if(resource != null) ctx.AddObjectToAsset("resource" + resource.GetInstanceID(), resource);
				}
			}
		}
	}
}

#endif
