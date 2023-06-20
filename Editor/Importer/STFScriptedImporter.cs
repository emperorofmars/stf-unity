
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
		public class AddonIdEnabled
		{
			public string AddonId;
			public bool AddonEnabled = true;
		}

		[HideInInspector] public bool SafeImagesExternal = false;
		[HideInInspector] public string OriginalTexturesFolder = "Assets/authoring_stf_external";
		public List<AddonIdEnabled> AddonsEnabled = new List<AddonIdEnabled>();

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
					STFAddonRegistry.RegisterAddon(addonInfo.targetAssetId, addonInfo.GetComponent<STFAssetInfo>());

					foreach(var addon in STFAddonRegistry.GetAddons(asset.Key))
					{
						if(AddonsEnabled.Find(a => a.AddonId == addon.assetId) == null)
						{
							AddonsEnabled.Add(new AddonIdEnabled {AddonId = addon.assetId, AddonEnabled = true});
						}
					}
				}
			}
			
			var objectsToDestroy = new List<UnityEngine.Object>();
			foreach(var asset in importer.GetAssets())
			{
				var unityAsset = asset.Value.GetAsset();
				// apply addons
				var addonList = STFAddonRegistry.GetAddons(asset.Key);
				foreach(var addon in addonList)
				{
					if(AddonsEnabled.Find(a => a.AddonId == addon.assetId)?.AddonEnabled == false) continue;
					unityAsset = AddonApplier.ApplyAddon((GameObject)unityAsset, addon);
					objectsToDestroy.Add(unityAsset);
				}

				foreach(var stage in STFImporterStageRegistry.GetStages())
				{
					if(stage.CanHandle(asset.Value, unityAsset))
					{
						var converted = stage.Convert(asset.Value, unityAsset);
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

			foreach(var o in objectsToDestroy) UnityEngine.Object.DestroyImmediate(o);
		}
	}
}

#endif
