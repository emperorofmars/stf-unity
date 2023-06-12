
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using stf.serialisation;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;


namespace stf
{
	[ScriptedImporter(1, new string[] {"stf"})]
	public class STFScriptedImporter : ScriptedImporter
	{
		[Serializable]
		public class IdToAddon
		{
			public string TargetId;
			public List<STFAddonAssetInfo> Addons = new List<STFAddonAssetInfo>();
		}

		//[HideInInspector] public bool AuthoringMode = false;
		[HideInInspector] public bool SafeImagesExternal = false;
		[HideInInspector] public string OriginalTexturesFolder = "Assets/authoring_stf_external";

		public List<IdToAddon> Addons = new List<IdToAddon>();

		private void ensureTexturePath()
		{
			if(!Directory.Exists(OriginalTexturesFolder))
			{
				Directory.CreateDirectory(OriginalTexturesFolder);
				AssetDatabase.Refresh();
			}
		}
		
		public override void OnImportAsset(AssetImportContext ctx)
		{
			byte[] byteArray = File.ReadAllBytes(ctx.assetPath);

			var context = STFRegistry.GetDefaultImportContext();
			if(SafeImagesExternal)
			{
				var image_bullshit = new STFEncodedImageTextureImporter();
				ensureTexturePath();
				image_bullshit.imageParentPath = OriginalTexturesFolder;
				context.ResourceImporters[STFEncodedImageTextureImporter._TYPE] = image_bullshit;
			}
			var importer = new STFImporter(byteArray, context);
			
			// Add All Resources
			foreach(var resource in importer.GetResources())
			{
				if(resource != null)
				{
					if(resource.GetType() == typeof(Mesh))
						ctx.AddObjectToAsset("meshes/" + resource.name + ".mesh", resource);
					else if(resource.GetType() == typeof(Texture2D))
						ctx.AddObjectToAsset("textures/" + resource.name + ".texture2d", resource);
					else
						ctx.AddObjectToAsset(resource.name, resource);
				}
			}
			foreach(var asset in importer.GetAssets())
			{
				ctx.AddObjectToAsset(asset.Key, asset.Value.GetAsset());

				// Handle Addons
				if(asset.Value.GetType() == typeof(STFAddonAsset))
				{
					var addonInfo = (asset.Value.GetAsset() as GameObject).GetComponent<STFAddonAssetInfo>();

					Debug.Log($"Addon: {addonInfo.targetAssetId}");

					var addonList = Addons.Find(k => k.TargetId == addonInfo.targetAssetId);
					if(addonList != null) addonList.Addons.Add(addonInfo);
					else Addons.Add(new IdToAddon {TargetId = addonInfo.targetAssetId, Addons = new List<STFAddonAssetInfo> {addonInfo}});
				}
				
				foreach(var stage in STFImporterStageRegistry.GetStages())
				{
					if(stage.CanHandle(asset.Value))
					{
						var converted = stage.Convert(asset.Value);
						foreach(var resource in converted.resources)
						{
							if(resource != null)
							{
								if(resource.GetType() == typeof(Mesh))
									ctx.AddObjectToAsset("meshes/" + resource.name + ".mesh", resource);
								else if(resource.GetType() == typeof(Texture2D))
									ctx.AddObjectToAsset("textures/" + resource.name + ".texture2d", resource);
								else
									ctx.AddObjectToAsset(resource.name, resource);
							}
						}
						foreach(var subAsset in converted.assets)
						{
							ctx.AddObjectToAsset(subAsset.GetSTFAssetName(), subAsset.GetAsset());
							var metaInfo = importer.GetMeta().importedRawAssets.Find(a => a.assetId == asset.Key);
							metaInfo.secondStageAssets.Add(new STFMeta.AssetInfo {assetId = subAsset.getId(), assetName = subAsset.GetSTFAssetName(), assetType = subAsset.GetSTFAssetType(), assetRoot = subAsset.GetAsset(), visible = true});
						}
					}
				}
			}
			ctx.SetMainObject(importer.GetAssets()[importer.mainAssetId].GetAsset());

			importer.GetMeta().name = "STFMeta";
			var icon = new Texture2D(256, 256);
			icon.LoadImage(STFIcon.icon_png_array.ToArray());

			ctx.AddObjectToAsset("STFMeta", importer.GetMeta(), icon);
		}
	}
}

#endif
