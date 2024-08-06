#if UNITY_EDITOR

using System;
using System.Linq;

namespace STF.Serialisation
{
	// Parses the Json and buffers based on the provided importers from the STFImportContext.
	public class SingleResourceImporter
	{
		//private STFImportState state;

		public SingleResourceImporter(STFFile Buffers)
		{
			Parse(STFRegistry.GetDefaultImportContext(), Buffers);
		}

		public SingleResourceImporter(STFImportContext Context, STFFile Buffers)
		{
			Parse(Context, Buffers);
		}

		private void Parse(STFImportContext Context, STFFile Buffers)
		{
			try
			{
				/*state = new RuntimeImportState(Context, JObject.Parse(Buffers.Json));

				ParseBuffers(buffers);
				ParseResources();
				Utils.RunTasks(state.Tasks);
				var Asset = ParseAsset();
				Utils.RunTasks(state.Tasks);
				Utils.RunTasks(state.PostprocessTasks);
				RunPostProcessors();
				Utils.RunTasks(state.Tasks);

				var path = System.IO.Path.Combine(TargetLocation, Asset.Name + ".Prefab");
				PrefabUtility.SaveAsPrefabAsset(Asset.gameObject, path);

				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();*/
			}
			catch(Exception e)
			{
				throw new Exception("Error during STF import: ", e);
			}
			finally
			{
				/*foreach(var trashObject in state.Trash)
				{
					if(trashObject != null)
					{
						UnityEngine.Object.DestroyImmediate(trashObject);
					}
				}*/
			}
		}

		private void ParseBuffers(STFFile buffers)
		{
			for(int i = 0; i < buffers.Buffers.Count(); i++)
			{
				//state.Buffers.Add((string)state.JsonRoot["buffers"][i], buffers.Buffers[i]);
			}
		}

		private void ParseResources()
		{
			/*foreach(var entry in (JObject)state.JsonRoot["resources"])
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
			}*/
		}

		private void RunPostProcessors()
		{
			/*foreach(var postProcessor in state.Context.ImportPostProcessors)
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
			}*/
		}
	}
}

#endif
