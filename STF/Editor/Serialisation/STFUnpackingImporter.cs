#if UNITY_EDITOR

using UnityEngine;
using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using STF.Util;
using static STF.Serialisation.STFConstants;

namespace STF.Serialisation
{
	// The main star for import!
	// Parses the Json and buffers based on the provided importers from the STFImportContext.
	public class STFUnpackingImporter
	{
		private STFImportState state;

		public STFUnpackingImporter(string TargetLocation, string ImportPath)
		{
			Parse(STFRegistry.GetDefaultImportContext(), TargetLocation, ImportPath);
		}

		public STFUnpackingImporter(STFImportContext Context, string TargetLocation, string ImportPath)
		{
			Parse(Context, TargetLocation, ImportPath);
		}

		private void Parse(STFImportContext Context, string TargetLocation, string ImportPath)
		{
			try
			{
				var buffers = new STFFile(ImportPath);
				var unityContext = new EditorUnityImportContext(TargetLocation);
				state = new STFImportState(Context, unityContext, buffers);

				EnsureFolderStructure(TargetLocation);
				ParseResources();
				Utils.RunTasks(state.Tasks);
				var Asset = ParseAsset();
				Asset.OriginalFileName = Path.GetFileNameWithoutExtension(ImportPath);
				Utils.RunTasks(state.Tasks);
				Utils.RunTasks(state.PostprocessTasks);
				RunPostProcessors();
				Utils.RunTasks(state.Tasks);

				var path = Path.Combine(TargetLocation, Asset.Name + ".Prefab");
				PrefabUtility.SaveAsPrefabAsset(Asset.gameObject, path);

				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}
			catch(Exception e)
			{
				throw new Exception("Error during STF import: ", e);
			}
			finally
			{
				foreach(var trashObject in state.Trash)
				{
					if(trashObject != null)
					{
						UnityEngine.Object.DestroyImmediate(trashObject);
					}
				}
				foreach(var trashObject in state.Nodes.Values)
				{
					if(trashObject != null)
					{
						UnityEngine.Object.DestroyImmediate(trashObject);
					}
				}
			}
		}

		private void EnsureFolderStructure(string TargetLocation)
		{
			var existingEntries = Directory.EnumerateFileSystemEntries(TargetLocation); foreach(var entry in existingEntries)
			{
				if(File.Exists(entry)) File.Delete(entry);
				else Directory.Delete(entry, true);
			}
			AssetDatabase.Refresh();
			AssetDatabase.CreateFolder(TargetLocation, STFConstants.ResourceDirectoryName);
			AssetDatabase.CreateFolder(TargetLocation, STFConstants.PreservedBuffersDirectoryName);
			AssetDatabase.Refresh();
		}

		private void ParseResources()
		{
			foreach(var entry in (JObject)state.JsonRoot["resources"])
			{
				var type = (string)entry.Value["type"];
				if(state.Context.ResourceImporters.ContainsKey(type))
				{
					state.Context.ResourceImporters[type].ParseFromJson(state, (JObject)entry.Value, entry.Key);
				}
				else
				{
					Debug.LogWarning($"Unrecognized Resource: {type}");
					STFUnrecognizedResourceImporter.ParseFromJson(state, (JObject)entry.Value, entry.Key);
				}
			}
		}

		private ISTFAsset ParseAsset()
		{
			var type = (string)state.JsonRoot["asset"]["type"];
			if(state.Context.AssetImporters.ContainsKey(type))
			{
				Debug.Log($"Parsing Asset: {type}");
				return state.Context.AssetImporters[type].ParseFromJson(state, (JObject)state.JsonRoot["asset"]);
			}
			else
			{
				Debug.LogWarning($"Unrecognized Asset: {type}");
				// Unrecognized Asset
				throw new Exception($"Parsing Unrecognized Asset Not Yet Supported: {type}");
			}
		}

		private void RunPostProcessors()
		{
			foreach(var postProcessor in state.Context.ImportPostProcessors)
			{
				switch(postProcessor.STFObjectType)
				{
					case STFObjectType.Asset:
						break;
					case STFObjectType.Node:
						break;
					case STFObjectType.NodeComponent:
						break;
					case STFObjectType.Resource:
						foreach(var r in state.Resources)
						{
							if(r.Value.GetType() == postProcessor.TargetType)
							{
								postProcessor.PostProcess(state, r.Value);
							}
						}
						break;
					case STFObjectType.ResourceComponent:
						break;
				}
			}
		}
	}
}

#endif
