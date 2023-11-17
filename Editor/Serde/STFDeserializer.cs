
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

namespace STF.Serde
{
	public class STFImportState
	{

		public string TargetLocation = "STF_Imports";
		public string Filename;
		public string MainAssetId;

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
		private List<UnityEngine.Object> Trash = new List<UnityEngine.Object>();
		private List<Task> Tasks = new List<Task>();

		public STFImportState(string TargetLocation, string Filename)
		{
			this.TargetLocation = TargetLocation;
			this.Filename = Filename;
		}
	}

	// The main star for import!
	// Parses the Json and buffers based on the provided importers from the STFImportContext.
	public class STFDeserializer
	{
		private STFImportState state;

		public STFDeserializer(string TargetLocation, string path)
		{
			var buffers = new STFBuffers(path);
			this.state = new STFImportState(TargetLocation, Path.GetFileNameWithoutExtension(path));

			EnsureFolderStructure();
			AssetDatabase.Refresh();
		}

		private void EnsureFolderStructure()
		{
			var existingEntries = Directory.EnumerateFileSystemEntries(state.TargetLocation);foreach(var entry in existingEntries)
			{
				if(File.Exists(entry)) File.Delete(entry);
				else Directory.Delete(entry);
			}
		}

		private void WriteToAssets()
		{
			
		}
	}
}

#endif
