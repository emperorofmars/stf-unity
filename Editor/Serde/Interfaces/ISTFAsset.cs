
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json.Linq;
using STF.IdComponents;
using System.Threading.Tasks;
using System.IO;

namespace STF.Serde
{
	public interface ISTFAssetImportState
	{
		STFImportContext Context {get;}
		STFAssetInfo AssetInfo {get;}
		JObject JsonRoot {get;}
		// id -> asset
		Dictionary<string, STFAsset> Assets {get;}

		// id -> resource
		Dictionary<string, UnityEngine.Object> Resources  {get;}

		// id -> node
		Dictionary<string, GameObject> Nodes {get;}

		// id -> component
		Dictionary<string, Component> Components {get;}

		void AddTask(Task task);
		void AddNode(GameObject Node, string Id);
		void AddTrash(UnityEngine.Object Trash);
	}

	public class STFAssetImportState : ISTFAssetImportState
	{
		private STFImportState State;
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

		public STFAssetImportState(STFAssetInfo AssetInfo, STFImportState State, STFImportContext Context)
		{
			this._AssetInfo = AssetInfo;
			this.State = State;
			this._Context = Context;
		}

		public void AddTask(Task task)
		{
			Tasks.Add(task);
		}

		public string GetResourceLocation()
		{
			return Path.Combine(State.TargetLocation, STFConstants.ResourceDirectoryName);
		}

		public void AddNode(GameObject Node, string Id)
		{
			Nodes.Add(Id, Node);
			AddTrash(Node);
		}

		public void AddTrash(UnityEngine.Object Trash)
		{
			this.State.AddTrash(Trash);
		}
	}
	
	public interface ISTFAssetExporter
	{
		JObject SerializeToJson(STFExportState State, System.Object Asset);
	}
	
	public interface ISTFAssetImporter
	{
		UnityEngine.Object ParseFromJson(STFImportState State, JObject JsonAsset, string Id);
	}
}

#endif
