
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
using UnityScript.Steps;
using Serilog;
using UnityEditorInternal;

namespace STF.Serde
{
	// The main star for import!
	// Parses the Json and buffers based on the provided importers from the STFImportContext.
	public class STFImporter
	{
		private STFImportState state;

		public STFImporter(string TargetLocation, string Path)
		{
			Parse(STFRegistry.GetDefaultImportContext(), TargetLocation, Path);
		}

		public STFImporter(STFImportContext Context, string TargetLocation, string Path)
		{
			Parse(Context, TargetLocation, Path);
		}

		private void Parse(STFImportContext Context, string TargetLocation, string Path)
		{
			try
			{
				var buffers = new STFFile(Path);
				this.state = new STFImportState(Context, TargetLocation, JObject.Parse(buffers.Json));
			
				EnsureFolderStructure();

				ParseBuffers(buffers);
				ParseResources();
				_runTasks();
				ParseAssets();
				_runTasks();
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
			}
		}

		private void EnsureFolderStructure()
		{
			var existingEntries = Directory.EnumerateFileSystemEntries(state.TargetLocation);foreach(var entry in existingEntries)
			{
				if(File.Exists(entry)) File.Delete(entry);
				else Directory.Delete(entry, true);
			}
			AssetDatabase.CreateFolder(state.TargetLocation, STFConstants.ResourceDirectoryName);
			AssetDatabase.CreateFolder(state.TargetLocation, STFConstants.SecondaryAssetsDirectoryName);
			AssetDatabase.CreateFolder(state.TargetLocation, STFConstants.PreservedBuffersDirectoryName);
			AssetDatabase.Refresh();
		}

		private void ParseBuffers(STFFile buffers)
		{
			for(int i = 0; i < buffers.Buffers.Count(); i++)
			{
				state.Buffers.Add((string)state.JsonRoot["buffers"][i], buffers.Buffers[i]);
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
					// Unrecognized Resource
				}
			}
		}

		private void ParseAssets()
		{
			foreach(var entry in (JObject)state.JsonRoot["assets"])
			{
				var type = (string)entry.Value["type"];
				if(state.Context.AssetImporters.ContainsKey(type))
				{
					Debug.Log($"Parsing Asset: {type}");
					state.Context.AssetImporters[type].ParseFromJson(state, (JObject)entry.Value, entry.Key);
				}
				else
				{
					Debug.LogWarning($"Unrecognized Asset: {type}");
					// Unrecognized Asset
				}
			}
		}

		private void WriteToAssets()
		{
			
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
