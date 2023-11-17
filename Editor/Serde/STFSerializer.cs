
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
	public class STFExportState
	{

		public string TargetLocation;
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
		public List<UnityEngine.Object> Trash = new List<UnityEngine.Object>();
		public List<Task> Tasks = new List<Task>();

		public STFExportState(string TargetLocation)
		{
			this.TargetLocation = TargetLocation;
		}
	}

	// The main star for import!
	// Parses the Json and buffers based on the provided importers from the STFImportContext.
	public class STFSerializer
	{
		private STFExportState state;

		public STFSerializer(string TargetLocation, string path)
		{
			try
			{
				this.state = new STFExportState(TargetLocation);
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
	}
}

#endif
