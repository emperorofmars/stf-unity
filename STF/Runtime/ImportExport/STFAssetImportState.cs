
#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace STF.Serialisation
{
	public class STFAssetImportState : ISTFAssetImportState
	{
		private string _AssetId;
		public string AssetId => _AssetId;
		private ISTFImportState State;
		STFImportContext _Context;
		public STFImportContext Context {get =>_Context;}
		//ISTFAsset _Asset;
		//public ISTFAsset Asset {get =>_Asset;}
		public JObject JsonRoot {get => State.JsonRoot;}

		Dictionary<string, GameObject> _Nodes = new Dictionary<string, GameObject>();
		public Dictionary<string, GameObject> Nodes {get => _Nodes;}
		Dictionary<string, Component> _Components = new Dictionary<string, Component>();
		public Dictionary<string, Component> Components {get => _Components;}
		public Dictionary<string, UnityEngine.Object> Resources {get => State.Resources;}

		public STFAssetImportState(string AssetId, ISTFImportState State, STFImportContext Context)
		{
			this._AssetId = AssetId;
			this.State = State;
			this._Context = Context;
		}

		public void AddTask(Task task)
		{
			State.AddTask(task);
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
