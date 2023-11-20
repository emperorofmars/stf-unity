
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json.Linq;
using STF.IdComponents;
using UnityEditor;
using System.Threading.Tasks;
using System.IO;

namespace STF.Serde
{
	public interface ISTFExportState
	{
		STFExportContext Context {get;}
		string TargetLocation {get;}
		string MainAssetId {get;}

		// Unity Asset -> Json Asset
		Dictionary<STFAsset, JObject> Assets {get;}
		Dictionary<STFAsset, string> AssetIds {get;}

		// Unity Resource -> Json Resource
		Dictionary<UnityEngine.Object, JObject> Resources {get;}
		Dictionary<UnityEngine.Object, string> ResourceIds {get;}

		// Unity GameObject -> STF Json Node
		Dictionary<GameObject, JObject> Nodes {get;}
		Dictionary<GameObject, string> NodeIds {get;}

		// Unity Component -> STF Json Component
		Dictionary<Component, JObject> Components {get;}
		Dictionary<Component, string> ComponentIds {get;}

		void AddTask(Task task);
		string AddAsset(STFAsset Asset, JObject Serialized, string Id = null);
		string AddNode(GameObject Go, JObject Serialized, string Id = null);
		string AddComponent(Component Component, JObject Serialized, string Id = null);
		string AddResource(UnityEngine.Object Resource, JObject Serialized, string Id = null);
		string AddBuffer(byte[] Data, string Id = null);
		void AddTrash(UnityEngine.Object Trash);
	}

	public class STFExportState : ISTFExportState
	{
		STFExportContext _Context;
		public STFExportContext Context {get =>_Context;}
		string _TargetLocation;
		public string TargetLocation {get =>_TargetLocation;}
		string _MainAssetId;
		public string MainAssetId {get => _MainAssetId;}

		public Dictionary<STFAsset, JObject> _Assets = new Dictionary<STFAsset, JObject>();
		public Dictionary<STFAsset, JObject> Assets {get => _Assets;}
		public Dictionary<STFAsset, string> _AssetIds = new Dictionary<STFAsset, string>();
		public Dictionary<STFAsset, string> AssetIds {get => _AssetIds;}


		public Dictionary<GameObject, JObject> _Nodes = new Dictionary<GameObject, JObject>();
		public Dictionary<GameObject, JObject> Nodes {get => _Nodes;}
		public Dictionary<GameObject, string> _NodeIds = new Dictionary<GameObject, string>();
		public Dictionary<GameObject, string> NodeIds {get => _NodeIds;}

		public Dictionary<UnityEngine.Object, JObject> _Resources = new Dictionary<UnityEngine.Object, JObject>();
		public Dictionary<UnityEngine.Object, JObject> Resources {get => Resources;}
		public Dictionary<UnityEngine.Object, string> _ResourceIds = new Dictionary<UnityEngine.Object, string>();
		public Dictionary<UnityEngine.Object, string> ResourceIds {get => _ResourceIds;}

		public Dictionary<Component, JObject> _Components = new Dictionary<Component, JObject>();
		public Dictionary<Component, JObject> Components {get => _Components;}
		public Dictionary<Component, string> _ComponentIds = new Dictionary<Component, string>();
		public Dictionary<Component, string> ComponentIds {get => _ComponentIds;}

		// id -> buffer
		public Dictionary<string, byte[]> Buffers = new Dictionary<string, byte[]>();

		// stuff to delete before the import finishes
		public List<UnityEngine.Object> Trash = new List<UnityEngine.Object>();
		public List<Task> Tasks = new List<Task>();

		public STFExportState(string TargetLocation)
		{
			this._TargetLocation = TargetLocation;
		}

		public void AddTask(Task task)
		{
			Tasks.Add(task);
		}

		public string AddAsset(STFAsset Asset, JObject Serialized, string Id = null)
		{
			if(Id == null || Id.Length == 0) Id = Guid.NewGuid().ToString();
			Assets.Add(Asset, Serialized);
			AssetIds.Add(Asset, Id);
			return Id;
		}

		public string AddNode(GameObject Go, JObject Serialized, string Id = null)
		{
			if(Id == null || Id.Length == 0) Id = Guid.NewGuid().ToString();
			Nodes.Add(Go, Serialized);
			NodeIds.Add(Go, Id);
			return Id;
		}

		public string AddComponent(Component Component, JObject Serialized, string Id = null)
		{
			if(Id == null || Id.Length == 0) Id = Guid.NewGuid().ToString();
			Components.Add(Component, Serialized);
			ComponentIds.Add(Component, Id);
			return Id;
		}

		public string AddResource(UnityEngine.Object Resource, JObject Serialized, string Id = null)
		{
			if(Id == null || Id.Length == 0) Id = Guid.NewGuid().ToString();
			Resources.Add(Resource, Serialized);
			ResourceIds.Add(Resource, Id);
			return Id;
		}

		public string AddBuffer(byte[] Data, string Id = null)
		{
			if(Id == null || Id.Length == 0) Id = Guid.NewGuid().ToString();
			Buffers.Add(Id, Data);
			return Id;
		}

		public void AddTrash(UnityEngine.Object Trash)
		{
			this.Trash.Add(Trash);
		}
	}
}

#endif
