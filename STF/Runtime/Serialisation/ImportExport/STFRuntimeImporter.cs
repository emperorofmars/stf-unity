
using UnityEngine;
using System;
using System.IO;
using Newtonsoft.Json.Linq;
using static STF.Util.STFConstants;
using System.Collections.Generic;
using System.Linq;
using STF.Util;

namespace STF.Serialisation
{
	// The main star for import!
	// Parses the Json and buffers based on the provided importers from the STFImportContext.
	public class STFRuntimeImporter
	{
		private STFImportState state;
		private RuntimeUnityImportContext unityContext;

		public ISTFAsset Asset;
		public List<ISTFResource> STFResources => state.Resources.Values.ToList();
		public List<UnityEngine.Object> UnityResources => unityContext.AssetCtxObjects;

		public STFRuntimeImporter(string ImportPath)
		{
			Parse(STFRegistry.GetDefaultImportContext(), ImportPath);
		}

		public STFRuntimeImporter(STFImportContext Context, string ImportPath)
		{
			Parse(Context, ImportPath);
		}

		private void Parse(STFImportContext Context, string ImportPath)
		{
			try
			{
				var buffers = new STFFile(ImportPath);
				unityContext = new RuntimeUnityImportContext();
				state = new STFImportState(Context, unityContext, buffers);

				ParseResources();
				Utils.RunTasks(state.Tasks);
				Asset = ParseAsset();
				Asset.OriginalFileName = Path.GetFileNameWithoutExtension(ImportPath);
				Asset.gameObject.name = Asset.name;
				Asset.Degraded = true;
				Utils.RunTasks(state.Tasks);
				Utils.RunTasks(state.PostprocessTasks);
				RunPostProcessors();
				Utils.RunTasks(state.Tasks);
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

