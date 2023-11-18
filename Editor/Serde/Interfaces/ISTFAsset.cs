
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
		// id -> asset
		Dictionary<string, STFAsset> Assets {get;}

		// id -> resource
		Dictionary<string, UnityEngine.Object> Resources  {get;}

		// id -> node
		Dictionary<string, GameObject> Nodes {get;}

		// id -> component
		Dictionary<string, Component> Components {get;}

		void AddTask(Task task);

		string GetResourceLocation();

		void AddResource(UnityEngine.Object Resource, string Id);

		void AddTrash(UnityEngine.Object Trash);
	}

	public class STFAssetImportState : ISTFAssetImportState
	{
		private STFImportState State;
		STFImportContext _Context;
		public STFImportContext Context {get =>_Context;}
		STFAssetInfo _AssetInfo;
		public STFAssetInfo AssetInfo {get =>_AssetInfo;}

		Dictionary<string, GameObject> _Nodes = new Dictionary<string, GameObject>();
		public Dictionary<string, GameObject> Nodes {get => _Nodes;}
		Dictionary<string, Component> _Components = new Dictionary<string, Component>();
		public Dictionary<string, Component> Components {get => _Components;}

		public Dictionary<string, STFAsset> Assets {get => State.Assets;}
		public Dictionary<string, UnityEngine.Object> Resources {get => State.Resources;}

		// stuff to delete before the import finishes
		public List<UnityEngine.Object> Trash = new List<UnityEngine.Object>();
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

		public void AddResource(UnityEngine.Object Resource, string Id)
		{
			Resources.Add(Id, Resource);
		}

		public void AddTrash(UnityEngine.Object Trash)
		{
			this.Trash.Add(Trash);
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
