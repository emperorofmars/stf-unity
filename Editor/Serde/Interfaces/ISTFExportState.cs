
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
		Dictionary<STFAsset, KeyValuePair<string, JObject>> Assets {get;}

		// Unity Resource -> Json Resource
		Dictionary<UnityEngine.Object, KeyValuePair<string, JObject>> Resources {get;}

		// Unity GameObject -> STF Json Node
		Dictionary<GameObject, KeyValuePair<string, JObject>> Nodes {get;}

		// Unity Component -> STF Json Component
		Dictionary<Component, KeyValuePair<string, JObject>> Components {get;}

		void AddTask(Task task);
		string AddAsset(STFAsset Asset, JObject Serialized, string Id = null);
		string AddNode(GameObject Go, JObject Serialized, string Id = null);
		string AddComponent(Component Component, JObject Serialized, string Id = null);
		string AddResource(UnityEngine.Object Resource, JObject Serialized, string Id = null);
		string AddBuffer(byte[] Data, string Id = null);
		void AddTrash(UnityEngine.Object Trash);

		T LoadMeta<T>(UnityEngine.Object Resource) where T: UnityEngine.Object, ISTFResource;
		(byte[], T, string) LoadAsset<T>(UnityEngine.Object Resource) where T: UnityEngine.Object, ISTFResource;
	}

	public class STFExportState : ISTFExportState
	{
		STFExportContext _Context;
		public STFExportContext Context {get =>_Context;}
		string _TargetLocation;
		public string TargetLocation {get =>_TargetLocation;}
		public string _MainAssetId;
		public string MainAssetId {get => _MainAssetId;}

		public Dictionary<STFAsset, KeyValuePair<string, JObject>> _Assets = new Dictionary<STFAsset, KeyValuePair<string, JObject>>();
		public Dictionary<STFAsset, KeyValuePair<string, JObject>> Assets {get => _Assets;}


		public Dictionary<GameObject, KeyValuePair<string, JObject>> _Nodes = new Dictionary<GameObject, KeyValuePair<string, JObject>>();
		public Dictionary<GameObject, KeyValuePair<string, JObject>> Nodes {get => _Nodes;}

		public Dictionary<UnityEngine.Object, KeyValuePair<string, JObject>> _Resources = new Dictionary<UnityEngine.Object, KeyValuePair<string, JObject>>();
		public Dictionary<UnityEngine.Object, KeyValuePair<string, JObject>> Resources {get => _Resources;}

		public Dictionary<Component, KeyValuePair<string, JObject>> _Components = new Dictionary<Component, KeyValuePair<string, JObject>>();
		public Dictionary<Component, KeyValuePair<string, JObject>> Components {get => _Components;}

		// id -> buffer
		public Dictionary<string, byte[]> Buffers = new Dictionary<string, byte[]>();

		// stuff to delete before the import finishes
		public List<UnityEngine.Object> Trash = new List<UnityEngine.Object>();
		public List<Task> Tasks = new List<Task>();

		public Dictionary<UnityEngine.Object, UnityEngine.Object> ResourceMeta = new Dictionary<UnityEngine.Object, UnityEngine.Object>();

		public STFExportState(STFExportContext Context, string TargetLocation, Dictionary<UnityEngine.Object, UnityEngine.Object> ResourceMeta)
		{
			this._Context = Context;
			this._TargetLocation = TargetLocation;
			this.ResourceMeta = ResourceMeta;
		}

		public void AddTask(Task task)
		{
			Tasks.Add(task);
		}

		public string AddAsset(STFAsset Asset, JObject Serialized, string Id = null)
		{
			if(Id == null || Id.Length == 0) Id = Guid.NewGuid().ToString();
			Assets.Add(Asset, new KeyValuePair<string, JObject>(Id, Serialized));
			return Id;
		}

		public string AddNode(GameObject Go, JObject Serialized, string Id = null)
		{
			if(Id == null || Id.Length == 0) Id = Guid.NewGuid().ToString();
			Nodes.Add(Go, new KeyValuePair<string, JObject>(Id, Serialized));
			return Id;
		}

		public string AddComponent(Component Component, JObject Serialized, string Id = null)
		{
			if(Id == null || Id.Length == 0) Id = Guid.NewGuid().ToString();
			Components.Add(Component, new KeyValuePair<string, JObject>(Id, Serialized));
			return Id;
		}

		public string AddResource(UnityEngine.Object Resource, JObject Serialized, string Id = null)
		{
			if(Id == null || Id.Length == 0) Id = Guid.NewGuid().ToString();
			Resources.Add(Resource, new KeyValuePair<string, JObject>(Id, Serialized));
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

		public T LoadMeta<T>(UnityEngine.Object Resource) where T: UnityEngine.Object, ISTFResource
		{
			if(ResourceMeta.ContainsKey(Resource)) return (T)ResourceMeta[Resource];
			
			var assetPath = AssetDatabase.GetAssetPath(Resource);
			var metaPath = Path.ChangeExtension(assetPath, "Asset");
			return AssetDatabase.LoadAssetAtPath<T>(metaPath);
		}
		public (byte[], T, string) LoadAsset<T>(UnityEngine.Object Resource) where T: UnityEngine.Object, ISTFResource
		{
			var assetPath = AssetDatabase.GetAssetPath(Resource);
			var arrayBuffer = File.ReadAllBytes(assetPath);
			var meta = AssetDatabase.LoadAssetAtPath<T>(Path.ChangeExtension(assetPath, "Asset"));
			return (arrayBuffer, meta, Path.GetFileName(assetPath));
		}
	}
}

#endif
