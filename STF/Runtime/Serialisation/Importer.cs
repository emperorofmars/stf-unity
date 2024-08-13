
using UnityEngine;
using System;
using Newtonsoft.Json.Linq;
using static STF.Util.STFConstants;
using STF.Util;

namespace STF.Serialisation
{
	// The main star for import!
	// Parses the Json and buffers based on the provided importers from the STFImportContext.
	public class Importer
	{
		public static (ISTFAsset Asset, STFImportState State) Parse(IUnityImportContext UnityContext, string ImportPath)
		{
			return Parse(STFRegistry.GetDefaultImportContext(), UnityContext, new STFFile(ImportPath));
		}

		public static (ISTFAsset Asset, STFImportState State) Parse(STFImportContext STFContext, IUnityImportContext UnityContext, string ImportPath)
		{
			return Parse(STFContext, UnityContext, new STFFile(ImportPath));
		}

		public static (ISTFAsset Asset, STFImportState State) Parse(STFImportContext Context, IUnityImportContext UnityContext, STFFile Buffers)
		{
			var state = new STFImportState(Context, UnityContext, Buffers);
			try
			{
				ParseResources(state);
				Utils.RunTasks(state.Tasks);
				var Asset = ParseAsset(state);
				Asset.OriginalFileName = Buffers.OriginalFileName;
				Asset.gameObject.name = Asset.name;
				Asset.Degraded = true;
				Utils.RunTasks(state.Tasks);
				Utils.RunTasks(state.PostprocessTasks);
				RunPostProcessors(state);
				Utils.RunTasks(state.Tasks);

				return (Asset, state);
			}
			catch(Exception e)
			{
				foreach(var trashObject in state.Nodes.Values)
				{
					if(trashObject != null)
					{
						UnityEngine.Object.DestroyImmediate(trashObject);
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

		private static ISTFAsset ParseAsset(STFImportState State)
		{
			var type = (string)State.JsonRoot["asset"]["type"];
			if(State.Context.AssetImporters.ContainsKey(type))
			{
				Debug.Log($"Parsing Asset: {type}");
				return State.Context.AssetImporters[type].ParseFromJson(State, (JObject)State.JsonRoot["asset"]);
			}
			else
			{
				//Debug.LogWarning($"Unrecognized Asset: {type}");
				throw new Exception($"Parsing unrecognized Assets is not yet supported: {type}");
			}
		}

		private static void ParseResources(STFImportState State)
		{
			foreach(var entry in (JObject)State.JsonRoot["resources"])
			{
				var type = (string)entry.Value["type"];
				if(State.Context.ResourceImporters.ContainsKey(type))
				{
					State.Context.ResourceImporters[type].ParseFromJson(State, (JObject)entry.Value, entry.Key);
				}
				else
				{
					Debug.LogWarning($"Unrecognized Resource: {type}");
					STFUnrecognizedResourceImporter.ParseFromJson(State, (JObject)entry.Value, entry.Key);
				}
			}
		}

		private static void RunPostProcessors(STFImportState State)
		{
			foreach(var postProcessor in State.Context.ImportPostProcessors)
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
						foreach(var r in State.Resources)
						{
							if(r.Value.GetType() == postProcessor.TargetType)
							{
								postProcessor.PostProcess(State, r.Value);
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

