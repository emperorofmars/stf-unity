
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

namespace STF.Serde
{
	public class STFImportState
	{
		public STFImportContext Context;

		public string TargetLocation;
		public string MainAssetId;

		public JObject JsonRoot;

		// id -> asset
		public Dictionary<string, STFAsset> Assets = new Dictionary<string, STFAsset>();

		// id -> node
		public Dictionary<string, GameObject> Nodes = new Dictionary<string, GameObject>();

		// id -> resource
		public Dictionary<string, UnityEngine.Object> Resources = new Dictionary<string, UnityEngine.Object>();

		// id -> component
		public Dictionary<string, Component> Components = new Dictionary<string, Component>();

		// id -> buffer
		public Dictionary<string, byte[]> Buffers = new Dictionary<string, byte[]>();

		// stuff to delete before the import finishes
		public List<UnityEngine.Object> Trash = new List<UnityEngine.Object>();
		public List<Task> Tasks = new List<Task>();

		public STFImportState(STFImportContext Context, string TargetLocation, JObject JsonRoot)
		{
			this.Context = Context;
			this.TargetLocation = TargetLocation;
			this.JsonRoot = JsonRoot;
		}

		public void AddTask(Task task)
		{
			Tasks.Add(task);
		}

		public string GetResourceLocation()
		{
			return Path.Combine(TargetLocation, STFConstants.ResourceDirectoryName);
		}

		public void AddResource(UnityEngine.Object Resource, string Id)
		{
			Resources.Add(Id, Resource);
		}
	}

	// The main star for import!
	// Parses the Json and buffers based on the provided importers from the STFImportContext.
	public class STFDeserializer
	{
		private STFImportState state;

		public STFDeserializer(string TargetLocation, string Path)
		{
			Parse(STFRegistry.GetDefaultImportContext(), TargetLocation, Path);
		}

		public STFDeserializer(STFImportContext Context, string TargetLocation, string Path)
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
			}
			catch(Exception e)
			{
				foreach(var node in state.Nodes.Values)
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
			for(int i = 1; i < buffers.Buffers.Count(); i++)
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
					Debug.Log($"Parsing Resource: {type}");
					state.Context.ResourceImporters[type].ParseFromJson(state, (JObject)entry.Value, entry.Key);
				}
				else
				{
					Debug.LogWarning($"Unrecognized Resource: {type}");
					// Unrecognized Resource
				}
			}
		}

		private void WriteToAssets()
		{
			
		}
	}
}

#endif
