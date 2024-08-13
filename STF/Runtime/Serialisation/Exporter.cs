
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
	public class Exporter
	{
		public static (STFFile stfFIle, JObject Json) Export(IUnityExportContext UnityContext, ISTFAsset Asset, STFResourceMeta ResourceMeta = null)
		{
			return Export(STFRegistry.GetDefaultExportContext(), UnityContext, Asset, ResourceMeta);
		}

		public static (STFFile stfFIle, JObject Json) Export(STFExportContext STFContext, IUnityExportContext UnityContext, ISTFAsset Asset, STFResourceMeta ResourceMeta = null)
		{
			var state = new STFExportState(STFContext, UnityContext, ResourceMeta);
			try
			{
				JObject JsonAsset = null;
				if(state.Context.AssetExporters.ContainsKey(Asset.Type))
				{
					Debug.Log($"Serializing Asset Type: {Asset.Type}");
					JsonAsset = state.Context.AssetExporters[Asset.Type].SerializeToJson(state, Asset);
				}
				else
				{
					//Debug.LogWarning($"Unrecognized Asset Type: {Asset.Type}");
					throw new Exception($"Can't export unrecognized Asset Type: {Asset.Type}");
				}
				Utils.RunTasks(state.Tasks);

				JsonAsset.Add("generator", "stf-unity");
				JsonAsset.Add("timestamp", DateTime.Now.ToString());
				JObject Json = new JObject
				{
					{"asset", JsonAsset},
					{"nodes", new JObject(state.Nodes.Select(entry => new JProperty(entry.Value.Id, entry.Value.JsonNode)))},
					{"resources", new JObject(state.Resources.Select(entry => new JProperty(entry.Value.Id, entry.Value.JsonResource)))},
					{"buffers", new JArray(state.Buffers.Select(entry => entry.Key))}
				};
				
				return (new STFFile(Json.ToString(Formatting.None), new List<byte[]>(state.Buffers.Select(entry => entry.Value))), Json);
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
				throw new Exception("Error during STF export: ", e);
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
