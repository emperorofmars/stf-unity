
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
		[Serializable]
		public class AddonExternalEnabled : AddonIdEnabled
		{
			public STFMeta Origin;
		}

		public bool SafeImagesExternal = false;
		public string OriginalTexturesFolder = "Assets/authoring_stf_external";
		public List<AddonIdEnabled> AddonsEnabled = new List<AddonIdEnabled>();
		public List<AddonExternalEnabled> ExternalAddonsEnabled = new List<AddonExternalEnabled>();

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
			}

			// Handle internal addons
			var addons = importer.GetMeta().importedRawAssets.FindAll(a => a.assetType == "addon");
			if(addons != null)
			{
				foreach(var addon in addons)
				{
					if(AddonsEnabled.Find(a => a.AddonId == addon.assetId) == null)
						AddonsEnabled.Add(new AddonIdEnabled {AddonId = addon.assetId, AddonEnabled = false});
				}
			}

			// Handle addons from the entire Unity project
			var externalAddons = STFAddonUtil.GatherAddons(importer.GetMeta());
			foreach(var externalAddon in externalAddons)
			{
				if(ExternalAddonsEnabled.Find(a => a.AddonId == externalAddon.Addon.assetId && a.Origin == externalAddon.Origin) == null)
					ExternalAddonsEnabled.Add(new AddonExternalEnabled {AddonId = externalAddon.Addon.assetId, Origin = externalAddon.Origin, AddonEnabled = false});
			}
			
			var objectsToDestroy = new List<UnityEngine.Object>();
			foreach(var asset in importer.GetAssets())
			{
				var unityAsset = asset.Value.GetAsset();

				// Apply internal addons
				if(addons != null)
				{
					foreach(var addon in addons)
					{
						var addonInfo = ((GameObject)addon.assetRoot).GetComponent<STFAddonAssetInfo>();
						var enabled = AddonsEnabled.Find(a => a.AddonId == addon.assetId);
						if(addonInfo != null && addonInfo.targetAssetId == asset.Key && enabled != null && enabled.AddonEnabled == true)
						{
							unityAsset = AddonApplier.ApplyAddon((GameObject)unityAsset, ((GameObject)addon.assetRoot).GetComponent<STFAddonAssetInfo>());
							objectsToDestroy.Add(unityAsset);
						}
					}
				}
				// Apply external addons
				foreach(var addon in externalAddons)
				{
					var addonInfo = ((GameObject)addon.Addon.assetRoot).GetComponent<STFAddonAssetInfo>();
					var enabled = ExternalAddonsEnabled.Find(a => a.AddonId == addon.Addon.assetId && a.Origin == addon.Origin);
					if(addonInfo != null && addonInfo.targetAssetId == asset.Key && enabled != null && enabled.AddonEnabled == true)
					{
						unityAsset = AddonApplier.ApplyAddon((GameObject)unityAsset, ((GameObject)addon.Addon.assetRoot).GetComponent<STFAddonAssetInfo>());
						objectsToDestroy.Add(unityAsset);
					}
				}

				// Run second stages
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
