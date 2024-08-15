
#if UNITY_EDITOR

using STF.ApplicationConversion;
using STF.Serialisation;
using UnityEngine;

namespace STF.Tools
{
	/*
		The scripted importer for STF files.
		By default it only parses the basic info about the contained assets, the full import happens on an explicit user action. Full import puts everything under the Assets/STF Imports/ folder.
		If `UnpackingImport` is set to false, it will import the file immediately into the asset context. Due to Unity limitations this will degrade the import. It may be perfectly usable for further use in Unity, but not for authoring STF files.
	*/
	[UnityEditor.AssetImporters.ScriptedImporter(1, new string[] {"stf"})]
	public class STFScriptedImporter : UnityEditor.AssetImporters.ScriptedImporter
	{
		public bool UnpackingImport = true;
		public STFImportInfo AssetInfo;

		public override void OnImportAsset(UnityEditor.AssetImporters.AssetImportContext ctx)
		{
			AssetInfo = new STFImportInfo(new STFFile(ctx.assetPath));
			
			if(UnpackingImport)
			{
				if(AssetInfo.Preview)
				{
					ctx.AddObjectToAsset("preview", AssetInfo.Preview);
					ctx.SetMainObject(AssetInfo.Preview);
				}
				// TODO: else use a generic logo
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
