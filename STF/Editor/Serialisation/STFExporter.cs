
#if UNITY_EDITOR

using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using STF.Util;

namespace STF.Serialisation
{
	// The main star for export!
	public class STFExporter
	{
		private STFExportState state;

		public STFExporter(ISTFAsset MainAsset, List<ISTFAsset> SecondaryAssets, string ExportPath, Dictionary<UnityEngine.Object, UnityEngine.Object> ResourceMeta = null, bool DebugExport = false)
		{
			export(STFRegistry.GetDefaultExportContext(), MainAsset, SecondaryAssets, ExportPath, ResourceMeta, DebugExport);
		}

		public STFExporter(STFExportContext Context, ISTFAsset MainAsset, List<ISTFAsset> SecondaryAssets, string ExportPath, Dictionary<UnityEngine.Object, UnityEngine.Object> ResourceMeta = null, bool DebugExport = false)
		{
			export(Context, MainAsset, SecondaryAssets, ExportPath, ResourceMeta, DebugExport);
		}

		private void export(STFExportContext Context, ISTFAsset MainAsset, List<ISTFAsset> SecondaryAssets, string ExportPath, Dictionary<UnityEngine.Object, UnityEngine.Object> ResourceMeta = null, bool DebugExport = false)
		{
			try
			{
				state = new STFExportState(Context, ExportPath, ResourceMeta);
				state._MainAssetId = MainAsset.Id;

				if(state.Context.AssetExporters.ContainsKey(MainAsset.Type))
				{
					Debug.Log($"Serializing Main Asset: {MainAsset.Type}");
					state.Context.AssetExporters[MainAsset.Type].SerializeToJson(state, MainAsset);
				}
				else
				{
					Debug.LogWarning($"Unrecognized Main Asset: {MainAsset.Type}");
					// Unrecognized Asset
				}
				foreach(var secondaryAsset in SecondaryAssets)
				{
					if(state.Context.AssetExporters.ContainsKey(secondaryAsset.Type))
					{
						Debug.Log($"Serializing Secondary Asset: {secondaryAsset.Type}");
						state.Context.AssetExporters[secondaryAsset.Type].SerializeToJson(state, secondaryAsset);
					}
					else
					{
						Debug.LogWarning($"Unrecognized Secondary Asset: {secondaryAsset.Type}");
						// Unrecognized Asset
					}
				}
				Utils.RunTasks(state.Tasks);

				JObject Json = new JObject
				{
					{"meta", new JObject {
						{"generator", "stf-unity"},
						{"timestamp", DateTime.Now.ToString()},
					}},
					{"main", state.MainAssetId},
					{"assets", new JObject(state.Assets.Select(entry => new JProperty(entry.Value.Id, entry.Value.JsonAsset)))},
					{"nodes", new JObject(state.Nodes.Select(entry => new JProperty(entry.Value.Id, entry.Value.JsonNode)))},
					{"resources", new JObject(state.Resources.Select(entry => new JProperty(entry.Value.Id, entry.Value.JsonResource)))},
					{"buffers", new JArray(state.Buffers.Select(entry => entry.Key))}
				};
				
				var file = new STFFile(Json.ToString(Formatting.None), new List<byte[]>(state.Buffers.Select(entry => entry.Value)));
				File.WriteAllBytes(ExportPath, file.CreateBinaryFromBuffers());
				if(DebugExport) File.WriteAllText(ExportPath + ".json", Json.ToString(Formatting.Indented));
			}
			catch(Exception e)
			{
				Debug.LogError(e);
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
	}
}

#endif
