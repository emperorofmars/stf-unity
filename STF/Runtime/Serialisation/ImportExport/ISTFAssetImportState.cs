
#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace STF.Serialisation
{
	
	public interface ISTFAssetImportState
	{
		string AssetId {get;}

		STFImportContext Context {get;}
		//ISTFAsset Asset {get;}
		JObject JsonRoot {get;}
		// id -> asset
		//Dictionary<string, STFAsset> Assets {get;}

		// id -> resource
		Dictionary<string, UnityEngine.Object> Resources  {get;}

		// id -> node
		Dictionary<string, GameObject> Nodes {get;}

		// id -> component
		Dictionary<string, Component> Components {get;}

		void AddTask(Task task);
		void AddNode(GameObject Node, string Id);
		void AddComponent(Component Node, string Id);
		void AddTrash(UnityEngine.Object Trash);
		UnityEngine.Object Instantiate(UnityEngine.Object Resource);
	}
	
	public interface ISTFAssetExporter
	{
		string SerializeToJson(ISTFExportState State, ISTFAsset Asset);
	}
	
	public interface ISTFAssetImporter
	{
		void ParseFromJson(ISTFImportState State, JObject JsonAsset, string Id);
	}
	
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
