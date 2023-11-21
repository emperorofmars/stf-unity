
#if UNITY_EDITOR

using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Text;
using STF.IdComponents;
using UnityEditor;
using UnityEditorInternal;
using Newtonsoft.Json;

namespace STF.Serde
{
	// The main star for import!
	// Parses the Json and buffers based on the provided importers from the STFImportContext.
	public class STFExporter
	{
		private STFExportState state;

		public STFExporter(STFAsset MainAsset, List<STFAsset> SecondaryAssets, string ExportPath, bool DebugExport = false)
		{
			export(STFRegistry.GetDefaultExportContext(), MainAsset, SecondaryAssets, ExportPath, DebugExport);
		}

		public STFExporter(STFExportContext Context, STFAsset MainAsset, List<STFAsset> SecondaryAssets, string ExportPath, bool DebugExport = false)
		{
			export(Context, MainAsset, SecondaryAssets, ExportPath, DebugExport);
		}

		private void export(STFExportContext Context, STFAsset MainAsset, List<STFAsset> SecondaryAssets, string ExportPath, bool DebugExport = false)
		{
			try
			{
				state = new STFExportState(Context, ExportPath);
				state._MainAssetId = MainAsset.assetInfo.assetId;

				if(state.Context.AssetExporters.ContainsKey(MainAsset.assetInfo.assetType))
				{
					Debug.Log($"Serializing Main Asset: {MainAsset.assetInfo.assetType}");
					state.Context.AssetExporters[MainAsset.assetInfo.assetType].SerializeToJson(state, MainAsset);
				}
				else
				{
					Debug.LogWarning($"Unrecognized Main Asset: {MainAsset.assetInfo.assetType}");
					// Unrecognized Asset
				}
				foreach(var secondaryAsset in SecondaryAssets)
				{
					if(state.Context.AssetExporters.ContainsKey(secondaryAsset.assetInfo.assetType))
					{
						Debug.Log($"Serializing Secondary Asset: {secondaryAsset.assetInfo.assetType}");
						state.Context.AssetExporters[secondaryAsset.assetInfo.assetType].SerializeToJson(state, secondaryAsset);
					}
					else
					{
						Debug.LogWarning($"Unrecognized Secondary Asset: {secondaryAsset.assetInfo.assetType}");
						// Unrecognized Asset
					}
				}
				_runTasks();

				JObject Json = new JObject
				{
					{"meta", new JObject {
						{"generator", "stf-unity"},
						{"timestamp", DateTime.Now.ToString()},
					}},
					{"main", state.MainAssetId},
					{"assets", new JObject(state.Assets.Select(entry => new JProperty(entry.Value.Key, entry.Value.Value)))},
					{"nodes", new JObject(state.Nodes.Select(entry => new JProperty(entry.Value.Key, entry.Value.Value)))},
					{"resources", new JObject(state.Resources.Select(entry => new JProperty(entry.Value.Key, entry.Value.Value)))},
					{"buffers", new JArray(state.Buffers.Select(entry => entry.Key))}
				};

				
				var file = new STFFile(Json.ToString(Formatting.None), new List<byte[]>(state.Buffers.Select(entry => entry.Value)));
				File.WriteAllBytes(ExportPath, file.CreateBinaryFromBuffers());
				if(DebugExport) File.WriteAllText(ExportPath + ".json", Json.ToString(Formatting.Indented));
			}
			catch(Exception e)
			{
				foreach(var node in state.Nodes.Keys)
				{
					if(node != null)
					{
						UnityEngine.Object.DestroyImmediate(node);
					}
				}
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
			}
		}

		private void _runTasks()
		{
			do
			{
				var currentTasks = state.Tasks;
				state.Tasks = new List<Task>();
				foreach(var task in currentTasks)
				{
					task.RunSynchronously();
					if(task.Exception != null) throw task.Exception;
				}
			}
			while(state.Tasks.Count > 0);
		}
	}
}

#endif
