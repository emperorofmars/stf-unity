
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
		//public bool ConvertImmediately = true;
		//public int ConverterSelection = 0;
		
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
				foreach(var resource in unityContext.AssetCtxObjects)
				{
					if(resource != null) ctx.AddObjectToAsset("resource" + resource.GetInstanceID(), resource);
				}

				/*if(ConvertImmediately && ConverterSelection >= 0 && ConverterSelection < STFRegistry.ApplicationConverters.Count)
				{
					var converter = STFRegistry.ApplicationConverters[ConverterSelection];
					if(!converter.CanConvert(asset)) throw new System.Exception($"{converter.TargetName} Can't convert imported asset!");

					var path = DirectoryUtil.EnsureConvertLocation(asset, converter.TargetName);
					var converted = converter.Convert(new STFApplicationConvertStorageContext(path), asset); // TODO make a runtime variant of this
					ctx.AddObjectToAsset("converted", converted);
					ctx.SetMainObject(converted);
				}
				else
				{*/
					ctx.SetMainObject(asset.gameObject);
				//}
			}
		}
	}
}

#endif
