
#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using STF.IdComponents;
using System.Threading.Tasks;

namespace STF.Serialisation
{
	public class STFAssetImportState : ISTFAssetImportState
	{
		private ISTFImportState State;
		STFImportContext _Context;
		public STFImportContext Context {get =>_Context;}
		STFAssetInfo _AssetInfo;
		public STFAssetInfo AssetInfo {get =>_AssetInfo;}
		public JObject JsonRoot {get => State.JsonRoot;}

		Dictionary<string, GameObject> _Nodes = new Dictionary<string, GameObject>();
		public Dictionary<string, GameObject> Nodes {get => _Nodes;}
		Dictionary<string, Component> _Components = new Dictionary<string, Component>();
		public Dictionary<string, Component> Components {get => _Components;}

		public Dictionary<string, STFAsset> Assets {get => State.Assets;}
		public Dictionary<string, UnityEngine.Object> Resources {get => State.Resources;}
		public List<Task> Tasks = new List<Task>();

		public STFAssetImportState(STFAssetInfo AssetInfo, ISTFImportState State, STFImportContext Context)
		{
			this._AssetInfo = AssetInfo;
			this.State = State;
			this._Context = Context;
		}

		public void AddTask(Task task)
		{
			Tasks.Add(task);
		}

		public void AddNode(GameObject Node, string Id)
		{
			Nodes.Add(Id, Node);
			AddTrash(Node);
		}

		public void AddComponent(Component Component, string Id)
		{
			Components.Add(Id, Component);
		}

		public void AddTrash(UnityEngine.Object Trash)
		{
			this.State.AddTrash(Trash);
		}

		public Object Instantiate(Object Resource)
		{
			return State.Instantiate(Resource);
		}
	}
}

#endif
