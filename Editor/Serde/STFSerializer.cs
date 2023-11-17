
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

		// Unity GameObject -> STF Json node
		public Dictionary<GameObject, JObject> Nodes = new Dictionary<GameObject, JObject>();
		public Dictionary<GameObject, string> NodeIds = new Dictionary<GameObject, string>();

		// Unity resource -> STF Json resource
		public Dictionary<UnityEngine.Object, JObject> Resources = new Dictionary<UnityEngine.Object, JObject>();
		public Dictionary<UnityEngine.Object, string> ResourceIds = new Dictionary<UnityEngine.Object, string>();

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

		public void AddTask(Task task)
		{
			Tasks.Add(task);
		}

		public string AddBuffer(byte[] Data, string Id = null)
		{
			if(Id == null || Id.Length == 0) Id = Guid.NewGuid().ToString();
			Buffers.Add(Id, Data);
			return Id;
		}

		public string AddResource(UnityEngine.Object Resource, JObject Serialized, string Id = null)
		{
			if(Id == null || Id.Length == 0) Id = Guid.NewGuid().ToString();
			Resources.Add(Resource, Serialized);
			ResourceIds.Add(Resource, Id);
			return Id;
		}

		public string AddNode(GameObject Go, JObject Serialized, string Id = null)
		{
			if(Id == null || Id.Length == 0) Id = Guid.NewGuid().ToString();
			Nodes.Add(Go, Serialized);
			NodeIds.Add(Go, Id);
			return Id;
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
